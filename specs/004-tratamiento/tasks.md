# Tasks: Tratamiento del paciente

**Input**: Design documents from `specs/004-tratamiento/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos, con auditoría (Art. VII.6) como test explícito desde el diseño.

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 (MVP) | 4.1 Alta y reutilización | CA-400, 403 |
| US2 | P2 | 4.2 Inmutabilidad + 4.3 Estados + 4.4 Ajuste manual | CA-401, 402, 405 (parcial) |

CA-404/406 diferidos (Spec 006/005 no existen en esta rama) — ver spec.md, "Fuera de alcance".

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Foundational (Blocking Prerequisites)

- [X] T001 Crear script `src/Spd.Infraestructura/Migraciones/0005_tratamiento.sql` con tabla `Tratamiento` (columnas de [data-model.md](./data-model.md))
- [X] T002 [P] Crear enum `FraccionDosis` en `src/Spd.Dominio/FraccionDosis.cs` (research.md Decisión 2), con `Valor`/`Texto`
- [X] T003 [P] Crear enums `TipoTratamiento`/`EstadoTratamiento` en `src/Spd.Dominio/`
- [X] T004 [P] Crear entidad `Tratamiento` en `src/Spd.Dominio/Tratamiento.cs`
- [X] T005 [P] Definir `IRepositorioTratamientos` en `src/Spd.Dominio/IRepositorioTratamientos.cs`
- [X] T006 Implementar `RepositorioTratamientos` en `src/Spd.Infraestructura/RepositorioTratamientos.cs` (depende de T001, T004, T005)
- [X] T007 [P] Test de humo: migración 0005 idempotente, en `tests/Spd.Aplicacion.Tests/InfraestructuraTratamientosFundamentosTests.cs`
- [X] T008 [P] Test: `FraccionDosis` expone 9 valores con `Texto`/`Valor` correctos, en `tests/Spd.Dominio.Tests/FraccionDosisTests.cs`

**Checkpoint**: esquema y entidad listos.

---

## Phase 2: User Story 1 — Alta y reutilización (Priority: P1) 🎯 MVP

**Goal**: dar de alta un tratamiento reutilizando medicamento/médico de los catálogos, con
posología restringida al vocabulario cerrado de fracciones (E1, E2, E5).

**Independent Test**: crear un tratamiento con médico de cabecera prerrellenado (CA-400); confirmar
que el selector de dosis solo ofrece las 9 fracciones cerradas (CA-403); crear uno con `en_spd=0`
y `pauta_texto` libre (E5).

### Tests for User Story 1

- [X] T009 [P] [US1] Test: `ServicioTratamientos.Crear` prerrellena el médico de cabecera del paciente si no se indica otro (CA-400), en `tests/Spd.Aplicacion.Tests/ServicioTratamientosTests.cs`
- [X] T010 [P] [US1] Test: `Crear` con `en_spd=1` exige pauta D/A/C/N de `FraccionDosis`; con `en_spd=0` acepta `pauta_texto` libre (FR-401)
- [X] T011 [P] [US1] Test: `ListarVigentesDePaciente` devuelve solo tratamientos sin `fecha_fin` y `estado != Finalizado`
- [X] T012 [P] [US1] Test: `Crear` registra en auditoría (Art. VII.6)

### Implementation for User Story 1

- [X] T013 [US1] Implementar `IServicioTratamientos.Crear`/`ListarVigentesDePaciente` en `src/Spd.Aplicacion/ServicioTratamientos.cs` (depende de T004-T006)
- [X] T014 [US1] Implementar `TratamientoViewModel` (alta) en `src/Spd.Presentacion/ViewModels/TratamientoViewModel.cs`, selector de medicamento (Spec 003) y médico (Spec 001) reutilizando los patrones ya existentes
- [X] T015 [US1] Implementar `TratamientoView`/añadir acceso desde `FichaPacienteView` (Spec 001) en `src/Spd.Presentacion/Views/Pacientes/`
- [X] T016 [P] [US1] Test Avalonia.Headless: la vista de tratamiento construye y muestra sin lanzar, en `tests/Spd.Presentacion.Tests/TratamientoViewTests.cs`

**Checkpoint**: US1 funcional de forma independiente — MVP entregable.

---

## Phase 3: User Story 2 — Inmutabilidad, estados y ajuste manual (Priority: P2)

**Goal**: cambiar la pauta cierra la fila anterior y abre una nueva sin perder historial; campos no
clínicos se editan en el sitio; estados administrativos y ajuste manual de unidades (E3, E4, E6).

**Independent Test**: cambiar la pauta de un tratamiento activo y comprobar que la fila original
queda `Finalizado` con `fecha_fin` = hoy y existe una nueva `Activo` (CA-401); revisar el historial
completo (CA-402); fijar y limpiar `ajuste_unidades_manual` (CA-405 parcial).

### Tests for User Story 2

- [X] T017 [P] [US2] Test: `CambiarPauta` cierra la fila original (`Finalizado`, `fecha_fin` = hoy) y crea una nueva `Activo` con `fecha_inicio` = hoy, heredando `fecha_prescripcion_inicial` (CA-401)
- [X] T018 [P] [US2] Test: `ListarHistorialDeMedicamento` devuelve las N versiones de un medicamento para un paciente, ninguna oculta (CA-402)
- [X] T019 [P] [US2] Test: `ActualizarCamposNoClinicos` no cierra la fila (observaciones/incidencias/ajuste manual) — FR-411
- [X] T020 [P] [US2] Test: `CambiarEstado` de Suspendido a Activo no cierra/abre fila (caso límite, sin cambio clínico)
- [X] T021 [P] [US2] Test: `ActualizarCamposNoClinicos` puede fijar `ajuste_unidades_manual` y volver a limpiarlo a `null` (CA-405)
- [X] T022 [P] [US2] Test: `CambiarPauta`/`ActualizarCamposNoClinicos`/`CambiarEstado` registran en auditoría (Art. VII.6)

### Implementation for User Story 2

- [X] T023 [US2] Implementar `CambiarPauta`/`ActualizarCamposNoClinicos`/`CambiarEstado`/`ListarHistorialDeMedicamento` en `ServicioTratamientos` (depende de T013)
- [X] T024 [US2] Añadir a `TratamientoViewModel`/vista: botón "Cambiar pauta" (precarga el formulario con la versión seleccionada) y campo de ajuste manual con "usar cálculo automático" (vía `ActualizarCamposNoClinicos`, sin botón dedicado todavía en la vista)
- [X] T025 [P] [US2] Vista de línea temporal del historial (`ListarHistorialDeMedicamento` ya implementado y testeado a nivel de servicio — CA-402 — pero sin pantalla propia todavía; pendiente de una sesión posterior) + su test Avalonia.Headless

**Checkpoint**: US1 + US2 funcionales de forma independiente.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [X] T026 [P] Ejecutar íntegramente [quickstart.md](./quickstart.md) y registrar el resultado en `PROGRESO.md`
- [X] T027 Revisar que ningún método de `Spd.Dominio`/`Spd.Aplicacion` supere ~40 líneas ni ninguna clase ~300 (Art. XI.6)
- [X] T028 [P] Actualizar `PROGRESO.md` marcando cada CA-400..403/405(parcial) como validado, dejando constancia de que CA-404/406 y el cálculo automático de FR-430 siguen diferidos

## Dependencies & Execution Order

- **Foundational (Fase 1)**: sin dependencias — bloquea las 2 user stories
- **US1 (Fase 2)**: depende de Foundational
- **US2 (Fase 3)**: depende de Foundational y de T013 (US1) para reutilizar `ServicioTratamientos`
- **Polish (Fase 4)**: depende de las user stories que se quieran dar por completas

## Implementation Strategy

1. Fase 1 (Foundational)
2. Fase 2 (US1) — parar y validar CA-400/403 antes de seguir
3. Fase 3 (US2), validando CA-401/402/405 en el checkpoint
4. Fase 4 (Polish) al final
