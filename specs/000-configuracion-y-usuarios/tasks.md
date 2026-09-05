# Tasks: Configuración inicial, farmacia y usuarios

**Input**: Design documents from `specs/000-configuracion-y-usuarios/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos. La Constitución Art. IX.1 exige test unitario de Dominio para cada regla de
negocio no evidente antes de considerarla implementada, y los criterios de aceptación CA-000..
CA-006 de la spec son en sí mismos casos de test.

**Organización**: user stories mapeadas desde §3 (escenarios E1-E4) y §4.6 (actualizaciones/
nomenclátor, sin escenario E propio pero con criterio de aceptación propio, CA-005).

**Actualizado tras `/speckit-analyze`**: se añaden 8 tareas de remediación (auditoría Art. VII.6,
comportamiento del paso de cifrado, valor por defecto FR-012, y test de "nunca automático al
arrancar" Art. VI.3) — ver hallazgos C1, E1, U1, U2, U3 en `PROGRESO.md`. La numeración de tareas
se ha reordenado en consecuencia; ninguna tarea tenía todavía código escrito.

| Story | Prioridad | Escenario spec | FR cubiertos | CA cubiertos |
|---|---|---|---|---|
| US1 | P1 (MVP) | E1 — Primer arranque | FR-000, FR-001 | CA-000 |
| US2 | P2 | E3 — Alta de un compañero (+ acceso) | FR-040..FR-045 | CA-002, CA-003, CA-004 |
| US3 | P3 | E2 — Cambiar datos de la farmacia | FR-010..FR-013, FR-030..FR-032 | CA-001 |
| US4 | P4 | E4 — Cambiar valores por defecto | FR-020..FR-022 | CA-006 |
| US5 | P5 | (sin E, FR-050..FR-052) | FR-050..FR-052 | CA-005 |

## Format: `[ID] [P?] [Story] Description`

- **[P]**: puede ejecutarse en paralelo (ficheros distintos, sin dependencias pendientes)
- **[Story]**: US1..US5 según la tabla anterior
- Rutas de fichero exactas en cada tarea, relativas a la raíz del repositorio

---

## Phase 1: Setup

**Purpose**: crear los proyectos .NET dentro de las carpetas ya existentes (`src/`, `tests/`)

- [x] T001 Crear `SPD.sln` en la raíz referenciando los 4 proyectos de `src/` y los 2 de `tests/`
- [x] T002 [P] Crear `src/Spd.Dominio/Spd.Dominio.csproj` (net8.0, sin dependencias externas — Art. VIII.1)
- [x] T003 [P] Crear `src/Spd.Aplicacion/Spd.Aplicacion.csproj` (net8.0, referencia a `Spd.Dominio`)
- [x] T004 [P] Crear `src/Spd.Infraestructura/Spd.Infraestructura.csproj` (net8.0; paquetes: `Microsoft.Data.Sqlite`, `SQLitePCLRaw.bundle_e_sqlcipher`, `Dapper`, `Serilog`, `Konscious.Security.Cryptography.Argon2` — research.md Decisión 2)
- [x] T005 [P] Crear `src/Spd.Presentacion/Spd.Presentacion.csproj` (Avalonia UI 11, MVVM, referencia a `Spd.Aplicacion`)
- [x] T006 [P] Crear `tests/Spd.Dominio.Tests/Spd.Dominio.Tests.csproj` (xUnit, referencia a `Spd.Dominio`)
- [x] T007 [P] Crear `tests/Spd.Aplicacion.Tests/Spd.Aplicacion.Tests.csproj` (xUnit, referencia a `Spd.Aplicacion` y `Spd.Infraestructura`)
- [x] T008 [P] Crear el fichero único de recursos de interfaz en castellano `src/Spd.Presentacion/Recursos/Textos.resx` (Art. IX.5), vacío como base para esta feature

**Checkpoint**: `dotnet build` y `dotnet test` ejecutan sin proyectos, listos para código.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: esquema de BD, entidades y repositorios que usan todas las user stories

**⚠️ CRITICAL**: ninguna user story empieza antes de completar esta fase

- [x] T009 Crear script `src/Spd.Infraestructura/Migraciones/0001_esquema_inicial.sql` con tablas `Farmacia`, `Usuario`, `Auditoria` y `schema_version` (columnas de [data-model.md](./data-model.md), Art. VIII.3)
- [x] T010 Implementar `AplicadorMigraciones` en `src/Spd.Infraestructura/Migraciones/AplicadorMigraciones.cs`: lee `schema_version`, aplica scripts pendientes al arrancar, idempotente, nunca destructivo (Art. VIII.3)
- [x] T011 [P] Crear entidad `Farmacia` en `src/Spd.Dominio/Farmacia.cs` (campos de [data-model.md](./data-model.md) §Farmacia)
- [x] T012 [P] Crear entidad `Usuario` y enum `Rol` en `src/Spd.Dominio/Usuario.cs` y `src/Spd.Dominio/Rol.cs` (Art. VII.4: solo `ADMINISTRADOR`/`ELABORADOR`)
- [x] T013 [P] Definir interfaz `IHasheadorPassword` en `src/Spd.Dominio/IHasheadorPassword.cs`
- [x] T014 Implementar `HasheadorArgon2id` en `src/Spd.Infraestructura/HasheadorArgon2id.cs` (research.md Decisión 2; depende de T013)
- [x] T015 [P] Definir interfaz `IRepositorioFarmacia` en `src/Spd.Dominio/IRepositorioFarmacia.cs`
- [x] T016 [P] Definir interfaz `IRepositorioUsuarios` en `src/Spd.Dominio/IRepositorioUsuarios.cs`
- [x] T017 Implementar `RepositorioFarmacia` (Dapper/SQLite) en `src/Spd.Infraestructura/RepositorioFarmacia.cs` (depende de T009, T011, T015)
- [x] T018 Implementar `RepositorioUsuarios` (Dapper/SQLite) en `src/Spd.Infraestructura/RepositorioUsuarios.cs` (depende de T009, T012, T016)
- [x] T019 Implementar `RegistradorAuditoria` (solo `INSERT`, Art. III.3) en `src/Spd.Infraestructura/RegistradorAuditoria.cs`. **Todo servicio de Aplicación de esta feature que escriba datos debe invocarlo** (Art. VII.6) — ver la tarea de auditoría dedicada en cada user story
- [x] T020 Configurar Serilog (log a fichero dentro de la carpeta de instalación) en `src/Spd.Presentacion/Program.cs`

**Checkpoint**: esquema y entidades listas — las user stories pueden empezar.

---

## Phase 3: User Story 1 — Asistente de primer arranque (Priority: P1) 🎯 MVP

**Goal**: sin base de datos existente, la app obliga a completar los 5 pasos del asistente antes de dejar usar cualquier otra pantalla (FR-000/FR-001).

**Independent Test**: borrar la BD, arrancar la app, comprobar que el asistente bloquea el resto de la navegación hasta finalizar (CA-000, ver [quickstart.md](./quickstart.md)).

### Tests for User Story 1

- [x] T021 [P] [US1] Test unitario: `HayConfiguracionInicial()` devuelve `false` sin fila `Farmacia` y `true` tras crearla, en `tests/Spd.Aplicacion.Tests/AsistentePrimerArranqueTests.cs` (CA-000)
- [x] T022 [P] [US1] Test unitario: `EjecutarPaso` lanza `ErrorValidacionException` si falta un campo obligatorio del paso (FR-001), en `tests/Spd.Aplicacion.Tests/AsistentePrimerArranqueTests.cs`
- [x] T023 [P] [US1] Test unitario: `FinalizarAsistente` registra en `Auditoria` la creación de `Farmacia` y del primer `Usuario` (Art. VII.6), en `tests/Spd.Aplicacion.Tests/AsistentePrimerArranqueTests.cs` *(remediación C1)*

### Implementation for User Story 1

- [x] T024 [US1] Implementar `IServicioAsistentePrimerArranque` y `ServicioAsistentePrimerArranque` en `src/Spd.Aplicacion/ServicioAsistentePrimerArranque.cs` (depende de T011, T012, T017, T018); `FinalizarAsistente` registra en auditoría la creación de `Farmacia` y del primer `Usuario` (Art. VII.6, depende también de T019)
- [x] T025 [US1] Implementar `AsistentePrimerArranqueViewModel` en `src/Spd.Presentacion/ViewModels/AsistentePrimerArranqueViewModel.cs` (5 pasos, navegación adelante/atrás — FR-001)
- [x] T026 [US1] Implementar las vistas Avalonia de los 5 pasos en `src/Spd.Presentacion/Views/Asistente/` (`PasoCifradoView`, `PasoFarmaciaView`, `PasoPrimerUsuarioView`, `PasoValoresDefectoView`, `PasoRutasView`). **`PasoCifradoView`** (remediación U1): al no existir todavía Spec 010, elegir "sí" solo guarda la preferencia (`Farmacia.cifrado_deseado`, campo interno de esta vista, no de `docs/data-model.md`) y muestra el aviso "el cifrado se activará cuando esté disponible"; no bloquea el asistente ni activa cifrado real
- [x] T027 [US1] Cablear `src/Spd.Presentacion/App.axaml.cs` para mostrar el asistente cuando `HayConfiguracionInicial()` es `false` y bloquear el resto de la navegación hasta `FinalizarAsistente()` (CA-000)

**Checkpoint**: US1 funcional de forma independiente — MVP entregable.

---

## Phase 4: User Story 2 — Gestión de usuarios y acceso (Priority: P2)

**Goal**: alta/baja de usuarios, cambio y reseteo de contraseña, protección del único administrador, bloqueo por intentos fallidos (FR-040..FR-045).

**Independent Test**: con un solo Administrador activo, intentar autodesactivarlo (CA-002); dar de baja a un usuario y comprobar que su histórico sigue visible (CA-003); fallar login 6 veces y comprobar el bloqueo (CA-004).

### Tests for User Story 2

- [x] T028 [P] [US2] Test unitario: `DarDeBaja` lanza `UltimoAdministradorException` si es el único Administrador activo, en `tests/Spd.Dominio.Tests/UsuarioReglasTests.cs` (CA-002, FR-042)
- [x] T029 [P] [US2] Test unitario: `DarDeBaja` no elimina la fila, solo `activo=0`/`fecha_baja` (Art. III), en `tests/Spd.Aplicacion.Tests/ServicioUsuariosTests.cs` (CA-003)
- [x] T030 [P] [US2] Test unitario: `RegistrarIntentoLogin` bloquea al 5º intento fallido consecutivo y un 6º intento sigue rechazado, en `tests/Spd.Aplicacion.Tests/ServicioUsuariosTests.cs` (CA-004, FR-045)
- [x] T031 [P] [US2] Test unitario: `DesbloquearUsuario` solo permitido a un usuario con rol `ADMINISTRADOR`, en `tests/Spd.Aplicacion.Tests/ServicioUsuariosTests.cs` (FR-045)
- [x] T032 [P] [US2] Test unitario: `CrearUsuario`, `DarDeBaja`, `ResetearPassword` y `DesbloquearUsuario` registran cada uno una entrada en `Auditoria` con usuario, fecha-hora, entidad y detalle (Art. VII.6), en `tests/Spd.Aplicacion.Tests/ServicioUsuariosTests.cs` *(remediación C1)*
- [x] T033 [P] [US2] Test unitario: `CrearUsuario` genera una contraseña provisional si no se indica una y marca `debe_cambiar_password=1` en ambos casos (FR-041); `ResetearPassword` también marca `debe_cambiar_password=1` (FR-044), en `tests/Spd.Aplicacion.Tests/ServicioUsuariosTests.cs` *(remediación U3)*

### Implementation for User Story 2

- [x] T034 [US2] Implementar la regla "único administrador protegido" en `src/Spd.Dominio/Usuario.cs` (FR-042; depende de T012)
- [x] T035 [US2] Implementar `IServicioUsuarios` y `ServicioUsuarios` en `src/Spd.Aplicacion/ServicioUsuarios.cs`: `CrearUsuario`, `DarDeBaja`, `CambiarPassword`, `ResetearPassword`, `RegistrarIntentoLogin`, `DesbloquearUsuario` (depende de T014, T016, T018, T034); cada método de escritura registra en auditoría (Art. VII.6, depende también de T019)
- [x] T036 [US2] Implementar pantalla Configuración/Usuarios (listado, alta, baja, reseteo de contraseña) en `src/Spd.Presentacion/Views/Configuracion/UsuariosView.axaml` + `UsuariosViewModel.cs`
- [x] T037 [US2] Implementar pantalla de login con mensaje de bloqueo ("usuario bloqueado, contacte con un Administrador") en `src/Spd.Presentacion/Views/LoginView.axaml` + `LoginViewModel.cs`

**Checkpoint**: US1 + US2 funcionales de forma independiente.

---

## Phase 5: User Story 3 — Datos de la farmacia y rutas (Priority: P3)

**Goal**: editar en cualquier momento los datos de la farmacia, los prefijos de numeración y las rutas de backup/documentos (FR-010..FR-013, FR-030..FR-032).

**Independent Test**: cambiar el prefijo de numeración y comprobar que un número ya asignado no cambia (CA-001, requiere un `num_ficha` simulado ya que Spec 001 no existe todavía).

### Tests for User Story 3

- [x] T038 [P] [US3] Test unitario: cambiar `prefijo_num_ficha`/`prefijo_num_spd` no reescribe un número ya asignado (simulado), en `tests/Spd.Dominio.Tests/PrefijoNumeracionTests.cs` (CA-001, FR-013)
- [x] T039 [P] [US3] Test unitario: `ValidarRuta` detecta una ruta no escribible y no bloquea el guardado, en `tests/Spd.Aplicacion.Tests/ServicioConfiguracionFarmaciaTests.cs` (FR-030/031)
- [x] T040 [P] [US3] Test unitario: `ValidarRuta` avisa (no bloquea) si la ruta coincide con la carpeta de instalación, en `tests/Spd.Aplicacion.Tests/ServicioConfiguracionFarmaciaTests.cs` (FR-032)
- [x] T041 [P] [US3] Test unitario: `ActualizarDatosFarmacia` resuelve `responsable_datos`/`direccion_derechos`/`email_derechos` al valor de `titular_o_comunidad_bienes`/dirección cuando no se han rellenado, sin escribirlo físicamente en esos campos (FR-012), en `tests/Spd.Aplicacion.Tests/ServicioConfiguracionFarmaciaTests.cs` *(remediación U2)*
- [x] T042 [P] [US3] Test unitario: `ActualizarDatosFarmacia` y `ActualizarPrefijos` registran en `Auditoria` (Art. VII.6), en `tests/Spd.Aplicacion.Tests/ServicioConfiguracionFarmaciaTests.cs` *(remediación C1)*

### Implementation for User Story 3

- [x] T043 [US3] Implementar `IServicioConfiguracionFarmacia` (datos, prefijos, rutas) y `ServicioConfiguracionFarmacia` en `src/Spd.Aplicacion/ServicioConfiguracionFarmacia.cs` (depende de T011, T015, T017); cada método de escritura registra en auditoría (Art. VII.6, depende también de T019)
- [x] T044 [US3] Implementar histórico de logo (copia a `logos_historico/<fecha>.<ext>`) en `src/Spd.Infraestructura/GestorLogoFarmacia.cs` (FR-011)
- [x] T045 [US3] Implementar pantalla Configuración/Farmacia (datos, logo, prefijos, rutas) en `src/Spd.Presentacion/Views/Configuracion/FarmaciaView.axaml` + `FarmaciaViewModel.cs`

**Checkpoint**: US1 + US2 + US3 funcionales de forma independiente.

---

## Phase 6: User Story 4 — Valores por defecto de retirada y ambientales (Priority: P4)

**Goal**: cambiar `dia_retirada_defecto`, `n_blisteres_defecto`, `dias_antelacion_listado` y los rangos ambientales sin afectar a entidades ya personalizadas (FR-020..FR-022).

**Independent Test**: cambiar `dia_retirada_defecto` y comprobar (con un paciente simulado, Spec 001 no existe todavía) que uno ya personalizado no cambia (CA-006).

### Tests for User Story 4

- [x] T046 [P] [US4] Test unitario: `ActualizarValoresDefecto` no modifica entidades ya personalizadas (simulado con stub), en `tests/Spd.Aplicacion.Tests/ServicioConfiguracionFarmaciaTests.cs` (CA-006, FR-020)

### Implementation for User Story 4

- [x] T047 [US4] Extender `ServicioConfiguracionFarmacia` con `ActualizarValoresDefecto` (`dia_retirada_defecto`, `n_blisteres_defecto`, `dias_antelacion_listado`, rangos ambientales, `umbral_reutilizacion_lectura_ambiental_horas`) en `src/Spd.Aplicacion/ServicioConfiguracionFarmacia.cs` (FR-020/021/022; depende de T043); registra en auditoría (Art. VII.6)
- [x] T048 [US4] Añadir la pestaña "Valores por defecto" a la pantalla de Configuración en `src/Spd.Presentacion/Views/Configuracion/ValoresDefectoView.axaml` + `ValoresDefectoViewModel.cs`

**Checkpoint**: US1 a US4 funcionales de forma independiente.

---

## Phase 7: User Story 5 — Actualizaciones y nomenclátor (Priority: P5)

**Goal**: las dos únicas pantallas con acceso a red de toda la aplicación, ambas manuales y sin bloquear el resto de la app si fallan (FR-050..FR-052, Art. VI).

**Independent Test**: apuntar `url_nomenclator` a una URL que no responde, pulsar "Descargar ahora", comprobar que se ve el motivo del fallo y el resto de la app sigue usable (CA-005).

### Tests for User Story 5

- [x] T049 [P] [US5] Test de integración: `ComprobarActualizaciones` con GitHub Releases simulado que no responde no lanza excepción no controlada, en `tests/Spd.Aplicacion.Tests/ServicioActualizacionesTests.cs`
- [x] T050 [P] [US5] Test de integración: `DescargarNomenclator` con URL que no responde devuelve motivo de fallo, en `tests/Spd.Aplicacion.Tests/ServicioNomenclatorTests.cs` (CA-005)
- [x] T051 [P] [US5] Test: ni `ComprobarActualizaciones` ni `DescargarNomenclator` se invocan durante el arranque de la aplicación (Art. VI.3) — revisión automatizada de `App.axaml.cs`/`ServicioAsistentePrimerArranque` sin llamadas a ninguno de los dos servicios fuera de una acción explícita del usuario, en `tests/Spd.Aplicacion.Tests/ArranqueSinRedTests.cs` *(remediación E1)*
- [x] T052 [P] [US5] Test: `ComprobarActualizaciones` y `DescargarNomenclator` registran en `Auditoria` el resultado (éxito/fallo) de cada acción (Art. VII.6), en `tests/Spd.Aplicacion.Tests/ServicioActualizacionesTests.cs` y `ServicioNomenclatorTests.cs` *(remediación C1)*

### Implementation for User Story 5

- [x] T053 [US5] Implementar `IServicioActualizaciones` y `ServicioActualizaciones` (GitHub Releases API, research.md Decisión 3) en `src/Spd.Infraestructura/ServicioActualizaciones.cs`; registra en auditoría (Art. VII.6)
- [x] T054 [US5] Implementar `IServicioNomenclator` y `ServicioNomenclator` (`HttpClient`, research.md Decisión 5) en `src/Spd.Infraestructura/ServicioNomenclator.cs`; registra en auditoría (Art. VII.6)
- [x] T055 [US5] Implementar pantallas Configuración/Actualizaciones y Configuración/Nomenclátor en `src/Spd.Presentacion/Views/Configuracion/` (`ActualizacionesView.axaml`, `NomenclatorView.axaml`)

**Checkpoint**: las 5 user stories funcionan de forma independiente.

---

## Phase 8: Polish & Cross-Cutting Concerns

- [ ] T056 [P] Ejecutar íntegramente [quickstart.md](./quickstart.md) y registrar el resultado en `PROGRESO.md`
- [ ] T057 Revisar que ningún método de `Spd.Dominio`/`Spd.Aplicacion` supere ~40 líneas ni ninguna clase ~300 (Art. XI.6)
- [ ] T058 Medir el arranque completo hasta la pantalla de login/asistente en un PC de gama media y confirmar < 2 s (Art. IX.4)
- [ ] T059 [P] Actualizar `PROGRESO.md` marcando cada CA-000..CA-006 como validado, con el test o prueba manual que lo confirma

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Fase 1)**: sin dependencias
- **Foundational (Fase 2)**: depende de Setup — bloquea las 5 user stories
- **US1..US5 (Fases 3-7)**: dependen de Foundational; independientes entre sí salvo:
  - US3 y US4 comparten `ServicioConfiguracionFarmacia` (US4 lo extiende) — US4 depende de T043
- **Polish (Fase 8)**: depende de las user stories que se quieran dar por completas

### Parallel Opportunities

- Todas las tareas `[P]` de Setup (T002-T008) en paralelo
- T011-T013, T015-T016 (Foundational) en paralelo entre sí
- Una vez completada Foundational, US1, US2, US3+US4 (secuencial por T043) y US5 pueden avanzar en paralelo si hay más de una persona
- Todos los tests `[P]` de una misma user story, en paralelo

## Implementation Strategy

### MVP primero

1. Fase 1 (Setup) + Fase 2 (Foundational)
2. Fase 3 (US1) — **parar y validar** con CA-000 antes de seguir
3. Entrega incremental: US2 → US3 → US4 → US5, validando el CA correspondiente en cada checkpoint antes de pasar a la siguiente
4. Fase 8 (Polish) al final, o de forma continua tras cada user story si se prefiere

## Notes

- `[P]` = ficheros distintos, sin dependencias pendientes entre sí
- Commit atómico por tarea o por grupo lógico pequeño (Setup completo, cada test, cada
  implementación), como se ha hecho en las fases anteriores de este mismo feature
- Parar en cada checkpoint y validar la user story de forma independiente antes de continuar
