# Tasks: Pacientes, contactos y catálogo de médicos

**Input**: Design documents from `specs/001-pacientes-y-medicos/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos, con auditoría (Art. VII.6) como test explícito en cada user story desde el
principio — en Spec 000 este punto se detectó tarde, en `/speckit-analyze`; aquí se evita
repitiendo el mismo patrón desde el diseño.

**Remediación de `/speckit-analyze` (2026-09-05)**: se renumeró de 54 a 59 tareas para incorporar
5 hallazgos, todos de severidad MEDIA/ALTA, ninguno crítico ni de violación de constitución — ver
`PROGRESO.md`. Renumeración segura porque aún no existe código de esta spec.

**Organización**: 3 user stories mapeadas desde §3 (escenarios E1-E7) y §4 (secciones 4.1-4.3).
Sin fase de Setup: no se crea ningún proyecto nuevo, se reutilizan los 6 de Spec 000.

| Story | Prioridad | Escenarios | Secciones FR | CA cubiertos |
|---|---|---|---|---|
| US1 | P1 (MVP) | E1, E6, E7 | 4.1 Paciente | CA-001, 002, 003, 009, 010, 011, 012, 013, 014, 015 |
| US2 | P2 | E2, E3, E4 | 4.3 Catálogo de médicos | CA-004, 005, 006, 007 |
| US3 | P3 | E5 | 4.2 Contactos | CA-008 |

## Format: `[ID] [P?] [Story] Description`

- **[P]**: puede ejecutarse en paralelo (ficheros distintos, sin dependencias pendientes)
- **[Story]**: US1..US3 según la tabla anterior

---

## Phase 1: Foundational (Blocking Prerequisites)

**Purpose**: esquema de BD, entidades y repositorios que usan las 3 user stories

**⚠️ CRITICAL**: ninguna user story empieza antes de completar esta fase

- [ ] T001 Crear script `src/Spd.Infraestructura/Migraciones/0002_pacientes_contactos_medicos.sql` con tablas `Medico`, `Paciente`, `Contacto` (columnas de [data-model.md](./data-model.md), incluidas `correlativo_num_ficha`, `motivo_baja`/`motivo_baja_detalle` con `CHECK`, `busqueda_normalizada` en Paciente y Medico — research.md Decisiones 1-3)
- [ ] T002 [P] Crear enum `EstadoPaciente` en `src/Spd.Dominio/EstadoPaciente.cs` (FR-006)
- [ ] T003 [P] Crear enum `TipoContacto` en `src/Spd.Dominio/TipoContacto.cs` (FR-020)
- [ ] T004 [P] Crear enum `MotivoBaja` en `src/Spd.Dominio/MotivoBaja.cs` (FR-007)
- [ ] T005 [P] Crear entidad `Medico` en `src/Spd.Dominio/Medico.cs` (FR-030)
- [ ] T006 [P] Crear entidad `Paciente` en `src/Spd.Dominio/Paciente.cs` con `TransicionValida(EstadoPaciente nuevo)` (máquina de estados de FR-006)
- [ ] T007 [P] Crear entidad `Contacto` en `src/Spd.Dominio/Contacto.cs` (FR-020)
- [ ] T008 [P] Crear `Normalizador` (minúsculas + quitar tildes) en `src/Spd.Dominio/Normalizador.cs` (research.md Decisión 1)
- [ ] T009 [P] Crear `ValidadorDni` (letra de control DNI/NIE) en `src/Spd.Dominio/ValidadorDni.cs` (research.md Decisión 4, FR-005)
- [ ] T010 [P] Crear `ValidadorCip` (validación + autocompletar posiciones 1-11) en `src/Spd.Dominio/ValidadorCip.cs` (research.md Decisión 5, FR-005/FR-005b)
- [ ] T011 [P] Definir `IRepositorioMedicos` en `src/Spd.Dominio/IRepositorioMedicos.cs`
- [ ] T012 [P] Definir `IRepositorioPacientes` en `src/Spd.Dominio/IRepositorioPacientes.cs` (incluye `ObtenerSiguienteCorrelativo()`)
- [ ] T013 [P] Definir `IRepositorioContactos` en `src/Spd.Dominio/IRepositorioContactos.cs`
- [ ] T014 Implementar `RepositorioMedicos` en `src/Spd.Infraestructura/RepositorioMedicos.cs` (depende de T001, T005, T011)
- [ ] T015 Implementar `RepositorioPacientes` en `src/Spd.Infraestructura/RepositorioPacientes.cs` (depende de T001, T006, T012)
- [ ] T016 Implementar `RepositorioContactos` en `src/Spd.Infraestructura/RepositorioContactos.cs` (depende de T001, T007, T013)
- [ ] T017 [P] Test de humo: la migración 0002 se aplica de forma idempotente sobre un esquema ya en versión 1 (Spec 000), `schema_version` pasa a 2, en `tests/Spd.Aplicacion.Tests/InfraestructuraPacientesFundamentosTests.cs`
- [ ] T018 [P] Test de humo: alta y lectura básica de Medico/Paciente/Contacto vía repositorios, en el mismo fichero

**Checkpoint**: esquema y entidades listas — las user stories pueden empezar.

---

## Phase 2: User Story 1 — Ficha de paciente: alta, estados y búsqueda (Priority: P1) 🎯 MVP

**Goal**: registrar la ficha de un paciente en una sola pantalla, con numeración automática,
validación de mínimos y de DNI/CIP, gestión de estados y búsqueda global (E1, E6, E7).

**Independent Test**: crear un paciente con nombre+apellidos+DNI, comprobar que recibe num_ficha
correlativo (CA-001), darlo de baja y reactivarlo (CA-009/010), buscarlo sin tildes (CA-011).

### Tests for User Story 1

- [ ] T019 [P] [US1] Test: `ValidadorDni` valida formato y letra de control de varios DNI/NIE conocidos, en `tests/Spd.Dominio.Tests/ValidadorDniTests.cs`
- [ ] T020 [P] [US1] Test: `ValidadorCip` detecta formato/correspondencia inválidos sin bloquear (CA-015), en `tests/Spd.Dominio.Tests/ValidadorCipTests.cs`
- [ ] T021 [P] [US1] Test: `ValidadorCip` autocompleta posiciones 1-11 dejando 12-14 en blanco (CA-014), en el mismo fichero
- [ ] T022 [P] [US1] Test: `Paciente.TransicionValida` permite las transiciones de FR-006 y rechaza el resto, en `tests/Spd.Dominio.Tests/PacienteTests.cs`
- [ ] T023 [P] [US1] Test: `ServicioPacientes.Crear` asigna `num_ficha` correlativo con el prefijo vigente, no editable después (CA-001), en `tests/Spd.Aplicacion.Tests/ServicioPacientesTests.cs`
- [ ] T024 [P] [US1] Test: `ServicioPacientes.Crear` rechaza si faltan los mínimos de FR-003 (CA-002)
- [ ] T025 [P] [US1] Test: `ServicioPacientes.Crear` avisa (no bloquea) de duplicado por **DNI o CIP** activo (CA-003, FR-004 — remediación F4: el análisis detectó que solo se probaba el caso DNI pese a que FR-004 cubre también CIP)
- [ ] T026 [P] [US1] Test: `ServicioPacientes.CambiarEstado` a BAJA exige fecha+motivo y no borra ningún dato relacionado (CA-009, Art. III.1)
- [ ] T027 [P] [US1] Test: `ServicioPacientes.CambiarEstado` de BAJA a EVALUACION (reactivación) — CA-010 a nivel de esta spec (el aviso "sin consentimiento vigente" es de Spec 002)
- [ ] T028 [P] [US1] Test: `Buscar` encuentra por fragmento sin tildes/mayúsculas (CA-011) y ordena activos→evaluación→suspendidos→bajas
- [ ] T029 [P] [US1] Test: `Crear`, `Actualizar` y `CambiarEstado` registran en auditoría con detalle antes/después (CA-013, Art. VII.6)
- [ ] T030 [P] [US1] Test: `ServicioPacientes.Crear`/`Actualizar` rechaza guardar con `cip` informado y `sexo` vacío (FR-002b — remediación F2: regla sin ninguna cobertura previa, ni de test ni de quickstart)
- [ ] T031 [P] [US1] Test: un usuario con rol Elaborador crea, edita y cambia de estado un paciente sin ninguna restricción de campo respecto a un Administrador (CA-012 — remediación F1: la spec exige el criterio explícitamente y solo estaba cubierto de forma manual en quickstart.md)

### Implementation for User Story 1

- [ ] T032 [US1] Implementar `IServicioPacientes` y `ServicioPacientes` en `src/Spd.Aplicacion/ServicioPacientes.cs` (Crear, Actualizar, CambiarEstado, Buscar, ValidarDni, ValidarCip, AutocompletarCip; depende de T006, T008, T009, T010, T012, T015); cada escritura registra en auditoría (Art. VII.6)
- [ ] T033 [US1] Implementar `FichaPacienteViewModel` en `src/Spd.Presentacion/ViewModels/FichaPacienteViewModel.cs` (pestaña Datos; cabecera con num_ficha/nombre/edad/estado/alertas, FR-008)
- [ ] T034 [US1] Implementar `FichaPacienteView`/`FichaPacienteWindow` en `src/Spd.Presentacion/Views/Pacientes/`
- [ ] T035 [US1] Implementar `BuscadorPacientesViewModel` + vista (listado, filtro por estado, FR-011) en `src/Spd.Presentacion/ViewModels/BuscadorPacientesViewModel.cs` + `src/Spd.Presentacion/Views/Pacientes/BuscadorPacientesView.axaml`
- [ ] T036 [US1] Añadir botón "Pacientes" a `MainWindow`/`MainViewModel` (visible para Elaborador y Administrador, FR-040)
- [ ] T037 [P] [US1] Test Avalonia.Headless: `BuscadorPacientesView` construye la ventana, renderiza el listado con su `ItemTemplate` y ejecuta `.Show()` sin lanzar excepción, en `tests/Spd.Presentacion.Tests/BuscadorPacientesViewTests.cs` (remediación F5/F3 — mismo patrón de incidente que `UsuariosWindowTests` en Spec 000: un `ItemTemplate` con binding a un comando de un ancestro es la categoría de bug que ya causó un cierre inesperado (SIGABRT) sin que ningún test de Aplicación lo detectara)

**Checkpoint**: US1 funcional de forma independiente — MVP entregable.

---

## Phase 3: User Story 2 — Catálogo de médicos y selector con autocompletado (Priority: P2)

**Goal**: reutilizar médicos ya registrados desde cualquier ficha, crearlos en contexto, y que
editar uno se refleje en todas partes (E2, E3, E4).

**Independent Test**: buscar un médico por fragmento de apellido (CA-004), crear uno nuevo sin
salir de la ficha (CA-005), editar su teléfono y comprobarlo en un paciente que lo referencia
(CA-006), intentar darlo de baja siendo cabecera de un paciente activo (CA-007).

### Tests for User Story 2

- [ ] T038 [P] [US2] Test: `Buscar` de médicos no busca con menos de 2 caracteres y encuentra por fragmento normalizado (CA-004), en `tests/Spd.Aplicacion.Tests/ServicioMedicosTests.cs`
- [ ] T039 [P] [US2] Test: `Crear` médico avisa (no bloquea) si existe otro activo con mismos apellidos+nombre o mismo colegiado (FR-034)
- [ ] T040 [P] [US2] Test: `Actualizar` médico se refleja al releer un paciente que lo referencia, sin copia de datos (CA-006, Art. IV.1)
- [ ] T041 [P] [US2] Test: `DarDeBaja` médico lanza excepción con la lista de pacientes si es cabecera de alguno activo/evaluación (CA-007)
- [ ] T042 [P] [US2] Test: `Crear` y `DarDeBaja` de médico registran en auditoría (Art. VII.6)

### Implementation for User Story 2

- [ ] T043 [US2] Implementar `IServicioMedicos` y `ServicioMedicos` en `src/Spd.Aplicacion/ServicioMedicos.cs` (depende de T005, T008, T011, T014); cada escritura registra en auditoría
- [ ] T044 [US2] Implementar `SelectorMedicoViewModel` (autocompletado + "Nuevo médico…", FR-032/033) en `src/Spd.Presentacion/ViewModels/SelectorMedicoViewModel.cs` + vista reutilizable
- [ ] T045 [US2] Integrar `SelectorMedicoViewModel` en `FichaPacienteView` como campo de médico de cabecera (depende de T033/T034, T044)
- [ ] T046 [US2] Implementar `CatalogoMedicosViewModel` + vista (listado con búsqueda y nº de pacientes activos por médico, FR-037) en `src/Spd.Presentacion/ViewModels/CatalogoMedicosViewModel.cs` + vista
- [ ] T047 [US2] Añadir botón "Catálogo de médicos" a `MainWindow`/`MainViewModel`
- [ ] T048 [P] [US2] Test Avalonia.Headless: `SelectorMedicoViewModel` muestra los resultados del autocompletado en su lista, y elegir "Nuevo médico…" crea el médico y lo deja seleccionado sin cerrar la ficha (CA-005), en `tests/Spd.Presentacion.Tests/SelectorMedicoViewTests.cs` (remediación F3 — CA-005 es una aserción de comportamiento de UI que ningún test de `ServicioMedicos` puede verificar)
- [ ] T049 [P] [US2] Test Avalonia.Headless: `CatalogoMedicosView` construye la ventana, renderiza el listado con su `ItemTemplate` y ejecuta `.Show()` sin lanzar excepción, en `tests/Spd.Presentacion.Tests/CatalogoMedicosViewTests.cs` (remediación F5)

**Checkpoint**: US1 + US2 funcionales de forma independiente.

---

## Phase 4: User Story 3 — Contactos del paciente (Priority: P3)

**Goal**: registrar representantes, familiares y cuidadores de un paciente, con las marcas
"principal" y "retira la medicación" (E5).

**Independent Test**: crear un contacto REPRESENTANTE_LEGAL sin DNI y comprobar que no guarda
(CA-008); marcar "retira la medicación" en uno y comprobar que se desmarca en cualquier otro del
mismo paciente.

### Tests for User Story 3

- [ ] T050 [P] [US3] Test: contacto REPRESENTANTE_LEGAL/PERSONA_AUTORIZADA sin DNI no se guarda (CA-008), en `tests/Spd.Aplicacion.Tests/ServicioContactosTests.cs`
- [ ] T051 [P] [US3] Test: marcar `retira_medicacion` exige DNI (FR-021c) y desmarca cualquier otro contacto del mismo paciente que lo tuviera (FR-021b)
- [ ] T052 [P] [US3] Test: marcar `es_principal` desmarca cualquier otro contacto principal del mismo paciente (FR-021)
- [ ] T053 [P] [US3] Test: la baja de contacto es lógica, no se elimina y no aparece salvo "ver histórico" (FR-023, Art. III.1)
- [ ] T054 [P] [US3] Test: alta y baja de contacto registran en auditoría (Art. VII.6)

### Implementation for User Story 3

- [ ] T055 [US3] Implementar `IServicioContactos` y `ServicioContactos` en `src/Spd.Aplicacion/ServicioContactos.cs` (depende de T007, T013, T016); cada escritura registra en auditoría
- [ ] T056 [US3] Implementar pestaña "Contactos" (`ContactosPacienteViewModel` + vista) integrada en `FichaPacienteView` como segunda pestaña (FR-009; depende de T034)

**Checkpoint**: las 3 user stories funcionan de forma independiente.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T057 [P] Ejecutar íntegramente [quickstart.md](./quickstart.md) y registrar el resultado en `PROGRESO.md`
- [ ] T058 Revisar que ningún método de `Spd.Dominio`/`Spd.Aplicacion` supere ~40 líneas ni ninguna clase ~300 (Art. XI.6)
- [ ] T059 [P] Actualizar `PROGRESO.md` marcando cada CA-001..CA-015 como validado, con el test que lo confirma

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Fase 1)**: sin dependencias — bloquea las 3 user stories
- **US1 (Fase 2)**: depende de Foundational; sin dependencias de otra user story
- **US2 (Fase 3)**: depende de Foundational; T045 (integrar el selector en la ficha) depende de
  T033/T034 de US1, el resto de US2 es independiente
- **US3 (Fase 4)**: depende de Foundational; T056 (pestaña Contactos) depende de T034 de US1
- **Polish (Fase 5)**: depende de las user stories que se quieran dar por completas

### Parallel Opportunities

- T002-T013 (Foundational) en paralelo entre sí
- Una vez completada Foundational: US1 puede avanzar en paralelo con la parte de US2/US3 que no
  depende de la ficha de paciente (T038-T043, T050-T055)
- Todos los tests `[P]` de una misma user story, en paralelo
- Los tests Avalonia.Headless (T037, T048, T049) dependen de que su vista/ViewModel ya esté
  implementado (no son TDD-first, son de regresión, igual que en Spec 000) — se ejecutan en
  paralelo entre sí una vez cumplida esa dependencia

## Implementation Strategy

### MVP primero

1. Fase 1 (Foundational)
2. Fase 2 (US1) — **parar y validar** con CA-001/002/003/009/010/011/012 antes de seguir
3. Entrega incremental: US2 → US3, validando el CA correspondiente en cada checkpoint
4. Fase 5 (Polish) al final

## Notes

- `[P]` = ficheros distintos, sin dependencias pendientes entre sí
- Commit atómico por fase o por grupo lógico pequeño, como en Spec 000
- Parar en cada checkpoint y validar la user story de forma independiente antes de continuar
