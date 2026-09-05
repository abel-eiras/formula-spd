# Implementation Plan: Backup, cifrado y purga

**Branch**: `010-backup-cifrado-purga` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/010-backup-cifrado-purga/spec.md`

## Summary

Copia de seguridad automática al cerrar (y bajo demanda) con rotación 30 diarios/12 mensuales,
activación/desactivación de cifrado completo de la base de datos con SQLCipher y clave de
recuperación de 24 palabras, y cambio de contraseña maestra sin exportar toda la base. Esta
iteración implementa solo 4.1 (Backup) y 4.2 (Cifrado); 4.3 (Purga) queda diferida — depende de la
entidad Paciente y su cadena (Specs 001, 002, 004, 005, 006, 008), que no existen todavía en esta
rama (bifurcada de `main`, solo Spec 000 mergeada). Ninguna dependencia nueva más allá de la ya
fijada por la constitución (`SQLitePCLRaw.bundle_e_sqlcipher`, ya referenciada en el `.csproj`
pero nunca activada por ninguna spec anterior).

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (Constitución Art. VIII.2, sin cambios).

**Primary Dependencies**: Las mismas de Spec 000 (Avalonia UI 11, Dapper, Microsoft.Data.Sqlite,
Serilog) más la activación explícita de `SQLitePCLRaw.bundle_e_sqlcipher` (ya referenciada en
`Spd.Infraestructura.csproj`, nunca registrada como proveedor activo hasta esta spec — research.md
Decisión 1). Para el cifrado de sobre de la MEK: `Konscious.Security.Cryptography.Argon2` (ya
usado por `HasheadorArgon2id`, reutilizado como KDF — Art. X.2) y `System.Security.Cryptography.
AesGcm` (BCL, sin dependencia nueva). Para el backup: `System.IO.Compression.ZipFile` (BCL).

**Storage**: SQLite existente, sin migración nueva (data-model.md: ninguna tabla nueva). El estado
de cifrado vive en `config.json`, fuera de la base de datos (Art. VI.4, VII.2).

**Testing**: xUnit (Dominio/Aplicación) + Avalonia.Headless.XUnit (Presentación), mismo patrón que
las specs anteriores. Los tests de `IServicioCifrado` que abran una segunda conexión a un fichero
en disco deben usar `Pooling=False` (research.md, hallazgo de la prueba de Decisión 1) — los tests
que usan `Data Source=:memory:` no se ven afectados por este hallazgo (cada conexión en memoria
sin caché compartida ya es independiente, con o sin pooling).

**Target Platform**: Windows 11 x64 portable, sin cambios (Art. VI.4-5).

**Project Type**: Aplicación de escritorio, 4 capas ya existentes — esta feature puebla
`Spd.Dominio` (entidades `BackupInfo`, `SobreClave`, ninguna persistida en SQL), `Spd.Aplicacion`
(`ServicioBackup`, `ServicioCifrado`), `Spd.Infraestructura` (implementaciones sobre
`SqliteConnection`/`ZipFile`/`AesGcm`/`Argon2id`, más el registro del proveedor SQLCipher en
`Program.cs`) y `Spd.Presentacion` (pantalla de Configuración / Seguridad, aviso de backup fallido
al cerrar).

**Performance Goals**: Ninguna pantalla de esta spec es la "pantalla de preparación" del Art. IX.4.
El backup al cerrar no debe alargar perceptiblemente el cierre salvo cuando falla la ruta (FR-1001,
aviso visible, no un bloqueo largo).

**Constraints**: Nada se borra salvo backups rotados (Art. III.2, única excepción de eliminación
automática, y solo de ficheros de backup — nunca de datos de negocio); la contraseña maestra y la
MEK nunca se almacenan en claro (Art. VII.2); SQL explícito sin ORM (Art. VIII.4); la única
conexión de red permitida sigue siendo actualizaciones/nomenclátor (Art. VI.2) — esta spec no abre
ninguna nueva.

**Scale/Scope**: Una única base de datos por instalación, backups de bajo volumen (decenas de
ficheros); sin requisitos de escala.

## Constitution Check

*GATE: debe superarse antes de la Fase 0. Se revalida tras el diseño de la Fase 1.*

| Artículo | Aplica porque | Cómo lo satisface este plan |
|---|---|---|
| III — Nada se borra | FR-1002 rotación de backups es la única eliminación automática de todo el sistema | Afecta solo a ficheros `.zip` de backup, nunca a `spd.db` ni a ninguna tabla de negocio (Art. III.2 ya prevé esta excepción tasada); la purga (única eliminación física de datos de negocio, también prevista por el propio Art. III.2) queda diferida a otra sesión, no se improvisa aquí. |
| VI — Aislamiento y portabilidad | FR-1000/1004 backup y restauración; Art. VI.4 y VI.6 | `VACUUM INTO` + zip a `Farmacia.RutaBackup` sin abrir red; restaurar copiando la carpeta sigue siendo válido por sí solo, el asistente de FR-1004 es una comodidad adicional, no la única vía. |
| VII — Seguridad y acceso | FR-1010..1014 cifrado; Art. VII.2/3 | MEK nunca se guarda en claro (cifrado de sobre, research.md Decisión 3); activar cifrado exige confirmar impresión de la clave de recuperación antes de proceder (CA-1003); toda operación de esta spec queda en `Auditoria` (data-model.md). |
| VIII — Arquitectura | Activación del proveedor SQLCipher ya fijado por Art. VIII.2 | `SQLitePCLRaw.bundle_e_sqlcipher` ya estaba en el `.csproj`; esta spec solo activa lo que la constitución ya decidió, sin ORM y con SQL explícito (`PRAGMA key`/`rekey`/`VACUUM INTO`). |
| IX — Calidad | CA-1000..1004 | Cada CA se traduce en un test antes de considerarse hecho (ver quickstart.md). |
| X — Simplicidad | Rotación calculada de nombres de fichero, no de una tabla nueva (research.md Decisión 6); detección de cifrado en tiempo de arranque, no un flag guardado (Decisión 2); reutilizar Argon2id existente en vez de una segunda librería de KDF (Decisión 3) | Menos piezas móviles en cada caso, con la alternativa descartada documentada en research.md. |
| XI — Legibilidad | Todo el código de esta spec | Nombres en castellano (`ServicioBackup`, `ServicioCifrado`, `SobreClave`, `claveMaestra`); cada decisión no evidente (por qué envelope encryption, por qué `Pooling=False`) lleva su comentario citando research.md. |

Sin violaciones que requieran justificación en `Complexity Tracking`.

*Re-chequeado tras el diseño de Fase 1 (`data-model.md`, `contracts/`, `quickstart.md`): sin
cambios de conclusión — el cifrado de sobre (Decisión 3) es la única pieza de diseño no trivial,
y usa exclusivamente primitivas ya presentes en el proyecto (Argon2id) o en el BCL (AesGcm), sin
añadir ninguna dependencia de criptografía nueva.*

## Project Structure

### Documentation (this feature)

```text
specs/010-backup-cifrado-purga/
├── spec.md
├── plan.md               # Este fichero
├── research.md           # Fase 0 — proveedor SQLCipher, cifrado de sobre, rotación
├── data-model.md         # Fase 1 — config.json, sin tablas nuevas
├── contracts/
│   └── servicios-aplicacion.md
├── quickstart.md
└── tasks.md              # Fase 2 (/speckit-tasks, no generado por /speckit-plan)
```

### Source Code (repository root)

```text
src/
├── Spd.Dominio/
│   ├── BackupInfo.cs
│   └── SobreClave.cs
├── Spd.Aplicacion/
│   ├── IServicioBackup.cs
│   ├── ResultadoBackup.cs
│   ├── ResultadoRestauracion.cs
│   ├── IServicioCifrado.cs
│   ├── ResultadoActivarCifrado.cs
│   └── ResultadoCambioContrasena.cs
├── Spd.Infraestructura/
│   ├── ServicioBackup.cs
│   ├── ServicioCifrado.cs
│   ├── GeneradorFraseRecuperacion.cs      # lista BIP39 español embebida (research.md Decisión 4)
│   └── Recursos/wordlist-es.txt
└── Spd.Presentacion/
    ├── ViewModels/SeguridadViewModel.cs
    └── Views/Configuracion/SeguridadView.axaml(.cs)

tests/
├── Spd.Aplicacion.Tests/
│   ├── ServicioBackupTests.cs
│   └── ServicioCifradoTests.cs
└── Spd.Presentacion.Tests/
    └── SeguridadViewTests.cs
```

**Structure Decision**: mismas 4 capas ya existentes desde Spec 000, sin proyectos nuevos. La
única pieza de infraestructura no estándar es el recurso embebido de la lista de palabras
(`Recursos/wordlist-es.txt`), incluido como `EmbeddedResource` en `Spd.Infraestructura.csproj`.

## Complexity Tracking

Sin violaciones que justificar.
