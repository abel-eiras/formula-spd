# Contratos — servicios de Aplicación de la Spec 007

## IServicioGeneracionDocumentos

- `ResultadoGeneracionDocumento GenerarFichaSpd(int spdId, int? usuarioQueEjecutaId)` — FR-700
  `FICHA`; incluye todas las líneas del SPD, con sus filas de envase si hay más de una (Spec 006
  FR-611), y la posología siempre en fracción (FR-740/743).
- `ResultadoGeneracionDocumento GenerarEtiquetaAnverso(int spdId, int? usuarioQueEjecutaId)` — `ETQ-A`.
- `ResultadoGeneracionDocumento GenerarEtiquetaReverso(int spdId, int? usuarioQueEjecutaId)` — `ETQ-R`;
  una fila por cada fila de envase de cada línea (serie/lote/caducidad).
- `ResultadoGeneracionDocumento GenerarInstrucciones(int spdId, int? usuarioQueEjecutaId)` — `INSTR`.
- `ResultadoGeneracionDocumento GenerarFichaPaciente(int pacienteId, int? usuarioQueEjecutaId)` — `FICHA-PAC`.

Cada método: construye el PDF con QuestPDF (Decisión 1), determina el nombre con
`GeneradorNombreFichero` (FR-710/711), escribe en `<RutaDocumentosGenerados>/<fecha>/` (FR-712) y
audita `GENERAR_DOCUMENTO` (FR-713) — toda esta parte común vive en un único método privado
(research.md Decisión 2), no repetida por generador.

Usado por: `PreparacionView` (Spec 006, botones "Imprimir ficha/etiquetas/instrucciones") y
`FichaPacienteView` (Spec 001, botón "Imprimir ficha"). Sustituye el punto de extensión
`ServicioPreparacion.RegistrarImpresion` (research.md Decisión 8 de Spec 006) por generación real
seguida del mismo registro de auditoría — las pantallas llaman primero a este servicio para
generar el fichero, y siguen llamando a `RegistrarImpresion` para marcar `impreso_*_en` en el SPD.
