# Fase 0 — Investigación: Backup, cifrado y purga (Spec 010, solo 4.1+4.2)

## Decisión 1 — Proveedor SQLCipher: ya fijado por la constitución, falta activarlo

**Contexto**: FR-1010/1011/1012 exigen `sqlcipher_export`/`PRAGMA rekey`, que requieren un SQLite
compilado con SQLCipher. El Artículo VIII.2 de la constitución ya fija el stack: "Microsoft.Data.
Sqlite + SQLitePCLRaw.bundle_e_sqlcipher" — no es una decisión nueva de esta spec, sino una pieza
del stack ya elegida en la Fase 1 y ya referenciada en `Spd.Infraestructura.csproj`
(`SQLitePCLRaw.bundle_e_sqlcipher` 2.1.11). Ninguna spec anterior (000, 001, 003, 009) llegó a
activarla: todas abren `SqliteConnection` confiando en la inicialización implícita de
`Microsoft.Data.Sqlite` (`Batteries_V2.Init()`), que registra el SQLite normal, no el compilado
con SQLCipher.

**Decisión**: en el arranque de la aplicación (`Program.cs`, antes de `BuildAvaloniaApp()` y antes
de que `App.axaml.cs` abra la primera conexión), registrar explícitamente el proveedor SQLCipher:

```csharp
SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlcipher());
```

Esto sustituye por completo la inicialización implícita — no se llama a `Batteries_V2.Init()` en
ningún sitio. A partir de ahí, Dapper y todos los repositorios existentes de Spec 000/001/003/009
siguen funcionando exactamente igual, sin cambiar una sola línea: es un cambio puramente de qué
biblioteca nativa carga el proceso, no del proveedor ADO.NET. Una base de datos sin cifrar (el
caso de hoy, y el de cualquier farmacia que nunca active el cifrado) se sigue abriendo con
normalidad bajo este mismo proveedor — el binario e_sqlcipher es un superconjunto de SQLite normal
que solo cifra cuando se le da una clave con `PRAGMA key`.

**Verificado empíricamente** (proyecto de consola de prueba, `Microsoft.Data.Sqlite` 8.0.10 +
`SQLitePCLRaw.bundle_e_sqlcipher` 2.1.11, descartado tras la prueba):
1. `PRAGMA key = '...'` + `CREATE TABLE` + `INSERT` sobre un fichero nuevo → OK.
2. Reabrir el mismo fichero sin dar la clave y leer → **falla** con `SQLite Error 26: 'file is not
   a database'` (el error clásico de SQLCipher para clave ausente o incorrecta) — confirma que el
   cifrado real se aplica, no es un cifrado de fachada.
3. Reabrir con la clave correcta y leer → OK, dato legible.
4. `PRAGMA rekey = 'clave-nueva'` sobre una conexión ya autenticada, cerrar, reabrir con la clave
   nueva y leer → OK. Confirma FR-1012 (rekey funciona sin exportar/reimportar).
5. Una base de datos creada **sin** `PRAGMA key` (caso no cifrado) se sigue leyendo con
   normalidad bajo el mismo proveedor e_sqlcipher → confirma que no hace falta un proveedor
   distinto según si el cifrado está activo o no.

**Hallazgo crítico de la prueba (no documentado en ningún sitio evidente, hay que tenerlo en
cuenta en la implementación)**: con la cadena de conexión por defecto (pooling de
`Microsoft.Data.Sqlite` activado), reabrir "sin clave" en el paso 2 **no fallaba** — leía el dato
igualmente. La causa: el pooling de `Microsoft.Data.Sqlite` reutiliza el descriptor nativo ya
autenticado de una conexión anterior con la misma cadena de conexión, en vez de abrir un
descriptor realmente nuevo que exigiría la clave otra vez. Añadiendo `Pooling=False` a la cadena
de conexión, la prueba 2 pasó a fallar como se esperaba. **Cualquier código que abra una conexión
nueva a un fichero potencialmente cifrado para comprobar una contraseña (activar/desactivar
cifrado, cualquier futuro flujo de verificación) debe usar `Pooling=False` explícitamente**, o el
pooling puede dar una falsa sensación de que una clave incorrecta fue aceptada. La conexión única
y persistente que ya usa `App.axaml.cs` durante el funcionamiento normal de la app no se ve
afectada (nunca se cierra y reabre en el uso normal), pero los flujos de activar/desactivar
cifrado (FR-1010/1011, que exportan a un fichero nuevo y luego intercambian cuál es el activo) sí
abren varias conexiones en secuencia y deben llevar `Pooling=False`.

