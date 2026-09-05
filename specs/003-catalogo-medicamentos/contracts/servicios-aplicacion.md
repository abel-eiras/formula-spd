# Contratos — servicios de Aplicación de la Spec 003

Interfaz entre `Spd.Presentacion` y `Spd.Aplicacion`. Firmas en pseudo-C#; la implementación exacta
se decide en `/speckit-tasks`/`/speckit-implement`.

## IServicioMedicamentos

- `IReadOnlyList<Medicamento> Buscar(string fragmento)` — FR-305; por CN exacto o fragmento de
  nombre, sin tildes ni mayúsculas.
- `Medicamento? ObtenerPorCn(string cn)` — FR-306.
- `Medicamento Crear(DatosAltaMedicamento datos, int? usuarioQueEjecutaId)` — FR-300/FR-302/FR-306;
  alta mínima con solo CN+nombre; si el CN ya existe de baja lo reactiva (CA-305); si ya existe
  activo, lanza `MedicamentoDuplicadoException` con el existente (CA-303, bloqueo real, no aviso).
- `void ActualizarDatos(Medicamento medicamento, int? usuarioQueEjecutaId)` — FR-300/FR-301; si
  `apto_spd` se fija distinto del derivado de la forma farmacéutica, exige `motivo_no_apto`
  (CA-302).
- `string ProponerDescripcionTexto(DatosDescripcionFisica datos)` — FR-303 (research.md Decisión
  3); solo propone, no persiste.
- `void ActualizarDescripcionFisica(int medicamentoId, DatosDescripcionFisica datos, int? usuarioQueEjecutaId)` —
  FR-303/FR-304; versiona la descripción anterior en `Medicamento_Hist` antes de sobrescribir
  (research.md Decisión 4, CA-301: no afecta a instantáneas ya congeladas fuera de esta spec).
- `IReadOnlyList<VersionDescripcionFisica> ListarHistorialDescripcion(int medicamentoId)` — FR-304.
- `void ActualizarUnidadesEnvase(int medicamentoId, int unidadesEnvase, int? usuarioQueEjecutaId)` —
  FR-310; siempre `MANUAL` cuando la llama la Presentación directamente (FR-311).
- `void DarDeBaja(int medicamentoId, int? usuarioQueEjecutaId)` — baja lógica (Art. III.1); no
  impide nada adicional en esta spec (a diferencia de Medico en Spec 001, un medicamento de baja no
  tiene la restricción de "no puede estar en uso" porque los tratamientos citan su propia
  instantánea, no una referencia viva).

Usado por: el catálogo de esta spec, y (más adelante) Spec 004 (selector de medicamento al crear
un tratamiento, mismo patrón de selector con autocompletado que el de médico en Spec 001) sin
necesitar cambios en este contrato.

## IServicioImportacionNomenclator

- `ResultadoComparacionNomenclator CompararConNomenclator(string rutaFicheroDescargado)` — FR-320;
  usa `LectorNomenclatorCsv` (research.md Decisión 5) para extraer filas `(Cn, Nombre)` y las
  compara con el catálogo: nuevas, con nombre distinto, sin cambios. Si el fichero no tiene las
  columnas `CN`/`Nombre`, el resultado indica el error sin lanzar excepción (es un fallo esperable
  de un fichero externo, no un error de programación).
- `Medicamento AplicarAltaDesdeNomenclator(string cn, string nombre, int? usuarioQueEjecutaId)` —
  FR-322; alta mínima de una fila del nomenclátor, confirmada una a una por el usuario.
- `void AplicarNombreDesdeNomenclator(int medicamentoId, string nombreNuevo, int? usuarioQueEjecutaId)` —
  FR-321/FR-322; solo el nombre, nunca descripción física ni aptitud SPD (esos campos no los toca
  ni siquiera con confirmación, porque FR-321 los excluye explícitamente de la importación).

Usado por: la pantalla de revisión de importación de esta spec. Spec 011 sustituirá
`LectorNomenclatorCsv` por un `PerfilImportacion` configurable sin cambiar este contrato.
