# Research: Depósito de envases y listado de retirada

## Decisión 1 — Campos de parámetros ya existentes, sin migración

`docs/data-model.md` y las specs 000/001 ya dejaron preparados los campos que FR-500/FR-501 piden:
`Farmacia.DiaRetiradaDefecto`, `NBlisteresDefecto`, `DiasAntelacionListado` (src/Spd.Dominio/Farmacia.cs)
y `Paciente.DiaRetirada`, `NBlisteres` (src/Spd.Dominio/Paciente.cs), ya persistidos por las
migraciones 0001 y 0002. Esta spec no toca esas tablas: solo lee esos campos. Verificado por
inspección directa de las entidades y sus migraciones antes de diseñar `0006_envase.sql`.

## Decisión 2 — Nueva migración `0006_envase.sql`

Solo dos tablas nuevas: `Envase` y `PerfilImportacionTratamiento`. Ninguna otra tabla existente
necesita columnas nuevas para esta iteración (`Medicamento.Fraccionable`, `UnidadesEnvase`,
`Contacto.RetiraMedicacion`, `Tratamiento.AjusteUnidadesManual` ya existen desde Specs 003/001/004).

## Decisión 3 — Punto de extensión para "SPD ya preparado/verificado" (FR-530, CA-502)

Sin la entidad SPD (Spec 006), la condición de exclusión del listado por "ya tiene un SPD que
cubre esa retirada" no puede evaluarse contra datos reales todavía. Se define la interfaz
`IComprobadorCoberturaSpd` con un único método `bool YaCubierta(int pacienteId, DateOnly
proximaRetirada)`. La implementación por defecto registrada en DI (`ComprobadorCoberturaSpdNulo`)
devuelve siempre `false` (nunca excluye a nadie por este motivo), documentado con un comentario
que remite a Spec 006. `ServicioListadoRetirada` depende de la interfaz, no de la implementación,
así que Spec 006 solo tendrá que registrar una implementación real sin tocar el servicio.
Alternativa descartada: dejar un TODO sin abstracción — violaría el principio de no bloquear specs
futuras con un cambio de firma pública (mismo patrón que Spec 010 dejó para la Purga, Art. X).

## Decisión 4 — "Fin de validez del último blíster previsto" sin SPD real (FR-531)

Definido en spec.md §9 (Assumptions): `próxima_retirada + 7 × n_blisteres − 1 día`. Es la fecha de
fin de validez que tendría el blíster si se preparase exactamente en la próxima retirada — la
única fecha determinable sin un SPD real. Cuando Spec 006 exista, el listado podrá sustituir este
cálculo por la validez real del último SPD PREPARADO/VERIFICADO si lo hay, sin cambiar la fórmula
de `disponibles`/`faltan` (que sigue siendo "envases con caducidad ≥ esa fecha").

## Decisión 5 — Algoritmo de asignación y descuento como servicio independiente de SPD

`IServicioAsignacionEnvases.Descontar(int tratamientoId, int unidadesADescontar, DateOnly
caducidadMinima, int? usuarioId)` opera sobre los envases `EN_CUSTODIA` del medicamento+paciente
del tratamiento indicado, aplica el orden de FR-520 (unidades restantes asc, caducidad asc),
actualiza la base de datos real (no una simulación en memoria) y audita. Se prueba llamándolo
directamente con envases de prueba, sin pasar por ninguna pantalla de preparación — igual que
Spec 010 probó `ServicioCifrado` sin que existiera todavía el flujo de arranque completo. Spec 006
lo invocará una vez por línea al pasar un SPD a PREPARADO.

## Decisión 6 — Cálculo de unidades a descontar (FR-522) como función pura

`CalculadoraUnidadesADescontar.Calcular(Tratamiento)` — sin acceso a datos, solo lee
`PautaD/A/C/N`, `DiasSemana` y `AjusteUnidadesManual` del propio tratamiento. Reglas:
suma semanal exacta si ninguna dosis del patrón semanal es fraccionaria; si alguna lo es,
`floor(suma) + 1` siempre (incluso si la suma ya es entera, CA-507b); el override
`AjusteUnidadesManual` gana siempre que esté informado. Al ser una función pura sobre `Tratamiento`
(igual patrón que `FraccionDosisExtensiones` de Spec 004), se prueba exhaustivamente sin base de
datos.

## Decisión 7 — Importación de fichero: CSV en esta iteración, no .xlsx

FR-570 pide "Excel/CSV". Añadir una librería de lectura de `.xlsx` es una dependencia nueva no
trivial (Art. X, simplicidad; Art. XI, software libre — hay que elegir con cuidado, no de pasada)
y el motor genérico de importación de ficheros ya está previsto explícitamente para Spec 011
("perfiles guardados... igual que Spec 011", FR-572). Esta iteración implementa el origen
`FICHERO` leyendo CSV con separador configurable (`PerfilImportacionTratamiento.Separador`) y deja
el soporte `.xlsx` como extensión de Spec 011 sin cambiar el contrato (`ParserLineasImportacion`
ya trabaja sobre filas de texto, sea el origen CSV o el resultado de parsear un `.xlsx` más
adelante). El origen `PORTAPAPELES` (texto tabulado, FR-570) sí se implementa completo: es el caso
de uso principal descrito en E7 (pegar desde el programa de gestión) y no requiere ninguna
dependencia nueva.

## Decisión 8 — Entrega fuera de blíster no pasa por `EN_CUSTODIA`

FR-543: el envase de un medicamento `en_spd = 0` se registra directamente en estado
`ENTREGADO_PACIENTE`, sin pasar por `EN_CUSTODIA` primero — no tiene sentido de custodia intermedia
para algo que no se emblistera. Se añade el campo `EntregadoA` (texto libre) al modelo de `Envase`
para "a quién se entrega" (FR-543), no presente en `docs/data-model.md` porque ese documento no
detalla los campos de la entrega fuera de blíster; se documenta aquí como la única extensión de
campo sobre el modelo ya publicado, y se reflejará en `docs/data-model.md` al mergear (Art. IV.1:
un dato, una entrada — el campo vive solo en `Envase`, no se duplica en otra tabla).
