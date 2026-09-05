# Quickstart — validación de Spec 005

## Prerrequisitos

- `dotnet build` sobre la rama `005-deposito-y-retirada`.
- Base de datos de pruebas: SQLite en memoria con `AplicadorMigraciones` aplicado (incluye
  `0006_envase.sql`).

## Escenario 1 — Alta de envase y listado (US1/US2, CA-500, CA-503)

1. Crear paciente `n_blisteres=2`, `dia_retirada` = pasado mañana.
2. Crear tratamiento `en_spd=1`, 1 cápsula/día, `unidades_envase=28` en el medicamento.
3. `ServicioListadoRetirada.ObtenerListado(hoy, filtros por defecto)` → fila con
   `necesarias=14, disponibles=0, faltan=14, envases_a_retirar=1`.
4. `ServicioEnvases.RegistrarEnvase(...)` con 28 unidades.
5. Repetir el listado → el paciente ya no aparece (`faltan=0`).

## Escenario 2 — Sobrante se usa primero (US3, CA-505/506)

1. Dos envases del mismo medicamento/paciente: E1 (3 restantes), E2 (28 restantes).
2. `ServicioAsignacionEnvases.Descontar(tratamientoId, unidadesOverride: 7, caducidadMinima, null)`.
3. Verificar en BD: E1 → `Agotado`, 0 restantes; E2 → `EnCustodia`, 24 restantes.

## Escenario 3 — Cese de tratamiento propone SIGRE (US4, CA-508)

1. Tratamiento con un envase de 12 restantes.
2. `ServicioTratamientos.CambiarEstado(tratamientoId, Finalizado, ...)` (o `CambiarPauta`, que
   también cierra la fila).
3. `ServicioEnvases.ProponerSalidaSigrePorFinDeTratamiento(pacienteId, medicamentoId)` → devuelve
   el envase de 12 restantes.
4. `ServicioEnvases.DarSalidaSigre(envaseId, CeseTratamiento, null, ...)` → estado
   `ResiduoSigre`, 12 unidades desechadas.

## Escenario 4 — Importar por pegado (US6, CA-516/517)

1. Perfil con mapeo `{cn: "CN", num_serie: "Serie", lote: "Lote", caducidad: "Caducidad"}`.
2. Texto tabulado de una línea con un CN sin tratamiento previo.
3. `ServicioImportacionTratamientoEnvase.ImportarDesdePegado(...)` → tratamiento nuevo pendiente
   de posología + envase creado; verificar en `ResultadoImportacion.TratamientosPendientes`.

## Validación de UI (manual, para el informe de mañana)

- Pestaña "Depósito" en la ficha de paciente: alta de envase, ver custodia, ver histórico.
- Botón "Retirada de envases" en `MainWindow`: listado, filtros, acción "Registrar envase" desde
  la fila.
- Diálogo de salida a SIGRE desde la pestaña Depósito.
- Pantalla "Importar tratamiento" con pegado y con fichero CSV.
