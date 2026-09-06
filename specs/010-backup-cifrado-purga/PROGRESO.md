# Plan y progreso — Spec 010: Backup, cifrado y purga

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: esta spec se implementa en una rama independiente (`010-backup-cifrado-purga`,
creada desde `main`, que solo tiene mergeada la Spec 000) mientras las Specs 001, 003 y 009 esperan
verificación manual del usuario en sus propias ramas. Solo se implementan 4.1 (Backup) y 4.2
(Cifrado) — 4.3 (Purga) queda diferida por completo: depende de la entidad Paciente y su cadena
(Specs 001, 002, 004, 005, 006, 008), que no existen todavía en esta rama (ver spec.md,
Clarifications 2026-09-05 y "Fuera de alcance").

- **2026-09-05/06** — Ciclo completo `/speckit-specify` → `/speckit-clarify` → `/speckit-plan` →
  `/speckit-tasks` → `/speckit-analyze` → `/speckit-implement`, ejecutado de forma autónoma.
  `/speckit-analyze` detectó 3 huecos antes de tocar código (research.md sin la secuencia SQL real
  de `sqlcipher_export`, `RestaurarDesdeZip` sin id de administrador para auditoría, FR-1014 sin
  ningún test asignado) — los tres remediados en research.md/contracts/tasks.md antes de
  implementar.

- **User Story 1 (P1, MVP) — Backup**: `ServicioBackup` (`VACUUM INTO` a fichero temporal, zip
  junto a `config.json`, rotación 30 diarios + 12 mensuales calculada del propio nombre de fichero
  sin tabla nueva, research.md Decisiones 5/6). Enganchado al cierre de la aplicación vía
  `Window.Closing` cancelable: si el backup falla, un `AvisoWindow` modal bloquea el cierre hasta
  que el administrador lo ve (FR-1001/CA-1001) — nunca solo un log. Botón "Generar backup ahora" en
  Configuración/Farmacia (FR-1003). `RestaurarDesdeZip` (FR-1004) descomprime sobre una carpeta
  destino y registra auditoría con el administrador que la ejecuta.
  CA-1000/1001/1002 cubiertos por `ServicioBackupTests` (5 tests) + `FarmaciaWindowTests`.

- **Hallazgo crítico #1 (antes de User Story 2)**: `Microsoft.Data.Sqlite` 10.0.11 — la versión que
  estaba fijada en `Spd.Infraestructura.csproj` — **ignora silenciosamente** un
  `SQLite3Provider_e_sqlcipher` registrado a mano y arranca con SQLite normal, sin cifrado real y
  sin ningún error visible. Reproducido con un proyecto de consola aislado variando solo esa
  versión (`PRAGMA cipher_version` vacío y sin `HAS_CODEC` en `compile_options` únicamente en
  10.0.11; 8.0.10, 9.0.x y 10.0.0..10.0.10 funcionan todos correctamente). Fijado a **10.0.10** —
  ver research.md, Decisión 1, "Hallazgo crítico #2" (numerado como segundo hallazgo de esa
  decisión, el primero fue el de `Pooling=False`). El peor tipo de fallo posible para una función
  de seguridad: parecía funcionar y no cifraba nada.

