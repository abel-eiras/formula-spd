# Quickstart — validación de Spec 008

## Escenario 1 — Presentación prerrellenada (US1, CA-800)

1. Crear paciente con médico de cabecera asignado.
2. `ServicioComunicacionesMedico.Crear(new DatosAltaComunicacionMedico(pacienteId, medicoId: null,
   TipoComunicacionMedico.Presentacion, null, null), usuarioId)`.
3. Verificar que la comunicación creada tiene `MedicoId` = médico de cabecera del paciente.

## Escenario 2 — Incidencia y respuesta (US2, CA-801/802)

1. Intentar `Crear` con `Tipo = Incidencia` y `Propuesta = null` → `ErrorValidacionException`.
2. Crear una incidencia completa (incidencias + propuesta).
3. Días después, `RegistrarRespuesta(id, "...", fecha, usuarioId)`.
4. Verificar que `Fecha` (creación) no cambió y `Respuesta`/`FechaRespuesta` sí se rellenaron.

## Escenario 3 — Desde tratamiento y desde aviso diferido (US3, CA-803)

1. `CrearDesdeTratamiento(tratamientoId, usuarioId)` → paciente y médico prescriptor fijados.
2. `CrearDesdeAvisoCambioReferido(pacienteId, medicoCabeceraId, usuarioId)` → paciente y médico de
   cabecera fijados, incidencias vacío.

## Validación de UI (manual, para el informe de mañana)

- Botón "Comunicar incidencia" en la ficha de tratamiento (Spec 004).
- Pantalla de comunicaciones del paciente: alta, registro de respuesta, listado.
