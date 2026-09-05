# Plan y progreso — Spec 000: Configuración inicial, farmacia y usuarios

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

## Fases del flujo Spec Kit

| # | Fase | Comando | Estado | Commit |
|---|---|---|---|---|
| 1 | Especificación | `/speckit-specify` | ✅ Hecho — 2026-09-05 | `bff6a93` |
| 2 | Aclaración | `/speckit-clarify` | ✅ Hecho — 2026-09-05 (Q1 y Q2 resueltas) | `b1422cb` |
| 3 | Plan de implementación | `/speckit-plan` | ✅ Hecho — 2026-09-05 | `f57ee1f` |
| 4 | Desglose de tareas | `/speckit-tasks` | ✅ Hecho — 2026-09-05 (52 tareas, 5 user stories) | `f3ad51c` |
| 5 | Análisis de coherencia | `/speckit-analyze` | ✅ Hecho — 2026-09-05 (1 CRITICAL + 4 mejoras, remediadas) | `86ee536` |
| 6 | Implementación | `/speckit-implement` | ✅ Hecho — 2026-09-05 (59/59 tareas) | `984d025`, `769788a`, `a4415c2`, `b6a1132`, `5a3dc51`, `ed5466a`, `018b9ad` (+ pendiente) |

## Preguntas abiertas resueltas en la fase 2 (Constitución Art. X.3)

| # | Pregunta | Respuesta |
|---|---|---|
| Q1 | FR-050: ¿URL de actualizaciones = GitHub Releases del proyecto, o servidor propio? | GitHub Releases del repositorio `abel-eiras/spd` |
| Q2 | FR-045: ¿bloqueo manual por Administrador, o bloqueo temporal automático (p. ej. 15 min)? | Solo desbloqueo manual por un Administrador, sin expiración automática |

## Criterios de aceptación de la spec y su tipo de test previsto

Formato Dado/Cuando/Entonces (Constitución Art. IX.2). Cada uno se implementa como test antes de
darse por completado (Art. IX.1); el tipo de test se confirma o ajusta en la fase de `/speckit-plan`.

| CA | Descripción | Test previsto (tasks.md, renumerado tras `/speckit-analyze`) | Estado |
|---|---|---|---|
| CA-000 | Asistente obligatorio en primer arranque | T021, T022, T023 | ✅ Validado (tests + arranque real) |
| CA-001 | Cambio de prefijo no afecta a numeración pasada | T038 | ✅ Validado (parcial — completo con Spec 001) |
| CA-002 | Único administrador protegido (no autobaja) | T028 | ✅ Validado (test) |
| CA-003 | Baja de usuario conserva histórico | T029 | ✅ Validado (test) |
| CA-004 | Bloqueo por intentos fallidos | T030, T031 | ✅ Validado (test) |
| CA-005 | Descarga de nomenclátor no bloquea la app | T049, T050 | ✅ Validado (test) |
| CA-006 | Cambio de valores por defecto no reescribe pacientes existentes | T046 | ✅ Validado (test) |

## Invariantes de constitución que aplican a esta spec

- Art. I.3 (documentación de paciente ≥ 1 año tras baja): no se ejercita en esta spec en sí (no hay
  pacientes todavía), pero `fecha_baja` de usuario sigue el mismo patrón de baja del Art. III.1.
- Art. III (nada se borra): FR-043 — baja de usuario, no eliminación física.
- Art. V (un dato, una entrada): FR-020, FR-013 — valores por defecto y prefijos no se piden dos veces.
- Art. VI (aislamiento y portabilidad): FR-050/FR-051/FR-052 — únicas dos excepciones de red permitidas.
- Art. VII (seguridad y acceso): FR-040..FR-045 — roles, hash Argon2id, bloqueo por intentos.
- Art. VIII (arquitectura): el plan de la fase 3 debe derivar de las 4 capas fijadas, sin proponer stack alternativo.

## Registro de avance

- **2026-09-05** — Fase 1 completada. `spec.md` trasladado literalmente desde
  `spec-000-configuracion-y-usuarios.md`, con Q1/Q2 marcadas `[NEEDS CLARIFICATION]`. Checklist de
  calidad de la especificación (`checklists/requirements.md`) generado; todo pasa salvo el punto de
  "sin NEEDS CLARIFICATION", bloqueado a propósito hasta la fase 2. Commit `bff6a93`.
