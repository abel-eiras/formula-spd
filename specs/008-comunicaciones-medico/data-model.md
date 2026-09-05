# Fase 1 — Modelo de datos: Comunicaciones al médico

## Migración nueva: `ComunicacionMedico`

Columnas (docs/data-model.md §ComunicacionMedico, sin cambios de contenido, solo de
implementación):

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| paciente_id | INTEGER FK Paciente | |
| medico_id | INTEGER FK Medico | |
| tipo | TEXT | PRESENTACION / INCIDENCIA / TELEFONO |
| fecha | TEXT | Fecha de creación, inmutable (FR-807) |
| incidencias_detectadas | TEXT NULL | Obligatorio si `tipo = INCIDENCIA` (FR-802) |
| propuesta | TEXT NULL | Obligatorio si `tipo = INCIDENCIA` (FR-802) |
| respuesta | TEXT NULL | Único campo editable tras guardarse (FR-807) |
| fecha_respuesta | TEXT NULL | Se completa junto con `respuesta` |
| farmaceutico_id | INTEGER FK Usuario NULL | Usuario que registró la comunicación |
| creado_en, creado_por, modificado_en, modificado_por | | Convención ya usada en las demás tablas de negocio |

Sin `ComunicacionMedico_Hist`: no hay ediciones retroactivas de una comunicación, solo el añadido
de respuesta (Art. III/FR-807).

## Enums nuevos (`Spd.Dominio`)

- **TipoComunicacionMedico**: `Presentacion, Incidencia, Telefono`.

## Relaciones

`ComunicacionMedico` referencia `Paciente` (Spec 001) y `Medico` (Spec 001) por clave.
`FarmaceuticoId` referencia `Usuario` (Spec 000).
