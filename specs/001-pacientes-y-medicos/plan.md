# Implementation Plan: Pacientes, contactos y catálogo de médicos

**Branch**: `001-pacientes-y-medicos` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/001-pacientes-y-medicos/spec.md`

## Summary

Ficha de paciente (Anexo 2 cara anterior) con sus contactos y catálogo de médicos reutilizable
desde cualquier punto de la aplicación. Numeración de ficha automática e inmutable, validación de
DNI/NIE y CIP gallego como reglas de Dominio puras, búsqueda global sin distinguir mayúsculas ni
tildes, selector de médico con autocompletado y alta en contexto. Ninguna dependencia nueva: se
reutiliza íntegramente el stack y la arquitectura ya fijados en la Spec 000.

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (Constitución Art. VIII.2, sin cambios respecto a Spec 000)

**Primary Dependencies**: Las mismas de Spec 000 (Avalonia UI 11.3.20, Dapper, Microsoft.Data.Sqlite,
Serilog). Esta spec no introduce ninguna dependencia nueva — es CRUD, validación y búsqueda sobre
el stack ya existente.

**Storage**: SQLite, nueva migración `0002_pacientes_contactos_medicos.sql` sobre el esquema ya
creado en Spec 000 (`schema_version` pasa de 1 a 2). SQL explícito (Art. VIII.4).

**Testing**: xUnit (Dominio/Aplicación) + Avalonia.Headless.XUnit (Presentación, ya incorporado en
Spec 000 tras el incidente de `UsuariosWindow`).

**Target Platform**: Windows 11 x64 portable, sin cambios (Art. VI.4-5).

**Project Type**: Aplicación de escritorio, 4 capas ya existentes — esta feature puebla
`Spd.Dominio` (entidades Paciente/Contacto/Medico + reglas de validación DNI/CIP),
`Spd.Aplicacion` (servicios de caso de uso), `Spd.Infraestructura` (repositorios) y
`Spd.Presentacion` (fichas y catálogo).

**Performance Goals**: Ninguna pantalla de esta spec es la "pantalla de preparación" que fija el
límite de 200 ms del Art. IX.4, pero el autocompletado de médico (FR-032) y la búsqueda global de
pacientes (FR-010) exigen la misma agilidad por buena práctica de UX — se adopta el mismo objetivo
de <200 ms por búsqueda como criterio de diseño, sin que la constitución lo exija numéricamente
aquí.

**Constraints**: SQL explícito sin ORM (Art. VIII.4); nada se borra físicamente, solo baja lógica
(Art. III.1); Medico es catálogo referenciado por id, sin copia de sus datos en Paciente (Art. IV.1,
FR-035); búsqueda insensible a mayúsculas/tildes sin depender de extensiones SQLite externas
(FTS5/unaccent no están garantizadas en el binario embebido del proyecto).

**Scale/Scope**: Una farmacia, cientos de pacientes y decenas de médicos en el catálogo — volumen
bajo, no requiere indexación avanzada más allá de índices SQL estándar.

## Constitution Check

*GATE: debe superarse antes de la Fase 0. Se revalida tras el diseño de la Fase 1.*

| Artículo | Aplica porque | Cómo lo satisface este plan |
|---|---|---|
| III — Nada se borra | FR-007 baja de paciente, FR-023 baja de contacto | Ambas son baja lógica (`activo`/`estado` + fecha), nunca `DELETE`. CA-009 lo verifica explícitamente. |
| IV — Catálogo vivo | Medico es catálogo (FR-030..FR-037) | `Paciente.medico_id` es FK, nunca se copian los datos del médico en Paciente (Art. IV.1); editar un médico se ve reflejado de inmediato en todas las fichas que lo referencian (FR-035, CA-006) — a diferencia de Medicamento en SPD_Linea (Spec 006), aquí **no hay instantánea**: esta spec no crea ninguna, es la referencia viva la que se consulta siempre. |
| V — Un dato, una entrada | FR-002c (valores por defecto prerrellenados), FR-032/033 (selector con autocompletado en vez de reteclear), FR-005b (autocompletar CIP) | El paciente no vuelve a escribir datos que ya existen en Farmacia o en el catálogo de médicos. |
| VII — Seguridad y acceso | FR-040 (sin distinción de categoría profesional), FR-042 (auditoría) | Cualquier Elaborador/Administrador puede todas las operaciones; cada escritura llama a `IRegistradorAuditoria` con el detalle antes/después (CA-013). |
| VIII — Arquitectura | Toda la spec | Reutiliza las 4 capas y el stack ya fijados; SQL explícito con una nueva migración numerada. |
| IX — Calidad | CA-001..CA-015 | Cada CA se traduce en un test antes de considerarse hecho; ver `PROGRESO.md` para el mapeo CA→test. |
| X — Simplicidad | Normalización de búsqueda sin extensión SQLite (research.md Decisión 1) | Se elige la solución con menos piezas móviles: una columna calculada en vez de registrar funciones SQLite personalizadas o depender de FTS5. |
| XI — Legibilidad | Todo el código de esta spec | Nombres de dominio en castellano (`Paciente`, `Contacto`, `Medico`, `ValidadorDni`); reglas de negocio (DNI, CIP) con comentario citando el FR de origen. |

Sin violaciones que requieran justificación en `Complexity Tracking`.

*Re-chequeado tras el diseño de Fase 1 (`data-model.md`, `contracts/`, `quickstart.md`): sin
cambios de conclusión — las columnas nuevas de `Paciente`/`Medico` (research.md, Decisiones 1-3)
son adición pura sobre entidades nuevas de esta feature, no rompen ningún gate.*

## Project Structure

### Documentation (this feature)

```text
specs/001-pacientes-y-medicos/
├── spec.md
├── plan.md               # Este fichero
├── research.md           # Fase 0
├── data-model.md          # Fase 1 — recorte de Paciente/Contacto/Medico de esta feature
├── quickstart.md          # Fase 1
├── contracts/
│   └── servicios-aplicacion.md
├── checklists/requirements.md
├── PROGRESO.md
└── tasks.md               # Fase 2
```

### Source Code (repository root)

Reutiliza la estructura ya creada en Spec 000; esta feature añade ficheros nuevos dentro de las
mismas 4 carpetas, ningún proyecto ni carpeta nueva.

```text
src/
├── Spd.Dominio/            # Paciente, Contacto, Medico, ValidadorDni, ValidadorCip, EstadoPaciente
├── Spd.Aplicacion/         # ServicioPacientes, ServicioMedicos (ver contracts/)
├── Spd.Infraestructura/    # RepositorioPacientes, RepositorioContactos, RepositorioMedicos
└── Spd.Presentacion/       # Ficha de paciente (pestañas Datos/Contactos), catálogo de médicos

tests/
├── Spd.Dominio.Tests/       # Validación DNI/CIP, reglas de estado, numeración (puras)
├── Spd.Aplicacion.Tests/    # Búsqueda, duplicados, propagación de médico, auditoría
└── Spd.Presentacion.Tests/  # Ventanas nuevas (mismo patrón Avalonia.Headless que Spec 000)
```

**Structure Decision**: sin cambios respecto a Spec 000 — misma opción única de proyecto de
escritorio en 4 capas.

## Complexity Tracking

Sin violaciones del Constitution Check que requieran justificación.