- **2026-09-05** — Fase 2 completada. Dos preguntas planteadas una a una; el propietario del
  producto confirmó en ambas la opción recomendada (la propuesta por defecto ya apuntada en la spec
  original): Q1 → GitHub Releases del repositorio del proyecto; Q2 → solo desbloqueo manual por un
  Administrador, sin expiración automática. `spec.md` actualizado con sección "Clarifications" y
  FR-045/FR-050 resueltos sin marcadores pendientes. Checklist de calidad al 100 %. Detectada y
  corregida sobre la marcha una desviación propia: al trasladar la spec en la fase 1 se había
  eliminado sin avisar la mención "Anthropic/" de FR-050 (texto original ambiguo) — se restauró
  antes de resolver Q1, en vez de dejarla editada en silencio.
- **2026-09-05** — Fase 3 completada. `plan.md` (Technical Context + Constitution Check con los 9
  artículos aplicables), `research.md` (5 decisiones técnicas, ninguna deja NEEDS CLARIFICATION),
  `data-model.md` (recorte Farmacia/Usuario de esta feature), `contracts/servicios-aplicacion.md`
  (5 interfaces de Aplicación) y `quickstart.md` (guía de validación por CA). Efecto colateral
  documentado: `docs/data-model.md` sube a v0.5 (añade `ruta_documentos_generados`,
  `url_nomenclator`, `umbral_reutilizacion_lectura_ambiental_horas` a Farmacia;
  `intentos_fallidos_consecutivos`, `bloqueado` a Usuario) porque esos campos los exige esta spec y
  no estaban en el documento consolidado.
- **2026-09-05** — Fase 4 completada. `tasks.md` con 52 tareas en 8 fases (Setup, Foundational,
  5 user stories por prioridad P1-P5, Polish). Cada user story mapeada a su escenario de la spec y
  a sus CA-xxx (tabla al inicio de `tasks.md`). Tests incluidos por decisión explícita (Art. IX.1
  exige test de Dominio antes de dar una regla por implementada). US1 (asistente) marcado como MVP.
- **2026-09-05** — Fase 5 completada. `/speckit-analyze` cruzó spec.md/plan.md/tasks.md contra la
  constitución: 1 hallazgo **CRITICAL** (C1 — Art. VII.6, traza de auditoría no cubierta
  explícitamente en la mayoría de escrituras) y 4 mejoras (E1 test de "nunca automático al
  arrancar" Art. VI.3; U1 comportamiento del paso de cifrado sin Spec 010; U2 test de valor por
  defecto FR-012; U3 test de FR-041/FR-044). Cobertura previa: 21/21 FR y 7/7 CA con ≥1 tarea, pero
  solo 18/21 FR con test dedicado. Remediación aplicada directamente en `tasks.md`: 8 tareas nuevas
  (T023, T032, T033, T041, T042, T051, T052, y notas de auditoría añadidas a T024, T035, T043,
  T047, T053, T054) — `tasks.md` pasa de 52 a 59 tareas, renumerado íntegramente porque ninguna
  tenía código escrito todavía. Tabla CA→tarea de este documento actualizada con la nueva
  numeración.
