# Research: Import/export con programas de gestión

## Decisión 1 — Sin perfiles de fábrica ni wizard de detección de cabeceras (FR-1101/1102)

No hay muestras reales de las columnas exportadas por Farmatic, Nixfarma o Unycop en este
repositorio ni en la conversación con el propietario. Inventar esas columnas es exactamente el
mismo riesgo que ya se evitó con el Anexo 9 de Spec 002 (contenido específico y verificable que no
debe fabricarse). Se implementa el mecanismo genérico de perfil (FR-1100) sin precarga; el
administrador crea sus propios perfiles indicando el índice de columna, igual que
`PerfilImportacionTratamiento` de Spec 005 — mismo patrón ya aceptado y probado.

## Decisión 2 — Extractor de unidades como función pura, sin conectar al lector real del nomenclátor

FR-1110 pide extraer un número de una columna de texto vía regex, con un contador de
aciertos/fallos antes de guardar (CA-1101). Se implementa `ExtractorUnidadesEnvase.Probar(texto[],
patron)` como función pura en `Spd.Dominio`, reutilizable e independiente del lector CSV real.
No se conecta a `LectorNomenclatorCsv`/`FilaNomenclator` (Spec 003): ese lector ya está afinado
contra el fichero real de la AEMPS (`fix(003)` en el historial de `main`) sin una columna de
"descripción con unidades" conocida — añadir un campo especulativo a `FilaNomenclator` sin una
muestra real de esa columna arriesga romper un parser ya verificado contra datos reales para
ganar una integración que no se puede probar. `ExtractorUnidadesEnvase` queda listo para que una
sesión futura, con una muestra real de esa columna, lo conecte sin cambios en su firma.

## Decisión 3 — Exportación de pacientes reutiliza el mismo `PerfilImportacion` en sentido inverso

FR-1130 pide exportación "con el mismo mecanismo de perfil, en sentido inverso". En vez de crear
una entidad `PerfilExportacion` separada, se reutiliza `PerfilImportacion` (tipo `PACIENTES`): su
`Mapeo` (columna origen → campo) se lee en sentido inverso (campo → columna de salida) porque es
la misma lista de pares campo/columna, solo que la dirección del volcado cambia. Alternativa
descartada: una entidad nueva — duplicaría el concepto de "lista de campos mapeados a columnas"
que ya existe (Art. V, un dato una entrada).

## Decisión 4 — Codificación del perfil se registra, no se aplica todavía

FR-1100 exige que el perfil registre la codificación (p. ej. Windows-1252). Como el único
consumidor real de ficheros hoy (Spec 005, importación de tratamiento) ya lee sus CSV en UTF-8 y
no hay ningún caso de uso real todavía que requiera leer con otra codificación, `PerfilImportacion.Codificacion`
se guarda como dato (para cuando haga falta) sin que esta iteración implemente la lectura con
codificaciones alternativas. Documentado en spec.md §7 (casos límite).
