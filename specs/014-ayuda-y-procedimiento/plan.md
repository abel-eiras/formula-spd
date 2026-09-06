# Implementation Plan: Ayuda de la aplicación y guía de procedimiento

**Branch**: `014-ayuda-y-procedimiento` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

## Summary

Ventana de ayuda con dos secciones (Uso / Procedimiento), búsqueda, F1 contextual desde cada
ventana, botón "¿Por qué?" en las pantallas con bloqueos normativos, y 26 apartados en Markdown
embebido redactados a partir de los PNT (13 de Procedimiento, 13 de Uso).

## Constitution Check

- **Art. XI.5**: contenido versionado junto al código (EmbeddedResource), no dato de usuario.
- **Art. VI**: sin red ni navegador externo; todo dentro de la aplicación.
- **Art. VIII**: sin dependencias nuevas (renderizador propio, research.md Decisión 1).
- **Art. I**: cada apartado de Procedimiento cita el PNT/constitución que lo exige (FR-1411).
- **Sin violaciones.**

## Technical Context

**Language/Version**: C# / .NET 8. **Dependencies**: ninguna nueva. **Storage**: ninguno.
**Testing**: xUnit sobre `IndiceAyuda` (carga, búsqueda, ids referenciados, cobertura de ventanas) y
Avalonia.Headless para `AyudaWindow`.
**Project Type**: solo `Spd.Presentacion` (`Ayuda/` contenido + `CatalogoAyuda`, `IndiceAyuda`,
`RenderizadorMarkdown`, `AyudaContextual`, `AyudaWindow`/`View`/`ViewModel`).