- **2026-09-05** — Fase 6 (implementación) en curso. Instalado `dotnet-sdk-8.0` vía dnf (el usuario
  ejecutó el comando, versión verificada 8.0.130 = la LTS fijada por la constitución).
  - **Setup (T001-T008)**: creados `SPD.sln` y los 6 proyectos .NET. Desviación del template
    corregida: `dotnet new avalonia.mvvm` instala Avalonia 12.x por defecto; se han pinnado los 4
    paquetes `Avalonia.*` a `11.3.20` (Art. VIII.2 fija Avalonia UI 11) y se ha quitado
    `WithDeveloperTools()` de `Program.cs` (API solo de v12). `dotnet build`/`dotnet test` en verde.
    Commit `984d025`.
  - **Foundational (T009-T020)**: esquema SQL (`Farmacia`, `Usuario`, `Auditoria`,
    `schema_version`), `AplicadorMigraciones` (embebido, idempotente), entidades `Farmacia`/
    `Usuario`/`Rol`, `HasheadorArgon2id`, `RepositorioFarmacia`/`RepositorioUsuarios`,
    `RegistradorAuditoria`, Serilog en `Program.cs`. 5 tests de humo en
    `InfraestructuraFundamentosTests.cs` (migración idempotente, alta/lectura de Farmacia, alta/
    lectura de Usuario con `Rol` correcto, hash/verificación Argon2id, inserción en auditoría) — los
    5 en verde. Corregido en el camino: `RepositorioUsuarios` no podía depender del mapeo de enums
    de Dapper para la columna `rol` (fallaba el CHECK de la tabla); se resolvió con una fila DTO
    explícita en vez de un `TypeHandler`, más acorde con Art. XI.2 (explícito antes que una
    abstracción que oculte el mapeo real).
  - **User Story 1 — Asistente de primer arranque (T021-T027, MVP)**: `ServicioAsistentePrimerArranque`
    acumula los datos de los 5 pasos en memoria y no escribe nada hasta `FinalizarAsistente()`
    (CA-000). `AsistentePrimerArranqueViewModel` (CommunityToolkit.Mvvm) navega entre los 5 pasos;
    el paso de cifrado solo guarda una preferencia de UI (remediación U1, Spec 010 no existe
    todavía). 4 tests en verde (`AsistentePrimerArranqueTests.cs`): `HayConfiguracionInicial`
    antes/después, validación de campos obligatorios (FR-001), y auditoría de la creación de
    Farmacia+Usuario (remediación C1). `App.axaml.cs` cablea el arranque: sin fila `Farmacia`
    muestra `AsistentePrimerArranqueWindow` como única ventana; al finalizar, abre la ventana
    principal. Verificado con una ejecución real de `dotnet run` (8 s, sin excepciones): crea
    `spd.db` con las 4 tablas y `schema_version=1`, sin fila `Farmacia` hasta completar el
    asistente. **Limitación de esta verificación**: no hay captura de pantalla de la ventana nativa
    de Avalonia — el aspecto visual de los 5 pasos no se ha comprobado ojo a ojo, solo que la app
    arranca, no lanza excepciones y persiste correctamente al completar el flujo por código.
  - **User Story 2 — Gestión de usuarios y acceso (T028-T037)**: regla "único administrador
    protegido" (`ReglaUnicoAdministrador`/`UltimoAdministradorException`, en ficheros propios de
    Dominio en vez de dentro de `Usuario.cs`, más acorde con Art. XI.6). `ServicioUsuarios`:
    `CrearUsuario` (contraseña provisional generada o dada, siempre `debe_cambiar_password=1`,
    remediación U3), `DarDeBaja` (lógica, Art. III.1), `CambiarPassword`/`ResetearPassword`,
    `IntentarLogin` (verifica bloqueo antes que la contraseña, y solo entonces llama a
    `RegistrarIntentoLogin`), `DesbloquearUsuario` (solo rol Administrador). Los 6 métodos de
    escritura registran en auditoría, incluido login y login fallido (remediación C1). Pantallas
    `LoginView`/`LoginWindow` (con mensaje de bloqueo) y `Configuracion/UsuariosView`/`UsuariosWindow`
    (listado, alta, baja, reseteo); `MainWindow` gana un botón "Configuración de usuarios" visible
    solo para `ADMINISTRADOR` (Elaborador no accede a Configuración, spec §2). `App.axaml.cs`
    encadena asistente → login → ventana principal.
    5 tests de Dominio (`UsuarioReglasTests.cs`) + 7 tests de Aplicación
    (`ServicioUsuariosTests.cs`) en verde: CA-002, CA-003, CA-004, auditoría y contraseña
    provisional. Corregido en el camino: `new { entidad.Id }` (nombre de propiedad inferido) hacía
    fallar el binding de parámetros de Dapper con Microsoft.Data.Sqlite ("Must add values for the
    following parameters: @id"); se resolvió nombrando el parámetro explícitamente
    (`new { id = entidad.Id }`). Verificado con dos ejecuciones reales de `dotnet run`: con BD
    vacía (repite el asistente) y con una fila `Farmacia` insertada a mano (salta directo al
    login) — ninguna lanza excepción en 6 s. Misma limitación que en US1: sin verificación visual.
  - **User Story 3 — Datos de la farmacia y rutas (T038-T045)**: `ServicioConfiguracionFarmacia`
    (`ObtenerConfiguracion`, `ActualizarDatosFarmacia`, `ActualizarPrefijos`, `ValidarRuta` — existe/
    escribible/coincide con la carpeta de instalación, FR-030..FR-032). `GestorLogoFarmacia`
    (Infraestructura) archiva el logo anterior con fecha antes de sustituirlo (FR-011).
    `FarmaciaView`/`FarmaciaWindow` con los 3 bloques (datos, logo, prefijos, rutas con botón
    "Validar" independiente por ruta). `MainWindow` gana un segundo botón "Configuración de la
    farmacia". 9 tests nuevos en verde: 2 en Dominio (`PrefijoNumeracionTests.cs` — CA-001, con la
    limitación ya documentada en `quickstart.md` de que la garantía completa depende de Spec 001,
    que no existe todavía) y 7 en Aplicación (`ServicioConfiguracionFarmaciaTests.cs` +
    `GestorLogoFarmaciaTests.cs` — rutas, FR-012 remediación U2, auditoría remediación C1, histórico
    de logo). Verificado con una ejecución real de `dotnet run` (6 s, sin excepciones) tras cablear
    los dos servicios nuevos en `App.axaml.cs`/`MainViewModel`. Misma limitación de siempre: sin
    verificación visual de las vistas Avalonia.
  - **User Story 4 — Valores por defecto de retirada y ambientales (T046-T048)**:
    `ServicioConfiguracionFarmacia.ActualizarValoresDefecto` (día de retirada, nº blísteres, días de
    antelación, rangos de temperatura/humedad, umbral de reutilización) solo escribe en `Farmacia`
    (CA-006); la garantía de que un paciente ya personalizado no cambia queda, igual que CA-001,
    pendiente de Spec 001 para verificarse end-to-end (ya documentado en `quickstart.md`). Sección
    "Valores por defecto" añadida a `FarmaciaView` con botón de guardado independiente. 1 test nuevo
    en verde (23/23 en Aplicación). `dotnet build`/`dotnet test` en verde para toda la solución
    (7 Dominio + 23 Aplicación = 30 tests).
  - **User Story 5 — Actualizaciones y nomenclátor (T049-T055)**: `ServicioActualizaciones`
    (GitHub Releases, research.md Decisión 3) y `ServicioNomenclator` (`HttpClient` con timeout de
    10 s, Decisión 5), cada uno con su `HttpClient` inyectado para poder simular fallos de red sin
    depender de conexión real en los tests (`FakeHttpMessageHandler`). Ambos registran en auditoría
    tanto el éxito como el fallo (remediación C1). `ActualizacionesView`/`Window` y
    `NomenclatorView`/`Window`; dos botones nuevos en `MainWindow`. 7 tests nuevos en verde,
    incluida la remediación E1 (`ArranqueSinRedTests.cs`): construir los dos servicios no dispara
    ninguna petición por sí solo — **limitación explícita**: esto no sustituye a una revisión de que
    `App.axaml.cs` nunca los invoca en el arranque, eso queda como verificación de código, no de
    test automatizado. 37/37 tests en verde en toda la solución. Ejecución real de `dotnet run` sin
    excepciones.
  - **Polish (T056-T059)**: `quickstart.md` ejecutado íntegramente — los 7 criterios ya estaban
    cubiertos por los tests de las fases anteriores (CA-001/003/006 al nivel "de esta spec" que el
    propio `quickstart.md` anticipaba, pendientes de Spec 001/006 para el end-to-end completo).
    Revisión de tamaño (Art. XI.6): ningún método de `Spd.Dominio`/`Spd.Aplicacion` se acerca a 40
    líneas; el fichero más largo es `ServicioUsuarios.cs` con 141 líneas, muy por debajo de las
    ~300 del límite de clase. Arranque medido con un cronómetro en `Program.cs`/`App.axaml.cs`
    (Serilog): **0 ms** hasta mostrar la ventana del asistente — muy por debajo del límite de 2 s
    del Art. IX.4.

