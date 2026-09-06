# Quickstart — validación de Spec 006

## Escenario 1 — Sesión de dos blísteres (US1, CA-600, CA-602)

1. Paciente `n_blisteres=2`, tratamiento activo con envases A (3 uds) y B (28 uds) para una línea
   de 7 uds/semana.
2. `ServicioPreparacion.CrearSesion(pacienteId, elaboradorId)` → 2 SPD en `Borrador`, validez
   consecutiva de 7 días.
3. `PasarAPreparado` en el primero → la línea queda con dos `SPD_Linea_Envase` (3 de A, 4 de B).

## Escenario 2 — Sin saldo, alta desde la preparación (US2, CA-603/604)

1. Línea sin envases suficientes → `PasarAPreparado` lanza `ErrorValidacionException` indicando
   medicamento y unidades que faltan.
2. `RegistrarEnvaseDesdeLinea` con los datos del envase nuevo.
3. `PasarAPreparado` ahora tiene éxito.

## Escenario 3 — Verificación independiente y entrega conjunta (US3/US4, CA-605/606/607)

1. Verificar el blíster 1 → pasa a `Verificado`; el blíster 2 sigue en `Preparado`.
2. `RegistrarEntrega` con ambos `spdIds` (tras verificar los dos) → ambos `Entregado`.
3. Repetir entregando solo uno → el otro permanece `Verificado`.

## Escenario 4 — Continuidad (US5, CA-608/609/610)

1. `PrepararSiguiente(pacienteId, elaboradorId, out resultado)` sobre un paciente sin cambios de
   tratamiento y con saldo suficiente → nuevo SPD listo salvo lectura ambiental.
2. Con una línea de posología modificada → `resultado.Modificadas` contiene solo esa línea.
3. Con un envase agotado sin sustituto → la línea nueva queda `EstadoLinea.EnvasePendiente`.

## Escenario 5 — Reelaboración (US6, CA-6120/6121/6123)

1. SPD `Verificado`, num_registro `F-000041`, version 1.
2. `Reelaborar` añadiendo una línea nueva → sigue `F-000041`, version 2, estado `Preparado`.
3. Verificar `SPD_Modificacion` contiene la copia íntegra de las líneas anteriores.

## Validación de UI (manual, para el informe de mañana)

- Pantalla de sesión de preparación con pestañas por blíster.
- Verificación con checklist de 5 ítems y motivo si verificador = elaborador.
- Entrega conjunta/parcial.
- "Preparar siguiente" y "Reelaborar" desde el listado de Preparaciones.