**Alternativas consideradas**: un binding de SQLCipher de otro autor (hay varios paquetes menos
mantenidos en NuGet) — descartada, `SQLitePCLRaw.bundle_e_sqlcipher` es del mismo autor que
`SQLitePCLRaw` (la capa que ya usa `Microsoft.Data.Sqlite` por debajo) y es la que la propia
constitución ya nombra. Mantener dos proveedores distintos según si el cifrado está activo —
descartada: el registro de proveedor de `SQLitePCLRaw` es un único valor estático por proceso: no
se puede cambiar a mitad de ejecución sin reiniciar la app, y no hace falta, porque el binario
e_sqlcipher abre bases sin cifrar exactamente igual (punto 5 de la prueba).

## Decisión 2 — Detección de si el cifrado está activo: en tiempo de arranque, no un flag guardado

**Decisión**: al arrancar, se intenta abrir `spd.db` sin `PRAGMA key`. Si una consulta trivial
(`SELECT 1`) tiene éxito, la base no está cifrada. Si falla con el error característico de
SQLCipher (`SQLITE_NOTADB`/"file is not a database"), está cifrada y se pide la contraseña
maestra antes de continuar (FR-1013). No se guarda un flag "cifrado=true/false" en `config.json`
ni en ninguna tabla: sería un dato derivable (Art. V.3) que además podría desincronizarse del
estado real del fichero.

**Alternativas consideradas**: guardar un flag en `config.json` — descartado, dato derivable y
redundante; si el flag y el fichero real discreparan (p. ej. tras restaurar un backup de otro
momento, Art. VI.4), el flag mentiría.

## Decisión 3 — Clave de recuperación de 24 palabras: cifrado de sobre (envelope encryption)

**Contexto**: FR-1010 pide una contraseña maestra elegida por el administrador **y**, por
separado, una clave de recuperación de 24 palabras generada por el sistema — dos secretos
independientes que deben poder desbloquear la misma base de datos. SQLCipher solo entiende una
clave por conexión (`PRAGMA key`); no tiene un mecanismo nativo de "dos contraseñas válidas".

**Decisión**: cifrado de sobre, el patrón estándar para este problema (mismo principio que la
clave de recuperación de BitLocker/LUKS/age):
1. Se genera una clave maestra de cifrado (MEK) aleatoria de 256 bits — **esta** es la clave real
   que se pasa a `PRAGMA key`/`PRAGMA rekey`, nunca la ve el administrador.
2. La contraseña elegida por el administrador se pasa por Argon2id (mismos parámetros que
   `HasheadorArgon2id`, ya usado para las contraseñas de usuario — Art. X.2, reutilizar en vez de
   añadir una segunda biblioteca de KDF) con una sal aleatoria, para derivar una clave de cifrado
   de clave (KEK-contraseña).
3. La frase de recuperación de 24 palabras se genera al azar (ver Decisión 4) y se deriva de la
   misma forma una segunda KEK-recuperación.
4. La MEK se cifra con AES-256-GCM (`System.Security.Cryptography.AesGcm`, en el BCL de .NET, sin
   dependencia nueva) bajo KEK-contraseña y, por separado, bajo KEK-recuperación. Ambos "sobres"
   (nonce + texto cifrado + tag, y su sal de Argon2id correspondiente) se guardan en `config.json`
   — información inútil sin la contraseña o la frase, así que no debilita la seguridad guardarla
   en claro junto a la base de datos (Art. VI.4, config.json ya vive junto a `spd.db`).
5. Arranque con cifrado activo: pedir la contraseña maestra (FR-1013), derivar KEK-contraseña,
   desenvolver la MEK, usarla en `PRAGMA key`. Si el administrador ha perdido la contraseña, una
   opción "usar clave de recuperación" pide la frase de 24 palabras en su lugar y repite el
   proceso con KEK-recuperación.
6. Cambiar la contraseña maestra (FR-1012) no cambia la MEK ni exige `PRAGMA rekey`: solo
   re-deriva KEK-contraseña con la contraseña nueva y re-envuelve la MEK existente. `PRAGMA rekey`
   se reserva para cuando de verdad se quiere cambiar la clave de cifrado de la base de datos en
   sí (que la propia FR-1012 sí pide expresamente) — en ese caso se genera una MEK nueva, se hace
   `PRAGMA rekey`, y se generan y envuelven una contraseña y clave de recuperación nuevas a la vez
   (FR-1012 exige "genera una nueva clave de recuperación").

**Alternativas consideradas**: pedir la contraseña maestra Y la frase de recuperación concatenadas
como si fueran una sola clave — descartada, no cumple FR-1010 ("clave de recuperación" debe
funcionar sola si se pierde la contraseña, no junto a ella). Guardar la MEK en claro en algún
sitio "oculto" — descartada de raíz, es la clave real, guardarla en claro anula el cifrado
(Art. VII.2, la contraseña/clave no se almacena en ningún lugar del sistema en claro).

## Decisión 4 — Lista de palabras para la clave de recuperación

