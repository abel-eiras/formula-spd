# Feature Specification: Depósito de envases y listado de retirada

**Feature Branch**: `005-deposito-y-retirada`

**Created**: 2026-09-06

**Status**: Draft — pendiente de `/speckit-clarify`

**Constitución aplicable:** 2.1.0 (Artículos I.3, III, IV, V, VII)

**Depende de:** Spec 001 (pacientes), Spec 003 (catálogo de medicamentos), Spec 004 (tratamiento)
— las tres ya mergeadas en `main`.

**Requerida por:** Spec 006 (preparación), 007 (impresión), 011 (import/export), 012 (DataMatrix)

**Input**: Especificación completa aportada literalmente por el propietario del producto
(`spec-005-deposito-y-retirada.md`, v0.1 — 2026-09-04). Se traslada tal cual: no se reinterpretan
los requisitos, no se añaden requisitos nuevos, no se cambia la numeración FR-5xx ni los criterios
de aceptación CA-5xx.

**Alcance de esta iteración**: FR-500 a FR-577 completos, salvo la ejecución real del descuento
"al pasar una preparación a PREPARADO" (FR-520/521), que depende de la entidad SPD de la Spec 006
(no existe todavía). Esta spec construye y prueba el **algoritmo de asignación y descuento**
(FR-520–522) como servicio de aplicación reutilizable e independiente de SPD, con tests directos
sobre listas de envases y unidades a descontar; Spec 006 será quien lo invoque desde el flujo real
de preparación. Esto es análogo a cómo Spec 010 (backup/cifrado) construyó y probó sus servicios
sin que existieran todavía las entidades de Paciente.

---

## Clarifications

### Session 2026-09-06

- Q: FR-502 — si el paciente lleva semanas sin SPD (suspendido y reactivado), ¿la próxima
  retirada es el primer día de retirada a partir de hoy? → A: Sí (propuesta única de la propia
  spec, sin alternativa razonable mejor: es la lectura literal de FR-502 cuando "no tiene ninguno"
  SPD entregado o verificado vigente).
- Q: FR-512/Q2 — ¿la caducidad del envase admite solo mes/año o fecha completa? → A: Fecha
  completa; si se introduce mes/año a mano se toma el último día del mes (propuesta única de la
  propia spec — el DataMatrix da día exacto, así que el modelo debe admitir esa precisión).
- Q: ¿El listado de retirada debe mostrar también el CN? → A: Sí, ya está en FR-532 (no era una
  ambigüedad real, sino una confirmación de alcance ya cubierto).

---

## 1. Propósito

Gestionar los envases en custodia de cada paciente con trazabilidad por número de serie, lote y
caducidad, y decirle a la farmacia **qué envases retirar y cuándo** para que nunca haya en
custodia más de lo necesario para la próxima preparación. El depósito es el motor de la
preparación: sin envases suficientes no hay blíster.

Base normativa: los envases retirados son de uso exclusivo del paciente; se custodian
identificados y separados; no se retira un envase nuevo hasta que hace falta para llenar el
blíster; ningún sobrante de un envase se desecha mientras el tratamiento siga activo.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Consultar el listado de retirada, registrar envases retirados, registrar salidas a SIGRE, registrar entregas fuera de blíster, importar tratamientos — sin distinción de categoría profesional |

## 3. Escenarios de usuario (User Stories)

Priorizados por valor entregable independiente, para permitir implementación incremental.

### US1 (P1) — Registrar y consultar envases en custodia (E2, E6)
Como farmacéutico, tras dispensar en el programa de gestión, registro el envase en el depósito
del paciente con serie, lote y caducidad, y puedo ver en todo momento qué envases tiene en
custodia, sus unidades restantes y avisos de caducidad.

**Prueba independiente**: dar de alta un envase desde la pestaña Depósito de un paciente con
tratamiento activo y verificar que aparece en su listado con los datos introducidos.

### US2 (P1) — Saber qué retirar antes de preparar (E1, E8)
Como farmacéutico, el día antes de preparar abro el listado de retirada y veo, por paciente, qué
medicamentos y cuántos envases necesito sacar del stock, con nombre, CIP y DNI de retirada, y me
lo llevo impreso.

**Prueba independiente**: con un paciente en ventana de antelación y stock insuficiente, el
listado muestra la fila con necesarias/disponibles/faltan/envases a retirar correctos.

### US3 (P2) — Aprovechar el sobrante en el descuento (E3)
Como elaborador, el algoritmo de asignación agota primero el envase ya empezado y nunca descarta
un sobrante por completar un blíster.

