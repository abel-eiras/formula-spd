# Tasks: Depósito de envases y listado de retirada

**Input**: Design documents from `specs/005-deposito-y-retirada/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos, con auditoría (Art. VII.6) como test explícito desde el diseño.

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 (MVP) | 4.2 Envase en custodia | CA-503, 504, 511, 512, 514, 515 |
| US2 | P1 | 4.1 Parámetros + 4.4 Listado | CA-500, 501, 502 (extensión), 512, 513 (extensión) |
| US3 | P2 | 4.3 Descuento y sobrante | CA-505, 506, 507, 507b, 507c |
| US4 | P2 | 4.5 Salidas (SIGRE, baja) | CA-508, 509 |
| US5 | P3 | 4.5 Entrega fuera de blíster | CA-510 |
| US6 | P3 | 4.8 Importación | CA-516, 517, 518 |

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Foundational (Blocking Prerequisites)

- [X] T001 Crear script `src/Spd.Infraestructura/Migraciones/0006_envase.sql` con tablas `Envase` y `PerfilImportacionTratamiento` (data-model.md)
- [X] T002 [P] Crear enums `EstadoEnvase`, `OrigenEnvase`, `MotivoSalidaEnvase`, `OrigenImportacionTratamiento` en `src/Spd.Dominio/`
- [X] T003 [P] Crear entidad `Envase` en `src/Spd.Dominio/Envase.cs`
- [X] T004 [P] Crear entidad `PerfilImportacionTratamiento` en `src/Spd.Dominio/PerfilImportacionTratamiento.cs`
- [X] T005 [P] Definir `IRepositorioEnvases` en `src/Spd.Dominio/IRepositorioEnvases.cs`
- [X] T006 [P] Definir `IRepositorioPerfilesImportacionTratamiento` en `src/Spd.Dominio/IRepositorioPerfilesImportacionTratamiento.cs`
- [X] T007 Implementar `RepositorioEnvases` en `src/Spd.Infraestructura/RepositorioEnvases.cs` (depende de T001, T003, T005)
- [X] T008 [P] Implementar `RepositorioPerfilesImportacionTratamiento` en `src/Spd.Infraestructura/RepositorioPerfilesImportacionTratamiento.cs` (depende de T001, T004, T006)
- [X] T009 [P] Test de humo: migración 0006 idempotente, en `tests/Spd.Aplicacion.Tests/InfraestructuraEnvasesFundamentosTests.cs`
- [X] T010 [P] Definir `IComprobadorCoberturaSpd` + `ComprobadorCoberturaSpdNulo` en `src/Spd.Dominio/` / `src/Spd.Infraestructura/` (research.md Decisión 3)

**Checkpoint**: esquema y entidades listos.

---

## Phase 2: User Story 1 — Registrar y consultar envases en custodia (Priority: P1) 🎯 MVP

**Goal**: dar de alta un envase con serie/lote/caducidad, verlo en la pestaña Depósito, con los
avisos y bloqueos de FR-510–FR-517 (E2, E6).

**Independent Test**: alta de envase en paciente con tratamiento activo; serie duplicada bloqueada
(CA-504); caducidad excluye de disponibles (CA-511); DNI de retirada resuelto (CA-514/515, aunque
la columna vive en US2, la resolución del dato es de Contacto/Paciente, ya existente).

### Tests for User Story 1

- [X] T011 [P] [US1] Test: `ServicioEnvases.RegistrarEnvase` exige tratamiento activo `en_spd=1`; lanza `ErrorValidacionException` si no existe (FR-515), en `tests/Spd.Aplicacion.Tests/ServicioEnvasesTests.cs`
- [X] T012 [P] [US1] Test: `RegistrarEnvase` bloquea serie duplicada en otro paciente (CA-504)
- [X] T013 [P] [US1] Test: `RegistrarEnvase` con caducidad pasada avisa pero no bloquea (FR-514)
- [X] T014 [P] [US1] Test: `RegistrarEnvase` prerrellena `unidades_iniciales` desde `Medicamento.UnidadesEnvase` si existe (FR-513)
- [X] T015 [P] [US1] Test: `ListarEnCustodiaDePaciente`/`ListarHistoricoDePaciente` separan por estado (FR-517)
- [X] T016 [P] [US1] Test: `RegistrarEnvase` registra en auditoría (Art. VII.6)

### Implementation for User Story 1

- [X] T017 [US1] Crear `DatosAltaEnvase` en `src/Spd.Aplicacion/DatosAltaEnvase.cs`
- [X] T018 [US1] Implementar `IServicioEnvases`/`ServicioEnvases` (alta, listar custodia/histórico) en `src/Spd.Aplicacion/{IServicioEnvases.cs,ServicioEnvases.cs}` (depende de T007, T010, T017)
- [X] T019 [US1] Implementar `DepositoViewModel` en `src/Spd.Presentacion/ViewModels/DepositoViewModel.cs`
- [X] T020 [US1] Implementar `DepositoView`/`DepositoWindow` + botón "Depósito" en `FichaPacienteView` en `src/Spd.Presentacion/Views/Pacientes/`
- [X] T021 [P] [US1] Test Avalonia.Headless: la vista de depósito construye y muestra sin lanzar, en `tests/Spd.Presentacion.Tests/DepositoViewTests.cs`

**Checkpoint**: US1 funcional de forma independiente.

---

## Phase 3: User Story 2 — Listado de retirada (Priority: P1)

**Goal**: pantalla que calcula qué retirar por paciente, con DNI de retirada y filtros (E1, E8).

**Independent Test**: paciente con stock insuficiente en ventana de antelación aparece con las
columnas correctas (CA-500); fuera de ventana no aparece (CA-501); registrar desde la fila hace
que desaparezca (CA-503, ya cubierto por US1 + este servicio).

### Tests for User Story 2

- [X] T022 [P] [US2] Test: `ObtenerListado` calcula necesarias/disponibles/faltan/envases_a_retirar (CA-500), en `tests/Spd.Aplicacion.Tests/ServicioListadoRetiradaTests.cs`
- [X] T023 [P] [US2] Test: ventana de antelación excluye/incluye correctamente (CA-501)
- [X] T024 [P] [US2] Test: `unidades_envase` desconocido → `EnvasesARetirar = null` sin romper el resto (CA-512)
- [X] T025 [P] [US2] Test: DNI de retirada = contacto marcado, o paciente si no hay contacto, o vacío con aviso (CA-514/515)
- [X] T026 [P] [US2] Test: filtro "solo con faltantes" vs "todos los previstos" (FR-534)
- [X] T027 [P] [US2] Test: `IComprobadorCoberturaSpd` nulo nunca excluye (documenta CA-502 diferido)
- [X] T028 [P] [US2] Test: `RegistrarImpresion` audita `IMPRIMIR` (FR-535, extensión)

### Implementation for User Story 2

- [X] T029 [US2] Crear `FilaListadoRetirada`, `FiltrosListadoRetirada` en `src/Spd.Aplicacion/`
- [X] T030 [US2] Implementar cálculo de "próxima retirada" (FR-502) como método reutilizable en `src/Spd.Dominio/Paciente.cs` o helper de `Spd.Aplicacion`
- [X] T031 [US2] Implementar `IServicioListadoRetirada`/`ServicioListadoRetirada` en `src/Spd.Aplicacion/` (depende de T007, T010, T018, T029, T030)
- [X] T032 [US2] Implementar `RetiradaEnvasesViewModel` + acción "Registrar envase" desde fila en `src/Spd.Presentacion/ViewModels/`
- [X] T033 [US2] Implementar `RetiradaEnvasesView`/`RetiradaEnvasesWindow` + botón en `MainWindow` en `src/Spd.Presentacion/Views/`
- [X] T034 [P] [US2] Test Avalonia.Headless: la vista de retirada construye y muestra sin lanzar, en `tests/Spd.Presentacion.Tests/RetiradaEnvasesViewTests.cs`

**Checkpoint**: US1 + US2 dan valor real (alta + saber qué retirar).

---

## Phase 4: User Story 3 — Sobrante en el descuento (Priority: P2)

**Goal**: algoritmo de asignación/descuento reutilizable por Spec 006 (E3).

### Tests for User Story 3

- [X] T035 [P] [US3] Test: `CalculadoraUnidadesADescontar` — sin fracción, suma exacta; con fracción, `floor+1` incluso con suma entera (CA-507, CA-507b), en `tests/Spd.Dominio.Tests/CalculadoraUnidadesADescontarTests.cs`
- [X] T036 [P] [US3] Test: `AjusteUnidadesManual` sobrescribe el cálculo (CA-507c)
- [X] T037 [P] [US3] Test: `ServicioAsignacionEnvases.Descontar` agota primero el envase con menos restantes (CA-505), en `tests/Spd.Aplicacion.Tests/ServicioAsignacionEnvasesTests.cs`
- [X] T038 [P] [US3] Test: sobrante permanece `EnCustodia`, nunca se descarta (CA-506)
- [X] T039 [P] [US3] Test: `Descontar` registra en auditoría y respeta `caducidadMinima`

### Implementation for User Story 3

- [X] T040 [US3] Implementar `CalculadoraUnidadesADescontar` en `src/Spd.Dominio/CalculadoraUnidadesADescontar.cs`
- [X] T041 [US3] Implementar `IServicioAsignacionEnvases`/`ServicioAsignacionEnvases`/`ResultadoDescuento` en `src/Spd.Aplicacion/` (depende de T007, T040)

**Checkpoint**: servicio de descuento probado y listo para que Spec 006 lo invoque.

---

## Phase 5: User Story 4 — Cese de tratamiento y bajas (Priority: P2)

**Goal**: proponer y ejecutar salida a SIGRE al finalizar tratamiento o dar de baja al paciente
(E4), sin ejecutar sin confirmación.

### Tests for User Story 4

- [X] T042 [P] [US4] Test: `ProponerSalidaSigrePorFinDeTratamiento` lista envases en custodia del medicamento sin ejecutar nada, en `tests/Spd.Aplicacion.Tests/ServicioEnvasesTests.cs`
- [X] T043 [P] [US4] Test: `DarSalidaSigre` pasa a `ResiduoSigre` con motivo y unidades desechadas (CA-508)
- [X] T044 [P] [US4] Test: `DarSalidaSigreMasiva` cubre todos los envases en custodia del paciente (baja/fallecimiento)
- [X] T045 [P] [US4] Test: ninguna acción devuelve un envase al stock ni lo reasigna (CA-509)

### Implementation for User Story 4

- [X] T046 [US4] Implementar salidas SIGRE (`DarSalidaSigre`, `Proponer*`, `DarSalidaSigreMasiva`) en `ServicioEnvases` (depende de T018)
- [X] T047 [US4] Enganchar la propuesta desde `ServicioTratamientos.CambiarPauta`/`CambiarEstado` (Spec 004) — el punto de enganche no ejecuta nada por sí mismo, solo lo deja disponible para la UI; y desde la baja de paciente (Spec 001) en la ViewModel correspondiente
- [X] T048 [US4] Implementar diálogo de confirmación de salida SIGRE en `src/Spd.Presentacion/Views/Pacientes/` (accesible desde Depósito)

**Checkpoint**: ciclo de vida completo del envase salvo entrega fuera de blíster e importación.

---

## Phase 6: User Story 5 — Medicación fuera de blíster (Priority: P3)

**Goal**: registrar entrega directa sin custodia para medicamentos `en_spd=0` (E5).

### Tests for User Story 5

- [X] T049 [P] [US5] Test: `RegistrarEntregaFueraBlister` exige `en_spd=0`; crea el envase directo en `ENTREGADO_PACIENTE` sin exigir serie (CA-510), en `tests/Spd.Aplicacion.Tests/ServicioEnvasesTests.cs`
- [X] T050 [P] [US5] Test: un envase `ENTREGADO_PACIENTE` no cuenta en `disponibles` del listado ni aparece en él

### Implementation for User Story 5

- [X] T051 [US5] Crear `DatosEntregaFueraBlister` + implementar `RegistrarEntregaFueraBlister` en `ServicioEnvases`
- [X] T052 [US5] Añadir acción "Entregar fuera de blíster" en `DepositoView`/`DepositoViewModel`

**Checkpoint**: todos los flujos de envase cubiertos.

---

## Phase 7: User Story 6 — Importación por pegado o fichero (Priority: P3)

**Goal**: alta masiva de tratamiento+envase desde el programa de gestión (E7).

### Tests for User Story 6

- [X] T053 [P] [US6] Test: `ParserLineasImportacion` parsea texto tabulado y CSV con cabecera opcional en filas mapeadas, en `tests/Spd.Aplicacion.Tests/ParserLineasImportacionTests.cs`
- [X] T054 [P] [US6] Test: `ImportarDesdePegado` con CN sin tratamiento crea tratamiento pendiente + envase (CA-517), en `tests/Spd.Aplicacion.Tests/ServicioImportacionTratamientoEnvaseTests.cs`
- [X] T055 [P] [US6] Test: `ImportarDesdePegado` con CN con tratamiento activo reutiliza y solo crea envase (CA-516)
- [X] T056 [P] [US6] Test: `ImportarDesdeFichero` con serie duplicada la separa en el resumen sin detener el resto (CA-518)
- [X] T057 [P] [US6] Test: CN no encontrado en catálogo se separa en el resumen sin bloquear las demás filas (FR-575)
- [X] T058 [P] [US6] Test: `GuardarPerfil`/`ListarPerfiles` persisten el mapeo (FR-572)

### Implementation for User Story 6

- [X] T059 [US6] Implementar `ParserLineasImportacion` en `src/Spd.Aplicacion/ParserLineasImportacion.cs`
- [X] T060 [US6] Implementar `ResultadoImportacion`, `MapeoColumnasImportacion` en `src/Spd.Aplicacion/`
- [X] T061 [US6] Implementar `IServicioImportacionTratamientoEnvase`/`ServicioImportacionTratamientoEnvase` en `src/Spd.Aplicacion/` (depende de T008, T018, T059, T060, y de `IServicioTratamientos` de Spec 004)
- [X] T062 [US6] Implementar `ImportarTratamientoViewModel` + vista con mapeo de columnas y resumen en `src/Spd.Presentacion/Views/Pacientes/`
- [X] T063 [P] [US6] Test Avalonia.Headless: la vista de importación construye y muestra sin lanzar

**Checkpoint**: todas las user stories completas.

---

## Phase 8: Polish

- [X] T064 [P] Revisar que ninguna prueba usa `Assert.Equal(N, version)` exacto sobre `schema_version` (usar patrón idempotencia + `>=`)
- [X] T065 Ejecutar `dotnet build` y `dotnet test` completos sobre la rama; corregir regresiones en `MainWindow`/`FichaPacienteView`/tests de otras specs si la nueva wiring los toca
- [X] T066 Actualizar `docs/data-model.md` con el campo `EntregadoA` de `Envase` (research.md Decisión 8)
