# Research — Spec 003: Catálogo de medicamentos

Fase 0 de `/speckit-plan`. Decisiones técnicas concretas dentro del stack ya fijado (Art. VIII).

## Decisión 1 — Formas farmacéuticas: dominio cerrado (FR-300, Clarifications Q1)

**Contexto**: FR-300 fija una lista cerrada; `/speckit-clarify` confirmó que no se amplía.

**Decisión**: enum de Dominio `FormaFarmaceutica` con exactamente los 8 valores de FR-300
(Comprimido, ComprimidoLiberacionProlongada, Capsula, CapsulaLiberacionProlongada, Gragea,
Pastilla, Pildora, OtraNoApta), mismo patrón que `Rol`/`EstadoPaciente` de specs anteriores: mapeo
explícito a texto en el repositorio, sin conversión automática de Dapper.

**Alternativas consideradas**: tabla de catálogo editable — rechazada por Art. X.2, la propia
clarificación confirmó que la lista fija basta.

## Decisión 2 — `apto_spd` derivado + `motivo_no_apto` como texto libre (FR-301, CA-302)

**Contexto**: `apto_spd` se deriva de `forma_farmaceutica` pero es editable a mano con motivo
obligatorio. El documento no fija si el motivo es de un catálogo cerrado o texto libre.

**Decisión**: `motivo_no_apto` es texto libre (TEXT, nullable), obligatorio únicamente cuando el
usuario fija `apto_spd` a un valor **distinto** del que se derivaría automáticamente de la forma
farmacéutica vigente (en cualquier sentido: forzar apto cuando la derivación diría que no, o al
revés). La razón depende de informes de laboratorio que el sistema no puede enumerar de antemano
(FR-301 lo dice explícitamente), a diferencia de `Paciente.motivo_baja` en Spec 001, que sí tiene
una lista cerrada porque el PNT la fija.

**Regla de derivación** (`AptoSpdPorDefecto(FormaFarmaceutica)`): apto por defecto excepto para
formas que la spec no lista pero que FR-301 menciona como quedando fuera (líquidas, efervescentes,
bucodispersables, parenterales, tópicas) — como esas formas no existen en el enum cerrado de
Decisión 1 (ninguno de los 8 valores es líquido/efervescente/etc.), la regla de derivación en esta
versión resulta: **todas las formas del enum son aptas por defecto salvo `OtraNoApta`** (que ya lo
dice su propio nombre). Se documenta así para que quede explícito y no se intente inventar una
tabla de excepciones que no tiene con qué rellenarse.

## Decisión 3 — `desc_texto`: propuesta autogenerada, nunca sobrescribe una edición manual (FR-303)

**Contexto**: `desc_texto` es "autogenerado a partir de los anteriores y editable a mano". Regenerar
en cada cambio de `desc_forma`/`desc_color`/`desc_ranura`/`desc_serigrafia`/`desc_tamano`
borraría silenciosamente una corrección manual previa.