**Prueba independiente**: servicio de asignación probado con dos envases (uno parcial, uno lleno)
y una cantidad a descontar; verificar el orden de consumo y los restantes resultantes.

### US4 (P2) — Cese de tratamiento y bajas (E4)
Como farmacéutico, cuando el médico retira o cambia un medicamento, o el paciente se da de baja,
el sistema propone (sin ejecutar sin confirmación) la salida a SIGRE de las unidades restantes.

**Prueba independiente**: finalizar un tratamiento con envases en custodia y verificar que se
proponen para salida a SIGRE; confirmar y verificar el nuevo estado.

### US5 (P3) — Medicación fuera de blíster (E5)
Como farmacéutico, para un medicamento con `en_spd = 0`, registro la entrega al paciente sin serie
obligatoria y sin que cuente en el listado de retirada.

### US6 (P3) — Alta de tratamiento y envase por copiar/pegar o fichero (E7)
Como farmacéutico, importo desde el programa de gestión (pegado o Excel/CSV) varias líneas de
CN + serie + lote + caducidad de una vez, creando tratamientos y envases sin teclear cada campo.

## 4. Requisitos funcionales

### 4.1 Parámetros

- **FR-500** Configuración de la farmacia (Spec 000): `dia_retirada_defecto` (lunes…domingo),
  `n_blisteres_defecto` (1 o 2), `dias_antelacion_listado` (entero ≥ 0; número de días antes del
  día de retirada en que el paciente aparece en el listado; por defecto 2).
- **FR-501** Ficha de paciente (Spec 001): `dia_retirada` y `n_blisteres`, prerrellenados con los
  valores de configuración, editables por paciente.
- **FR-502** Fecha de la **próxima retirada** de un paciente = primer `dia_retirada` posterior a
  `validez_hasta` del último SPD entregado o verificado; si no tiene ninguno, el primer
  `dia_retirada` a partir de hoy. Se calcula, no se almacena.

### 4.2 Envase en custodia

- **FR-510** Un envase pertenece a un paciente y a un medicamento del catálogo. Campos: número de
  serie, lote, caducidad (fecha completa), unidades iniciales, unidades restantes, fecha de
  retirada, origen (`ESCANEADO`, `MANUAL`, `IMPORTADO`), estado, fecha y motivo de salida.
- **FR-511** Estados: `EN_CUSTODIA` → `AGOTADO` (unidades restantes = 0 tras una preparación) |
  `RESIDUO_SIGRE` (cese de tratamiento, caducidad, deterioro) | `ENTREGADO_PACIENTE` (solo
  medicamentos con `en_spd = 0`). No hay eliminación.
- **FR-512** Obligatorios al registrar un envase de un medicamento `en_spd = 1`: serie, lote,
  caducidad, unidades iniciales. El sistema advierte si la serie ya existe en cualquier paciente
  (misma serie = mismo envase físico) y no permite duplicarla. Caducidad: fecha completa; si se
  introduce mes/año a mano se toma el último día del mes.
- **FR-513** Unidades iniciales se prerrellenan con `unidades_envase` del catálogo (Spec 003);
  editables en el alta porque **el envase puede darse de alta ya empezado** (parte del contenido
  usado antes de entrar en custodia SPD) — el valor introducido es siempre "lo que hay dentro en
  el momento del alta", no necesariamente el tamaño de envase completo. Si el valor del catálogo
  está vacío, el usuario lo introduce y el sistema ofrece guardar el tamaño de envase completo en
  el catálogo (no el valor parcial de este envase concreto).
- **FR-514** Caducidad anterior a la fecha de hoy: aviso al registrar; no bloquea el alta pero el
  envase nunca se propone para una preparación.
- **FR-515** Solo se puede registrar un envase de un medicamento que tenga tratamiento activo para
  ese paciente. Si no lo tiene, el sistema ofrece crear el tratamiento (Spec 004) antes.
- **FR-516** El registro de un envase se puede hacer desde: la pestaña Depósito del paciente, el
  listado de retirada (fila del paciente/medicamento, prerrellena ambos), o la pantalla de
  preparación (Spec 006, fuera de esta iteración) cuando falta stock.
- **FR-517** La pestaña Depósito muestra los envases `EN_CUSTODIA` con unidades restantes,
  caducidad y un aviso "caduca antes de la próxima validez" cuando
  `caducidad < próxima retirada + 7 × n_blisteres`. Los envases en otros estados se ven con
  "mostrar histórico".

