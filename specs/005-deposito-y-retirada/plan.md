# Implementation Plan: Depósito de envases y listado de retirada

**Branch**: `005-deposito-y-retirada` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/005-deposito-y-retirada/spec.md`

## Summary

Depósito de envases en custodia por paciente, con listado de retirada calculado (no almacenado),
algoritmo de asignación/descuento que agota primero el envase empezado, salidas a SIGRE/entrega
fuera de blíster, y alta masiva de tratamiento+envase por copiar/pegar o fichero CSV. FR-520/521
(descuento disparado por SPD) y FR-530 (exclusión por SPD ya preparado) dependen de Spec 006, que
no existe en esta rama: se implementan como servicio/interfaz invocable con punto de extensión
documentado (research.md Decisiones 3 y 5), no como lógica ficticia de preparación.

## Constitution Check

- **Art. III (nada se borra)**: `Envase` no tiene operación de borrado; solo transiciones de
  estado (`EnCustodia → Agotado|ResiduoSigre|EntregadoPaciente`). Confirmado en data-model.md.
- **Art. IV (catálogo vivo, historia congelada)**: `Envase` no versiona nada por sí mismo (no es un
  catálogo); referencia a `Medicamento` y `Paciente` por clave, nunca copia sus datos salvo el
  campo de auditoría estándar. El campo `EntregadoA` es la única extensión de campo sobre el
  modelo publicado en `docs/data-model.md` (research.md Decisión 8) — se documentará allí al
  mergear.
- **Art. V (un dato, una entrada)**: `unidades_envase` sigue viviendo solo en `Medicamento` (Spec
  003); esta spec solo lo lee. `DiaRetirada`/`NBlisteres` siguen solo en `Paciente`/`Farmacia`
  (Specs 000/001); esta spec solo los lee (research.md Decisión 1: sin migración para ellos).
- **Art. VII (seguridad y acceso)**: sin cambios; reutiliza `RegistradorAuditoria` para FR-560.
- **Sin violaciones.** No se necesita justificar ninguna excepción.

## Technical Context

**Language/Version**: C# sobre .NET 8 LTS (sin cambios).

**Primary Dependencies**: Las mismas ya fijadas (Avalonia UI 11, Dapper, Microsoft.Data.Sqlite
10.0.10, Serilog). Sin dependencias nuevas (research.md Decisión 7: CSV con `string.Split`, sin
librería de Excel).

**Storage**: SQLite, nueva migración `0006_envase.sql` (main ya tiene 0001-0005 tras fusionar
Specs 001/003/009/010/004).

**Testing**: xUnit (Dominio/Aplicación) + Avalonia.Headless.XUnit (Presentación), mismo patrón.

**Target Platform**: Windows 11 x64 portable, sin cambios.

**Project Type**: Aplicación de escritorio, 4 capas ya existentes — puebla `Spd.Dominio` (`Envase`,
`EstadoEnvase`, `OrigenEnvase`, `MotivoSalidaEnvase`, `PerfilImportacionTratamiento`,
`OrigenImportacionTratamiento`), `Spd.Aplicacion` (`ServicioEnvases`, `ServicioAsignacionEnvases`,
`CalculadoraUnidadesADescontar`, `ServicioListadoRetirada`, `ServicioImportacionTratamientoEnvase`),
`Spd.Infraestructura` (`RepositorioEnvases`, `RepositorioPerfilesImportacionTratamiento`),
`Spd.Presentacion` (pestaña Depósito en la ficha de paciente, pantalla Retirada de envases desde
`MainWindow`, pantalla Importar tratamiento).

**Performance Goals**: Sin objetivo numérico propio; el listado de retirada es una consulta sobre
volumen bajo por farmacia (decenas de pacientes), recalculada bajo demanda (FR-537).

**Constraints**: Ningún envase se reasigna ni se borra (Art. III); SQL explícito sin ORM salvo
Dapper como mapeador fino (Art. VIII.4, patrón ya establecido); el punto de extensión hacia Spec
006 no debe obligar a cambiar la firma pública de los servicios de esta spec cuando se complete.

**Scale/Scope**: Varios envases por paciente/medicamento a lo largo del tiempo, volumen bajo por
farmacia.
