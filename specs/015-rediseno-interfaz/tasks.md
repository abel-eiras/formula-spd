# Tasks: Rediseño de la interfaz — una sola ventana

**Input**: Design documents from `specs/015-rediseno-interfaz/`

**Prerequisites**: [plan.md](./plan.md) (hitos y pruebas de aceptación por fase), [spec.md](./spec.md),
[research.md](./research.md), [contracts/navegacion-y-tema.md](./contracts/navegacion-y-tema.md)

**Tests**: incluidos en cada fase. Ninguna fase se da por terminada con tests en rojo (FR-1541).

| Fase | Story | Prioridad | FR | CA |
|---|---|---|---|---|
| 1 | US5 | P1 | 4.1 Marco único y sistema visual | CA-1500..1504 |
| 2 | US1, US3 | P1 | 4.2 Inicio y tablas | CA-1510..1513 |
| 3 | US2 | P1 | 4.3 Espacio del paciente | CA-1520..1523 |
| 4 | US4 | P2 | 4.4 Preparación como proceso | CA-1530..1533 |

## Format: `[ID] [P?] [Fase] Description`

`[P]` = puede hacerse en paralelo con otra tarea `[P]` de la misma fase (ficheros distintos).

---

## Fase 1 — Marco único y sistema visual

### Tema y componentes (H1.1, H1.2)

- [X] T001 [F1] Descargar e incorporar IBM Plex Sans e IBM Plex Mono (SIL OFL 1.1) a `src/Spd.Presentacion/Assets/Fuentes/` y declararlas en el csproj como `AvaloniaResource`
- [X] T002 [F1] `src/Spd.Presentacion/Estilos/Paleta.axaml`: las 14 claves de color de `data-model.md` en variante clara y oscura (`ResourceDictionary.ThemeDictionaries`)
- [ ] T003 [P] [F1] `src/Spd.Presentacion/Estilos/Tipografia.axaml`: `FuenteInterfaz`, `FuenteDatos` y la escala tipográfica (11/13/15/18/22)
- [ ] T004 [P] [F1] `src/Spd.Presentacion/Estilos/Controles.axaml`: estilos de `Button` (primario, secundario, enlace), `TextBox`, `ComboBox`, `CheckBox`, `ListBox`, `TabControl`, `Separator`
- [X] T005 [F1] Aplicar los tres diccionarios en `src/Spd.Presentacion/App.axaml`, después de `FluentTheme`
- [ ] T006 [P] [F1] `src/Spd.Presentacion/Controles/Pastilla.axaml(.cs)` con las cinco variantes
- [ ] T007 [P] [F1] `src/Spd.Presentacion/Controles/FranjaSeveridad.axaml(.cs)`
- [ ] T008 [P] [F1] `src/Spd.Presentacion/Controles/BarraMensaje.axaml(.cs)`: mensaje + "¿Por qué?" que abre el apartado indicado (sustituye el `Border` + `Button` repetido hoy en Preparación e Idoneidad)
- [ ] T009 [P] [F1] Test `tests/Spd.Presentacion.Tests/TemaTests.cs`: toda clave de color existe en ambas variantes; las dos fuentes embebidas se resuelven

### Marco y navegación (H1.3, H1.4)

- [X] T010 [F1] `src/Spd.Presentacion/Navegacion/Seccion.cs`, `Destino.cs`, `EntradaNavegacion.cs`, `Navegador.cs` según `contracts/navegacion-y-tema.md`
- [X] T011 [F1] `src/Spd.Presentacion/FabricaViewModels.cs`: recibe los servicios una vez y construye cada ViewModel a demanda
- [X] T012 [F1] `src/Spd.Presentacion/Views/AppShell.axaml(.cs)` y `ViewModels/AppShellViewModel.cs`: navegación lateral con grupos, contadores, usuario y cierre de sesión; región de contenido resuelta por el `ViewLocator` existente
- [X] T013 [F1] `src/Spd.Presentacion/App.axaml.cs`: al iniciar sesión se abre `AppShell` en lugar de `MainWindow`; se conserva el backup de cierre y el apagado explícito
- [X] T014 [F1] Contadores de la navegación desde `IServicioAvisosInicio` (retirada y preparaciones)

### Migración de secciones (H1.5)