**Con esto, las 59 tareas de `tasks.md` están completadas.** `dotnet build`/`dotnet test` en verde
(37/37 tests) para toda la solución. Limitación transversal a toda la Fase 6, repetida
deliberadamente aquí para que quede en un solo sitio: ninguna vista Avalonia se ha verificado
visualmente (sin herramienta de captura de ventana nativa disponible) — el usuario debe ejecutar
`dotnet run --project src/Spd.Presentacion` y revisar a ojo el asistente, el login y las pantallas
de Configuración antes de considerar la spec 000 realmente cerrada.

## Correcciones tras la primera prueba manual del usuario (2026-09-05)

El usuario ejecutó la app de verdad y reportó 5 problemas. Correcciones aplicadas:

1. **La app se cerraba al terminar el asistente en vez de pasar al login.** Bug real de Avalonia:
   con el `ShutdownMode` por defecto (`OnMainWindowClose`), cerrar la ventana que en su momento se
   asignó a `MainWindow` cierra toda la app aunque ya se haya sustituido por otra ventana antes de
   cerrarla. Corregido con `desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown` y un
   `Shutdown()` explícito solo al cerrar la ventana principal real (tras iniciar sesión) —
   `App.axaml.cs`.
2. **Alta de usuario con una sola contraseña, sin poder verla ni confirmarla.** Añadido un segundo
   campo "Repetir contraseña" con validación de coincidencia, y una casilla "Mostrar contraseña"
   que usa `TextBox.RevealPassword` (confirmado disponible en Avalonia 11.3.20). Aplicado tanto en
   `UsuariosView` (alta de usuario) como en `PasoPrimerUsuarioView` (asistente), que tenía el mismo
   problema.