**Decisión**: la lista oficial de palabras en español de BIP39 (2048 palabras, dominio público,
mantenida por el proyecto Bitcoin/bip39), empaquetada como recurso embebido. Se usan 24 palabras
de esa lista para codificar los 256 bits aleatorios de forma legible y fácil de transcribir a
mano — no se implementa el checksum ni el resto del estándar BIP39 (esto no es una cartera de
criptomonedas ni necesita interoperar con ningún estándar externo, solo una codificación
humana-amigable de bytes aleatorios; Art. X.2, menos piezas móviles que inventar un checksum
propio). 24 palabras × 11 bits/palabra = 264 bits, más que suficientes para los 256 bits de la
MEK.

**Alternativas consideradas**: generar una lista de palabras propia en castellano — descartada,
reinventar una rueda ya resuelta y auditada (Art. X.2); usar la lista inglesa de BIP39 — descartada,
la interfaz y todo lo demás está en castellano (Art. IX.5, XI.3) y transcribir palabras en un
idioma no nativo es más propenso a errores para el público de esta aplicación.

## Decisión 5 — Backup: `VACUUM INTO` + zip, sin biblioteca nueva

**Decisión**: `VACUUM INTO 'ruta-temporal.db'` (comando SQL nativo desde SQLite 3.27, disponible
en el sqlite3 empaquetado por `SQLitePCLRaw.bundle_e_sqlcipher`) para obtener una copia consistente
de un único fichero sin bloquear escrituras concurrentes más que el tiempo de la copia. Si el
cifrado está activo, el `VACUUM INTO` produce igualmente un fichero cifrado (FR-1014: el backup no
añade ni quita seguridad). El fichero temporal más `config.json` se comprimen con
`System.IO.Compression.ZipFile` (BCL, sin dependencia nueva) en `spd-aaaammdd-hhmm.zip`, y se
copian a `Farmacia.RutaBackup` (ya existe desde Spec 000).

**Alternativas consideradas**: copiar el fichero `.db` directamente sin `VACUUM INTO` — descartada,
una copia de fichero mientras SQLite tiene páginas sin volcar (WAL) puede producir una copia
inconsistente; `VACUUM INTO` genera un snapshot consistente por diseño de SQLite. Una librería de
terceros para backups programados — descartada, el propio artículo VI.6 solo exige "al cerrar la
aplicación", sin necesidad de un programador de tareas.

## Decisión 6 — Rotación de backups: se calcula de los nombres de fichero, no de una tabla

**Decisión**: FR-1002 (30 diarios + 12 mensuales) se resuelve listando `Farmacia.RutaBackup`,
parseando la fecha del nombre `spd-aaaammdd-hhmm.zip`, y aplicando la regla de promoción/purga en
memoria cada vez que se genera un backup nuevo. No se guarda ningún registro de backups en la base
de datos (Art. V.3, es derivable del propio sistema de ficheros; también evita el problema de que
la tabla de backups viviera dentro de la misma base de datos que se hace backup).

## Decisión 7 — Secuencia SQL real de `sqlcipher_export` (FR-1010/FR-1011)

**Contexto**: FR-1010/FR-1011 piden literalmente reescribir la base con `sqlcipher_export`, pero
las decisiones anteriores solo cubren el envoltorio de la MEK, no la secuencia SQL en sí
(hallazgo de `/speckit-analyze`, ver PROGRESO.md).

**Decisión**: la secuencia estándar de SQLCipher, sin variación:

Activar cifrado (de plano a cifrado, FR-1010):
```sql
ATTACH DATABASE 'spd-nueva-cifrada.db' AS cifrada KEY 'x''<mek-en-hex>''';
SELECT sqlcipher_export('cifrada');
DETACH DATABASE cifrada;
```
Desactivar cifrado (de cifrado a plano, FR-1011; la conexión de origen ya está abierta con
`PRAGMA key` puesto):
```sql
ATTACH DATABASE 'spd-nueva-plana.db' AS plana KEY '';
SELECT sqlcipher_export('plana');
DETACH DATABASE plana;
```
En ambos casos, tras el `DETACH`, se cierra la conexión de origen y se sustituye el fichero
`spd.db` por el nuevo (mismo patrón atómico que el backup de Decisión 5: escribir al lado, y solo
entonces reemplazar — un corte de luz a mitad dejaría como mucho un fichero temporal descartable,
nunca un `spd.db` corrupto). La MEK se pasa como clave raw en hexadecimal (`x'...'`), no como
passphrase textual, porque ya es aleatoria de 256 bits (Decisión 3) — no hace falta que SQLCipher
la derive con su propio KDF interno (`PRAGMA kdf_iter`), que es para cuando el usuario teclea una
frase directamente.

## Output

Todas las incógnitas técnicas de esta iteración (4.1 Backup + 4.2 Cifrado) quedan resueltas. La
sección 4.3 (Purga) queda fuera de esta iteración por la dependencia de datos ya documentada en
`spec.md` (Clarifications 2026-09-05); no arrastra ningún `NEEDS CLARIFICATION` a `tasks.md`.
