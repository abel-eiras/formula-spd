# Implementation Plan: Tratamiento del paciente

**Branch**: `004-tratamiento` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/004-tratamiento/spec.md`

## Summary

Tratamiento del paciente, versionado en la misma tabla (cada fila es una versión completa, cierre
y apertura en vez de edición en el sitio para cambios clínicamente relevantes), con un vocabulario
cerrado de fracciones para la posología D/A/C/N. FR-421/422/430 (dependientes de Specs 005/006, que
no existen en esta rama) se implementan solo hasta el punto de extensión (estados y campos), sin
inventar su lógica futura.

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (sin cambios).

**Primary Dependencies**: Las mismas ya fijadas (Avalonia UI 11, Dapper, Microsoft.Data.Sqlite,
Serilog). Sin dependencias nuevas.

**Storage**: SQLite, nueva migración `0005_tratamiento.sql` (main ya tiene 0001-0004 tras fusionar
Specs 001/003/009).

**Testing**: xUnit (Dominio/Aplicación) + Avalonia.Headless.XUnit (Presentación), mismo patrón.

**Target Platform**: Windows 11 x64 portable, sin cambios.

**Project Type**: Aplicación de escritorio, 4 capas ya existentes — puebla `Spd.Dominio`
(`Tratamiento`, `FraccionDosis`, `TipoTratamiento`, `EstadoTratamiento`), `Spd.Aplicacion`
(`ServicioTratamientos`), `Spd.Infraestructura` (`RepositorioTratamientos`), `Spd.Presentacion`
(ficha de tratamiento accesible desde la ficha de paciente de Spec 001).

**Performance Goals**: Sin objetivo numérico propio; el selector de medicamento/médico reutiliza
los mismos patrones de búsqueda ya validados en Specs 001/003.

**Constraints**: Inmutabilidad de instantáneas clínicas (Art. IV.3/IV.4): un cambio relevante nunca
sobrescribe, siempre cierra y abre; SQL explícito sin ORM (Art. VIII.4).

**Scale/Scope**: Varios tratamientos por paciente, volumen bajo por farmacia.

## Constitution Check

*GATE: debe superarse antes de la Fase 0. Se revalida tras el diseño de la Fase 1.*

| Artículo | Aplica porque | Cómo lo satisface este plan |
|---|---|---|
| I.3 | FR-421 depende de un flujo de entrega (Spec 006) que valida idoneidad/consentimiento en la práctica real | No implementado aquí (Spec 006 no existe); el estado `PendienteRevision` queda disponible como punto de extensión. |
| IV — Catálogo vivo, historia congelada | Núcleo de FR-410/411/412 | Cada cambio clínicamente relevante cierra y abre fila (IV.4); los campos no clínicos se editan en el sitio (IV.2 aplicado de forma más granular que en Medicamento, ver research.md Decisión 1). |
| V — Un dato, una entrada | FR-400 prerrelleno de médico de cabecera y datos del medicamento | El profesional no reteclea lo que ya está en Paciente/Medicamento (Spec 001/003). |
| XI — Legibilidad | Todo el código de esta spec | Nombres en castellano (`Tratamiento`, `FraccionDosis`, `pautaTexto`); comentarios que citan el artículo/FR de origen para cada decisión no evidente. |

Sin violaciones que requieran justificación en `Complexity Tracking`.

## Project Structure

### Documentation (this feature)

```text
specs/004-tratamiento/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── contracts/servicios-aplicacion.md
├── quickstart.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── Spd.Dominio/
│   ├── Tratamiento.cs
│   ├── FraccionDosis.cs
│   ├── TipoTratamiento.cs
│   ├── EstadoTratamiento.cs
│   ├── DatosAltaTratamiento.cs
│   ├── DatosNoClinicos.cs
│   └── IRepositorioTratamientos.cs
├── Spd.Aplicacion/
│   ├── IServicioTratamientos.cs
│   └── ServicioTratamientos.cs
├── Spd.Infraestructura/
│   ├── Migraciones/0005_tratamiento.sql
│   └── RepositorioTratamientos.cs
└── Spd.Presentacion/
    ├── ViewModels/TratamientoViewModel.cs
    └── Views/Pacientes/TratamientoView.axaml(.cs)

tests/
├── Spd.Dominio.Tests/FraccionDosisTests.cs
├── Spd.Aplicacion.Tests/
│   ├── InfraestructuraTratamientosFundamentosTests.cs
│   └── ServicioTratamientosTests.cs
└── Spd.Presentacion.Tests/TratamientoViewTests.cs
```

**Structure Decision**: mismas 4 capas ya existentes, sin proyectos nuevos.

## Complexity Tracking

Sin violaciones que justificar.
