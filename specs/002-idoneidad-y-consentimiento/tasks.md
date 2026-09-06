# Tasks: Idoneidad y consentimiento informado

**Input**: Design documents from `specs/002-idoneidad-y-consentimiento/`

**Tests**: Incluidos (Art. IX.1 para la regla de Dominio; auditoría explícita).

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 | 4.1 Evaluación | CA-201, 204, 205 |
| US2/US3 | P1 | 4.2 Consentimiento | CA-200, 202 |
| US4/US5 | P2 | Revocación / reevaluación | CA-203, 204 |

## Phase 1: Foundational

- [X] T001 Migración `src/Spd.Infraestructura/Migraciones/0011_idoneidad_consentimiento.sql`
- [X] T002 [P] Dominio: `ResultadoIdoneidad`, `TipoConsentimiento`, `EvaluacionIdoneidad` (con `ResultadoPropuesto`, `RequiereObservaciones`), `Consentimiento` (`Vigente`), interfaces de repositorio, `ComprobadorIdoneidadYConsentimientoReal`
- [X] T003 [P] Test Dominio: `EvaluacionIdoneidadTests` (propuesta, observaciones y vigencia del consentimiento)
- [X] T004 Infraestructura: `RepositorioEvaluacionesIdoneidad`, `RepositorioConsentimientos` (Dapper, `Fila` explícita)
- [X] T005 Test: `InfraestructuraIdoneidadFundamentosTests` (migración y ida-vuelta)

## Phase 2: US1–US5 — Servicio de aplicación

- [X] T006 `DatosEvaluacionIdoneidad`, `DatosContactoRepresentante`, `EstadoIdoneidadConsentimiento`, `ResultadoEvaluacion`, `ResultadoConsentimiento`, `IServicioIdoneidadConsentimiento` en `src/Spd.Aplicacion/`
- [X] T007 `ServicioIdoneidadConsentimiento` (FR-200..215, research.md Decisiones 1/4/5)
- [X] T008 Tests `ServicioIdoneidadConsentimientoTests` (CA-200, 202, 203, 204, 205, FR-211, FR-215, firma futura) — CA-201 sobre el comprobador real dentro del mismo fichero

## Phase 3: Documentos (Spec 007)

- [X] T009 `GenerarConsentimiento` (Anexo I.B) en `ServicioGeneracionDocumentos` + evaluación vigente en `GenerarFichaPaciente`; tests de presencia de elementos

## Phase 4: Presentación

- [X] T010 `IdoneidadConsentimientoViewModel` / `View` / `Window` en `src/Spd.Presentacion/`
- [X] T011 Botón "Idoneidad y consentimiento" en `FichaPacienteView`; cascada `FichaPacienteWindow` → `BuscadorPacientes*` → `MainViewModel` → `App.axaml.cs`; comprobador real en `App.axaml.cs`
- [X] T012 Test Avalonia.Headless `IdoneidadConsentimientoViewTests`

## Phase 5: Polish

- [X] T013 `dotnet build` + `dotnet test` completos; `docs/data-model.md`; PROGRESO.md