3. **Los usuarios creados no tenían forma de conocer su contraseña.** Bug real: si el administrador
   dejaba el campo en blanco, `ServicioUsuarios.CrearUsuario`/`ResetearPassword` generaban una
   contraseña provisional pero nunca la devolvían a nadie — el usuario creado no podía entrar.
   `CrearUsuario` ahora devuelve `ResultadoAltaUsuario(Usuario, PasswordProvisional)` y
   `ResetearPassword` devuelve la nueva contraseña en claro; `UsuariosViewModel` la muestra en el
   mensaje cuando no la ha escrito el propio Administrador (Art. VII.1: solo se puede leer en claro
   en este momento, después solo se guarda el hash).
4. **Rutas y logo solo se podían escribir o pegar a mano.** Añadidos botones "Explorar…" en
   `FarmaciaView` que usan `TopLevel.StorageProvider` (selector nativo de carpetas para
   backup/documentos, selector de fichero de imagen para el logo) — `FarmaciaView.axaml.cs`.
5. **La fecha de descarga del nomenclátor se mostraba en UTC en vez de hora local.** `FechaUtc` se
   sigue guardando en UTC (correcto para auditoría), pero `NomenclatorViewModel` la convierte con
   `.ToLocalTime()` antes de mostrarla — usa la zona horaria del sistema, así que ya tiene en cuenta
   el horario de verano sin necesidad de fijar un desfase a mano.

**Pendiente, marcado explícitamente como no prioritario por el usuario**: las ventanas emergentes
de Configuración (Usuarios, Farmacia, Actualizaciones, Nomenclátor) obligan a cerrarlas para volver;
el usuario preferiría una navegación integrada con un botón "Volver" en vez de ventanas nuevas, pero
pidió dejarlo para más adelante — es un cambio de arquitectura de navegación, no una corrección
puntual.

Verificación: `dotnet build`/`dotnet test` en verde (37/37 tests, algunos actualizados para la nueva
firma de `CrearUsuario`/`ResetearPassword`) y una ejecución real de `dotnet run` sin excepciones.
**Sigue pendiente que el usuario confirme a ojo** que el asistente ya pasa al login sin cerrarse —
es el único de los 5 puntos que no se puede demostrar con un test automatizado sin un driver de UI.
