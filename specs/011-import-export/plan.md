# Implementation Plan: Import/export con programas de gestión

**Branch**: `011-import-export` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/011-import-export/spec.md`

## Summary

Perfil de importación/exportación genérico y reutilizable (`PerfilImportacion`), extractor de
unidades por regex probado contra una muestra antes de guardar, y exportación de pacientes a CSV
respetando el mapeo explícito del perfil. FR-1101 (perfiles de fábrica) y la integración real con
el lector del nomenclátor (FR-1110 conectado a `LectorNomenclatorCsv`) quedan diferidos por falta
de datos reales (research.md Decisión 1 y 2), no por dificultad técnica.

## Constitution Check

- **Art. V (un dato, una entrada)**: la exportación reutiliza `PerfilImportacion.Mapeo` en sentido
  inverso en vez de crear una entidad `PerfilExportacion` duplicada (research.md Decisión 3).
- **Art. VI (aislamiento y portabilidad)**: import/export de ficheros locales no es una conexión
  de red; sin excepción que justificar.
- **Sin violaciones.**

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (sin cambios).

**Primary Dependencies**: Las mismas ya fijadas (Dapper, Microsoft.Data.Sqlite 10.0.10). Sin
dependencias nuevas — el CSV de exportación se genera con `string.Join`/`StringBuilder`, mismo
patrón que los ficheros CSV ya escritos/leídos en el proyecto.

**Storage**: SQLite, nueva migración `0008_perfiles_importacion.sql` (main ya tiene 0001-0007 tras
fusionar Specs 001/003/009/010/004/005/008).

**Testing**: xUnit (Dominio/Aplicación) + Avalonia.Headless.XUnit (Presentación), mismo patrón.

**Target Platform**: Windows 11 x64 portable, sin cambios.

**Project Type**: Aplicación de escritorio, 4 capas ya existentes — puebla `Spd.Dominio`
(`PerfilImportacion`, `TipoPerfilImportacion`, `ExtractorUnidadesEnvase`), `Spd.Aplicacion`
(`ServicioPerfilesImportacion`, `ServicioExportacionPacientes`), `Spd.Infraestructura`
(`RepositorioPerfilesImportacion`), `Spd.Presentacion` (pantalla de perfiles, pantalla de
exportación de pacientes).

**Performance Goals**: Sin objetivo numérico propio; volumen bajo por farmacia.

**Constraints**: FR-1131 (nunca exportar campos no mapeados explícitamente) es una regla de
seguridad de datos, no solo de negocio — se verifica con un test explícito (CA-1103).

**Scale/Scope**: Unos pocos perfiles por farmacia, exportaciones puntuales bajo demanda.
