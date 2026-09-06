# Contratos — servicios de Aplicación de la Spec 002

## IServicioIdoneidadConsentimiento

- `EstadoIdoneidadConsentimiento Consultar(int pacienteId)` — paciente, evaluación vigente,
  consentimiento vigente, históricos completos (FR-202/CA-204) y contactos elegibles como representante
  (FR-210); `CumpleParaActivo`.
- `ResultadoEvaluacion RegistrarEvaluacion(int pacienteId, DatosEvaluacionIdoneidad datos, int? usuarioId)` —
  FR-200..204; lanza si NO_APTO (o resultado ≠ propuesta) sin observaciones (CA-205); tras guardar
  aplica FR-213 (`PacienteActivado`) y devuelve `SugerirSuspension` si NO_APTO sobre un ACTIVO.
- `Consentimiento CrearConsentimiento(int pacienteId, TipoConsentimiento tipo, int? contactoId, int? usuarioId)` —
  FR-210; si REPRESENTANTE valida contacto del paciente, activo, de tipo elegible y con DNI (CA-202).
- `Contacto CrearRepresentante(int pacienteId, DatosContactoRepresentante datos, int? usuarioId)` — FR-211;
  solo tipos REPRESENTANTE_LEGAL / PERSONA_AUTORIZADA, DNI obligatorio.
- `ResultadoConsentimiento RegistrarFirma(int consentimientoId, DateOnly fechaFirma, int? usuarioId)` —
  FR-212; no admite fecha futura ni firmar dos veces; aplica FR-213.
- `ResultadoConsentimiento Revocar(int consentimientoId, DateOnly fecha, string motivo, int? usuarioId)` —
  FR-214/CA-203; `SugerirSuspension` si no queda otro vigente y el paciente está ACTIVO.
- `void Suspender(int pacienteId, int? usuarioId)` — delega en `IServicioPacientes.CambiarEstado(SUSPENDIDO)`;
  solo lo invoca la pantalla tras la sugerencia.

## IServicioGeneracionDocumentos (Spec 007, ampliación)

- `ResultadoGeneracionDocumento GenerarConsentimiento(int consentimientoId, int? usuarioId)` — `CONSENT`,
  Anexo I.B; marca `impreso_en`. `GenerarFichaPaciente` imprime ahora la evaluación vigente en el bloque de
  idoneidad del Anexo I.E.

## IComprobadorIdoneidadYConsentimiento (Spec 006, implementación real)

- `ComprobadorIdoneidadYConsentimientoReal.Aprobado(pacienteId)` = evaluación vigente APTO ∧ consentimiento vigente.
