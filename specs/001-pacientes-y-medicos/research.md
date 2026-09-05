# Research — Spec 001: Pacientes, contactos y catálogo de médicos

Fase 0 de `/speckit-plan`. Decisiones técnicas concretas dentro del stack ya fijado (Art. VIII).

## Decisión 1 — Búsqueda sin distinguir mayúsculas ni tildes (FR-010, FR-032, CA-011)

**Contexto**: `docs/data-model.md` menciona un "índice de búsqueda sobre `apellidos || ' ' || nombre`
normalizado sin tildes" para Medico, y la spec exige lo mismo para pacientes (nombre, apellidos,
DNI, CIP, num_ficha). SQLite no tiene una función `unaccent`/`LOWER` que quite tildes de forma
nativa, y el proyecto no depende de la extensión FTS5 (no está garantizada en el binario embebido
que trae `SQLitePCLRaw.bundle_e_sqlcipher`).

**Decisión**: se añade una columna calculada y persistida `busqueda_normalizada` (TEXT) tanto en
`Paciente` como en `Medico`, mantenida por la capa de Aplicación (no por un trigger SQL): al
crear/editar, el servicio concatena los campos buscables, pasa el resultado por un normalizador de
Dominio (`Normalizador.QuitarTildesYMayusculas`, minúsculas + descomposición Unicode NFD +
eliminación de diacríticos) y lo guarda. Las búsquedas comparan
`busqueda_normalizada LIKE '%' || @terminoNormalizado || '%'`, con el término de búsqueda pasado
por el mismo normalizador antes de la consulta. SQL explícito, sin funciones SQLite registradas en
tiempo de ejecución.

**Alternativas consideradas**: registrar una función SQLite personalizada (`SqliteConnection.
CreateFunction`) — descartada por Art. X.2 (más piezas móviles: lógica repartida entre C# y una
función registrada por conexión, más difícil de testear de forma aislada). FTS5 — descartada por no
estar garantizada en el paquete SQLCipher embebido que exige el Art. VII.2.

## Decisión 2 — `motivo_baja` de Paciente: dominio cerrado

**Contexto**: `docs/data-model.md` no detalla el dominio de `motivo_baja`; FR-007 sí lo fija
(`FALLECIMIENTO`, `RENUNCIA`, `TRASLADO`, `HOSPITALIZACION_PROLONGADA`, `CRITERIO_FARMACEUTICO`,
`OTRO` con texto libre).

**Decisión**: `motivo_baja TEXT` con `CHECK` sobre los 6 valores (mismo patrón que `Usuario.rol` en
Spec 000), más `motivo_baja_detalle TEXT` nullable para el texto libre cuando `motivo_baja = 'OTRO'`.
Se actualiza `docs/data-model.md` a v0.6 con ambas columnas.

**Alternativas consideradas**: tabla de catálogo `MotivoBaja` — rechazada por Art. X.2 (6 valores
fijos por la propia constitución del PNT, no configurables por el usuario; una tabla añadiría una
pantalla de mantenimiento que nadie pidió).

## Decisión 3 — Numeración de ficha (FR-001, CA-001)

**Contexto**: `num_ficha` debe ser un correlativo de 6 dígitos con el prefijo vigente, que **no se
reinicia** al cambiar el prefijo (spec 000 CA-001: cambiar de "F-" a "PAC-" hace que el siguiente
alta sea "PAC-000042", continuando desde 41, no desde 0). `docs/data-model.md` solo tiene
`num_ficha TEXT UNIQUE`, sin una columna de correlativo numérico independiente del texto.

**Decisión**: se añade `Paciente.correlativo_num_ficha INTEGER NOT NULL`, un entero secuencial
independiente del prefijo. Al crear un paciente: `siguiente = MAX(correlativo_num_ficha) + 1` (o 1
si no hay ninguno), `num_ficha = prefijo_num_ficha_vigente || siguiente.ToString("D6")`. `num_ficha`
se calcula una vez y no se recalcula nunca (FR-001, "no editable"); `correlativo_num_ficha` es la
única fuente de verdad para el "siguiente número", evitando parsear el texto de `num_ficha` (que
puede tener prefijos de longitud variable de altas pasadas). Se actualiza `docs/data-model.md`
añadiendo esta columna.

**Alternativas consideradas**: parsear `num_ficha` con `SUBSTR`/`LENGTH` del prefijo actual para
extraer el correlativo — rechazada porque falla en cuanto cambia la longitud del prefijo entre
altas (Art. X.2, más frágil que una columna dedicada).

## Decisión 4 — Validación de DNI/NIE (FR-005)

**Decisión**: regla de Dominio pura (`ValidadorDni`), sin dependencias externas (Art. VIII.1):
algoritmo estándar español — para NIE se sustituye la letra inicial (X→0, Y→1, Z→2) por su dígito
equivalente antes de aplicar el mismo cálculo que un DNI; el número resultante mod 23 indexa la
tabla de letras `TRWAGMYFPDXBNJZSQVHLCKE`. Devuelve si el formato y la letra de control son
válidos; el resultado se usa como aviso (FR-004 estilo), nunca bloquea el guardado salvo por los
mínimos de FR-003.

**Alternativas consideradas**: ninguna — es un algoritmo público y determinista, no hay elección de
librería que tenga sentido para 20 líneas de lógica pura.

## Decisión 5 — Validación de CIP gallego (FR-005, FR-005b)

**Decisión**: regla de Dominio pura (`ValidadorCip`) que implementa exactamente el algoritmo de
FR-005 (posiciones 1–6 fecha `aammdd`, 7–10 iniciales/segundas letras de apellidos sin tildes,
posición 11 sexo, 12–14 no verificables) más el autocompletado de FR-005b (genera las posiciones
1–11 a partir de fecha de nacimiento, apellidos y sexo, dejando 12–14 en blanco). Reutiliza
`Normalizador.QuitarTildesYMayusculas` (Decisión 1) para las iniciales/segundas letras de apellidos.

**Alternativas consideradas**: ninguna — mismo razonamiento que Decisión 4.

---

**Output**: todas las incógnitas técnicas de esta feature quedan resueltas; ninguna arrastra
`NEEDS CLARIFICATION` a `tasks.md`.