- [X] T015 [F1] Migrar a la región de contenido y **eliminar** `BuscadorPacientesWindow`, `PreparacionesWindow`, `RetiradaEnvasesWindow`, `ExportarPacientesWindow`
- [X] T016 [F1] Ídem `CatalogoMedicamentosWindow`, `RevisionNomenclatorWindow`, `RegistrosCalidadWindow`, `ControlDocumentalWindow`
- [X] T017 [F1] Ídem `FarmaciaWindow`, `UsuariosWindow`, `ActualizacionesWindow`, `NomenclatorWindow`, `SeguridadWindow`, `PerfilesImportacionWindow`
- [X] T018 [F1] `AyudaWindow` pasa a sección del marco; `AyudaWindow.Abrir` se sustituye por `Navegador.Navegar(Seccion.Ayuda, apartado)`
- [X] T019 [F1] **Eliminar** `MainWindow.axaml(.cs)` y `MainViewModel` (sus comandos pasan a la navegación); conservar `AvisoWindow` como diálogo modal

### Ayuda y tests (H1.6)

- [X] T020 [F1] `AyudaContextual`: tablas por **vista** en vez de por ventana; F1 resuelto sobre la vista activa del shell
- [X] T021 [F1] `tests/Spd.Presentacion.Tests/AnfitrionDeVista.cs`: `Mostrar(Control vista)` crea ventana anfitriona, asigna contenido y muestra (conserva la regresión F5)
- [X] T022 [F1] Migrar los 24 ficheros de test de Presentación al anfitrión; actualizar `IndiceAyudaTests` a las tablas por vista
- [X] T023 [F1] Tests `tests/Spd.Presentacion.Tests/AppShellTests.cs`: navegación a cada sección, visibilidad por rol, contadores
- [X] T024 [F1] `dotnet build` + `dotnet test` completos; actualizar `PROGRESO.md`; fusionar a `main`

---

## Fase 2 — Inicio accionable, tablas y búsqueda

- [X] T025 [F2] `AvisoInicio` gana `Destino` en `src/Spd.Aplicacion/IServicioAvisosInicio.cs` y `ServicioAvisosInicio.cs` (sección + paciente + pestaña)
- [X] T026 [F2] `ViewModels/InicioViewModel.cs` y `Views/InicioView.axaml(.cs)`: cuatro indicadores + lista de avisos accionables + acciones rápidas
- [ ] T027 [P] [F2] Test `tests/Spd.Aplicacion.Tests/ServicioAvisosInicioTests.cs` (ampliado): cada aviso lleva su destino
- [ ] T028 [P] [F2] Test `tests/Spd.Presentacion.Tests/InicioViewTests.cs`: la vista se realiza con los cuatro tipos de aviso
- [X] T029 [F2] Decidir Q4 y, según la respuesta, añadir `Avalonia.Controls.DataGrid` 11.3.20 + su tema, o implementar la alternativa de `ItemsControl` con anchos compartidos (research.md Decisión 4)
- [X] T030 [F2] `Estilos/Tabla.axaml` y conversión de `PreparacionesView` a tabla ordenable con selección múltiple
- [ ] T031 [P] [F2] Conversión de `RetiradaEnvasesView` a tabla
- [ ] T032 [P] [F2] Conversión de `BuscadorPacientesView` a tabla
- [ ] T033 [P] [F2] Conversión de `CatalogoMedicamentosView` a tabla
- [X] T034 [F2] `src/Spd.Aplicacion/IServicioBusquedaGlobal.cs` + `ServicioBusquedaGlobal.cs`: pacientes, medicamentos y blísteres, normalizados
- [X] T035 [F2] Búsqueda global en la cabecera del shell (`AppShellViewModel`), con navegación al resultado
- [ ] T036 [P] [F2] Tests `ServicioBusquedaGlobalTests` y `PreparacionesViewModelTests.Ordenar_conserva_la_seleccion`
- [X] T037 [F2] `dotnet build` + `dotnet test`; `PROGRESO.md`; fusionar a `main`

---

## Fase 3 — Espacio del paciente

