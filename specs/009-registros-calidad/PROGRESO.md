# Plan y progreso — Spec 009: Registros de calidad

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: rama independiente (`009-registros-calidad`, creada desde `main`) mientras
las Specs 001 y 003 esperan verificación manual del usuario. Spec 009 no depende funcionalmente de
ninguna de las dos (solo de Spec 000, ya mergeada) — el usuario pidió continuar con desarrollo que
no requiera su intervención.

**Nota sobre `/speckit-clarify`**: la única pregunta de esta spec (Q1) tiene una propuesta por
defecto documentada en la spec original. Siguiendo la instrucción explícita del usuario de avanzar
sin necesitar su intervención, se resuelve adoptando esa propuesta directamente en vez de esperar
respuesta, y se deja registrado aquí como una decisión autónoma, no como una respuesta del usuario.

## Fases del flujo Spec Kit

| # | Fase | Comando | Estado | Commit |
|---|---|---|---|---|
| 1 | Especificación | `/speckit-specify` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 2 | Aclaración | `/speckit-clarify` | ✅ Hecho — 2026-09-05 (Q1 resuelta de forma autónoma) | *(pendiente de commit)* |
| 3 | Plan de implementación | `/speckit-plan` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 4 | Desglose de tareas | `/speckit-tasks` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 5 | Análisis de coherencia | `/speckit-analyze` | ✅ Hecho — 2026-09-05 (sin hallazgos) | *(pendiente de commit)* |
| 6 | Implementación | `/speckit-implement` | ✅ Hecho — 2026-09-05 (pendiente prueba manual del usuario) | *(pendiente de commit)* |

## Criterios de aceptación de la spec y su tipo de test previsto

Formato Dado/Cuando/Entonces (Constitución Art. IX.2). El tipo de test se confirma o ajusta en la
fase de `/speckit-plan`.

| CA | Descripción | Capa / tipo de test previsto |
|---|---|---|
| CA-900 | Ambiental fuera de rango se marca al registrar, no se recalcula si cambia la configuración | ✅ `RegistrarAmbiental_marca_fuera_de_rango_...CA_900` + `..._conserva_fuera_de_rango_...CA_900` |
| CA-901 | Limpieza de un clic con fecha/hora y usuario actual | ✅ `RegistrarLimpieza_guarda_con_fecha_y_usuario_actuales_...CA_901` |
| CA-902 | Formación acumulativa, ninguna sustituye a otra | ✅ `RegistrarFormacion_tres_veces_deja_las_tres_consultables_CA_902` |
| CA-903 | Control de cambios del PNT solo Administrador | ✅ `RegistrarCambioPnt_lanza_...CA_903`, `ListarCambiosPnt_lanza_...CA_903`, `RegistrarCopia_y_ListarCopias_...CA_903` |
| CA-904 | Aviso de registro atrasado en el panel de inicio | ✅ `ComprobarAvisos_detecta_atraso_...CA_904` + `..._no_avisa_...` |

## Invariantes de constitución que aplican a esta spec

- Art. I.3 (invariantes del PNT): "toda preparación registra temperatura y humedad" (RegistroAmbiental
  es la entidad que lo soporta, aunque su vínculo directo con una preparación es de Spec 006).
- Art. III (nada se borra): FR-921 (formación acumulativa, nunca se sustituye), duplicados
  ambientales no se eliminan (§7 casos límite).
- Art. IV (catálogo vivo, historia congelada): FR-901 es el ejemplo de instantánea de esta spec —
  `fuera_rango` se congela con los rangos vigentes en el momento del registro, no se recalcula si
  cambia `Farmacia.temp_min/max`.
- Art. VII (seguridad y acceso): FR-942 (Control de cambios/copias solo Administrador); FR-920 sin
  distinción de categoría profesional.
- Art. IX (calidad): CA-900..CA-904 se traducen en tests antes de darse por completado.

## Registro de avance

