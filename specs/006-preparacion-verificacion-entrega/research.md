# Research: Preparación, verificación y entrega del SPD

## Decisión 1 — Punto de extensión para idoneidad/consentimiento (FR-602, Constitución Art. I.3)

Sin Spec 002, no existe todavía la evaluación de idoneidad ni el consentimiento real. Se define
`IComprobadorIdoneidadYConsentimiento.Aprobado(int pacienteId)`, con
`ComprobadorIdoneidadYConsentimientoNulo` como implementación por defecto que siempre devuelve
`true`. Mismo patrón que `IComprobadorCoberturaSpd` de Spec 005 hacia esta misma spec. La regla
constitucional queda con test unitario sobre el punto de extensión (verificando que
`ServicioPreparacion.CrearSesion` la consulta y respeta su resultado), satisfaciendo la exigencia
de "regla implementada en Dominio con test unitario" sin inventar los criterios clínicos de
Spec 002. Cuando Spec 002 exista, registrará su propia implementación sin cambiar
`ServicioPreparacion`.

## Decisión 2 — Cuándo se consume el envase: al pasar a PREPARADO, no al crear la sesión

Spec 005 FR-520 ya dice literalmente "al pasar una preparación a PREPARADO (Spec 006), cada línea
descuenta unidades". Se diseña así: `CrearSesion` crea el/los SPD en `BORRADOR` con sus
`SPD_Linea` (instantánea + `unidades_dosis`/`unidades_envase` calculados), pero **sin**
`SPD_Linea_Envase` todavía — nada se ha consumido. `PasarAPreparado` es quien, línea a línea, llama
a `IServicioAsignacionEnvases.Descontar` (ya construido y probado en Spec 005) y crea las filas de
`SPD_Linea_Envase` a partir de su resultado (research.md Decisión 5 de Spec 005: el propio
`Descontar` ya reparte por FR-520/FR-611 sin cambios). Esto permite que, entre crear la sesión y
pasar a PREPARADO, el elaborador use FR-620 para completar envases sin haber tocado ya la
custodia.

## Decisión 3 — Atomicidad de "pasar a PREPARADO" sin transacción cruzando repositorios

`Descontar` (Spec 005) ya es atómico **por línea**: comprueba el saldo total antes de mutar nada y
lanza si no alcanza. Para que un fallo en la línea 3 de 5 no deje las líneas 1-2 ya descontadas,
`PasarAPreparado` hace una **pasada de validación previa** (sin mutar: suma disponible vs.
necesaria por línea, misma fórmula que `ServicioListadoRetirada`) sobre todas las líneas no
excluidas; solo si todas pasan se ejecuta `Descontar` línea a línea. No hace falta una transacción
SQL que cruce `RepositorioEnvases`/`RepositorioSpdLineas`: en el modelo de uso de la aplicación (un
elaborador trabajando una preparación a la vez, sin escritura concurrente sobre el mismo paciente)
la doble comprobación es suficiente y evita acoplar los repositorios de dos specs a una
transacción compartida. Alternativa descartada: pasar un `IDbTransaction` a través de todos los
repositorios ya existentes de Spec 005 — cambiaría contratos ya fusionados y probados sin necesidad
real en este caso de uso.

## Decisión 4 — RegistroAmbiental y MaterialAcondicionamiento nacen en esta spec

`0004_registros_calidad.sql` (Spec 009) ya deja escrito que estas dos entidades se retiraron de su
alcance a favor de esta spec ("la temperatura/humedad se rellenará más adelante... al generar la
hoja de elaboración del blíster"). Se crean aquí tal como esa nota anticipaba: `RegistroAmbiental`
(temperatura, humedad, `fuera_rango` congelado contra `Farmacia.TempMin/Max`/`HrMin/Max` en el
momento del registro, igual patrón de instantánea que Spec 009 usó para sus propios registros) y
`MaterialAcondicionamiento` (catálogo simple: descripción, lote, fecha de entrada, activo).

## Decisión 5 — Continuidad (FR-670..674) como comparación en el momento de generar, no como estado persistido

`MODIFICADA`/`NUEVA`/`ELIMINADA` de FR-673 no son valores de `SPD_Linea.estado_linea` en
`docs/data-model.md` (que solo define `NORMAL`/`EXCLUIDA`/`ENVASE_PENDIENTE`). Se implementan como
el resultado de una comparación que `PrepararSiguiente` calcula al generar la sesión siguiente
(tratamientos activos de ahora vs. líneas de la sesión anterior), devuelto en un DTO
`ResultadoContinuidad` para que la pantalla resalte qué revisar; las líneas que sí se persisten
llevan `EstadoLinea.Normal` o `EnvasePendiente` únicamente. `ENVASE_PENDIENTE` sí es un valor real
de `estado_linea` porque bloquea esa línea de forma persistente hasta resolverse con FR-620.

## Decisión 6 — Reelaboración reutiliza el mismo servicio de asignación, en modo diferencial

FR-6124 pide consumir solo la *diferencia* de unidades cuando una línea aumenta, y devolver la
diferencia a custodia cuando disminuye o se elimina. Se implementa como dos operaciones sobre
`IRepositorioEnvases` ya existente: `Descontar` (diferencia positiva, mismo servicio de Spec 005)
y una nueva `ServicioPreparacion.DevolverACustodia(envaseId, unidades)` — sencilla suma sobre
`unidades_restantes`, sin necesitar un servicio nuevo en Spec 005 (Art. V: la operación de devolver
es específica de la reelaboración, no una operación general de depósito — FR-544 sigue prohibiendo
"devolución al stock" como concepto general; esto es devolver *a la misma custodia del mismo
paciente*, el envase nunca cambia de dueño).

## Decisión 7 — Numeración de `num_registro`

Mismo patrón que `Paciente.NumFicha` (Spec 001): un correlativo entero (`MAX+1` sobre todos los
SPD) formateado como `{Farmacia.PrefijoNumSpd}{correlativo:D6}`, ya que `Farmacia.PrefijoNumSpd`
existe desde Spec 000 sin usar todavía.

## Decisión 8 — Impresión real diferida a Spec 007 (FR-680/681)

`ServicioPreparacion.RegistrarImpresion(spdId, tipoDocumento, usuarioId)` solo actualiza el
timestamp `impreso_*_en` correspondiente y audita `IMPRIMIR`; no genera ningún fichero. Mismo
patrón que Spec 005 dejó para `ServicioListadoRetirada.RegistrarImpresion` hacia Spec 007.
