# Tasks: Comunicaciones al médico

**Input**: Design documents from `specs/008-comunicaciones-medico/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos, con auditoría (Art. VII.6) como test explícito desde el diseño.

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 (MVP) | 4 Presentación | CA-800 |
| US2 | P1 | 4 Incidencia y respuesta | CA-801, 802 |
| US3 | P2 | 4 Creación prerrellenada | CA-803 (adaptado), CA-804 |

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Foundational (Blocking Prerequisites)

- [X] T001 Crear script `src/Spd.Infraestructura/Migraciones/0007_comunicaciones_medico.sql` con tabla `ComunicacionMedico` (data-model.md)
- [X] T002 [P] Crear enum `TipoComunicacionMedico` en `src/Spd.Dominio/TipoComunicacionMedico.cs`
- [X] T003 [P] Crear entidad `ComunicacionMedico` en `src/Spd.Dominio/ComunicacionMedico.cs`, con propiedad calculada `EsImprimible` (research.md Decisión 3)
- [X] T004 [P] Definir `IRepositorioComunicacionesMedico` en `src/Spd.Dominio/IRepositorioComunicacionesMedico.cs`
- [X] T005 Implementar `RepositorioComunicacionesMedico` en `src/Spd.Infraestructura/RepositorioComunicacionesMedico.cs` (depende de T001, T003, T004)
- [X] T006 [P] Test de humo: migración 0007 idempotente, en `tests/Spd.Aplicacion.Tests/InfraestructuraComunicacionesMedicoFundamentosTests.cs`

**Checkpoint**: esquema y entidad listos.

---

## Phase 2: User Story 1 — Presentación prerrellenada (Priority: P1) 🎯 MVP

- [X] T007 [P] [US1] Test: `Crear` con `Tipo = Presentacion` y `MedicoId = null` prerrellena el médico de cabecera del paciente (CA-800), en `tests/Spd.Aplicacion.Tests/ServicioComunicacionesMedicoTests.cs`
- [X] T008 [P] [US1] Test: `Crear` registra en auditoría (Art. VII.6)
- [X] T009 [US1] Crear `DatosAltaComunicacionMedico` en `src/Spd.Aplicacion/DatosAltaComunicacionMedico.cs`
- [X] T010 [US1] Implementar `IServicioComunicacionesMedico`/`ServicioComunicacionesMedico.Crear`/`ListarDePaciente` en `src/Spd.Aplicacion/` (depende de T005, T009)

**Checkpoint**: US1 funcional de forma independiente.

---

## Phase 3: User Story 2 — Incidencia y respuesta (Priority: P1)

- [X] T011 [P] [US2] Test: `Crear` con `Tipo = Incidencia` exige `IncidenciasDetectadas` y `Propuesta` (CA-801)
- [X] T012 [P] [US2] Test: `RegistrarRespuesta` añade respuesta/fecha sin cambiar `Fecha` de creación (CA-802)
- [X] T013 [P] [US2] Test: comunicación `Telefono` no es imprimible; `Presentacion`/`Incidencia` sí (CA-804, sobre la propiedad `EsImprimible`)
- [X] T014 [US2] Implementar `RegistrarRespuesta` en `ServicioComunicacionesMedico` (depende de T010)

**Checkpoint**: US1 + US2 dan valor real (alta + ciclo de vida completo salvo borrado).

---

## Phase 4: User Story 3 — Creación prerrellenada (Priority: P2)

- [X] T015 [P] [US3] Test: `PrepararDesdeTratamiento` fija paciente y médico prescriptor del tratamiento sin guardar nada (FR-804), en `tests/Spd.Aplicacion.Tests/ServicioComunicacionesMedicoTests.cs`
- [X] T016 [P] [US3] Test: `PrepararDesdeAvisoCambioReferido` fija paciente y médico de cabecera con incidencias vacío, sin guardar nada (CA-803 adaptado)
- [X] T017 [US3] Implementar `PrepararDesdeTratamiento`/`PrepararDesdeAvisoCambioReferido` en `ServicioComunicacionesMedico` (depende de T010, `IRepositorioTratamientos` de Spec 004)
- [X] T018 [US3] Añadir botón "Comunicar incidencia" en `TratamientoView`/`TratamientoViewModel` (Spec 004) en `src/Spd.Presentacion/Views/Pacientes/`
- [X] T019 [US3] Implementar `ComunicacionesMedicoViewModel`/vista del paciente (listado + alta + respuesta) en `src/Spd.Presentacion/Views/Pacientes/`
- [X] T020 [P] [US3] Test Avalonia.Headless: la vista de comunicaciones construye y muestra sin lanzar, en `tests/Spd.Presentacion.Tests/ComunicacionesMedicoViewTests.cs`

**Checkpoint**: todas las user stories completas.

---

## Phase 5: Polish

- [X] T021 Ejecutar `dotnet build` y `dotnet test` completos sobre la rama; corregir regresiones en `MainWindow`/`FichaPacienteView`/`TratamientoView`/tests de otras specs si la nueva wiring los toca
