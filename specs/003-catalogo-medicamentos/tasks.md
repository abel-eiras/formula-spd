# Tasks: Catálogo de medicamentos

**Input**: Design documents from `specs/003-catalogo-medicamentos/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos, con auditoría (Art. VII.6) como test explícito desde el diseño, igual que en
Spec 001. Sin fase de Setup: no se crea ningún proyecto nuevo, se reutilizan los 6 de Spec 000.

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 (MVP) | 4.1 Alta y edición + 4.2 Unidades por envase | CA-300, 301, 302, 303, 305 |
| US2 | P2 | 4.3 Importación del nomenclátor | CA-304 |

## Format: `[ID] [P?] [Story] Description`

- **[P]**: puede ejecutarse en paralelo (ficheros distintos, sin dependencias pendientes)
- **[Story]**: US1/US2 según la tabla anterior

---

## Phase 1: Foundational (Blocking Prerequisites)

**Purpose**: esquema de BD, entidad y repositorio que usan las 2 user stories

**⚠️ CRITICAL**: ninguna user story empieza antes de completar esta fase

- [X] T001 Crear script `src/Spd.Infraestructura/Migraciones/0002_catalogo_medicamentos.sql` con tablas `Medicamento` y `Medicamento_Hist` (columnas de [data-model.md](./data-model.md), incluida `desc_vigente_desde` — research.md Decisión 4). **Nota**: se numera `0002` en esta rama porque parte de `main` (solo Spec 000); ver plan.md §Project Structure sobre la coordinación pendiente con la rama 001 al mergear.
- [X] T002 [P] Crear enum `FormaFarmaceutica` en `src/Spd.Dominio/FormaFarmaceutica.cs` (FR-300, research.md Decisión 1)
- [X] T003 [P] Crear `ReglaAptitudSpd` (regla de derivación pura) en `src/Spd.Dominio/ReglaAptitudSpd.cs` (FR-301, research.md Decisión 2)
- [X] T004 [P] Crear entidad `Medicamento` en `src/Spd.Dominio/Medicamento.cs`
- [X] T005 [P] Crear `VersionDescripcionFisica` (record de solo lectura para el historial) en `src/Spd.Dominio/VersionDescripcionFisica.cs` (FR-304)
- [X] T006 [P] Definir `IRepositorioMedicamentos` en `src/Spd.Dominio/IRepositorioMedicamentos.cs`
- [X] T007 Implementar `RepositorioMedicamentos` en `src/Spd.Infraestructura/RepositorioMedicamentos.cs` (depende de T001, T004, T006)
- [X] T008 [P] Test de humo: la migración 0002 (de esta rama) se aplica de forma idempotente sobre el esquema de Spec 000, `schema_version` pasa a 2, en `tests/Spd.Aplicacion.Tests/InfraestructuraMedicamentosFundamentosTests.cs`
- [X] T009 [P] Test de humo: alta y lectura básica de Medicamento vía repositorio, en el mismo fichero

**Checkpoint**: esquema y entidad listos — las user stories pueden empezar.

---

## Phase 2: User Story 1 — Alta, edición y búsqueda del catálogo (Priority: P1) 🎯 MVP

**Goal**: dar de alta un medicamento con lo mínimo, completar su descripción física y aptitud SPD
más tarde, versionando cada cambio sin afectar a nada fuera del catálogo (E1, E2, E3).

**Independent Test**: crear un medicamento con solo CN+nombre (CA-300), completar su descripción
física y comprobar que el cambio queda versionado (CA-301), marcarlo no apto sin motivo y
comprobar que no guarda (CA-302), intentar duplicar su CN (CA-303), reactivar uno de baja (CA-305).

### Tests for User Story 1

- [X] T010 [P] [US1] Test: `ReglaAptitudSpd.PorDefecto` devuelve apto para las 7 formas del enum salvo `OtraNoApta`, en `tests/Spd.Dominio.Tests/ReglaAptitudSpdTests.cs`
- [X] T011 [P] [US1] Test: `ServicioMedicamentos.Crear` guarda con solo CN+nombre (CA-300), en `tests/Spd.Aplicacion.Tests/ServicioMedicamentosTests.cs`
- [X] T012 [P] [US1] Test: `ServicioMedicamentos.Crear` bloquea (lanza, no avisa) un CN ya activo (CA-303)
- [X] T013 [P] [US1] Test: `ServicioMedicamentos.Crear` reactiva un CN existente dado de baja en vez de duplicar (CA-305)
- [X] T014 [P] [US1] Test: `ServicioMedicamentos.ActualizarDatos` exige `motivo_no_apto` solo cuando `apto_spd` se fija distinto del derivado por `ReglaAptitudSpd` (CA-302)
- [X] T015 [P] [US1] Test: `ServicioMedicamentos.ActualizarDescripcionFisica` versiona la descripción anterior en `Medicamento_Hist` con su periodo de vigencia, y dos cambios sucesivos generan dos filas de histórico distintas (CA-301, Art. IV.3)
- [X] T016 [P] [US1] Test: `ServicioMedicamentos.ProponerDescripcionTexto` construye el texto a partir de los campos `desc_*` sin persistir nada (FR-303, research.md Decisión 3)
- [X] T017 [P] [US1] Test: `Buscar` encuentra por CN exacto y por fragmento de nombre sin tildes (FR-305)
- [X] T018 [P] [US1] Test: `ActualizarUnidadesEnvase` fija `unidades_envase_origen = MANUAL` (FR-310/FR-311)
- [X] T019 [P] [US1] Test: `Crear`, `ActualizarDatos`, `ActualizarDescripcionFisica`, `ActualizarUnidadesEnvase` y `DarDeBaja` registran en auditoría (Art. VII.6 — remediación F2: `ActualizarUnidadesEnvase` faltaba en la lista pese a ser una escritura)
- [X] T020 [P] [US1] Test: `DarDeBaja` no elimina la fila, solo marca `activo = 0` (Art. III.1 — remediación F1: invariante crítico sin test dedicado, a diferencia de Spec 001)

### Implementation for User Story 1

- [X] T021 [US1] Implementar `IServicioMedicamentos` y `ServicioMedicamentos` en `src/Spd.Aplicacion/ServicioMedicamentos.cs` (depende de T004, T003, T006, T007); cada escritura registra en auditoría
- [X] T022 [US1] Implementar `CatalogoMedicamentosViewModel` en `src/Spd.Presentacion/ViewModels/CatalogoMedicamentosViewModel.cs` (listado con búsqueda a la izquierda, formulario de alta/edición a la derecha — mismo patrón que `UsuariosViewModel` de Spec 000)
- [X] T023 [US1] Implementar `CatalogoMedicamentosView`/`Window` en `src/Spd.Presentacion/Views/Medicamentos/`
- [X] T024 [US1] Añadir botón "Catálogo de medicamentos" a `MainWindow`/`MainViewModel` (visible para Elaborador y Administrador, mismo criterio que "Pacientes" en Spec 001)
- [X] T025 [P] [US1] Test Avalonia.Headless: `CatalogoMedicamentosView` construye la ventana, renderiza el listado con su `ItemTemplate` y ejecuta `.Show()` sin lanzar excepción, en `tests/Spd.Presentacion.Tests/CatalogoMedicamentosViewTests.cs` (mismo patrón de regresión que `BuscadorPacientesViewTests` de Spec 001, por el incidente de `UsuariosWindow` en Spec 000)

**Checkpoint**: US1 funcional de forma independiente — MVP entregable.

---

## Phase 3: User Story 2 — Importación del nomenclátor (Priority: P2)

**Goal**: comparar el nomenclátor ya descargado (Spec 000) con el catálogo y aplicar altas o
cambios de nombre solo con confirmación explícita, sin tocar nunca descripción física ni aptitud
SPD (E4).

**Independent Test**: descargar (o simular) un CSV de nomenclátor con un CN nuevo y otro con
nombre distinto al del catálogo; comprobar que la pantalla de revisión los distingue y que
confirmar no cambia la descripción física de ningún medicamento existente (CA-304).

### Tests for User Story 2

- [ ] T026 [P] [US2] Test: `LectorNomenclatorCsv` extrae filas `(Cn, Nombre)` de un CSV con cabecera `CN,Nombre` y devuelve error explícito si faltan esas columnas (research.md Decisión 5), en `tests/Spd.Aplicacion.Tests/LectorNomenclatorCsvTests.cs`
- [ ] T027 [P] [US2] Test: `CompararConNomenclator` distingue medicamentos nuevos, con nombre distinto y sin cambios, en `tests/Spd.Aplicacion.Tests/ServicioImportacionNomenclatorTests.cs`
- [ ] T028 [P] [US2] Test: `AplicarNombreDesdeNomenclator` cambia solo el nombre y nunca toca `desc_*` ni `apto_spd` (CA-304)
- [ ] T029 [P] [US2] Test: `AplicarAltaDesdeNomenclator` y `AplicarNombreDesdeNomenclator` registran en auditoría (Art. VII.6)

### Implementation for User Story 2

- [ ] T030 [US2] Implementar `LectorNomenclatorCsv` en `src/Spd.Infraestructura/LectorNomenclatorCsv.cs` (research.md Decisión 5, simplificación deliberada a sustituir por Spec 011)
- [ ] T031 [US2] Implementar `IServicioImportacionNomenclator` y `ServicioImportacionNomenclator` en `src/Spd.Aplicacion/ServicioImportacionNomenclator.cs` (depende de T021, T030)
- [ ] T032 [US2] Implementar `RevisionNomenclatorViewModel` + vista (tabla de comparación con acción "Aplicar" fila a fila, FR-322) en `src/Spd.Presentacion/ViewModels/RevisionNomenclatorViewModel.cs` + `src/Spd.Presentacion/Views/Medicamentos/RevisionNomenclatorView.axaml`
- [ ] T033 [US2] Añadir botón "Revisar nomenclátor" a `CatalogoMedicamentosView` (depende de T023, T032)
- [ ] T034 [P] [US2] Test Avalonia.Headless: `RevisionNomenclatorView` renderiza su listado de comparación con una fila real sin lanzar excepción, en `tests/Spd.Presentacion.Tests/RevisionNomenclatorViewTests.cs`

**Checkpoint**: US1 + US2 funcionales de forma independiente.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T035 [P] Ejecutar íntegramente [quickstart.md](./quickstart.md) y registrar el resultado en `PROGRESO.md`
- [ ] T036 Revisar que ningún método de `Spd.Dominio`/`Spd.Aplicacion` supere ~40 líneas ni ninguna clase ~300 (Art. XI.6)
- [ ] T037 [P] Actualizar `PROGRESO.md` marcando cada CA-300..CA-305 como validado, con el test que lo confirma

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Fase 1)**: sin dependencias — bloquea las 2 user stories
- **US1 (Fase 2)**: depende de Foundational; sin dependencias de otra user story
- **US2 (Fase 3)**: depende de Foundational y de T021 (US1) para reutilizar `ServicioMedicamentos`; T033 depende también de T023 (US1)
- **Polish (Fase 4)**: depende de las user stories que se quieran dar por completas

### Parallel Opportunities

- T002-T006 (Foundational) en paralelo entre sí
- Todos los tests `[P]` de una misma user story, en paralelo
- Los tests Avalonia.Headless (T025, T034) dependen de que su vista ya esté implementada (tests de
  regresión, no TDD-first, igual que en Spec 000/001)

## Implementation Strategy

### MVP primero

1. Fase 1 (Foundational)
2. Fase 2 (US1) — **parar y validar** con CA-300/301/302/303/305 antes de seguir
3. Fase 3 (US2), validando CA-304 en el checkpoint
4. Fase 4 (Polish) al final

## Notes

- `[P]` = ficheros distintos, sin dependencias pendientes entre sí
- Commit atómico por fase o por grupo lógico pequeño, como en Spec 000/001
- Parar en cada checkpoint y validar la user story de forma independiente antes de continuar
