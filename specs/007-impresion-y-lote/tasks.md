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

---

## Phase 6: Corrección 2026-09-06 — elementos mínimos de cada anexo (Art. I.2) y documento RGPD

Origen: `docs/analisis-resources.md` §2.3/§2.4/§2.8 y decisión del propietario (solo `RGPD` como documento nuevo).

- [X] T023 Migración `0010_documentos_minimos_pnt.sql`: `Farmacia.dpo_nombre/dpo_contacto`; `SPD_Verificacion` +3 preguntas del Anexo I.G
- [X] T024 `ChecklistVerificacion` con las 8 preguntas del Anexo I.G; `SpdVerificacion`, repositorio, `ServicioPreparacion.Verificar`, `PreparacionView`/ViewModel (trazabilidad e instrucciones pre-marcadas por la app)
- [X] T025 `FICHA` (Anexo I.G): CN, lote, caducidad, serie por envase, material y lote, temperatura/humedad, control de adherencia, 8 preguntas de verificación, elaborado/verificado/entregado por con fecha, leyenda D/A/C/N
- [X] T026 `ETQ-A`/`ETQ-R` (Anexo I.F): dirección y teléfono de la farmacia, fecha de preparación, teléfono del paciente, "recuerde que además hay que administrar" (tratamientos no incluidos), CN/posología/aspectos físicos por envase, advertencias literales
- [X] T027 `INSTR` (Anexo I.H): cabecera de farmacia, nº registro y fecha, CN, médico prescriptor, fecha de prescripción/última modificación, tabla de medicamentos no incluidos, advertencias literales
- [X] T028 `FICHA-PAC` (Anexo I.E): fecha, Nº SS, teléfonos, e-mail, familiar/cuidador (contacto principal), médico de familia, observaciones, bloque de evaluación de idoneidad (a mano hasta Spec 002), tablas de medicamentos incluidos/no incluidos con problema de salud, prescriptor, fechas, PRM/RNM e intervención, control de adherencia desde las entregas
- [X] T029 `RGPD` (Anexo I.D): `GenerarInformacionProteccionDatos`, texto literal con responsable, plazos, base jurídica, derechos y DPO; botón en `FichaPacienteView`; campos de protección de datos en `FarmaciaView`
- [X] T030 Tests de presencia de elementos por documento (PdfPig extrae el texto del PDF generado) en `ServicioGeneracionDocumentosTests`
