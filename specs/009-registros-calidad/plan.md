# Implementation Plan: Registros de calidad

**Branch**: `009-registros-calidad` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/009-registros-calidad/spec.md`

## Summary

Seis registros de calidad del PNT independientes del circuito de cada paciente: ambiental (con
`fuera_rango` congelado al registrar), limpieza de un clic, formación del personal (sin distinción
de categoría profesional), recogida de residuos no SIGRE, y control documental (cambios del PNT y
copias controladas) exclusivo de Administrador. Ninguna dependencia nueva: se reutiliza el stack
ya fijado en la Spec 000, sobre la que esta feature añade una columna aditiva a `Farmacia`.

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (Constitución Art. VIII.2, sin cambios)

**Primary Dependencies**: Las mismas de Spec 000 (Avalonia UI 11.3.20, Dapper,
Microsoft.Data.Sqlite, Serilog). Sin dependencias nuevas.

**Storage**: SQLite, nueva migración `0002_registros_calidad.sql` sobre el esquema de Spec 000
(esta rama parte de `main`, solo Spec 000 mergeada — igual que Specs 001 y 003, que también
numeran su propia migración `0002` en sus respectivas ramas; ver nota de coordinación en Project
Structure). Incluye un `ALTER TABLE Farmacia ADD COLUMN umbral_dias_aviso_calidad` aditivo
(research.md Decisión 3).

**Testing**: xUnit (Dominio/Aplicación) + Avalonia.Headless.XUnit (Presentación), mismo patrón que
las specs anteriores.

**Target Platform**: Windows 11 x64 portable, sin cambios (Art. VI.4-5).

**Project Type**: Aplicación de escritorio, 4 capas ya existentes — esta feature puebla
`Spd.Dominio` (6 entidades + enums `TipoLimpieza`), `Spd.Aplicacion`
(`ServicioRegistrosCalidad`, `ServicioControlDocumental`), `Spd.Infraestructura` (2 repositorios) y
`Spd.Presentacion` (menú de registros + control documental + aviso en panel de inicio).

**Performance Goals**: Ninguna pantalla de esta spec es la "pantalla de preparación" del Art.
IX.4; sin objetivo numérico propio.

**Constraints**: SQL explícito sin ORM (Art. VIII.4); nada se borra físicamente (Art. III.1);
`fuera_rango` se congela al registrar, nunca se recalcula (Art. IV, CA-900); Control de
cambios/copias exclusivo de Administrador comprobado en la capa de Aplicación, no solo en la
pantalla (Art. VII.4).

**Scale/Scope**: Decenas de registros al mes por farmacia — volumen bajo.

## Constitution Check

*GATE: debe superarse antes de la Fase 0. Se revalida tras el diseño de la Fase 1.*

| Artículo | Aplica porque | Cómo lo satisface este plan |
|---|---|---|
| I — Base normativa | FR-900/FR-910 son los registros que el PNT exige de temperatura/humedad y limpieza | Se modelan como entidades propias, reutilizables desde Spec 006 cuando exista (`spd_id` nullable). |
| II — El papel es la base legal | Esta spec no genera ningún documento imprimible (eso es Spec 007) | Fuera de alcance explícito (spec.md §8); esta spec solo registra los datos que Spec 007 imprimirá después. |
| III — Nada se borra | FR-921 (formación acumulativa), duplicados ambientales (§7 casos límite) | Todas las tablas son de alta únicamente en esta spec; no hay ningún UPDATE/DELETE de negocio, solo INSERT. |
| VII — Seguridad y acceso | FR-942 (Control de cambios/copias solo Administrador) | Comprobación de rol en la capa de Aplicación (research.md Decisión 4), nunca solo ocultando el botón. |
| IV — Catálogo vivo, historia congelada | FR-901 (`fuera_rango` congelado) | Es una instantánea tomada al registrar con los rangos vigentes de `Farmacia` en ese instante (research.md Decisión 2, CA-900). |
| VIII — Arquitectura | Toda la spec | Reutiliza las 4 capas y el stack ya fijados; SQL explícito con una migración numerada. |
| IX — Calidad | CA-900..CA-904 | Cada CA se traduce en un test antes de considerarse hecho; ver `PROGRESO.md`. |
| X — Simplicidad | Un único umbral configurable en vez de dos (research.md Decisión 3); se ignora el campo `tipo`/`formador_id` de Fase 1 que ya no aplica (Decisión 1) | Menos piezas móviles que lo que la propia clarificación o la constitución vigente exigen. |
| XI — Legibilidad | Todo el código de esta spec | Nombres de dominio en castellano (`RegistroAmbiental`, `FueraDeRango`); `ServicioControlDocumental` separado de `ServicioRegistrosCalidad` precisamente para que la restricción de permisos sea evidente por el propio nombre de la clase, no por un `if` disperso. |

Sin violaciones que requieran justificación en `Complexity Tracking`.

*Re-chequeado tras el diseño de Fase 1 (`data-model.md`, `contracts/`, `quickstart.md`): sin
cambios de conclusión.*

## Project Structure

### Documentation (this feature)

```text
specs/009-registros-calidad/
├── spec.md
├── plan.md               # Este fichero
├── research.md           # Fase 0
├── data-model.md          # Fase 1 — recorte de las 6 entidades + adición a Farmacia
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

**Nota de numeración de migración**: esta rama (`009-registros-calidad`) parte de `main`, igual
que `001-pacientes-y-medicos` y `003-catalogo-medicamentos`; las tres numeran su propia migración
como `0002`. Al integrar las tres en `main`, cada mergeo sucesivo deberá renumerar su migración
(`0003`, `0004`...) antes de mergear — mismo aviso ya dejado en los `plan.md` de las Specs 001 y
003.

```text
src/
├── Spd.Dominio/            # RegistroAmbiental, RegistroLimpieza, FormacionPersonal,
│                           # RecogidaResiduos, ControlCambiosPNT, ControlCopias, TipoLimpieza
├── Spd.Aplicacion/         # ServicioRegistrosCalidad, ServicioControlDocumental (ver contracts/)
├── Spd.Infraestructura/    # RepositorioRegistrosCalidad, RepositorioControlDocumental
└── Spd.Presentacion/       # Menú de registros, control documental, aviso en panel de inicio

tests/
├── Spd.Dominio.Tests/       # Cálculo de fuera_rango como regla pura, si se extrae
├── Spd.Aplicacion.Tests/    # Alta de cada registro, permisos, avisos
└── Spd.Presentacion.Tests/  # Ventanas nuevas (mismo patrón Avalonia.Headless)
```

**Structure Decision**: sin cambios respecto a Spec 000 — misma opción única de proyecto de
escritorio en 4 capas.

## Complexity Tracking

Sin violaciones del Constitution Check que requieran justificación.
