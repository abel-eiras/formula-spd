# Contratos — servicios de Aplicación de la Spec 008

## IServicioComunicacionesMedico

- `ComunicacionMedico Crear(DatosAltaComunicacionMedico datos, int? usuarioQueEjecutaId)` —
  FR-800/801/802; exige `IncidenciasDetectadas` y `Propuesta` cuando `Tipo = Incidencia`
  (CA-801, lanza `ErrorValidacionException` si faltan).
- `DatosAltaComunicacionMedico PrepararDesdeTratamiento(int tratamientoId)` — FR-804; no guarda
  nada (FR-807: no hay borradores), solo resuelve paciente y médico prescriptor del tratamiento
  indicado para prerrellenar el formulario; el alta real se hace después con `Crear`.
- `DatosAltaComunicacionMedico PrepararDesdeAvisoCambioReferido(int pacienteId, int medicoId)` —
  FR-805 (research.md Decisión 2); mismo contrato que `PrepararDesdeTratamiento`, punto de
  extensión hacia Spec 006, invocable ya sin que exista el aviso real.
- `void RegistrarRespuesta(int comunicacionId, string respuesta, DateOnly fechaRespuesta, int? usuarioQueEjecutaId)` —
  FR-803/807; único UPDATE permitido, no toca el resto de campos (CA-802).
- `IReadOnlyList<ComunicacionMedico> ListarDePaciente(int pacienteId)`.

Usado por: la ficha de tratamiento (Spec 004, botón "Comunicar incidencia") y, más adelante,
Spec 006 (aviso de entrega) y Spec 007 (impresión de `CARTA-PRES`/`CARTA-INC`) sin necesitar
cambios en este contrato (research.md Decisiones 2 y 3).
