# Fase 1 — Modelo de datos: Depósito de envases y listado de retirada

## Migración nueva: `Envase`

Columnas (docs/data-model.md §Envase, con la extensión `EntregadoA` documentada en research.md
Decisión 8):

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| paciente_id | INTEGER FK Paciente | Nunca se reasigna (FR-544, CA-509) |
| medicamento_id | INTEGER FK Medicamento | |
| serie | TEXT NULL UNIQUE | NULL solo para entrega fuera de blíster (FR-543); único entre no-NULL (FR-512, CA-504) |
| lote | TEXT NULL | |
| caducidad | TEXT NULL | Fecha completa ISO (FR-512/Q2); NULL solo para entrega fuera de blíster |
| unidades_iniciales | INTEGER NULL | NULL solo para entrega fuera de blíster |
| unidades_restantes | INTEGER NULL | Se actualiza en cada descuento (FR-520) |
| fecha_entrada | TEXT | |
| origen | TEXT | ESCANEADO / MANUAL / IMPORTADO |
| estado | TEXT | EN_CUSTODIA / AGOTADO / RESIDUO_SIGRE / ENTREGADO_PACIENTE |
| fecha_salida | TEXT NULL | |
| motivo_salida | TEXT NULL | CESE_TRATAMIENTO / CAMBIO_TRATAMIENTO / CADUCADO / DETERIORADO / FALLECIMIENTO / BAJA_PACIENTE / OTRO |
| motivo_salida_detalle | TEXT NULL | Solo relevante con motivo OTRO |
| entregado_a | TEXT NULL | Solo para ENTREGADO_PACIENTE (FR-543) |
| creado_en, creado_por, modificado_en, modificado_por | | Convención ya usada en las demás tablas de negocio |

Sin `Envase_Hist`: no hay ediciones retroactivas de un envase, solo transiciones de estado
auditadas (FR-560) y actualización de `unidades_restantes`. Ningún estado se revierte (Art. III).

## Migración nueva: `PerfilImportacionTratamiento`

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| nombre | TEXT UNIQUE | |
| origen | TEXT | PORTAPAPELES / FICHERO |
| separador | TEXT NULL | Solo si origen = FICHERO (research.md Decisión 7: CSV en esta iteración) |
| tiene_cabecera | INTEGER (bool) NULL | Solo si origen = FICHERO |
| mapeo | TEXT (JSON) | `{"cn": "<columna>", "num_serie": "<columna>", "lote": "<columna>", "caducidad": "<columna>"}` |
| creado_en, creado_por | | |

## Enums nuevos (`Spd.Dominio`)

- **EstadoEnvase**: `EnCustodia, Agotado, ResiduoSigre, EntregadoPaciente`.
- **OrigenEnvase**: `Escaneado, Manual, Importado`.
- **MotivoSalidaEnvase**: `CeseTratamiento, CambioTratamiento, Caducado, Deteriorado,
  Fallecimiento, BajaPaciente, Otro`.
- **OrigenImportacionTratamiento**: `Portapapeles, Fichero`.

## Relaciones

`Envase` referencia `Paciente` (Spec 001) y `Medicamento` (Spec 003) por clave. El vínculo con
`Tratamiento` (Spec 004) es indirecto, por `medicamento_id` + `paciente_id` — un envase no
pertenece a una fila de tratamiento concreta (que puede cerrarse y abrirse, Art. IV.3), sino al
medicamento que el paciente tiene en custodia mientras algún tratamiento activo lo cubra.

`PerfilImportacionTratamiento` no referencia a nadie: es un perfil reutilizable de mapeo de
columnas, igual que `PerfilImportacion` de Spec 011 pero con su propio conjunto mínimo de campos
(CN, serie, lote, caducidad) según FR-571.