**Decisión**: mismo patrón que el autocompletado de CIP en Spec 001 (Art. V.1, "propone, el
usuario confirma o corrige"): `ServicioMedicamentos` expone `ProponerDescripcionTexto(...)` que
construye el texto a partir de los campos `desc_*` informados (`"{forma} {color}"` + `", ranura
{ranura}"` si hay ranura + `", serigrafía {serigrafía}"` si hay + `", {tamaño}"` si hay). La
Presentación la ofrece como sugerencia editable en un campo de texto ya rellenable a mano; el
servicio nunca sobrescribe `desc_texto` por su cuenta al guardar.

**Alternativas consideradas**: regenerar siempre al guardar — rechazada porque contradice
literalmente "editable a mano" de FR-303 y el propio Art. V.1.

## Decisión 4 — Versionado de `Medicamento_Hist` (FR-303/FR-304, CA-301, Art. IV.2/IV.3)

**Contexto**: cada cambio de descripción física añade una fila a `Medicamento_Hist` con su
`vigente_desde`. Para saber desde cuándo era vigente la versión que se está sustituyendo,
`Medicamento` necesita su propia fecha de inicio de vigencia — `docs/data-model.md` no la
menciona explícitamente.

**Decisión**: se añade `Medicamento.desc_vigente_desde` (TEXT, no nula, se fija al crear el
registro). Al cambiar cualquier campo `desc_*`: se inserta en `Medicamento_Hist` una fila con los
valores **anteriores** de `desc_*` y el `desc_vigente_desde` **anterior**; después se actualiza
`Medicamento` con los valores nuevos y `desc_vigente_desde = ahora`. Así cada fila de
`Medicamento_Hist` documenta un periodo de vigencia cerrado, y `Medicamento` siempre refleja el
periodo abierto actual. Las líneas de SPD (Spec 006, fuera de esta spec) seguirán citando su
propia instantánea tomada en el momento de crear la línea (Art. IV.3), no esta tabla — esta tabla
es solo el historial del catálogo, no la instantánea de un SPD concreto.

**Alternativas consideradas**: derivar `vigente_desde` de `MAX(vigente_desde)` de las filas de
`Medicamento_Hist` ya existentes — rechazada porque falla en el primer cambio (no hay fila previa
en Hist todavía) y duplica en tiempo de consulta lo que una sola columna resuelve en tiempo de
escritura (Art. X.2).

## Decisión 5 — Importación del nomenclátor: mapeo mínimo, ampliable por Spec 011 (FR-320..322)

**Contexto**: FR-320 exige una pantalla de revisión que compare el nomenclátor descargado (Spec
000, `IServicioNomenclator`, ya implementado — solo descarga, no parsea) con el catálogo. El
mapeo columna-a-columna configurable es explícitamente de Spec 011 ("Fuera de alcance de esta
spec"), que en el orden de construcción del proyecto va **después** de esta spec 003. No existe
todavía ningún `PerfilImportacion` real que consultar.

**Decisión**: se implementa un `LectorNomenclatorCsv` mínimo y explícito que interpreta el fichero
descargado como CSV con cabecera y dos columnas reconocidas por nombre exacto (`CN`, `Nombre`) —
suficiente para que FR-320 funcione de extremo a extremo con un nomenclátor real de esas dos
columnas. Si el fichero no tiene esas columnas, `ProcesarNomenclator` devuelve un resultado de
error explicando qué columnas faltan, en vez de adivinar un mapeo. Esta es una simplificación
deliberada y documentada: Spec 011 sustituirá este lector fijo por `PerfilImportacion`
configurable sin cambiar el contrato de `IServicioMedicamentos.CompararConNomenclator`, que ya
trabaja sobre una lista de `(Cn, Nombre)` ya extraída, independiente del formato de origen.

**Alternativas consideradas**: esperar a Spec 011 para implementar FR-320..322 — descartada porque
la propia spec 003 los incluye como requisitos de esta iteración (Art. X.1, "se construye lo que
la spec pide"); no implementarlos sin más justificación sería incumplir la spec sin motivo
documentado. Adivinar varios formatos de fichero (Excel real, columnas por posición) — descartada
por Art. X.2, más piezas móviles sin ninguna certeza de que coincidan con el nomenclátor real de
Pontevedra; se prefiere fallar con un mensaje claro a asumir un formato no confirmado.

---

**Output**: todas las incógnitas técnicas de esta feature quedan resueltas; ninguna arrastra
`NEEDS CLARIFICATION` a `tasks.md`. La simplificación de Decisión 5 queda documentada para
revisarla explícitamente cuando se implemente Spec 011.

### Corrección 2026-09-05 (prueba manual con el nomenclátor oficial real)

La premisa de la Decisión 5 — "suficiente para que FR-320 funcione de extremo a extremo con un
nomenclátor real de esas dos columnas" — era incorrecta: el fichero oficial de facturación real
(descargado y probado en producción de pruebas) no tiene columnas llamadas `CN`/`Nombre`, sino
`Código Nacional`/`Nombre del producto farmacéutico` (entre otras 18 columnas), y cita entre
comillas los campos que contienen comas (nombres de producto con la presentación tras una coma,
razón social de laboratorios como "SANOFI AVENTIS, S.A"). Con la cabecera y el `Split(',')`
ingeniosos originales, el fichero real siempre fallaba con "no tiene las columnas esperadas" o,
si hubiera coincidido por casualidad, habría desalineado columnas en cualquier fila con una coma
citada.

`LectorNomenclatorCsv` (`src/Spd.Infraestructura/LectorNomenclatorCsv.cs`) se corrige para: (1)
reconocer ambas cabeceras, la mínima de pruebas y la real oficial, y (2) parsear la línea CSV
respetando comillas (RFC 4180) en vez de partir por comas a ciegas. Se añade además una lectura
opcional, cuando la columna existe, de `Principio activo o asociación de principios activos` y
`Nombre del laboratorio ofertante` — campos ya modelados en `Medicamento` (FR-300) y sin ninguna
de las restricciones de FR-321 (que solo protege descripción física y aptitud SPD) — para que el
alta desde nomenclátor (FR-322) los rellene si están disponibles, en vez de dejarlos vacíos
pudiendo obtenerse gratis del propio fichero. `FilaNomenclator` gana esos dos campos opcionales;
el contrato de `ILectorNomenclator`/`CompararConNomenclator` no cambia.

Sigue sin implementarse (Spec 011, sin cambios): extraer `unidades_envase` por regex del texto
tras la coma en el nombre del producto (p. ej. "20 comprimidos"). Confirmado explícitamente al
detectar este error: no sirve para inferir descripción física (forma/color/ranura/serigrafía) ni
si el envase es emblistable a nivel oficial — solo el número de unidades y una forma farmacéutica
en texto libre que habría que normalizar contra el catálogo cerrado de FR-300.