- **2026-09-05** — Fase 1 completada. `spec.md` trasladado literalmente desde
  `spec-009-registros-calidad.md`, con Q1 (umbral de días del aviso) marcada
  `[NEEDS CLARIFICATION]`. Checklist de calidad generado; todo pasa salvo "sin NEEDS
  CLARIFICATION", bloqueado a propósito hasta la fase 2.
- **2026-09-05** — Fase 2 completada. Q1 resuelta de forma **autónoma** (sin plantearla al
  usuario): se adopta la propuesta por defecto de la spec original, 7 días como umbral único para
  ambos avisos (ambiental y limpieza rutinaria). Esto se aparta del patrón habitual de Specs
  000/001/003 (donde cada pregunta se planteó una a una); el motivo es la instrucción explícita del
  usuario de seguir avanzando en desarrollo que no requiera su intervención. `spec.md` actualizado
  con sección "Clarifications" y §9 sin marcadores pendientes. Checklist de calidad al 100 %.
- **2026-09-05** — Fase 3 completada. `plan.md` (Technical Context + Constitution Check con 9
  artículos), `research.md` (4 decisiones: `FormacionPersonal` sin distinción de categoría
  profesional —corrige una discrepancia real con el documento de Fase 1, que quedó desactualizado
  tras la enmienda 2.0.0 de la constitución—, cálculo congelado de `fuera_rango`, umbral único
  configurable en `Farmacia.umbral_dias_aviso_calidad`, y comprobación de rol en la capa de
  Aplicación para Control documental). `data-model.md` (recorte de las 6 entidades + adición a
  Farmacia), `contracts/servicios-aplicacion.md` (2 interfaces separadas por permisos),
  `quickstart.md`. Misma nota de coordinación de numeración de migración ya dejada en Specs 001 y
  003 (las tres ramas parten de `main` y numeran su propia migración `0002`).
- **2026-09-05** — Fase 4 completada. `tasks.md` generado: Foundational (T001-T014: migración
  0002 + ALTER TABLE Farmacia, 6 entidades, 2 repositorios, tests de humo) y 4 user stories — US1
  Ambiental+Limpieza P1/MVP (T015-T023), US2 Formación+Residuos P2 (T024-T030), US3 Control
  documental P2 (T031-T038), US4 Avisos P3 (T039-T042, depende de US1) — más Polish (T043-T045).
  45 tareas en total. Auditoría y test Avalonia.Headless de regresión incluidos desde el diseño.
- **2026-09-05** — Fase 5 completada. Sin hallazgos: los 5 CA están cubiertos por tests
  dedicados, la auditoría se verificó entidad por entidad (evitando el hueco de Spec 003), y el
  diseño no expone ningún método de actualización/eliminación en los repositorios — todas las
  entidades son de solo alta, lo que satisface el Art. III sin necesitar un test dedicado. 100 %
  de cobertura FR→tarea.
- **2026-09-05** — Fase 6 (Implementación) iniciada. Foundational (T001-T014) completada: migración
  `0002_registros_calidad.sql` (6 tablas + `ALTER TABLE Farmacia`), 6 entidades y enum
  `TipoLimpieza`, 2 repositorios con `Fila` explícita. **Hallazgo relevante**: se descubrió un
  comportamiento real de Dapper 2.1.79 — la materialización automática de un `record` con un
  parámetro `long?` desde una columna snake_case puede fallar aunque
  `MatchNamesWithUnderscores` esté activo, dependiendo de la forma exacta del record (reproducido
  en aislamiento; no es simplemente "toda columna nula falla"). Corregido en
  `RepositorioRegistrosCalidad.ListarAmbiental()` con alias explícitos `SELECT col AS
  PascalCaseName`. Se ha dejado una tarea en segundo plano para revisar si `PacienteFila.MedicoId`
  (Spec 001, mismo patrón `long?`) tiene el mismo riesgo latente, aunque sus tests actuales pasan.
  `dotnet build` sin errores; 7+2+34 = 43 tests en verde.