### 4.3 Descuento y sobrante (regla de consumo)

- **FR-520** Al pasar una preparación a PREPARADO (Spec 006), cada línea descuenta unidades de los
  envases asignados. Orden de propuesta de envases para una línea: `EN_CUSTODIA`, caducidad ≥
  `validez_hasta` del blíster, ordenados por **unidades restantes ascendente** y a igualdad por
  caducidad ascendente. Es decir: primero se agota el envase empezado.
- **FR-521** Un envase con unidades restantes > 0 tras una preparación permanece `EN_CUSTODIA` y es
  el primero propuesto en la siguiente. Nunca se descuenta a residuo por sobrante.
- **FR-522** Fracciones: los trozos cortados de un comprimido/cápsula fraccionable que no se usan
  en el blíster **se desechan**; no se guardan de una preparación a otra. Regla de cálculo de
  unidades a descontar del envase por semana:
  - Si ningún día de la pauta tiene una dosis fraccionaria (todas las dosis son números enteros),
    unidades a descontar = suma semanal exacta.
  - Si algún día tiene una dosis fraccionaria, unidades a descontar = `floor(suma semanal) + 1`,
    **siempre**, incluso cuando la suma semanal ya sea un número entero (p. ej. 0,5/día seis días
    = 3,0 exactos → se descuentan 4, no 3, porque cada fragmento cortado y no usado se pierde y no
    se puede garantizar aprovechamiento perfecto entre cortes). Ejemplo del propietario: 0,5/día
    siete días = 3,5 → se descuentan 4 (no 2,5 ni 3,5).
  - Este cálculo puede sobrescribirse con `Tratamiento.ajuste_unidades_manual` (ya existente en el
    dominio desde Spec 004), editable desde la ficha de tratamiento del paciente para corregir un
    desajuste puntual. Cuando hay override, se usa ese valor y se ignora la fórmula.
  - El medicamento debe tener `fraccionable = 1` para admitir dosis fraccionarias; si no, la línea
    se bloquea con aviso (sin cambios respecto a versiones anteriores).

### 4.4 Listado de retirada

- **FR-530** Pantalla "Retirada de envases" con fecha de referencia (por defecto hoy). Incluye a
  todo paciente `ACTIVO` cuya próxima retirada (FR-502) esté entre `hoy` y
  `hoy + dias_antelacion_listado`, inclusive, y que no tenga ya un SPD en estado PREPARADO o
  VERIFICADO cuyo periodo cubra esa retirada (sin SPD en esta iteración, esta condición nunca
  excluye a nadie por este motivo hasta que exista Spec 006 — ver Assumptions).
- **FR-531** Para cada paciente incluido y cada tratamiento activo `en_spd = 1`:
  `necesarias = ceil(unidades_blister) × n_blisteres`
  `disponibles = Σ unidades restantes de envases EN_CUSTODIA con caducidad ≥ fin de validez del
  último blíster previsto`
  `faltan = max(0, necesarias − disponibles)`
  `envases_a_retirar = ceil(faltan / unidades_envase)`
  Solo se listan las filas con `faltan > 0`.
- **FR-532** Columnas: paciente (apellidos, nombre), CIP, **DNI de retirada**, medicamento (nombre
  y CN), unidades necesarias, disponibles, faltan, envases a retirar, próxima retirada, nº de
  blísteres. Ordenado por próxima retirada y apellidos. Agrupado por paciente.
- **FR-532b** `DNI de retirada` = DNI del contacto marcado "retira la medicación" (Spec 001
  FR-021b) si existe; si no, el DNI del propio paciente. Si ninguno de los dos está informado, la
  celda queda vacía con aviso "sin DNI registrado", sin bloquear el listado.
- **FR-533** Cada fila tiene la acción "Registrar envase" (FR-516). Al registrar, la fila se
  recalcula; desaparece cuando `faltan = 0`. Un paciente sin filas desaparece del listado.
- **FR-534** Filtros: por día de retirada, por paciente, "solo pacientes con faltantes" (por
  defecto) / "todos los pacientes previstos" (muestra también los que ya tienen stock, como lista
  de preparación).
- **FR-535** Imprimible (Spec 007, documento "Listado de retirada" — fuera de esta iteración; se
  deja el punto de extensión) con fecha, hora y usuario. Cada impresión se registra en auditoría.
- **FR-536** Un paciente con `unidades_envase` desconocido en algún medicamento aparece con
  `envases_a_retirar = "?"` y aviso; el listado sigue funcionando para el resto.
