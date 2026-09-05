# Implementation Plan: Catálogo de medicamentos

**Branch**: `003-catalogo-medicamentos` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/003-catalogo-medicamentos/spec.md`

## Summary

Catálogo de medicamentos por CN con alta mínima (CN+nombre), descripción física versionada sin
afectar a instantáneas ya congeladas, aptitud SPD derivada pero editable con motivo, y una
pantalla de revisión de importación del nomenclátor que nunca sobrescribe datos sensibles sin
confirmación. Ninguna dependencia nueva: se reutiliza íntegramente el stack ya fijado en la Spec
000, incluido `IServicioNomenclator` para la descarga del fichero.

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (Constitución Art. VIII.2, sin cambios respecto a las
specs anteriores)

**Primary Dependencies**: Las mismas de Spec 000/001 (Avalonia UI 11.3.20, Dapper,
Microsoft.Data.Sqlite, Serilog). Esta spec no introduce ninguna dependencia nueva; el parseo CSV
del nomenclátor (research.md Decisión 5) se hace a mano con `string.Split`, sin librería de CSV,
porque el formato mínimo (dos columnas con cabecera) no lo justifica (Art. X.2).

**Storage**: SQLite, nueva migración `0003_catalogo_medicamentos.sql`. Esta rama parte de `main`
(solo Spec 000 mergeada), así que la migración se numera `0002` en esta rama — ver nota de
numeración en Project Structure.

**Testing**: xUnit (Dominio/Aplicación) + Avalonia.Headless.XUnit (Presentación), mismo patrón que
Spec 000/001.

**Target Platform**: Windows 11 x64 portable, sin cambios (Art. VI.4-5).

**Project Type**: Aplicación de escritorio, 4 capas ya existentes — esta feature puebla
`Spd.Dominio` (entidad Medicamento + `FormaFarmaceutica` + regla de derivación de aptitud),
`Spd.Aplicacion` (`ServicioMedicamentos`, `ServicioImportacionNomenclator`), `Spd.Infraestructura`
(repositorio + `LectorNomenclatorCsv`) y `Spd.Presentacion` (catálogo + pantalla de importación).

**Performance Goals**: Ninguna pantalla de esta spec es la "pantalla de preparación" del Art.
IX.4; la búsqueda del catálogo (FR-305) adopta el mismo objetivo de agilidad que la búsqueda de
pacientes/médicos de Spec 001, sin que la constitución lo exija numéricamente aquí.

**Constraints**: SQL explícito sin ORM (Art. VIII.4); nada se borra físicamente, solo baja lógica
(Art. III.1); la descripción física versiona en `Medicamento_Hist` sin tocar instantáneas de otras
specs (Art. IV.3); la importación del nomenclátor reutiliza la única conexión de red ya prevista
(Art. VI.2), no abre ninguna nueva.

**Scale/Scope**: Cientos de medicamentos en el catálogo de una farmacia — volumen bajo, índices
SQL estándar bastan.

## Constitution Check

*GATE: debe superarse antes de la Fase 0. Se revalida tras el diseño de la Fase 1.*

| Artículo | Aplica porque | Cómo lo satisface este plan |
|---|---|---|
| III — Nada se borra | FR-306 baja de medicamento | Baja lógica (`activo`), nunca `DELETE`; reactivación en vez de duplicado (CA-305). |
| IV — Catálogo vivo, historia congelada | Medicamento es catálogo (FR-303/FR-304), es el ejemplo canónico del artículo | Editar la descripción física actualiza el catálogo para todo uso futuro (IV.2) pero `Medicamento_Hist` conserva cada versión con su periodo de vigencia (IV.3, research.md Decisión 4); ninguna instantánea de SPD_Linea (fuera de esta spec) se ve afectada (CA-301). |
| V — Un dato, una entrada | FR-303 (`desc_texto` propuesto, no reteclear los campos que ya están en `desc_forma`/`desc_color`/etc.) | El profesional no repite en texto libre lo que ya escribió en los campos estructurados (research.md Decisión 3). |
| VI — Aislamiento y portabilidad | FR-320 reutiliza la descarga del nomenclátor | No se abre ninguna conexión de red nueva; se reutiliza `IServicioNomenclator` de Spec 000 tal cual. |
| VIII — Arquitectura | Toda la spec | Reutiliza las 4 capas y el stack ya fijados; SQL explícito con una migración numerada. |
| IX — Calidad | CA-300..CA-305 | Cada CA se traduce en un test antes de considerarse hecho; ver `PROGRESO.md` para el mapeo CA→test. |
| X — Simplicidad | Lector de nomenclátor mínimo en vez de esperar a Spec 011 (research.md Decisión 5); regla de derivación de aptitud sin tabla de excepciones (Decisión 2) | Se elige la solución con menos piezas móviles y más honesta sobre lo que se sabe hoy del formato real del nomenclátor. |
| XI — Legibilidad | Todo el código de esta spec | Nombres de dominio en castellano (`Medicamento`, `FormaFarmaceutica`, `desc_vigente_desde`); `LectorNomenclatorCsv` documentado como simplificación deliberada a sustituir por Spec 011. |

Sin violaciones que requieran justificación en `Complexity Tracking`.

*Re-chequeado tras el diseño de Fase 1 (`data-model.md`, `contracts/`, `quickstart.md`): sin
cambios de conclusión — la columna `desc_vigente_desde` (research.md Decisión 4) es una adición
necesaria para poder versionar, no rompe ningún gate.*

## Project Structure

### Documentation (this feature)

```text
specs/003-catalogo-medicamentos/
├── spec.md
├── plan.md               # Este fichero
├── research.md           # Fase 0
├── data-model.md          # Fase 1 — recorte de Medicamento/Medicamento_Hist de esta feature
├── quickstart.md          # Fase 1
├── contracts/
│   └── servicios-aplicacion.md
├── checklists/requirements.md
├── PROGRESO.md
└── tasks.md               # Fase 2
```

### Source Code (repository root)

Reutiliza la estructura ya creada en Spec 000/001; esta feature añade ficheros nuevos dentro de
las mismas 4 carpetas, ningún proyecto ni carpeta nueva.

**Nota de numeración de migración**: esta rama (`003-catalogo-medicamentos`) parte de `main`, que
solo tiene la migración `0001` de Spec 000 (la `0002` de Spec 001 vive en la rama
`001-pacientes-y-medicos`, todavía sin mergear). La migración de esta spec se numera **`0002`** en
esta rama. Cuando ambas ramas se integren en `main`, la que se mergee en segundo lugar deberá
renumerar su migración a `0003` antes de mergear — se deja anotado aquí para no olvidarlo, y se
registra también en `PROGRESO.md`.

```text
src/
├── Spd.Dominio/            # Medicamento, FormaFarmaceutica, regla de derivación de aptitud
├── Spd.Aplicacion/         # ServicioMedicamentos, ServicioImportacionNomenclator (ver contracts/)
├── Spd.Infraestructura/    # RepositorioMedicamentos, LectorNomenclatorCsv
└── Spd.Presentacion/       # Catálogo de medicamentos, pantalla de revisión de importación

tests/
├── Spd.Dominio.Tests/       # Regla de derivación de aptitud, FormaFarmaceutica (puras)
├── Spd.Aplicacion.Tests/    # Alta/edición/versionado/importación
└── Spd.Presentacion.Tests/  # Ventanas nuevas (mismo patrón Avalonia.Headless que Spec 000/001)
```

**Structure Decision**: sin cambios respecto a Spec 000/001 — misma opción única de proyecto de
escritorio en 4 capas.

## Complexity Tracking

Sin violaciones del Constitution Check que requieran justificación.
