# Fase 1 — Modelo de datos: Impresión, generación en lote y documentación base

Sin migración nueva: esta spec no añade tablas de negocio (spec.md §5). Reutiliza `Farmacia.RutaDocumentosGenerados`
(Spec 000), `SPD`/`SpdLinea`/`SpdLineaEnvase` (Spec 006) y `Paciente` (Spec 001) tal cual.

## Tipos nuevos (`Spd.Dominio`, sin persistencia)

- **GeneradorNombreFichero** (función pura): decide entre nombre largo y corto (FR-710/711).

## Tipos nuevos (`Spd.Aplicacion`)

- **TipoDocumentoGenerado** (enum): `Ficha, EtiquetaAnverso, EtiquetaReverso, Instrucciones,
  FichaPaciente` — los cinco tipos de esta iteración (spec.md §4.1).
- **ResultadoGeneracionDocumento** (record): `RutaCompleta`, `NombreFichero`.

## Relaciones

Ninguna nueva. `ResultadoGeneracionDocumento` no se persiste: la traza que sí se persiste es la
entrada de `Auditoria` (FR-713), igual que cualquier otra escritura de la aplicación.