- **FR-537** El listado se recalcula al abrir la pantalla y tras cada registro. No se almacena; es
  una vista sobre tratamiento y depósito.

### 4.5 Salidas

- **FR-540** Salida a SIGRE: acción sobre un envase `EN_CUSTODIA` con motivo
  (`CESE_TRATAMIENTO`, `CAMBIO_TRATAMIENTO`, `CADUCADO`, `DETERIORADO`, `FALLECIMIENTO`,
  `BAJA_PACIENTE`, `OTRO` con texto). Registra fecha, usuario y unidades desechadas, y alimenta el
  registro de residuos no SIGRE solo si el motivo lo requiere (los medicamentos van a SIGRE; el
  registro "no SIGRE" es para material de acondicionamiento y otros, Spec 009 — fuera de esta
  iteración, se deja el punto de extensión).
- **FR-541** Al finalizar o cambiar un tratamiento (Spec 004), el sistema muestra los envases
  `EN_CUSTODIA` de ese medicamento y propone la salida a SIGRE. No la ejecuta sin confirmación.
- **FR-542** Al dar de baja a un paciente (Spec 001), el sistema muestra todos sus envases
  `EN_CUSTODIA` y propone salida masiva a SIGRE con motivo `BAJA_PACIENTE` o `FALLECIMIENTO`. No la
  ejecuta sin confirmación.
- **FR-543** Entrega al paciente: solo para envases de medicamentos con `en_spd = 0`. Registra
  fecha, usuario y a quién se entrega. Serie y lote son opcionales para estos envases; el sistema
  no calcula unidades ni los incluye en el listado de retirada.
- **FR-544** No existe "devolución al stock de la farmacia". Un envase retirado para un paciente no
  vuelve al stock general bajo ninguna acción de la aplicación.

### 4.6 Avisos en inicio

- **FR-550** Panel de inicio: número de pacientes en el listado de retirada de hoy con faltantes;
  envases en custodia que caducan en los próximos 30 días; envases en custodia de medicamentos
  cuyo tratamiento ya no está activo (candidatos a SIGRE).

### 4.7 Auditoría

- **FR-560** Alta de envase, descuento por preparación, salida a SIGRE, entrega al paciente e
  impresión del listado dejan traza con usuario, fecha-hora y detalle.

### 4.8 Alta de tratamiento y envase por copiar/pegar o fichero

- **FR-570** Pantalla "Importar tratamiento" accesible desde la pestaña Depósito o Tratamiento del
  paciente. Dos orígenes: **pegar** (el usuario pega texto tabulado copiado del programa de
  gestión) o **fichero** (Excel/CSV).
- **FR-571** Columnas mínimas reconocidas, en cualquier orden, mapeadas por un perfil de
  importación (`PerfilImportacionTratamiento`, ver data-model.md): CN, número de serie, lote,
  caducidad. El asistente muestra una vista previa de las primeras filas y permite mapear cada
  columna del origen a cada campo destino antes de confirmar.
- **FR-572** Perfiles guardados por nombre para reutilizar (p. ej. "Farmatic — pegado desde
  dispensación"), igual que Spec 011 (fuera de esta iteración; se reutiliza aquí el mismo concepto
  de perfil).
- **FR-573** Para cada fila importada: si el CN no tiene tratamiento activo `en_spd = 1` para ese
  paciente, el sistema lo crea con los datos mínimos (medicamento por CN, `en_spd = 1`) y **exige
  completar la posología antes de que el tratamiento pueda usarse en una preparación** — el envase
  se registra igualmente. Si el CN ya tiene tratamiento activo, se reutiliza y solo se da de alta
  el envase.
- **FR-574** El envase se registra con las reglas de FR-510–FR-514: serie, lote, caducidad de la
  fila; unidades iniciales desde `Medicamento.unidades_envase` si existe, o pendiente de completar
  si no.
- **FR-575** Filas con CN no encontrado en el catálogo de medicamentos: se listan aparte al final
  de la importación para alta manual del medicamento (Spec 003); no bloquean la importación de las
  demás filas.
- **FR-576** Filas con serie ya existente en el sistema (FR-512): se listan aparte como no
  importadas, con el paciente al que pertenece esa serie, sin detener el resto de la importación.
- **FR-577** Resumen final de la importación: filas importadas, tratamientos nuevos creados
  (pendientes de posología), envases dados de alta, filas con error, con acceso directo a resolver
  cada pendiente.

