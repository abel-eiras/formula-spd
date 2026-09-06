# Contratos — servicios de Aplicación de la Spec 011

## IServicioPerfilesImportacion

- `PerfilImportacion Crear(DatosAltaPerfilImportacion datos, int? usuarioQueEjecutaId)` — FR-1100.
- `void Actualizar(int perfilId, DatosAltaPerfilImportacion datos, int? usuarioQueEjecutaId)` —
  CA-1102 adaptado: editar un perfil ya creado, conservando su id.
- `PerfilImportacion? ObtenerPorNombre(string nombre)` — CA-1100.
- `IReadOnlyList<PerfilImportacion> ListarPorTipo(TipoPerfilImportacion tipo)`.

## ExtractorUnidadesEnvase (`Spd.Dominio`, función pura)

- `ResultadoPruebaExtraccion Probar(IReadOnlyList<string> filasDeMuestra, string patron)` —
  FR-1110/CA-1101; aplica el patrón a cada fila de muestra y devuelve cuántas aciertan (con el
  valor extraído) y cuántas fallan, sin guardar nada.
- `int? Extraer(string texto, string patron)` — aplica el patrón a una fila real (FR-1111, se usa
  como propuesta en la revisión de Spec 003, nunca se escribe directo).

## IServicioExportacionPacientes

- `string ExportarACsv(PerfilImportacion perfil, IReadOnlyList<Paciente> pacientes)` — FR-1130/1131;
  usa `perfil.Mapeo` en sentido inverso (campo → columna de salida, research.md Decisión 3);
  solo incluye los campos mapeados explícitamente (CA-1103).

Usado por: la pantalla de exportación de pacientes (nueva) y, más adelante, cuando exista una
muestra real de las columnas de Farmatic/Nixfarma/Unycop o de la columna de unidades del
nomenclátor, por las pantallas correspondientes de Specs 003/005 sin cambios en estos contratos.
