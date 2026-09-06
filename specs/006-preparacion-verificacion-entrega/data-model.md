# Fase 1 — Modelo de datos: Preparación, verificación y entrega del SPD

Todas las columnas de `SPD`, `SPD_Linea`, `SPD_Linea_Envase`, `SPD_Verificacion`,
`SPD_Modificacion` son las ya publicadas en `docs/data-model.md` (sin cambios de contenido, solo
de implementación). `RegistroAmbiental` y `MaterialAcondicionamiento` son nuevas de esta spec
(research.md Decisión 4).

## Migración nueva: `0009_preparacion.sql`

### MaterialAcondicionamiento (catálogo)

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| descripcion | TEXT NOT NULL | |
| lote | TEXT NOT NULL | |
| fecha_entrada | TEXT NOT NULL | |
| activo | INTEGER NOT NULL DEFAULT 1 | |

### RegistroAmbiental

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| fecha | TEXT NOT NULL | |
| temperatura | REAL NOT NULL | |
| humedad | REAL NOT NULL | |
| fuera_rango | INTEGER NOT NULL | Congelado contra `Farmacia.temp_min/max`/`hr_min/max` en el momento del registro (mismo patrón que Spec 009) |
| usuario_id | INTEGER FK Usuario | |

### SPD

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| num_registro | TEXT UNIQUE | `{Farmacia.PrefijoNumSpd}{correlativo:D6}` (research.md Decisión 7) |
| correlativo_num_registro | INTEGER | Fuente de verdad del correlativo, independiente del prefijo (mismo patrón que `Paciente.correlativo_num_ficha`) |
| paciente_id | INTEGER FK Paciente | |
| version | INTEGER NOT NULL DEFAULT 1 | |
| sesion_id | TEXT NOT NULL | GUID; agrupador, no FK a tabla de negocio (CA-611) |
| validez_desde, validez_hasta | TEXT NOT NULL | 7 días exactos |
| fecha_preparacion | TEXT NULL | Se rellena al pasar a PREPARADO |
| material_id | INTEGER FK MaterialAcondicionamiento NULL | |
| registro_ambiental_id | INTEGER FK RegistroAmbiental NULL | |
| elaborador_id | INTEGER FK Usuario NOT NULL | |
| verificador_id | INTEGER FK Usuario NULL | |
| fecha_verificacion | TEXT NULL | |
| excepcion_verificador_motivo | TEXT NULL | |
| resultado_verificacion | TEXT NULL | Apto / NoApto |
| entregador_id | INTEGER FK Usuario NULL | |
| fecha_entrega | TEXT NULL | |
| entregado_a | TEXT NULL | |
| primera_entrega | INTEGER NOT NULL DEFAULT 0 | |
| spd_anterior_recogido | INTEGER NULL | bool; vacío hasta la entrega |
| unidades_no_administradas | TEXT NULL | JSON `{medicamentoId: unidades}` del SPD anterior (simplificación de implementación, ver research.md) |
| observaciones_adherencia | TEXT NULL | |
| cambios_medicacion_preguntado | INTEGER NOT NULL DEFAULT 0 | |
| observaciones_etiqueta | TEXT NULL | |
| estado | TEXT NOT NULL DEFAULT 'Borrador' | Borrador/Preparado/Verificado/Entregado/Anulado |
| motivo_anulacion | TEXT NULL | |
| impreso_ficha_en, impreso_etiquetas_en, impreso_instrucciones_en | TEXT NULL | |
| creado_en, creado_por, modificado_en, modificado_por | | Convención ya usada |

### SPD_Linea

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| spd_id | INTEGER FK SPD | |
| tratamiento_id | INTEGER FK Tratamiento | |
| medicamento_id | INTEGER FK Medicamento | |
| snap_nombre, snap_cn | TEXT NOT NULL | |
| snap_pauta_d, snap_pauta_a, snap_pauta_c, snap_pauta_n | TEXT NULL | Nombre de `FraccionDosis` |
| snap_dias_semana | TEXT NOT NULL | |
| snap_desc_texto | TEXT NULL | |
| snap_momento | TEXT NULL | |
| unidades_dosis | REAL NOT NULL | Suma semanal real (puede ser fraccionaria) |
| unidades_envase | INTEGER NOT NULL | Unidades enteras a descontar |
| incidencias | TEXT NULL | |
| estado_linea | TEXT NOT NULL DEFAULT 'Normal' | Normal/Excluida/EnvasePendiente (research.md Decisión 5) |
| motivo_exclusion | TEXT NULL | |

### SPD_Linea_Envase

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| spd_linea_id | INTEGER FK SPD_Linea | |
| envase_id | INTEGER FK Envase | |
| unidades_tomadas | REAL NOT NULL | |
| snap_serie, snap_lote, snap_caducidad | TEXT NULL | Copiados del envase en el momento del descuento |

### SPD_Verificacion

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| spd_id | INTEGER FK SPD | |
| verificador_id | INTEGER FK Usuario | |
| fecha | TEXT NOT NULL | |
| verif_aspecto, verif_etiqueta_datos, verif_etiqueta_validez, verif_instrucciones, verif_contenido | INTEGER NOT NULL | bool, los 5 ítems del checklist |
| resultado | TEXT NOT NULL | Apto/NoApto |
| excepcion_motivo | TEXT NULL | |

### SPD_Modificacion

| Campo | Tipo SQL | Notas |
|---|---|---|
| id | INTEGER PK | |
| spd_id | INTEGER FK SPD | |
| version_anterior, version_nueva | INTEGER NOT NULL | |
| fecha | TEXT NOT NULL | |
| usuario_id | INTEGER FK Usuario | |
| origen_solicitud | TEXT NOT NULL | Paciente/Familiar/Medico/Farmaceutico/Otro |
| motivo | TEXT NOT NULL | |
| resumen_cambios | TEXT NOT NULL | JSON |
| lineas_snapshot_anterior | TEXT NOT NULL | JSON: copia íntegra de líneas + filas de envase |

## Enums nuevos (`Spd.Dominio`)

- **EstadoSpd**: `Borrador, Preparado, Verificado, Entregado, Anulado`.
- **EstadoLinea**: `Normal, Excluida, EnvasePendiente`.
- **ResultadoVerificacion**: `Apto, NoApto`.
- **OrigenSolicitudReelaboracion**: `Paciente, Familiar, Medico, Farmaceutico, Otro`.

## Relaciones

`SPD` referencia `Paciente`, `MaterialAcondicionamiento`, `RegistroAmbiental` y `Usuario` (varias
veces, por rol) por clave. `SPD_Linea` referencia `Tratamiento` y `Medicamento`. `SPD_Linea_Envase`
referencia `Envase` (Spec 005). Ninguna de estas referencias copia datos del catálogo: cada línea
guarda su propia instantánea (Art. IV.3).
