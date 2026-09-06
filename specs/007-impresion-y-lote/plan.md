# Implementation Plan: Impresión, generación en lote y documentación base

**Branch**: `007-impresion-y-lote` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/007-impresion-y-lote/spec.md`

## Summary

Motor único de generación de documentos PDF (QuestPDF, Constitución Art. VIII.2 — corrección
formal sobre el `.docx` del documento fuente), con nombrado de fichero y carpeta de salida
comunes, aplicado a los cinco documentos generables hoy con datos reales:
`FICHA`/`ETQ-A`/`ETQ-R`/`INSTR` (Spec 006) y `FICHA-PAC` (Spec 001). FR-720..733 (lote,
documentación base) y el resto del catálogo quedan diferidos por depender de Spec 002 o de
contenido real (`resources/`) que el usuario ha pedido dejar para una sesión con más capacidad de
análisis.

## Constitution Check

- **Art. II.1/II.2 (el papel es la base legal)**: el PDF generado se imprime y firma a mano; el
  sistema registra quién y cuándo lo generó (FR-713), no almacena firmas.
- **Art. IV.3 (instantánea congelada)**: los generadores leen `SpdLinea`/`SpdLineaEnvase` (ya
  instantáneas, Spec 006), nunca el catálogo de medicamentos actual.
- **Art. VIII.2 (stack fijado)**: motivo mismo de la corrección de FR-701 — se sigue el stack ya
  fijado (QuestPDF) en vez de proponer uno nuevo.
- **Art. XI (legibilidad)**: un único método de nombrado/carpeta/auditoría reutilizado por los
  cinco generadores (research.md Decisión 2), sin duplicar esa lógica.
- **Sin violaciones.**

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (sin cambios).

**Primary Dependencies**: QuestPDF (nueva, ya fijada en la Constitución Art. VIII.2 desde antes de
esta spec — research.md Decisión 1 sobre la licencia Community). El resto, sin cambios.

**Storage**: Sin migración nueva (spec.md §5, data-model.md).

**Testing**: xUnit (Dominio/Aplicación). Sin test Avalonia.Headless nuevo dedicado: los botones se
añaden a vistas ya probadas (Spec 001/006); se prueba que la vista sigue construyendo sin lanzar.

**Target Platform**: Windows 11 x64 portable, sin cambios.

**Project Type**: Aplicación de escritorio, 4 capas ya existentes — puebla `Spd.Dominio`
(`GeneradorNombreFichero`), `Spd.Aplicacion` (`IServicioGeneracionDocumentos`,
`TipoDocumentoGenerado`, `ResultadoGeneracionDocumento`), `Spd.Infraestructura`
(`ServicioGeneracionDocumentos`, E/S real con QuestPDF), `Spd.Presentacion` (botones "Imprimir" en
`PreparacionView`/`FichaPacienteView`).

**Performance Goals**: Sin objetivo numérico propio; generación bajo demanda, un documento a la
vez en esta iteración (el lote queda diferido).

**Constraints**: `QuestPDF.Settings.License` debe declararse una vez en `Program.cs` antes de
generar cualquier documento (research.md Decisión 1).

**Scale/Scope**: Cinco tipos de documento en esta iteración; el catálogo completo (FR-700) queda
como trabajo futuro sobre el mismo motor.
