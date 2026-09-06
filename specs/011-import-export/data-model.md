# Fase 1 — Modelo de datos: Import/export con programas de gestión

## Migración nueva: `PerfilImportacion`

Columnas (docs/data-model.md §PerfilImportacion, sin cambios de contenido, solo de
implementación):

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| nombre | TEXT UNIQUE | |
| tipo | TEXT | PACIENTES / DISPENSACIONES / MEDICAMENTOS / NOMENCLATOR |
| separador | TEXT | |
| codificacion | TEXT | Registrado, no aplicado todavía (research.md Decisión 4) |
| tiene_cabecera | INTEGER (bool) | |
| mapeo | TEXT (JSON) | Lista de pares `{campo, columna}`; en exportación se lee campo→columna (research.md Decisión 3) |
| regex_unidades_envase | TEXT NULL | Solo relevante en perfiles tipo NOMENCLATOR (FR-1110) |
| creado_en, creado_por, modificado_en, modificado_por | | Convención ya usada en las demás tablas de negocio |

Sin tabla separada de "PerfilExportacion": FR-1130 reutiliza `PerfilImportacion` (research.md
Decisión 3).

## Enums nuevos (`Spd.Dominio`)

- **TipoPerfilImportacion**: `Pacientes, Dispensaciones, Medicamentos, Nomenclator`.

## Relaciones

`PerfilImportacion` no referencia a nadie: es un perfil reutilizable de mapeo, igual que
`PerfilImportacionTratamiento` de Spec 005 pero para los cuatro tipos genéricos de FR-1100.
