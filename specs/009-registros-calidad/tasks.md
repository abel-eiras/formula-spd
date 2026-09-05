# Tasks: Registros de calidad

**Input**: Design documents from `specs/009-registros-calidad/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos, con auditoría (Art. VII.6) como test explícito desde el diseño y test
Avalonia.Headless de regresión para cada listado nuevo, igual que en Specs 000/001/003. Sin fase
de Setup: no se crea ningún proyecto nuevo, se reutilizan los 6 de Spec 000.

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 (MVP) | 4.1 Ambiental + 4.2 Limpieza | CA-900, 901 |
| US2 | P2 | 4.3 Formación + 4.4 Residuos | CA-902 |
| US3 | P2 | 4.5 Control documental | CA-903 |
| US4 | P3 | 4.6 Avisos (depende de US1) | CA-904 |

## Format: `[ID] [P?] [Story] Description`

- **[P]**: puede ejecutarse en paralelo (ficheros distintos, sin dependencias pendientes)
- **[Story]**: US1..US4 según la tabla anterior

---

## Phase 1: Foundational (Blocking Prerequisites)

**Purpose**: esquema de BD, entidades y repositorios que usan las 4 user stories

**⚠️ CRITICAL**: ninguna user story empieza antes de completar esta fase

- [X] T001 Crear script `src/Spd.Infraestructura/Migraciones/0002_registros_calidad.sql` con tablas `RegistroAmbiental`, `RegistroLimpieza`, `FormacionPersonal`, `RecogidaResiduos`, `ControlCambiosPNT`, `ControlCopias` (columnas de [data-model.md](./data-model.md)) y un `ALTER TABLE Farmacia ADD COLUMN umbral_dias_aviso_calidad INTEGER NOT NULL DEFAULT 7` (research.md Decisión 3). **Nota**: se numera `0002` en esta rama, igual que Specs 001/003; ver plan.md sobre la coordinación pendiente al mergear.
- [X] T002 [P] Crear enum `TipoLimpieza` en `src/Spd.Dominio/TipoLimpieza.cs` (FR-910)
- [X] T003 [P] Crear entidad `RegistroAmbiental` en `src/Spd.Dominio/RegistroAmbiental.cs` (FR-900/FR-901)
- [X] T004 [P] Crear entidad `RegistroLimpieza` en `src/Spd.Dominio/RegistroLimpieza.cs` (FR-910)
- [X] T005 [P] Crear entidad `FormacionPersonal` en `src/Spd.Dominio/FormacionPersonal.cs` (FR-920, research.md Decisión 1: sin `tipo`/`formador_id`)
- [X] T006 [P] Crear entidad `RecogidaResiduos` en `src/Spd.Dominio/RecogidaResiduos.cs` (FR-930)
- [X] T007 [P] Crear entidad `ControlCambiosPNT` en `src/Spd.Dominio/ControlCambiosPNT.cs` (FR-940)
- [X] T008 [P] Crear entidad `ControlCopias` en `src/Spd.Dominio/ControlCopias.cs` (FR-941)
- [X] T009 [P] Definir `IRepositorioRegistrosCalidad` (Ambiental/Limpieza/Formación/Residuos) en `src/Spd.Dominio/IRepositorioRegistrosCalidad.cs`
- [X] T010 [P] Definir `IRepositorioControlDocumental` (CambiosPNT/Copias) en `src/Spd.Dominio/IRepositorioControlDocumental.cs`
- [X] T011 Implementar `RepositorioRegistrosCalidad` en `src/Spd.Infraestructura/RepositorioRegistrosCalidad.cs` (depende de T001, T003-T006, T009)
- [X] T012 Implementar `RepositorioControlDocumental` en `src/Spd.Infraestructura/RepositorioControlDocumental.cs` (depende de T001, T007-T008, T010)
- [X] T013 [P] Test de humo: la migración 0002 (de esta rama) se aplica de forma idempotente sobre el esquema de Spec 000, `schema_version` pasa a 2, en `tests/Spd.Aplicacion.Tests/InfraestructuraRegistrosCalidadFundamentosTests.cs`
- [X] T014 [P] Test de humo: alta y lectura básica de las 6 entidades vía repositorios, en el mismo fichero

**Checkpoint**: esquema y entidades listos — las user stories pueden empezar.

---

## Phase 2: User Story 1 — Registro ambiental y limpieza (Priority: P1) 🎯 MVP

**Goal**: registrar temperatura/humedad y limpieza de la zona de preparación sin fricción,
marcando de inmediato si el ambiental está fuera de rango (E1, E2).

**Independent Test**: registrar una lectura ambiental fuera de rango y comprobar que queda
marcada aunque luego cambie la configuración (CA-900); registrar limpieza con un clic y comprobar
fecha/usuario automáticos (CA-901).

### Tests for User Story 1

- [X] T015 [P] [US1] Test: `RegistrarAmbiental` calcula y guarda `fuera_rango = true` para una lectura fuera del rango configurado, en `tests/Spd.Aplicacion.Tests/ServicioRegistrosCalidadTests.cs`
- [X] T016 [P] [US1] Test: una lectura ya registrada conserva su `fuera_rango` aunque el rango de `Farmacia` cambie después (CA-900, Art. IV)
- [X] T017 [P] [US1] Test: `RegistrarLimpieza` guarda con la fecha/hora y el usuario actuales sin pasos adicionales (CA-901)
- [X] T018 [P] [US1] Test: `RegistrarAmbiental` y `RegistrarLimpieza` registran en auditoría (Art. VII.6)

### Implementation for User Story 1

- [X] T019 [US1] Implementar `IServicioRegistrosCalidad` (parte Ambiental/Limpieza) y `ServicioRegistrosCalidad` en `src/Spd.Aplicacion/ServicioRegistrosCalidad.cs` (depende de T003, T004, T009, T011); cada escritura registra en auditoría
- [X] T020 [US1] Implementar `RegistroAmbientalViewModel` + vista (alta rápida + listado) en `src/Spd.Presentacion/ViewModels/RegistroAmbientalViewModel.cs` + `src/Spd.Presentacion/Views/RegistrosCalidad/RegistroAmbientalView.axaml`
- [X] T021 [US1] Implementar `RegistroLimpiezaViewModel` + vista (dos botones de un clic + listado, FR-911) en `src/Spd.Presentacion/ViewModels/RegistroLimpiezaViewModel.cs` + `src/Spd.Presentacion/Views/RegistrosCalidad/RegistroLimpiezaView.axaml`
- [X] T022 [US1] Añadir botón "Registros de calidad" a `MainWindow`/`MainViewModel` que abre una ventana con pestañas Ambiental/Limpieza (visible para Elaborador y Administrador, FR-940 se restringe aparte en US3)
- [X] T023 [P] [US1] Test Avalonia.Headless: la ventana de registros de calidad construye y muestra el listado de ambiental con una lectura real sin lanzar excepción, en `tests/Spd.Presentacion.Tests/RegistrosCalidadViewTests.cs`

**Checkpoint**: US1 funcional de forma independiente — MVP entregable.

---

## Phase 3: User Story 2 — Formación del personal y recogida de residuos (Priority: P2)

**Goal**: registrar formaciones acumulativas por usuario y recogidas de residuos no SIGRE (E3, E4).

**Independent Test**: registrar tres formaciones para un usuario y comprobar que las tres siguen
consultables (CA-902); registrar una recogida de residuos con empresa gestora.

### Tests for User Story 2

- [X] T024 [P] [US2] Test: `RegistrarFormacion` tres veces para el mismo usuario deja las tres consultables, ninguna sustituye a otra (CA-902), en `tests/Spd.Aplicacion.Tests/ServicioRegistrosCalidadTests.cs`
- [X] T025 [P] [US2] Test: `RegistrarRecogidaResiduos` guarda fecha/empresa gestora/usuario
- [X] T026 [P] [US2] Test: `RegistrarFormacion` y `RegistrarRecogidaResiduos` registran en auditoría (Art. VII.6)

### Implementation for User Story 2

- [X] T027 [US2] Ampliar `ServicioRegistrosCalidad` con Formación y Recogida de residuos (depende de T005, T006, T019)
- [X] T028 [US2] Implementar `FormacionPersonalViewModel` + vista en `src/Spd.Presentacion/ViewModels/FormacionPersonalViewModel.cs` + vista, como pestaña de la ventana de T022
- [X] T029 [US2] Implementar `RecogidaResiduosViewModel` + vista en `src/Spd.Presentacion/ViewModels/RecogidaResiduosViewModel.cs` + vista, como pestaña de la ventana de T022
- [X] T030 [P] [US2] Test Avalonia.Headless: la pestaña de formación renderiza su listado con una formación real sin lanzar excepción, en el mismo fichero de T023

**Checkpoint**: US1 + US2 funcionales de forma independiente.

---

## Phase 4: User Story 3 — Control de cambios del PNT y control de copias (Priority: P2)

**Goal**: registro documental exclusivo de Administrador, con comprobación de rol en la capa de
Aplicación tanto para escribir como para consultar (CA-903).

**Independent Test**: con un Elaborador, intentar listar o registrar en Control de cambios → el
sistema lo impide; con un Administrador, funciona con normalidad.

### Tests for User Story 3

- [X] T031 [P] [US3] Test: `RegistrarCambioPnt` lanza `ErrorValidacionException` si el usuario no es Administrador (CA-903), en `tests/Spd.Aplicacion.Tests/ServicioControlDocumentalTests.cs`
- [X] T032 [P] [US3] Test: `ListarCambiosPnt` también lanza para un Elaborador (CA-903 habla de "acceder", no solo de escribir)
- [X] T033 [P] [US3] Test: `RegistrarCopia` y `ListarCopias` tienen la misma restricción
- [X] T034 [P] [US3] Test: `RegistrarCambioPnt` y `RegistrarCopia` registran en auditoría (Art. VII.6)

### Implementation for User Story 3

- [X] T035 [US3] Implementar `IServicioControlDocumental` y `ServicioControlDocumental` en `src/Spd.Aplicacion/ServicioControlDocumental.cs` (depende de T007, T008, T010, T012); comprueba el rol vía `IRepositorioUsuarios` (research.md Decisión 4)
- [X] T036 [US3] Implementar `ControlDocumentalViewModel` + vista en `src/Spd.Presentacion/ViewModels/ControlDocumentalViewModel.cs` + `src/Spd.Presentacion/Views/RegistrosCalidad/ControlDocumentalWindow.axaml`
- [X] T037 [US3] Añadir botón "Control documental" a `MainWindow`/`MainViewModel`, visible solo para Administrador (`PuedeAccederAConfiguracion`, mismo criterio que los botones de Configuración de Spec 000)
- [X] T038 [P] [US3] Test Avalonia.Headless: `ControlDocumentalWindow` construye y muestra su listado con una entrada real sin lanzar excepción, en `tests/Spd.Presentacion.Tests/ControlDocumentalViewTests.cs`

**Checkpoint**: US1 + US2 + US3 funcionales de forma independiente.

---

## Phase 5: User Story 4 — Avisos de registro atrasado (Priority: P3)

**Goal**: avisar en el panel de inicio si faltan registros de ambiental o limpieza rutinaria por
encima del umbral configurado (E5). Depende de US1 (necesita `RegistroAmbiental`/`RegistroLimpieza`).

**Independent Test**: sin registros en los últimos 8 días y umbral en 7 → aparece el aviso; con un
registro de hoy, desaparece (CA-904).

### Tests for User Story 4

- [X] T039 [P] [US4] Test: `ComprobarAvisos` detecta atraso cuando no hay ambiental ni limpieza rutinaria en más días que `Farmacia.umbral_dias_aviso_calidad` (CA-904), en `tests/Spd.Aplicacion.Tests/ServicioRegistrosCalidadTests.cs`
- [X] T040 [P] [US4] Test: `ComprobarAvisos` no avisa si hay un registro dentro del umbral

### Implementation for User Story 4

- [X] T041 [US4] Implementar `ComprobarAvisos` en `ServicioRegistrosCalidad` (depende de T019, T027)
- [X] T042 [US4] Mostrar el aviso en `MainViewModel`/`MainWindow` (panel de inicio, FR-950)

**Checkpoint**: las 4 user stories funcionan de forma independiente.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T043 [P] Ejecutar íntegramente [quickstart.md](./quickstart.md) y registrar el resultado en `PROGRESO.md`
- [X] T044 Revisar que ningún método de `Spd.Dominio`/`Spd.Aplicacion` supere ~40 líneas ni ninguna clase ~300 (Art. XI.6)
- [X] T045 [P] Actualizar `PROGRESO.md` marcando cada CA-900..CA-904 como validado, con el test que lo confirma

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Fase 1)**: sin dependencias — bloquea las 4 user stories
- **US1 (Fase 2)**: depende de Foundational; sin dependencias de otra user story
- **US2 (Fase 3)**: depende de Foundational; T027 amplía la misma clase que T019 (US1), pero sus
  tests y su UI son independientes
- **US3 (Fase 4)**: depende de Foundational; totalmente independiente de US1/US2 (repositorio y
  servicio propios)
- **US4 (Fase 5)**: depende de US1 (T019/T027 ya deben existir para poder consultar registros)
- **Polish (Fase 6)**: depende de las user stories que se quieran dar por completas

### Parallel Opportunities

- T002-T010 (Foundational) en paralelo entre sí
- US1, US2 y US3 pueden avanzar en paralelo entre sí una vez completada Foundational (T027 de US2
  toca el mismo fichero que T019 de US1, así que esas dos tareas concretas no son paralelas entre
  sí, pero sí lo son respecto a US3)
- Todos los tests `[P]` de una misma user story, en paralelo
- Los tests Avalonia.Headless (T023, T030, T038) dependen de que su vista ya esté implementada

## Implementation Strategy

### MVP primero

1. Fase 1 (Foundational)
2. Fase 2 (US1) — **parar y validar** con CA-900/901 antes de seguir
3. Entrega incremental: US2 → US3 → US4, validando el CA correspondiente en cada checkpoint
4. Fase 6 (Polish) al final

## Notes

- `[P]` = ficheros distintos, sin dependencias pendientes entre sí
- Commit atómico por fase o por grupo lógico pequeño, como en las specs anteriores
- Parar en cada checkpoint y validar la user story de forma independiente antes de continuar
