# Contratos — servicios de Aplicación de la Spec 001

Interfaz entre `Spd.Presentacion` y `Spd.Aplicacion`. Firmas en pseudo-C#; la implementación exacta
se decide en `/speckit-tasks`/`/speckit-implement`.

## IServicioMedicos

- `IReadOnlyList<Medico> Buscar(string fragmento)` — FR-032; mínimo 2 caracteres, sobre
  `busqueda_normalizada`. Devuelve lista vacía si `fragmento.Length < 2`.
- `Medico Crear(DatosAltaMedico datos, int? usuarioQueEjecutaId)` — FR-033/034; devuelve el médico
  ya creado para dejarlo seleccionado en el campo de origen (CA-005).
- `void Actualizar(Medico medico, int? usuarioQueEjecutaId)` — FR-035, propaga a toda referencia
  por ser FK, no copia (CA-006).
- `void DarDeBaja(int medicoId, int? usuarioQueEjecutaId)` — FR-036; lanza
  `MedicoConPacientesActivosException` con la lista de pacientes si aplica (CA-007).
- `ResultadoDuplicadoMedico ComprobarDuplicado(string apellidos, string nombre, string? colegiado)` — FR-034.

Usado por: el selector de médico de esta spec, y (más adelante) Spec 004 (prescriptor) y Spec 008
(destinatario de comunicaciones) — sin necesitar cambios en este contrato.

## IServicioPacientes

- `Paciente Crear(DatosAltaPaciente datos, int? usuarioQueEjecutaId)` — FR-001/FR-002/FR-003;
  asigna `num_ficha` (research.md Decisión 3), valida mínimos (CA-002), avisa de duplicado por
  DNI/CIP (FR-004, CA-003) sin bloquear si el usuario decide continuar.
- `void Actualizar(Paciente paciente, int? usuarioQueEjecutaId)` — audita antes/después (FR-042,
  CA-013).
- `void CambiarEstado(int pacienteId, EstadoPaciente nuevoEstado, DatosBaja? datosBaja, int? usuarioQueEjecutaId)` —
  FR-006/FR-007; valida transición permitida; exige `datosBaja` (fecha+motivo) al pasar a `BAJA`
  (CA-009); al reactivar (`BAJA→EVALUACION`) no reasigna consentimiento ni idoneidad — eso es
  Spec 002 (CA-010).
- `IReadOnlyList<Paciente> Buscar(string fragmento, EstadoPaciente[]? filtroEstados, int? filtroMedicoId)` —
  FR-010/FR-011; orden activos→evaluación→suspendidos→bajas, por apellidos dentro de cada grupo
  (CA-011).
- `ResultadoValidacionDni ValidarDni(string dni)` — FR-005 (Dominio puro tras la fachada de
  Aplicación).
- `ResultadoValidacionCip ValidarCip(string cip, DateOnly fechaNacimiento, string apellidos, string sexo)` — FR-005.
- `string AutocompletarCip(DateOnly fechaNacimiento, string apellidos, string sexo)` — FR-005b
  (CA-014).

Usado por: la ficha de paciente de esta spec, y (más adelante) Spec 002/004/005/006/008 que leen
`Paciente`/`Contacto` sin necesitar un contrato nuevo (acceden vía `IRepositorioPacientes`
directamente, ya que son consultas, no casos de uso de esta spec).

## IServicioContactos

- `Contacto Crear(DatosAltaContacto datos, int? usuarioQueEjecutaId)` — FR-020/021/021b/021c/022;
  valida DNI obligatorio según tipo/marca (CA-008), y desmarca cualquier otro `es_principal`/
  `retira_medicacion` previo del mismo paciente al marcar uno nuevo (FR-021/021b).
- `void DarDeBaja(int contactoId, int? usuarioQueEjecutaId)` — FR-023, baja lógica.
- `IReadOnlyList<Contacto> ListarDePaciente(int pacienteId, bool incluirBaja = false)` — FR-023.
