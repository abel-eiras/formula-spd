# Tasks: Ayuda de la aplicación y guía de procedimiento

**Input**: Design documents from `specs/014-ayuda-y-procedimiento/`

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 | 4.1/4.2 Procedimiento completo | CA-1400 |
| US2 | P1 | F1 contextual | CA-1401 |
| US3 | P2 | Checklist de inspección | CA-1403 |
| US4 | P2 | "¿Por qué?" | CA-1402 |

## Phase 1: Foundational

- [X] T001 `src/Spd.Presentacion/Spd.Presentacion.csproj`: `EmbeddedResource Ayuda\*.md`
- [X] T002 [P] `Ayuda/CatalogoAyuda.cs` (carga de recursos, `EntradaAyuda`) e `IndiceAyuda` (`Buscar`, `Obtener`)
- [X] T003 [P] `Ayuda/RenderizadorMarkdown.cs` (títulos, párrafos, listas, negrita, enlaces `[[seccion:id]]`)
- [X] T004 Tests `IndiceAyudaTests`: carga, búsqueda "verificador" (CA-1404), ids referenciados existen, toda ventana registrada tiene apartado de Uso (FR-1412)

## Phase 2: US1/US2 — Ventana y F1

- [X] T005 `AyudaViewModel`, `AyudaView.axaml(.cs)`, `AyudaWindow.axaml(.cs)` con `Abrir(seccion, id)`
- [X] T006 `AyudaContextual.Registrar(this)` en cada ventana posterior al inicio de sesión; botón "Ayuda" en `MainWindow`
- [X] T007 Test headless `AyudaWindowTests`

## Phase 3: US4 — "¿Por qué?"

- [X] T008 Botón "¿Por qué? (F1)" junto a la barra de mensaje en `PreparacionView` e `IdoneidadConsentimientoView`; apartado `porque-de-los-bloqueos`

## Phase 4: Contenido

- [X] T009 13 apartados de Procedimiento (`Ayuda/procedimiento__*.md`) con los bloques *Qué exige el PNT / Qué hago en la aplicación / Qué pasa si se omite*
- [X] T010 13 apartados de Uso (`Ayuda/uso__*.md`), uno por pantalla o grupo de pantallas
- [X] T011 Apartado `documentacion-y-conservacion` con la lista de comprobación por momento del servicio (FR-1403)

## Phase 5: Polish

- [X] T012 `dotnet build` + `dotnet test`; PROGRESO.md
