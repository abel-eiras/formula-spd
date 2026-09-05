# Quickstart de validación — Spec 009

## Prerrequisitos

```bash
dotnet build
dotnet test
```

## Escenarios

### CA-900 — Ambiental fuera de rango se marca al registrar
Con rango configurado 15–25 °C, registrar 27 °C → la entrada queda `fuera_rango = true`. Cambiar
el rango a 15–30 °C y volver a consultar esa misma entrada → sigue marcada fuera de rango.

### CA-901 — Limpieza de un clic
Pulsar "Registrar limpieza pre-preparación" → se crea el registro con fecha/hora actual y usuario
actual, sin ningún formulario adicional.

### CA-902 — Formación acumulativa
Registrar dos formaciones para un usuario, añadir una tercera → las tres siguen siendo
consultables en el listado de ese usuario.

### CA-903 — Control de cambios solo Administrador
Con un usuario Elaborador, intentar listar o registrar en Control de cambios del PNT → el sistema
lo impide. Con un Administrador, funciona con normalidad.

### CA-904 — Aviso de registro atrasado
Sin ningún registro ambiental rutinario en los últimos 8 días y umbral en 7 → el panel de inicio
muestra el aviso correspondiente. Con un registro de hoy, el aviso desaparece.

## Notas

Todos los escenarios son verificables end-to-end dentro de esta spec; no dependen de Specs 001/003
(sin mergear) ni de Spec 006 (todavía no existe) — `RegistroAmbiental.spd_id` se deja siempre nulo.