- **User Story 2 (P2) — Cifrado**: proveedor SQLCipher registrado en `Program.cs` antes de
  `BuildAvaloniaApp()`. Cifrado de sobre (research.md Decisión 3): MEK aleatoria de 256 bits nunca
  expuesta al administrador, envuelta con AES-256-GCM bajo dos claves derivadas por Argon2id
  (mismos parámetros que `HasheadorArgon2id`, reutilizado en vez de una segunda librería de KDF) —
  una de la contraseña maestra, otra de la frase de recuperación de 24 palabras (lista oficial
  BIP39 en español, dominio público, embebida como recurso). Los dos sobres viven en `config.json`,
  nunca en `spd.db`. `ActivarCifrado`/`DesactivarCifrado` usan la secuencia real de SQLCipher
  (`ATTACH ... KEY ...` + `SELECT sqlcipher_export(...)` + `DETACH`, ver research.md Decisión 7);
  `CambiarContrasenaMaestra` usa `PRAGMA rekey` in situ, sin exportar nada (FR-1012).

  **Corrección de diseño encontrada al escribir los tests**: la primera versión de `ActivarCifrado`
  generaba la frase y reescribía la base de datos en la misma llamada — pero FR-1010/CA-1003 exigen
  que la confirmación de haber guardado la frase ocurra **antes** de tocar la base de datos.
  Separado en dos pasos: `GenerarClaveYFraseNuevas()` (no escribe nada) y `ActivarCifrado(mek,
  frase, ...)`/`CambiarContrasenaMaestra(..., mekNueva, fraseNueva, ...)` (ahora sí, solo tras la
  confirmación de la Presentación). `SeguridadViewModel` implementa este flujo de dos pasos con una
  pantalla intermedia que muestra la frase y no continúa sin el botón "Ya la he guardado".

  Arranque (`App.axaml.cs`): si `ServicioCifrado.EstaActivo()` (comprobado abriendo la base sin
  clave, nunca un flag guardado — research.md Decisión 2), se muestra
  `PeticionContrasenaMaestraWindow` antes que nada; acepta indistintamente la contraseña maestra o
  la frase de recuperación (prueba ambas, la que encaje gana). La conexión principal de la app pasa
  a abrirse con `Pooling=False` (research.md, corolario del hallazgo de pooling): con una única
  conexión de por vida no aporta nada, y evita que reabrir tras el intercambio de fichero de
  Activar/DesactivarCifrado sirva contenido obsoleto de un descriptor nativo cacheado.

  CA-1003/1004 cubiertos por `ServicioCifradoTests` (8 tests, incluida la verificación real de
  `PRAGMA cipher_version`/apertura sin clave con `Pooling=False`) + `SeguridadWindowTests`.

- **Fase Polish**: `ServicioBackupConCifradoTests` cubre FR-1014 (backup de una base cifrada
  produce un `.zip` cuya base interna también exige la clave) — hallazgo de `/speckit-analyze`, sin
  cubrir por ninguna de las dos user stories por separado. Revisión de tamaño (Art. XI.6): todas
  las clases nuevas muy por debajo de ~300 líneas (`ServicioCifrado.cs` es la mayor, 233).

  `dotnet build` sin errores; **7+4+45 = 56 tests en verde**.

## Pendiente

- Sección 4.3 (Purga, FR-1020..1024, CA-1005..1007): diferida hasta que Specs 001/002/004/005/006/
  008 estén mergeadas y la entidad Paciente exista de verdad en esta rama.
- Prueba manual real (`dotnet run`) por el usuario, igual que en Specs 001/003/009 — no se puede
  verificar una interfaz gráfica de escritorio sin ejecutarla de verdad, y esta spec en particular
  toca el arranque (petición de contraseña maestra) y el cierre (backup) de la aplicación, los dos
  puntos más sensibles a un fallo no capturado por un test automatizado.

## 2026-09-06 (noche) — Sección 4.3 Purga (FR-1020..1024)

Ya existen todas las entidades de paciente, así que se construye: `ServicioPurga` (lista bajas con
antigüedad ≥ `Farmacia.anios_retencion_purga`, mínimo 5 por Art. III.2 — migración 0012; verifica
rol y contraseña del administrador con el hasheador Argon2id; bloquea si quedan envases en custodia;
audita `PURGAR_PACIENTE` con nº de ficha, nombre y fecha antes de borrar) y `RepositorioPurga` (el
único DELETE de datos de paciente de la aplicación, en cascada y en transacción). Pantalla en
Configuración → Seguridad, paciente a paciente con la contraseña. El plazo configurado se imprime en
el documento RGPD (Anexo I.D) para que lo comunicado y lo aplicado coincidan. CA-1005/1006/1007 con
test. Sin purga automática (FR-1023).
