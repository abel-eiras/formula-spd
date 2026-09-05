# Quickstart de validación — Spec 000

Guía para comprobar, una vez implementada la feature, que cada criterio de aceptación de
[spec.md](./spec.md) §6 se cumple. No sustituye a los tests automatizados de `tasks.md`; es la
guía de validación manual/end-to-end.

## Prerrequisitos

```bash
dotnet build
dotnet test
```

Todos los tests de `tests/Spd.Dominio.Tests` y `tests/Spd.Aplicacion.Tests` deben pasar antes de
validar manualmente.

## Escenarios

### CA-000 — Asistente obligatorio en primer arranque

1. Borrar/renombrar la base de datos si existe.
2. Arrancar la aplicación.
3. **Esperado**: se muestra el asistente; no hay forma de llegar a ninguna otra pantalla sin
   completarlo.

### CA-001 — Prefijo no afecta a numeración pasada

Requiere Spec 001 (pacientes) implementada para verificarse end-to-end; mientras tanto, se valida
a nivel de `IServicioConfiguracionFarmacia.ActualizarPrefijos` con un test de integración que
simula un `num_ficha` ya asignado.

### CA-002 — Único administrador protegido

1. Con un solo usuario Administrador activo, iniciar sesión como ese usuario.
2. Intentar autodesactivarse desde Configuración/Usuarios.
3. **Esperado**: la acción se rechaza con un mensaje explicando el motivo (FR-042).

### CA-003 — Baja de usuario conserva histórico

Requiere Spec 006 (preparación) para el caso completo con SPD reales; a nivel de esta spec se
valida que `DarDeBaja` no borra la fila `Usuario`, solo cambia `activo`/`fecha_baja`.

### CA-004 — Bloqueo por intentos fallidos

1. Intentar iniciar sesión con contraseña incorrecta 5 veces consecutivas.
2. Intentar una sexta vez.
3. **Esperado**: el usuario queda bloqueado (`bloqueado=1`); un sexto intento con la contraseña
   correcta también es rechazado hasta que un Administrador lo desbloquee.

### CA-005 — Descarga de nomenclátor no bloquea la app

1. Configurar `url_nomenclator` a una URL que no responde (p. ej. `http://localhost:1/nada`).
2. Pulsar "Descargar ahora".
3. **Esperado**: se muestra el motivo del fallo; el resto de la aplicación sigue usable.

### CA-006 — Cambio de valores por defecto no reescribe pacientes existentes

Requiere Spec 001; a nivel de esta spec se valida que `ActualizarValoresDefecto` solo escribe en
`Farmacia`, nunca en `Paciente`.

## Notas

- Los escenarios que dependen de otra spec (CA-001, CA-003, CA-006) se marcan como validados
  "a nivel de esta spec" hasta que la spec dependiente exista; `PROGRESO.md` registra ese matiz por
  criterio.
