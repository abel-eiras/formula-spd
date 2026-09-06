# Tasks: Preparación, verificación y entrega del SPD

**Input**: Design documents from `specs/006-preparacion-verificacion-entrega/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos, con auditoría (Art. VII.6) e invariantes de Art. I.3 como tests explícitos
desde el diseño.

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 (MVP) | 4.1/4.2 Sesión, líneas, multi-envase | CA-600, 601, 602 |
| US2 | P2 | 4.3 Alta de envase desde preparación | CA-603, 604 |
| US3 | P1 | 4.6 Verificación | CA-605 |
| US4 | P1 | 4.7 Entrega | CA-606, 607 |
| US5 | P2 | 4.8 Continuidad | CA-608, 608b, 609, 610 |
| US6 | P3 | 4.12 Reelaboración | CA-6120..6126 |

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Foundational (Blocking Prerequisites)

- [X] T001 Crear script `src/Spd.Infraestructura/Migraciones/0009_preparacion.sql` con las 7 tablas (data-model.md)
- [X] T002 [P] Crear enums `EstadoSpd`, `EstadoLinea`, `ResultadoVerificacion`, `OrigenSolicitudReelaboracion` en `src/Spd.Dominio/`
- [X] T003 [P] Crear entidades `Spd`, `SpdLinea`, `SpdLineaEnvase`, `SpdVerificacion`, `SpdModificacion` en `src/Spd.Dominio/`
- [X] T004 [P] Crear entidades `RegistroAmbiental`, `MaterialAcondicionamiento` en `src/Spd.Dominio/`
- [X] T005 [P] Definir `IComprobadorIdoneidadYConsentimiento` + `ComprobadorIdoneidadYConsentimientoNulo` en `src/Spd.Dominio/`
- [X] T006 [P] Definir `IRepositorioSpd`, `IRepositorioSpdLineas`, `IRepositorioSpdLineaEnvases`, `IRepositorioSpdVerificaciones`, `IRepositorioSpdModificaciones`, `IRepositorioRegistrosAmbientales`, `IRepositorioMaterialAcondicionamiento` en `src/Spd.Dominio/`
- [X] T007 Implementar los 7 repositorios en `src/Spd.Infraestructura/` (depende de T001, T003-T006)
- [X] T008 [P] Test de humo: migración 0009 idempotente + repositorio SPD crea/obtiene, en `tests/Spd.Aplicacion.Tests/InfraestructuraPreparacionFundamentosTests.cs`
- [X] T009 [P] Test: regla Art. I.3 — `ComprobadorIdoneidadYConsentimientoNulo.Aprobado` siempre true, documentando el punto de extensión, en `tests/Spd.Dominio.Tests/ComprobadorIdoneidadYConsentimientoTests.cs`

**Checkpoint**: esquema y entidades listos.

---

## Phase 2: User Story 1 — Sesión, líneas y multi-envase (Priority: P1) 🎯 MVP

**Goal**: crear una sesión de 1 o 2 SPD con sus líneas, y pasar a PREPARADO con reparto
multi-envase correcto.

### Tests for User Story 1

- [X] T010 [P] [US1] Test: `CrearSesion` con `n_blisteres=2` crea 2 SPD con validez consecutiva de 7 días (CA-600), en `tests/Spd.Aplicacion.Tests/ServicioPreparacionTests.cs`
- [X] T011 [P] [US1] Test: `CrearSesion` bloquea si hay faltantes en el listado de retirada y no crea nada (CA-601)
- [X] T012 [P] [US1] Test: `CrearSesion` respeta `IComprobadorIdoneidadYConsentimiento` (bloquea si devuelve false)
- [X] T013 [P] [US1] Test: `PasarAPreparado` con envase repartido en dos filas (CA-602)
- [X] T014 [P] [US1] Test: `PasarAPreparado` exige `registroAmbientalId` (Art. I.3, "toda preparación registra temperatura y humedad")
- [X] T015 [P] [US1] Test: `CrearSesion`/`PasarAPreparado` registran en auditoría (Art. VII.6)

### Implementation for User Story 1

- [X] T016 [US1] Implementar `IServicioPreparacion.CrearSesion` en `src/Spd.Aplicacion/ServicioPreparacion.cs` (depende de T007; reutiliza `IServicioListadoRetirada`, `IRepositorioTratamientos`, `CalculadoraUnidadesADescontar`)
- [X] T017 [US1] Implementar `ObtenerOCrearLecturaAmbiental`, `AsignarMaterial`, `ExcluirLinea`
- [X] T018 [US1] Implementar `PasarAPreparado` (validación previa + `IServicioAsignacionEnvases.Descontar` por línea, research.md Decisiones 2/3)
- [X] T019 [US1] Implementar `SesionPreparacionViewModel`/vista (pestañas por blíster, llenado 7×4) en `src/Spd.Presentacion/Views/Preparacion/`
- [X] T020 [P] [US1] Test Avalonia.Headless: la vista de sesión construye y muestra sin lanzar

**Checkpoint**: US1 funcional de forma independiente.

---

## Phase 3: User Story 2 — Alta de envase desde la preparación (Priority: P2)

- [X] T021 [P] [US2] Test: `PasarAPreparado` sin saldo lanza indicando medicamento y unidades que faltan (CA-603), en `tests/Spd.Aplicacion.Tests/ServicioPreparacionTests.cs`
- [X] T022 [P] [US2] Test: `RegistrarEnvaseDesdeLinea` + `PasarAPreparado` tiene éxito tras el alta (CA-604)
- [X] T023 [US2] Implementar `RegistrarEnvaseDesdeLinea` (delega en `IServicioEnvases.RegistrarEnvase`)
- [X] T024 [US2] Añadir botón "Registrar envase" en la vista de sesión, sin recargar la pantalla

**Checkpoint**: US1 + US2 cubren el alta completa de un blíster.

---

## Phase 4: User Story 3 — Verificación por blíster (Priority: P1)

- [X] T025 [P] [US3] Test: verificar el blíster 1 no cambia el estado del blíster 2 (CA-605), en `tests/Spd.Aplicacion.Tests/ServicioPreparacionTests.cs`
- [X] T026 [P] [US3] Test: `Verificar` exige `excepcionMotivo` (≥10 caracteres) si verificador = elaborador (Art. I.3/VII.5)
- [X] T027 [P] [US3] Test: `Verificar` sobre un SPD no `Preparado` lanza
- [X] T028 [US3] Implementar `Verificar` en `ServicioPreparacion`
- [X] T029 [US3] Implementar pantalla de verificación (checklist de 5 ítems) en `src/Spd.Presentacion/Views/Preparacion/`
- [X] T030 [P] [US3] Test Avalonia.Headless: la vista de verificación construye y muestra sin lanzar

**Checkpoint**: US1+US2+US3 cubren preparar y verificar.

---

## Phase 5: User Story 4 — Entrega conjunta o parcial (Priority: P1)

- [X] T031 [P] [US4] Test: `RegistrarEntrega` con dos SPD verificados los pasa a Entregado con los mismos datos (CA-606), en `tests/Spd.Aplicacion.Tests/ServicioPreparacionTests.cs`
- [X] T032 [P] [US4] Test: `RegistrarEntrega` con uno solo deja el otro en Verificado (CA-607)
- [X] T033 [US4] Implementar `RegistrarEntrega` en `ServicioPreparacion`
- [X] T034 [US4] Implementar pantalla de entrega (selección de blísteres pendientes) en `src/Spd.Presentacion/Views/Preparacion/`
- [X] T035 [P] [US4] Test Avalonia.Headless: la vista de entrega construye y muestra sin lanzar

**Checkpoint**: ciclo completo alta→preparación→verificación→entrega funcional de extremo a extremo.

---

## Phase 6: User Story 5 — Continuidad (Priority: P2)

- [X] T036 [P] [US5] Test: `PrepararSiguiente` sin cambios solo exige lectura ambiental (CA-608), en `tests/Spd.Aplicacion.Tests/ServicioPreparacionTests.cs`
- [X] T037 [P] [US5] Test: `PrepararSiguiente` con línea cubierta por dos envases resuelve ambos automáticamente (CA-608b)
- [X] T038 [P] [US5] Test: `PrepararSiguiente` con posología modificada marca solo esa línea (CA-609)
- [X] T039 [P] [US5] Test: `PrepararSiguiente` con envase agotado sin sustituto marca `EnvasePendiente` solo en esa línea (CA-610)
- [X] T040 [US5] Implementar `PrepararSiguiente` + `ResultadoContinuidad` en `ServicioPreparacion`
- [X] T041 [US5] Añadir acción "Preparar siguiente" en el listado de Preparaciones

**Checkpoint**: continuidad probada.

---

## Phase 7: User Story 6 — Reelaboración (Priority: P3)

- [X] T042 [P] [US6] Test: reelaborar conserva num_registro y sube version (CA-6120), en `tests/Spd.Aplicacion.Tests/ServicioPreparacionTests.cs`
- [X] T043 [P] [US6] Test: línea sin cambios no mueve envase (CA-6121)
- [X] T044 [P] [US6] Test: línea aumentada consume solo la diferencia (CA-6122)
- [X] T045 [P] [US6] Test: línea eliminada devuelve unidades a `unidades_restantes` (CA-6123)
- [X] T046 [P] [US6] Test: `SPD_Modificacion` contiene copia íntegra de la versión anterior (CA-6124)
- [X] T047 [P] [US6] Test: un SPD Entregado no admite Reelaborar (CA-6125)
- [X] T048 [P] [US6] Test: tras reelaborar, el estado es Preparado y exige nueva verificación (CA-6126)
- [X] T049 [US6] Implementar `Reelaborar` en `ServicioPreparacion`
- [X] T050 [US6] Añadir acción "Reelaborar" en la pantalla de sesión/listado

**Checkpoint**: todas las user stories completas.

---

## Phase 8: Listados, avisos e impresión diferida

- [X] T051 [P] Implementar `ListarPorFiltro` (FR-690) en `ServicioPreparacion`
- [X] T052 [P] Implementar `RegistrarImpresion` (FR-680/681, research.md Decisión 8) — solo timestamp + auditoría
- [X] T053 Implementar pantalla "Preparaciones" (listado con filtros) en `src/Spd.Presentacion/Views/Preparacion/`

## Phase 9: Polish

- [X] T054 Ejecutar `dotnet build` y `dotnet test` completos sobre la rama; corregir regresiones en `MainWindow`/tests de otras specs si la nueva wiring los toca