- [ ] T038 [F3] `src/Spd.Presentacion/Pacientes/PacienteContexto.cs` (research.md Decisión 5)
- [ ] T039 [F3] `Views/Pacientes/CabeceraPacienteView.axaml(.cs)`: estado, alergias, idoneidad y consentimiento, retirada, blísteres, médico
- [ ] T040 [F3] `ViewModels/PacienteWorkspaceViewModel.cs` + `Views/Pacientes/PacienteWorkspaceView.axaml(.cs)`: cabecera fija + siete pestañas alojando las vistas existentes
- [ ] T041 [F3] Indicadores de pendiente por pestaña (idoneidad, depósito, preparación, comunicaciones)
- [ ] T042 [F3] `Controles/PanelLateral.axaml(.cs)`
- [ ] T043 [F3] Migrar a panel lateral: registrar envase (Preparación), alta de representante (Idoneidad), respuesta del médico (Comunicaciones)
- [ ] T044 [F3] Migrar a panel lateral: importar tratamiento (Depósito), revocación (Idoneidad), reelaboración (Preparación)
- [ ] T045 [F3] Todos los ViewModels de pestaña usan `PacienteContexto`; eliminar el parche `FichaPacienteViewModel.RecargarPaciente` y los `Closed +=`
- [ ] T046 [F3] **Eliminar** `FichaPacienteWindow`, `TratamientoWindow`, `DepositoWindow`, `ComunicacionesMedicoWindow`, `IdoneidadConsentimientoWindow`, `PreparacionWindow`, `ImportarTratamientoWindow`
- [ ] T047 [P] [F3] Tests `PacienteWorkspaceViewTests` (siete pestañas), `PacienteContextoTests` (activación en el momento), `PanelLateralTests`
- [ ] T048 [P] [F3] Ayuda (Spec 014 FR-1525): reescribir `uso__030-ficha-paciente`, `uso__040-idoneidad-consentimiento`, `uso__050-tratamientos`, `uso__060-deposito`, `uso__070-preparacion`, `uso__080-comunicaciones-medico` a pestañas y paneles; `uso__010-inicio` al marco único
- [ ] T049 [F3] `dotnet build` + `dotnet test`; `PROGRESO.md`; fusionar a `main`

---

## Fase 4 — Preparación como carril y blíster dibujado

- [ ] T050 [F4] Confirmar Q2 (tomas por día y días por blíster) con el propietario
- [ ] T051 [F4] `ViewModels/PasoPreparacion.cs` + `Controles/CarrilPasos.axaml(.cs)`: cinco pasos derivados del estado del SPD, con motivo de bloqueo
- [ ] T052 [P] [F4] Test `CarrilPasosTests`: paso actual y bloqueos por cada estado del SPD; motivos correctos
- [ ] T053 [F4] `Controles/RejillaAlveolos.axaml(.cs)`: días × tomas desde las líneas del SPD, fracción con `FraccionDosis.Texto()`, inicial del medicamento
- [ ] T054 [P] [F4] Test `RejillaAlveolosTests`: pauta completa, pauta con días parciales, dos líneas en el mismo alvéolo
- [ ] T055 [F4] `PreparacionView` reorganizada sobre el carril: llenado, verificación y entrega como paneles del paso
- [ ] T056 [F4] Verificación con selector de **verificador por nombre** (usuarios activos) y las ocho preguntas del Anexo I.G con su texto
- [ ] T057 [P] [F4] Selector de médico por nombre en `TratamientoView` y `ComunicacionesMedicoView` (sustituye el campo numérico de id)
- [ ] T058 [P] [F4] Test `SinIdentificadoresEnLaInterfazTests`: ningún `.axaml` enlaza un control numérico a un `*Id` de usuario o médico
- [ ] T059 [F4] Impresión (ficha, etiquetas, instrucciones) ofrecida desde el paso correspondiente del carril
- [ ] T060 [F4] `dotnet build` + `dotnet test`; `PROGRESO.md`; fusionar a `main`
- [ ] T061 [F4] **Prueba manual del ciclo completo por el propietario** (quickstart.md), pendiente desde el inicio del proyecto

---

## Dependencias entre fases

- Fase 1 es prerrequisito de todas: aporta marco, tema y componentes.
- Fase 2 depende de F1 (cabecera del shell para la búsqueda; región para el inicio).
- Fase 3 depende de F1 (panel lateral y componentes) pero **no** de F2.
- Fase 4 depende de F3 (la preparación es una pestaña del espacio del paciente).

## Estrategia de implementación

MVP = **fase 1**: por sí sola cambia la impresión de toda la aplicación y elimina 14 de las 27 ventanas.
Cada fase siguiente se fusiona por separado tras su prueba manual (plan.md, "Secuencia recomendada").