## 5. Entidades clave

| Entidad | Notas |
|---|---|
| Envase | docs/data-model.md §Envase. Estado y unidades restantes son el corazón de esta spec |
| Paciente.dia_retirada, Paciente.n_blisteres | Añadidos por esta spec |
| Farmacia.dia_retirada_defecto, n_blisteres_defecto, dias_antelacion_listado | Añadidos por esta spec |
| Medicamento.unidades_envase | Spec 003; origen manual o regla sobre nomenclátor |
| PerfilImportacionTratamiento | docs/data-model.md §PerfilImportacionTratamiento |

## 6. Criterios de aceptación

**CA-500 Cálculo de faltantes**
Dado un paciente con `n_blisteres = 2`, un tratamiento de 1 cápsula/día en SPD y un envase con 4
unidades en custodia, cuando abro el listado en su ventana de antelación, entonces la fila muestra
necesarias 14, disponibles 4, faltan 10, y con `unidades_envase = 28`, envases a retirar 1.

**CA-501 Paciente fuera de ventana**
Dado `dias_antelacion_listado = 2`, un paciente con día de retirada jueves y hoy lunes, cuando
abro el listado, entonces el paciente no aparece; el martes sí.

**CA-502 Paciente ya preparado**
Dado un paciente con un SPD VERIFICADO que cubre su próxima retirada, cuando abro el listado,
entonces no aparece aunque esté en ventana. *(Diferido: sin entidad SPD en esta iteración, se
prueba la condición como no-op que nunca excluye; el gancho queda documentado en el servicio para
que Spec 006 lo complete.)*

**CA-503 Registrar desde el listado**
Dado una fila con faltan 10, cuando pulso "Registrar envase" y guardo un envase de 28 unidades,
entonces la fila desaparece y el envase está en la pestaña Depósito del paciente con 28 restantes.

**CA-504 Serie duplicada**
Dado un envase registrado con serie X en el paciente A, cuando intento registrar serie X en el
paciente B, entonces el sistema lo impide indicando que esa serie ya está en custodia de A.

**CA-505 Sobrante se usa primero**
Dado envases E1 (3 restantes, cad. 2027-06) y E2 (28 restantes, cad. 2027-01) del mismo
medicamento y una línea de 7 unidades, cuando se propone la asignación, entonces el orden es E1
(3) y luego E2 (4); tras preparar, E1 queda AGOTADO y E2 con 24.

**CA-506 Sobrante permanece**
Dado un envase de 28 y una línea de 7, cuando se ejecuta el descuento, entonces el envase queda
EN_CUSTODIA con 21 restantes y no existe ninguna acción que lo descarte por sobrante.

**CA-507 Fracción consume entero+1 (suma no entera)**
Dado una pauta 0,5/día todos los días (suma semanal 3,5), cuando se calcula la línea, entonces las
unidades a descontar son 4.

**CA-507b Fracción consume entero+1 aunque la suma ya sea entera**
Dado una pauta 0,5/día en seis días de la semana (suma semanal 3,0 exactos), cuando se calcula la
línea, entonces las unidades a descontar son 4, no 3.

**CA-507c Ajuste manual sobrescribe el cálculo**
Dado un tratamiento con `ajuste_unidades_manual = 5` y una pauta cuyo cálculo automático daría 4,
cuando se genera la línea, entonces se descuentan 5 unidades del envase.

**CA-508 Cese propone SIGRE**
Dado un tratamiento con un envase de 12 restantes, cuando finalizo el tratamiento con motivo
"retirado por el médico", entonces el sistema muestra el envase y propone salida a SIGRE; al
confirmar, el estado es RESIDUO_SIGRE con 12 unidades desechadas y motivo CESE_TRATAMIENTO.

**CA-509 Sin devolución al stock**
Dado cualquier envase en cualquier estado, cuando busco acciones disponibles, entonces ninguna lo
devuelve al stock de la farmacia ni lo reasigna a otro paciente.

**CA-510 Entrega fuera de blíster**
Dado un medicamento con `en_spd = 0`, cuando registro un envase sin serie y lo marco como
entregado al paciente, entonces se guarda con estado ENTREGADO_PACIENTE y no aparece en el listado
de retirada ni en el cálculo de disponibles.

**CA-511 Caducidad excluye de disponibles**
Dado un envase con caducidad 2026-09-15 y una próxima retirada el 2026-09-14 con 2 blísteres
(validez hasta 2026-09-27), cuando se calcula disponibles, entonces ese envase no cuenta y aparece
marcado "caduca antes de la próxima validez".

