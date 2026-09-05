# Quickstart de validación — Spec 004

## Prerrequisitos

```bash
dotnet build
dotnet test
```

## Escenarios

### CA-400 — Prerrelleno de prescriptor
Crear un tratamiento para un paciente con médico de cabecera asignado → el campo prescriptor
aparece prerrellenado con ese médico, editable antes de guardar.

### CA-401 — Cambio de pauta cierra y abre
Tratamiento activo 1-0-0-0 → `CambiarPauta` a 1-0-1-0 → la fila original queda `Finalizado` con
`fecha_fin` = hoy; existe una fila nueva `Activo` con `fecha_inicio` = hoy y la pauta nueva.

### CA-402 — Historial consultable
Tres versiones de pauta del mismo medicamento para el mismo paciente → `ListarHistorialDeMedicamento`
devuelve las tres, ninguna oculta.

### CA-403 — Selector de fracciones, no decimal libre
El campo de dosis expone únicamente los valores de `FraccionDosis`; no hay forma de introducir un
decimal arbitrario.

### CA-405 (parcial) — Ajuste manual
`ajuste_unidades_manual` se puede fijar y volver a `null` con "usar cálculo automático"; el propio
valor calculado de referencia queda pendiente de Spec 005 (documentado en spec.md).

## Notas

- CA-404 (bloqueo automático desde Spec 006) y CA-406 (propuesta SIGRE desde Spec 005) quedan
  diferidos — ver spec.md, "Fuera de alcance".