- **2026-09-05** — User Story 1 (P1, MVP) completada: T015-T023. `ServicioRegistrosCalidad`
  (Ambiental/Limpieza) con `fuera_rango` congelado al registrar (CA-900) y limpieza de un clic con
  fecha/usuario automáticos (CA-901). Se añadió `Farmacia.UmbralDiasAvisoCalidad` para soportar
  FR-950 (US4). En Presentación: `RegistrosCalidadWindow` con pestañas Ambiental/Limpieza, botón
  en `MainWindow` visible para Elaborador y Administrador. Test Avalonia.Headless de regresión
  para ambos listados. `dotnet build` sin errores; 7+3+38 = 48 tests en verde.
- **2026-09-05** — User Story 2 (P2) completada: T024-T030. Formación acumulativa (tres
  formaciones consultables, CA-902) y recogida de residuos no SIGRE. Dos pestañas nuevas
  (Formación, Residuos) en `RegistrosCalidadWindow`. Se detectó que `Spd.Presentacion.csproj` no
  trae `System` implícito en algunos ficheros nuevos (a diferencia de los proyectos de test);
  corregido con `using System;` explícito en los dos ViewModels nuevos. `dotnet build` sin
  errores; 7+3+41 = 51 tests en verde.
- **2026-09-05** — User Story 3 (P2) completada: T031-T038. `ServicioControlDocumental` con
  comprobación de rol en la propia capa de Aplicación tanto para registrar como para listar
  (CA-903) — un Elaborador no puede ni escribir ni consultar. `ControlDocumentalWindow`, ventana
  separada visible solo para Administrador (mismo criterio que los botones de Configuración de
  Spec 000). Test Avalonia.Headless de regresión para ambos listados. `dotnet build` sin errores;
  7+4+46 = 57 tests en verde.
- **2026-09-05** — User Story 4 (P3) completada: T039-T042. `ComprobarAvisos` (FR-950, CA-904) ya
  estaba en `ServicioRegistrosCalidad` desde US1; al escribir su test se descubrió un **bug real**
  de redondeo: `(int)(ahora - ultima).TotalDays` trunca 7,92 días a 7, así que con umbral 7 no
  avisaba de un hueco de "8 días naturales" tal y como pide CA-904 literalmente. Corregido con
  `Math.Ceiling` (redondeo al alza: pasado el día 7 ya se considera atrasado). Aviso mostrado en el
  panel de inicio (`MainWindow`), informativo, nunca bloquea nada. `dotnet build` sin errores;
  7+4+48 = 59 tests en verde. **Todas las user stories de esta spec están completas.**
- **2026-09-05** — Fase Polish completada (T043-T045). Los 5 escenarios de `quickstart.md`
  (CA-900..CA-904) están cubiertos 1:1 por tests automatizados — ver tabla de criterios de
  aceptación arriba, actualizada con el nombre exacto de cada test. Revisión de tamaño (Art.
  XI.6): todas las clases muy por debajo de ~300 líneas (`RepositorioRegistrosCalidad.cs` es la
  mayor, 159); ningún método supera ~40 líneas. **Spec 009 queda completa en código y tests,
  pendiente de la prueba manual real (`dotnet run`) del usuario** cuando pueda sentarse al
  ordenador, igual que Specs 001 y 003 — no se puede verificar una interfaz gráfica de escritorio
  sin ejecutarla de verdad.

## Cambio de alcance tras prueba manual (2026-09-05)

El usuario probó esta rama en su escritorio y reportó varios puntos. Registro completo de lo
encontrado y de lo aplicado:

**1. Bug real: cerrar con la X no apagaba la aplicación en la ventana de login/asistente.**
Solo `MainWindow` tenía un manejador `Closed` que llamaba a `desktop.Shutdown()` al cerrar con la
X nativa; ni la ventana de login ni la del asistente de primer arranque lo tenían, a pesar de
`ShutdownMode.OnExplicitShutdown` — cerrarlas con la X podía dejar la aplicación como proceso en
segundo plano sin ninguna ventana visible. Corregido aplicando el mismo patrón (bandera
"cerrada por el usuario para continuar el flujo" + `Closed` → `Shutdown()` salvo que ese flujo
esté en marcha) a las tres ventanas en `App.axaml.cs`.

