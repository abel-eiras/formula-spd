# Implementation Plan: Configuración inicial, farmacia y usuarios

**Branch**: `000-configuracion-y-usuarios` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/000-configuracion-y-usuarios/spec.md`

## Summary

Asistente de primer arranque, gestión de los datos de la farmacia (fila única de configuración),
valores por defecto reutilizados por el resto de specs, rutas de backup/documentos, gestión de
usuarios (alta, baja lógica, bloqueo, cambio de contraseña) y las dos únicas pantallas con acceso
a red permitido (actualizaciones vía GitHub Releases, descarga del nomenclátor). Enfoque técnico:
cuatro capas ya fijadas por la constitución (Presentación Avalonia MVVM → Aplicación → Dominio →
Infraestructura SQLite/SQLCipher), sin introducir dependencias ni patrones no cubiertos por el
Artículo VIII.

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (Constitución Art. VIII.2)

**Primary Dependencies**: Avalonia UI 11 (MVVM) · Microsoft.Data.Sqlite + SQLitePCLRaw.bundle_e_sqlcipher · Dapper · QuestPDF (no se usa en esta spec, pero es parte del stack fijo) · Serilog (Art. VIII.2). Para el hash Argon2id (Art. VII.1) se añade una librería Argon2id para .NET (decisión concreta de paquete en `research.md`) — es una dependencia que implementa un requisito ya fijado, no un cambio de stack.

**Storage**: SQLite, cifrado completo opcional con SQLCipher activable por un Administrador (Art. VII.2-3). Esquema gestionado con scripts SQL numerados embebidos y tabla `schema_version`; migraciones idempotentes, nunca destructivas (Art. VIII.3).

**Testing**: xUnit (Art. VIII.2). Tests unitarios de Dominio antes de dar por implementada cualquier regla de negocio (Art. IX.1); tests de integración para el asistente de arranque y las dos excepciones de red.

**Target Platform**: Windows 11 x64, sin instalador, sin dependencias de runtime externas, carpeta portable con ejecutable + BD + configuración + logs + backups (Art. VI.4-5).

**Project Type**: Aplicación de escritorio, 4 capas (Art. VIII.1): `src/Spd.Presentacion` (Avalonia/MVVM) → `src/Spd.Aplicacion` (servicios de caso de uso) → `src/Spd.Dominio` (entidades y reglas, sin dependencias) → `src/Spd.Infraestructura` (SQLite/SQLCipher, ficheros, red).

**Performance Goals**: Arranque hasta pantalla de login/asistente < 2 s en PC de gama media (Art. IX.4). Esta spec no incluye pantallas de preparación, así que el límite de 200 ms de Art. IX.4 no aplica directamente aquí, pero toda operación de Configuración debe ser igual de ágil por buena práctica, sin que la constitución lo exija numéricamente fuera de la pantalla de preparación.

**Constraints**: Sin conexión de red salvo las dos excepciones tasadas — actualizaciones (FR-050, ahora resuelto a GitHub Releases) y nomenclátor (FR-051) — nunca automáticas sin acción explícita del usuario (Art. VI.1-3). Nada se borra físicamente: baja de usuario es lógica (Art. III.1). SQL explícito, sin ORM que genere esquema o consultas implícitas (Art. VIII.4). Sin telemetría.

**Scale/Scope**: Instalación de una sola farmacia por despliegue (una carpeta = una instancia, Art. VI.4); pocos usuarios (Administrador/Elaborador) que inician sesión secuencialmente en el mismo puesto, no acceso concurrente multi-PC a la misma base de datos.

## Constitution Check

*GATE: debe superarse antes de la Fase 0. Se revalida tras el diseño de la Fase 1.*

| Artículo | Aplica porque | Cómo lo satisface este plan |
|---|---|---|
| III — Nada se borra | FR-043 da de baja usuarios | Baja lógica (`activo=0`, `fecha_baja`); ninguna operación de esta spec hace `DELETE` de negocio. Tablas de auditoría solo-INSERT (FR-045 `LOGIN_FALLIDO`, y toda acción de Configuración). |
| IV — Catálogo vivo | Farmacia y Usuario son catálogos/configuración editables en cualquier momento | Los cambios de valores por defecto (FR-020) y de datos de farmacia (E2) no reescriben instantáneas ya tomadas por otras specs (CA-006); esta spec no crea instantáneas propias, solo los valores que otras specs instantanean después. |
| V — Un dato, una entrada | FR-013, FR-020 valores por defecto reutilizados | Farmacia como fila única de configuración; el asistente los pide una vez, Configuración los edita después sin repetir el asistente (FR-001). |
| VI — Aislamiento y portabilidad | FR-050/051/052 son las dos únicas excepciones de red de toda la aplicación | Ambas acciones son manuales (botón explícito), muestran resultado y no bloquean el resto de la app si fallan (CA-005). Ninguna otra pantalla de esta spec abre red. |
| VII — Seguridad y acceso | FR-040..FR-045 gestión de usuarios y acceso | Hash Argon2id, roles Administrador/Elaborador sin categoría profesional, protección de único administrador (FR-042/CA-002), bloqueo manual sin expiración automática (FR-045, Q2), traza de auditoría (FR-045, `LOGIN_FALLIDO`). |
| VIII — Arquitectura | Toda la spec | 4 capas, stack fijo, SQL explícito con scripts numerados + `schema_version`; sin ORM implícito. |
| IX — Calidad | CA-000..CA-006 | Cada CA se traduce en un test (unitario en Dominio o de integración) antes de considerarse hecho; ver `PROGRESO.md` para el mapeo CA→test. |
| X — Simplicidad | FR-032 (aviso, no bloqueo), FR-041 (contraseña generada o manual) | No se añaden validaciones ni pantallas no pedidas por la spec; ante dos soluciones igual de válidas se elige la de menos piezas móviles. |
| XI — Legibilidad | Todo el código de esta spec | Nombres de dominio en castellano (`Farmacia`, `Usuario`, `IntentosFallidos`), mecánica técnica en inglés (`IRepository`, `ILogger`); comentario de cabecera por clase citando el artículo/FR de origen cuando la regla no sea obvia. |

Sin violaciones que requieran justificación en `Complexity Tracking`.

*Re-chequeado tras el diseño de Fase 1 (`data-model.md`, `contracts/`, `quickstart.md`): sin
cambios de conclusión — las dos columnas nuevas de `Farmacia`/`Usuario` (research.md, Decisión 1)
son adición pura sobre un catálogo (Art. IV.2), no rompen ningún gate.*

## Project Structure

### Documentation (this feature)

```text
specs/000-configuracion-y-usuarios/
├── spec.md               # Especificación (Fase de /speckit-specify + /speckit-clarify)
├── plan.md               # Este fichero (/speckit-plan)
├── research.md           # Fase 0 (/speckit-plan)
├── data-model.md          # Fase 1 — recorte de Farmacia/Usuario específico de esta feature
├── quickstart.md          # Fase 1 — guía de validación manual/automatizada
├── contracts/              # Fase 1 — interfaces de los servicios de Aplicación
│   └── servicios-aplicacion.md
├── checklists/requirements.md
├── PROGRESO.md            # Seguimiento de fases, CA→test, commits
└── tasks.md               # Fase 2 (/speckit-tasks — no se crea aquí)
```

### Source Code (repository root)

Estructura ya creada en la raíz del repositorio (no es específica de esta feature, la reutilizan
todas): cuatro proyectos + dos de test, según Art. VIII.1 y `CLAUDE.md`.

```text
src/
├── Spd.Dominio/            # Entidades Farmacia, Usuario, Rol; reglas FR-042, FR-045 (política de bloqueo)
├── Spd.Aplicacion/         # Servicios de caso de uso: ver contracts/servicios-aplicacion.md
├── Spd.Infraestructura/    # Repositorios SQLite, validación de rutas, HttpClient para FR-050/051
└── Spd.Presentacion/       # ViewModels/Views Avalonia: asistente de arranque, pantallas de Configuración

tests/
├── Spd.Dominio.Tests/       # CA-001, CA-002, CA-003, CA-006 (reglas puras de Dominio)
└── Spd.Aplicacion.Tests/    # CA-000, CA-004, CA-005 (orquestación, asistente, red simulada)
```

**Structure Decision**: Opción única de proyecto (no hay frontend/backend separados ni mobile) —
la app de escritorio ya tiene su propia separación en 4 capas, que es la que sustituye a las
"Option 1/2/3" genéricas de la plantilla. No se crean carpetas nuevas: esta feature es la primera
en poblar las ya existentes desde el punto anterior de la guía.

## Complexity Tracking

Sin violaciones del Constitution Check que requieran justificación — tabla omitida a propósito
(Art. X: no se añade contenido que no aporte).
