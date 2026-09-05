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
| 6 | Implementación | `/speckit-implement` | 🔄 En curso — Setup+Foundational+US1+US2+US3 hechas (45/59) | `984d025`, `769788a`, `a4415c2`, `b6a1132` (+ pendiente) |

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
| CA-005 | Descarga de nomenclátor no bloquea la app | T049, T050 | ⏳ Pendiente de implementar |
| CA-006 | Cambio de valores por defecto no reescribe pacientes existentes | T046 | ⏳ Pendiente de implementar |

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