**2. Confusión de ramas (no era un bug)**: el usuario probó esta rama (`009-registros-calidad`,
partida de `main`) y no encontraba Pacientes ni Catálogo de medicamentos ni la pantalla de
revisión del nomenclátor. Esto es esperado: esas funcionalidades viven en las ramas
`001-pacientes-y-medicos` y `003-catalogo-medicamentos`, que no se han fusionado ni entre sí ni
con esta — se construyeron en paralelo mientras el usuario podía probar cada una por separado.
Se le explicó y quedó aclarado en la conversación, no en el código.

**3. Cambio de alcance real, decidido por el usuario**: los registros ambiental y de limpieza
(FR-900/901/902/910/911, escenarios E1/E2, CA-900/901) **no son necesarios en absoluto** como
pantallas independientes de esta spec. Los valores de temperatura/humedad se contemplarán, si
acaso, al generar la hoja de elaboración del blíster (Spec 006/007) — pueden quedar en blanco y
rellenarse a mano en papel sin problema, porque las hojas de un día se generan todas juntas pero
los blísteres se preparan a lo largo de la jornada con condiciones distintas; un único registro
ambiental "suelto" no reflejaría eso. Por depender de ambos, el aviso de registro atrasado
(FR-950, escenario E5, CA-904, User Story 4 completa) también se retiró — avisar de que faltan
registros que ya no existen en la aplicación no tendría sentido.

**Código retirado** (build y tests verificados en verde tras la retirada, 7+4+41 = 52 tests):
- `src/Spd.Dominio/RegistroAmbiental.cs`, `RegistroLimpieza.cs`, `TipoLimpieza.cs`
- `src/Spd.Aplicacion/DatosRegistroAmbiental.cs`, `ResultadoAvisoRegistros.cs`
- Métodos de ambiental/limpieza/avisos en `IServicioRegistrosCalidad`/`ServicioRegistrosCalidad`,
  `IRepositorioRegistrosCalidad`/`RepositorioRegistrosCalidad` (se quedan solo Formación/Residuos)
- `src/Spd.Presentacion/ViewModels/RegistroAmbientalViewModel.cs`, `RegistroLimpiezaViewModel.cs`
  y sus vistas; pestañas correspondientes en `RegistrosCalidadWindow` (quedan Formación/Residuos)
- `Farmacia.UmbralDiasAvisoCalidad` y la columna `umbral_dias_aviso_calidad` (migración editada
  directamente porque el esquema no está desplegado en ningún sitio real todavía)
- Aviso en el panel de inicio (`MainViewModel`/`MainWindow`)
- Tests correspondientes en `ServicioRegistrosCalidadTests.cs`,
  `InfraestructuraRegistrosCalidadFundamentosTests.cs`, `RegistrosCalidadViewTests.cs` (reescritos
  para cubrir solo Formación/Residuos)

**No afectado**: Formación del personal (US2), Recogida de residuos (US2), Control de cambios del
PNT y control de copias (US3) — el usuario no reportó ningún problema con estos.

`spec.md` actualizado con marcas `⚠️ RETIRADO` en los FR/CA/escenarios afectados (sin borrar el
texto original, para conservar la numeración y la trazabilidad de la decisión) y una nueva entrada
en "Clarifications". `tasks.md` anota igual las tareas T015-T023 (US1) y T039-T042 (US4).
- **2026-09-06 (fusión a `main`)** — Resuelta la coordinación de numeración anotada más arriba:
  001 y 003 se fusionaron primero (0002 y 0003 respectivamente), así que esta migración se
  renombra de `0002` a `0004_registros_calidad.sql` al fusionar esta rama. El asistente y el login
  de esta rama ya llevaban el cierre explícito con la X (`Closed` → `desktop.Shutdown()`), que
  `main` todavía no tenía — se conserva ese arreglo al fusionar.
