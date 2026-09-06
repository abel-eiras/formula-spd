# Tasks: Import/export con programas de gestión

**Input**: Design documents from `specs/011-import-export/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos, con auditoría (Art. VII.6) como test explícito desde el diseño.

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 (MVP) | 4.1 Perfiles genéricos | CA-1100, 1102 (adaptado) |
| US2 | P2 | 4.2 Extracción de unidades | CA-1101 |
| US3 | P2 | 4.4 Exportación | CA-1103 |

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Foundational (Blocking Prerequisites)

- [X] T001 Crear script `src/Spd.Infraestructura/Migraciones/0008_perfiles_importacion.sql` con tabla `PerfilImportacion` (data-model.md)
- [X] T002 [P] Crear enum `TipoPerfilImportacion` en `src/Spd.Dominio/TipoPerfilImportacion.cs`
- [X] T003 [P] Crear entidad `PerfilImportacion` en `src/Spd.Dominio/PerfilImportacion.cs`
- [X] T004 [P] Definir `IRepositorioPerfilesImportacion` en `src/Spd.Dominio/IRepositorioPerfilesImportacion.cs`
- [X] T005 Implementar `RepositorioPerfilesImportacion` en `src/Spd.Infraestructura/RepositorioPerfilesImportacion.cs` (depende de T001, T003, T004)
- [X] T006 [P] Test de humo: migración 0008 idempotente, en `tests/Spd.Aplicacion.Tests/InfraestructuraPerfilesImportacionFundamentosTests.cs`

**Checkpoint**: esquema y entidad listos.

---

## Phase 2: User Story 1 — Perfil genérico reutilizable (Priority: P1) 🎯 MVP

- [X] T007 [P] [US1] Test: `Crear` + `ObtenerPorNombre` devuelve el mismo mapeo (CA-1100), en `tests/Spd.Aplicacion.Tests/ServicioPerfilesImportacionTests.cs`
- [X] T008 [P] [US1] Test: `Actualizar` conserva el id y el nombre tras cambiar el mapeo (CA-1102 adaptado)
- [X] T009 [P] [US1] Test: `ListarPorTipo` filtra por tipo de perfil
- [X] T010 [P] [US1] Test: `Crear` registra en auditoría (Art. VII.6)
- [X] T011 [US1] Crear `DatosAltaPerfilImportacion` en `src/Spd.Aplicacion/DatosAltaPerfilImportacion.cs`
- [X] T012 [US1] Implementar `IServicioPerfilesImportacion`/`ServicioPerfilesImportacion` en `src/Spd.Aplicacion/` (depende de T005, T011)
- [X] T013 [US1] Implementar `PerfilesImportacionViewModel`/vista (listar, crear, editar) en `src/Spd.Presentacion/Views/Configuracion/`
- [X] T014 [P] [US1] Test Avalonia.Headless: la vista de perfiles construye y muestra sin lanzar

**Checkpoint**: US1 funcional de forma independiente.

---

## Phase 3: User Story 2 — Extracción de unidades probada (Priority: P2)

- [X] T015 [P] [US2] Test: `Probar` cuenta aciertos y fallos sobre una muestra sin guardar nada (CA-1101), en `tests/Spd.Dominio.Tests/ExtractorUnidadesEnvaseTests.cs`
- [X] T016 [P] [US2] Test: `Extraer` devuelve el grupo de captura como entero, o null si no hay coincidencia
- [X] T017 [US2] Implementar `ExtractorUnidadesEnvase` en `src/Spd.Dominio/ExtractorUnidadesEnvase.cs`

**Checkpoint**: extractor probado y listo para conectarse cuando exista una muestra real de la columna del nomenclátor.

---

## Phase 4: User Story 3 — Exportación de pacientes (Priority: P2)

- [X] T018 [P] [US3] Test: `ExportarACsv` incluye solo los campos mapeados explícitamente (CA-1103), en `tests/Spd.Aplicacion.Tests/ServicioExportacionPacientesTests.cs`
- [X] T019 [P] [US3] Test: `ExportarACsv` con varios pacientes genera una fila por paciente en el mismo orden
- [X] T020 [US3] Implementar `IServicioExportacionPacientes`/`ServicioExportacionPacientes` en `src/Spd.Aplicacion/` (depende de T012)
- [X] T021 [US3] Implementar pantalla de exportación de pacientes en `src/Spd.Presentacion/Views/Pacientes/`
- [X] T022 [P] [US3] Test Avalonia.Headless: la vista de exportación construye y muestra sin lanzar

**Checkpoint**: todas las user stories completas.

---

## Phase 5: Polish

- [X] T023 Ejecutar `dotnet build` y `dotnet test` completos sobre la rama; corregir regresiones en `MainWindow`/tests de otras specs si la nueva wiring los toca
