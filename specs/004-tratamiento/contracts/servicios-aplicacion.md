# Contratos — servicios de Aplicación de la Spec 004

## IServicioTratamientos

- `Tratamiento Crear(DatosAltaTratamiento datos, int? usuarioQueEjecutaId)` — FR-400; alta nueva
  con `estado = Activo`, `fecha_inicio` = hoy si no se indica otra.
- `Tratamiento CambiarPauta(int tratamientoId, DatosAltaTratamiento datosNuevos, int? usuarioQueEjecutaId)` —
  FR-410; cierra la fila `tratamientoId` (`Finalizado`, `fecha_fin` = hoy) y crea una fila nueva con
  `fecha_prescripcion_inicial` heredada de la original.
- `void ActualizarCamposNoClinicos(int tratamientoId, DatosNoClinicos datos, int? usuarioQueEjecutaId)` —
  FR-411; edita en el sitio observaciones/incidencias/conocimiento del cumplimiento/ajuste manual,
  sin cerrar la fila.
- `void CambiarEstado(int tratamientoId, EstadoTratamiento nuevoEstado, int? usuarioQueEjecutaId)` —
  FR-420; transición administrativa (p. ej. Suspendido↔Activo) sin cerrar/abrir fila (caso límite:
  reactivar sin cambios clínicos).
- `IReadOnlyList<Tratamiento> ListarVigentesDePaciente(int pacienteId)` — tratamientos sin
  `fecha_fin` y `estado` distinto de `Finalizado`.
- `IReadOnlyList<Tratamiento> ListarHistorialDeMedicamento(int pacienteId, int medicamentoId)` —
  FR-412; todas las versiones de un medicamento para un paciente, ordenadas por `fecha_inicio`.

Usado por: la ficha de tratamiento del paciente (nueva pantalla) y, más adelante, Spec 005/006/007
sin necesitar cambios en este contrato (research.md Decisión 3).
