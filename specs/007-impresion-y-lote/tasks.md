# Tasks: Impresión, generación en lote y documentación base

**Input**: Design documents from `specs/007-impresion-y-lote/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos, con auditoría (Art. VII.6) como test explícito desde el diseño.

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 (MVP) | 4.1/4.5 Ficha/etiquetas/instrucciones de SPD | CA-709, 710 |
| US2 | P1 | 4.1 Ficha del paciente | — |
| US3 | P1 | 4.2 Nombre de fichero | CA-700, 701 |

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Foundational (Blocking Prerequisites)

- [X] T001 Añadir paquete `QuestPDF` a `src/Spd.Infraestructura/Spd.Infraestructura.csproj`
- [X] T002 Declarar `QuestPDF.Settings.License = LicenseType.Community` en `src/Spd.Presentacion/Program.cs` (research.md Decisión 1)
- [X] T003 [P] Implementar `GeneradorNombreFichero` en `src/Spd.Dominio/GeneradorNombreFichero.cs`
- [X] T004 [P] Test: `Generar` devuelve el nombre largo por defecto (CA-700), en `tests/Spd.Dominio.Tests/GeneradorNombreFicheroTests.cs`
- [X] T005 [P] Test: `Generar` usa el nombre corto si supera 120 caracteres (CA-701)
- [X] T006 [P] Test: `Generar` usa el nombre corto si el paciente no tiene apellidos (research.md Decisión 3)
- [X] T007 [P] Test: `Generar` usa el nombre corto si colisiona con un nombre ya usado en el lote

**Checkpoint**: nombrado de fichero probado, listo para el motor de generación.

---

## Phase 2: User Story 3 — Motor de generación (nombrado, carpeta, auditoría) (Priority: P1) 🎯 base

- [X] T008 [US3] Crear `TipoDocumentoGenerado`, `ResultadoGeneracionDocumento` en `src/Spd.Aplicacion/`
- [X] T009 [US3] Definir `IServicioGeneracionDocumentos` en `src/Spd.Aplicacion/IServicioGeneracionDocumentos.cs`
- [X] T010 [US3] Implementar `ServicioGeneracionDocumentos` con el método privado común `GuardarDocumento` (research.md Decisión 2) en `src/Spd.Infraestructura/ServicioGeneracionDocumentos.cs`

**Checkpoint**: mecanismo común listo; falta el contenido de cada documento.

---

## Phase 3: User Story 1 — Ficha, etiquetas e instrucciones de un SPD (Priority: P1) 🎯 MVP

### Tests for User Story 1

- [X] T011 [P] [US1] Test: `GenerarFichaSpd` crea el fichero, audita, y usa fracción para la posología (CA-709/710), en `tests/Spd.Aplicacion.Tests/ServicioGeneracionDocumentosTests.cs`
- [X] T012 [P] [US1] Test: `GenerarEtiquetaAnverso`/`GenerarEtiquetaReverso` crean su fichero y audita
- [X] T013 [P] [US1] Test: `GenerarInstrucciones` crea su fichero y audita
- [X] T014 [P] [US1] Test: `GenerarFichaSpd` con una línea de dos filas de envase (Spec 006 CA-602) incluye ambas filas en el contenido

### Implementation for User Story 1

- [X] T015 [US1] Implementar `GenerarFichaSpd` en `ServicioGeneracionDocumentos` (contenido con QuestPDF)
- [X] T016 [US1] Implementar `GenerarEtiquetaAnverso`/`GenerarEtiquetaReverso`
- [X] T017 [US1] Implementar `GenerarInstrucciones`
- [X] T018 [US1] Añadir botones "Imprimir ficha/etiquetas/instrucciones" en `PreparacionView`/`PreparacionViewModel`, llamando primero al servicio de generación y después a `RegistrarImpresion` (Spec 006)

**Checkpoint**: US1 funcional; ciclo de preparación tiene documentación real.

---

## Phase 4: User Story 2 — Ficha del paciente (Priority: P1)

- [X] T019 [P] [US2] Test: `GenerarFichaPaciente` crea el fichero con los datos del paciente y audita, en `tests/Spd.Aplicacion.Tests/ServicioGeneracionDocumentosTests.cs`
- [X] T020 [US2] Implementar `GenerarFichaPaciente`
- [X] T021 [US2] Añadir botón "Imprimir ficha" en `FichaPacienteView`/`FichaPacienteViewModel`

**Checkpoint**: todas las user stories de esta iteración completas.

---

## Phase 5: Polish

- [X] T022 Ejecutar `dotnet build` y `dotnet test` completos sobre la rama; corregir regresiones en `MainWindow`/tests de otras specs si la nueva wiring los toca