**CA-512 Unidades por envase desconocidas**
Dado un medicamento sin `unidades_envase`, cuando aparece en el listado, entonces la columna
envases a retirar muestra "?" y las demás filas se calculan con normalidad.

**CA-513 Listado impreso**
*(Diferido a Spec 007 — impresión real no existe en esta iteración; se prueba solo que la acción
de auditoría IMPRIMIR queda registrada cuando se invoca el punto de extensión.)*

**CA-514 DNI de retirada, con responsable**
Dado un paciente con un contacto marcado "retira la medicación" con DNI 11111111A, cuando abro el
listado, entonces la columna DNI muestra 11111111A y no el DNI del paciente.

**CA-515 DNI de retirada, sin responsable**
Dado un paciente sin contacto marcado "retira la medicación" y DNI propio 22222222B, cuando abro
el listado, entonces la columna DNI muestra 22222222B.

**CA-516 Importar por pegado, tratamiento y envase existentes**
Dado un paciente con tratamiento activo para el CN 654321, cuando pego una línea con ese CN, serie
nueva S123, lote L1, caducidad 2027-01, entonces se crea un envase EN_CUSTODIA para ese
tratamiento sin duplicar el tratamiento.

**CA-517 Importar por pegado, tratamiento nuevo**
Dado un paciente sin tratamiento para el CN 999999, cuando pego una línea con ese CN, entonces se
crea un tratamiento pendiente de posología y un envase asociado, y el resumen final lista ese
tratamiento como pendiente.

**CA-518 Importar fichero con serie duplicada**
Dado un fichero de 5 filas donde una serie ya existe en otro paciente, cuando importo, entonces se
dan de alta 4 envases, la fila conflictiva aparece en el resumen como no importada con el nombre
del paciente que ya tiene esa serie, y la importación no se detiene.

## 7. Casos límite

- Envase registrado a un paciente por error: no se elimina ni se reasigna; se da salida con motivo
  `OTRO` y texto explicativo, y se registra el correcto. El error queda trazado.
- Medicamento con `n_blisteres = 2` y un solo envase de 10 unidades para 14 necesarias: el listado
  pide 1 envase más; en la preparación el blíster 1 consume 7 del primer envase y el blíster 2
  consume 3 del primero y 4 del segundo; el primero queda AGOTADO.
- Paciente que pasa de 1 a 2 blísteres: la siguiente ventana calcula el doble; los envases en
  custodia siguen valiendo.
- Cambio de posología a la baja: puede dejar más unidades en custodia de las necesarias. No se
  hace nada; se consumirán en preparaciones sucesivas. La norma prohíbe retirar de más, no
  conservar sobrantes de un tratamiento activo.
- Paciente SUSPENDIDO: no aparece en el listado; sus envases siguen en custodia y su caducidad se
  sigue vigilando.
- Dos pacientes con el mismo medicamento: sus envases nunca se mezclan; el sistema no ofrece "usar
  el envase de otro paciente" en ningún caso.

## 8. Fuera de alcance de esta spec

- Lectura de DataMatrix (Spec 012): aquí solo se define que el alta puede recibir serie, lote,
  caducidad y CN ya parseados.
- Regla regex sobre nomenclátor para extraer unidades por envase (Spec 003 / 011).
- Registro de residuos no SIGRE (Spec 009).
- Stock general de la farmacia: no existe en esta aplicación.
- Entidad SPD, preparación real y su pantalla (Spec 006): el descuento (FR-520–522) se construye
  como servicio invocable, sin el flujo de preparación que lo dispara.
- Impresión real del listado (Spec 007): se deja el punto de extensión y el registro de auditoría.
- Perfiles de importación compartidos con Spec 011: se implementa aquí el perfil específico de
  tratamiento+envase (`PerfilImportacionTratamiento`), no el motor genérico de Spec 011.

## 9. Assumptions

- FR-530 (exclusión por SPD ya preparado/verificado) no puede aplicarse hoy porque no existe la
  entidad SPD; el servicio de listado expone el punto de extensión (una comprobación que hoy
  siempre devuelve "no cubierto") documentado para que Spec 006 lo complete sin cambiar la firma
  pública.
- "Fin de validez del último blíster previsto" (FR-531) se calcula, en ausencia de SPD real, como
  `próxima retirada + 7 × n_blisteres − 1 día` (la ventana de validez que tendría el próximo
  blíster si se preparase hoy), documentado en research.md.
