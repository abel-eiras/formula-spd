# Tasks: Backup, cifrado y purga (solo 4.1 Backup + 4.2 Cifrado)

**Input**: Design documents from `specs/010-backup-cifrado-purga/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/servicios-aplicacion.md](./contracts/servicios-aplicacion.md)

**Tests**: Incluidos, con auditoría (Art. VII.6) como test explícito desde el diseño, igual que en
Spec 003. Sin fase de Setup ni Foundational: ninguna migración SQL nueva (data-model.md), se
reutilizan los 6 proyectos ya existentes desde Spec 000.

| Story | Prioridad | FR | CA cubiertos |
|---|---|---|---|
| US1 | P1 (MVP) | 4.1 Backup | CA-1000, 1001, 1002 |
| US2 | P2 | 4.2 Cifrado | CA-1003, 1004 |

4.3 (Purga, CA-1005..1007) diferida — ver spec.md, "Fuera de alcance".

## Format: `[ID] [P?] [Story] Description`

- **[P]**: puede ejecutarse en paralelo (ficheros distintos, sin dependencias pendientes)
- **[Story]**: US1/US2 según la tabla anterior

---

## Phase 1: User Story 1 — Backup automático y manual (Priority: P1) 🎯 MVP

**Goal**: al cerrar la aplicación (o bajo demanda), guardar una copia consistente y rotada, con
aviso visible si la ruta de backup falla (E1, E2 parcial — el asistente de restaurar es FR-1004).

**Independent Test**: cerrar la app con cambios pendientes y comprobar que aparece un `.zip`
íntegro (CA-1000); apuntar `RutaBackup` a una ruta inválida y comprobar el aviso al cerrar
(CA-1001); generar 41 backups de prueba y comprobar la rotación 30+12 (CA-1002).

### Tests for User Story 1

- [X] T001 [P] [US1] Test: `ServicioBackup.GenerarBackup` produce un `.zip` fechado cuya base de datos interna pasa `PRAGMA integrity_check` (CA-1000), en `tests/Spd.Aplicacion.Tests/ServicioBackupTests.cs`
- [X] T002 [P] [US1] Test: `ServicioBackup.GenerarBackup` con una `RutaBackup` inaccesible devuelve un resultado de fallo explícito en vez de lanzar (FR-1001; la Presentación decide cómo mostrarlo)
- [X] T003 [P] [US1] Test: dados 40 backups diarios simulados (nombres de fichero con fechas distintas, sin contenido real necesario para probar la regla), generar el 41 dejando solo 30 diarios + 12 mensuales promovidos (CA-1002)
- [X] T004 [P] [US1] Test: `GenerarBackup` registra en auditoría (`BACKUP_MANUAL`/`BACKUP_AUTOMATICO`, Art. VII.6)
- [X] T005 [P] [US1] Test: `ServicioBackup.RestaurarDesdeZip` descomprime un `.zip` de prueba sobre una carpeta destino, dicha carpeta queda con `spd.db`/`config.json`, y la operación registra en auditoría con el id del administrador que la ejecuta (FR-1004, Art. VII.6 — hallazgo de `/speckit-analyze`)

### Implementation for User Story 1

- [X] T006 [P] [US1] Crear `BackupInfo` en `src/Spd.Dominio/BackupInfo.cs` (no persistida — data-model.md)
- [X] T007 [US1] Implementar `IServicioBackup`/`ServicioBackup` en `src/Spd.Aplicacion/IServicioBackup.cs` + `src/Spd.Infraestructura/ServicioBackup.cs`: `VACUUM INTO` a fichero temporal, zip junto a `config.json` (research.md Decisión 5), rotación por nombre de fichero (Decisión 6), copia a `Farmacia.RutaBackup`
- [X] T008 [US1] Implementar `RestaurarDesdeZip` en `ServicioBackup` (FR-1004, `System.IO.Compression.ZipFile`), registrando `RESTAURAR_BACKUP` en auditoría con el administrador recibido por parámetro
- [X] T009 [US1] Enganchar `GenerarBackup` al cierre de la aplicación en `App.axaml.cs` (mismo punto donde hoy se hace `desktop.Shutdown()`), con aviso visible (no un log) si el resultado es de fallo (FR-1001, CA-1001)
- [X] T010 [US1] Añadir botón "Generar backup ahora" en Configuración (`FarmaciaView` o una nueva sección), llamando al mismo `GenerarBackup` (FR-1003)
- [X] T011 [P] [US1] Test Avalonia.Headless: la pantalla con el botón de backup manual construye y muestra sin lanzar, en `tests/Spd.Presentacion.Tests/` (mismo patrón de regresión que el resto de vistas)

**Checkpoint**: US1 funcional de forma independiente — MVP entregable, sin depender de que el
cifrado esté implementado.

---

## Phase 2: User Story 2 — Cifrado de la base de datos (Priority: P2)

**Goal**: activar/desactivar el cifrado completo con confirmación de clave de recuperación
impresa, y cambiar la contraseña maestra sin re-exportar toda la base (E3, E4).

**Independent Test**: activar el cifrado y comprobar que no continúa sin confirmar la impresión de
la clave de recuperación (CA-1003); con el cifrado activo, cambiar la contraseña maestra y
comprobar que es una operación de re-envuelto de clave, no una exportación completa (CA-1004);
abrir la base sin la clave y comprobar que falla (research.md, verificación del hallazgo de
`Pooling=False`); desenvolver la MEK con la frase de recuperación en vez de la contraseña y
comprobar que da la misma clave.

### Tests for User Story 2

- [ ] T012 [P] [US2] Test: registrar el proveedor `SQLite3Provider_e_sqlcipher` al arrancar y comprobar que una base sin `PRAGMA key` se sigue abriendo con normalidad (research.md Decisión 1, punto 5 de la verificación), en `tests/Spd.Aplicacion.Tests/ServicioCifradoTests.cs`
- [ ] T013 [P] [US2] Test: tras `ActivarCifrado`, reabrir el fichero **con `Pooling=False`** y sin `PRAGMA key` → falla; con la clave correcta → funciona (research.md, hallazgo crítico de la prueba)
- [ ] T014 [P] [US2] Test: `ActivarCifrado` genera una frase de 24 palabras de la lista embebida y la MEK se puede desenvolver tanto con la contraseña maestra como con esa frase, dando la misma clave (Decisión 3)
- [ ] T015 [P] [US2] Test: `CambiarContrasenaMaestra` (FR-1012) re-envuelve la MEK sin tocar `PRAGMA rekey` y genera una frase de recuperación nueva (CA-1004)
- [ ] T016 [P] [US2] Test: `DesactivarCifrado` (FR-1011) deja el fichero legible sin `PRAGMA key`
- [ ] T017 [P] [US2] Test: cada operación de esta user story registra en auditoría (`ACTIVAR_CIFRADO`, `DESACTIVAR_CIFRADO`, `CAMBIAR_CONTRASENA_MAESTRA`, Art. VII.6)

### Implementation for User Story 2

- [ ] T018 [US2] Registrar `SQLitePCL.raw.SetProvider(new SQLite3Provider_e_sqlcipher())` en `Program.cs`, antes de `BuildAvaloniaApp()` (research.md Decisión 1)
- [ ] T019 [P] [US2] Crear `SobreClave` en `src/Spd.Dominio/SobreClave.cs` (data-model.md)
- [ ] T020 [P] [US2] Crear `GeneradorFraseRecuperacion` en `src/Spd.Infraestructura/GeneradorFraseRecuperacion.cs` + recurso embebido `Recursos/wordlist-es.txt` (lista BIP39 español, research.md Decisión 4)
- [ ] T021 [US2] Implementar `IServicioCifrado`/`ServicioCifrado` en `src/Spd.Aplicacion/IServicioCifrado.cs` + `src/Spd.Infraestructura/ServicioCifrado.cs`: generación de MEK, envolver/desenvolver con Argon2id + AES-256-GCM (depende de T019, T020)
- [ ] T022 [US2] Implementar `ActivarCifrado`/`DesactivarCifrado`/`CambiarContrasenaMaestra`/`EstaActivo`/`ClaveActualParaConexion` en `ServicioCifrado` (depende de T021)
- [ ] T023 [US2] Al arrancar (`App.axaml.cs`), si `EstaActivo()` pedir la contraseña maestra (o la frase de recuperación) antes de abrir la conexión real y usar la MEK desenvuelta en `PRAGMA key` (FR-1013)
- [ ] T024 [US2] Implementar `SeguridadViewModel` + vista (activar/desactivar/cambiar contraseña, mostrar la frase de recuperación una vez con checkbox de confirmación obligatorio antes de continuar — CA-1003) en `src/Spd.Presentacion/ViewModels/SeguridadViewModel.cs` + `src/Spd.Presentacion/Views/Configuracion/SeguridadView.axaml`
- [ ] T025 [US2] Añadir botón "Seguridad" a `MainWindow`/`MainViewModel` (solo Administrador, Art. VII.4)
- [ ] T026 [P] [US2] Test Avalonia.Headless: `SeguridadView` construye y muestra sin lanzar, en `tests/Spd.Presentacion.Tests/SeguridadViewTests.cs`

**Checkpoint**: US1 + US2 funcionales de forma independiente.

---

## Phase 3: Polish & Cross-Cutting Concerns

- [ ] T027 [P] Ejecutar íntegramente [quickstart.md](./quickstart.md) y registrar el resultado en `PROGRESO.md`
- [ ] T028 Revisar que ningún método de `Spd.Dominio`/`Spd.Aplicacion` supere ~40 líneas ni ninguna clase ~300 (Art. XI.6) — vigilar especialmente `ServicioCifrado` por la cantidad de primitivas criptográficas que combina
- [ ] T029 [P] Actualizar `PROGRESO.md` marcando cada CA-1000..1004 como validado, con el test que lo confirma, y dejando constancia explícita de que CA-1005..1007 (purga) siguen diferidos
- [ ] T030 Test de integración cruzado US1+US2: con el cifrado ya activo (`ActivarCifrado`), `GenerarBackup` produce un `.zip` cuya base de datos interna también exige la clave para leerse — FR-1014, hallazgo de `/speckit-analyze` (ninguna de las dos user stories por separado lo cubre)

---

## Dependencies & Execution Order

### Phase Dependencies

- **US1 (Fase 1)**: sin dependencias — no necesita el proveedor SQLCipher activo (`VACUUM INTO` funciona igual con o sin él, research.md Decisión 1)
- **US2 (Fase 2)**: sin dependencia de US1; T018 (registrar el proveedor) bloquea el resto de esta fase
- **Polish (Fase 3)**: depende de las user stories que se quieran dar por completas

### Parallel Opportunities

- Todos los tests `[P]` de una misma user story, en paralelo
- T006, T019, T020 en paralelo entre sí (ficheros distintos, sin dependencias mutuas)
- Los tests Avalonia.Headless (T011, T026) dependen de que su vista ya esté implementada (tests de
  regresión, no TDD-first, igual que en specs anteriores)

## Implementation Strategy

### MVP primero

1. Fase 1 (US1) — **parar y validar** con CA-1000/1001/1002 antes de seguir; es entregable por sí
   sola (una farmacia con backup pero sin cifrado ya está mejor protegida que sin nada)
2. Fase 2 (US2), validando CA-1003/1004 en el checkpoint
3. Fase 3 (Polish) al final

## Notes

- `[P]` = ficheros distintos, sin dependencias pendientes entre sí
- Commit atómico por fase o por grupo lógico pequeño, como en specs anteriores
- Parar en cada checkpoint y validar la user story de forma independiente antes de continuar
- Cualquier test de US2 que abra una segunda conexión de disco para comprobar una clave debe usar
  `Pooling=False` (research.md) — si no, puede pasar por error incluso con una clave incorrecta
