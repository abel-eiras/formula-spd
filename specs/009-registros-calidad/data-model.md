# Data Model — Spec 009 (recorte específico de esta feature)

Fase 1 de `/speckit-plan`. Referencia global: [docs/data-model.md](../../docs/data-model.md) v0.5
+ `SPD_Diseno_Tecnico_Fase1_Arquitectura_y_Datos.md` líneas 441-446 (con la corrección de
research.md Decisión 1).

## RegistroAmbiental

| Campo | Origen FR | Regla |
|---|---|---|
| `fecha`, `hora` | FR-900/FR-902 | Prerrellenadas a "ahora", editables |
| `temperatura`, `humedad` | FR-900 | Obligatorias |
| `usuario_id` | FR-900 | El usuario que registra |
| `observaciones` | FR-900 | Opcional |
| `spd_id` | FR-900 | Nullable; siempre null en esta spec (Spec 006 no existe todavía — spec.md Assumptions) |
| `fuera_rango` | FR-901 | Calculado y congelado al crear (research.md Decisión 2, CA-900) |

## RegistroLimpieza

| Campo | Origen FR | Regla |
|---|---|---|
| `fecha`, `usuario_id` | FR-910 | Prerrellenados a "ahora"/usuario actual (FR-911, CA-901) |
| `tipo` | FR-910 | Enum cerrado: `PRE_PREPARACION`, `POST_PREPARACION`, `RUTINARIA` |
| `observaciones` | FR-910 | Opcional |

## FormacionPersonal

| Campo | Origen FR | Regla |
|---|---|---|
| `usuario_id`, `nombre_curso`, `entidad_organizadora`, `fecha`, `acreditado` | FR-920 | Sin distinción de categoría profesional (research.md Decisión 1, se ignora `tipo`/`formador_id` de Fase 1) |

**Invariante FR-921** (Art. III): nunca se sustituye una formación anterior; todas se acumulan y
son consultables (CA-902).

## RecogidaResiduos

| Campo | Origen FR | Regla |
|---|---|---|
| `fecha`, `empresa_gestora`, `usuario_id`, `observaciones` | FR-930 | Solo residuos no SIGRE (material de acondicionamiento sobrante, etc.) |

## ControlCambiosPNT

| Campo | Origen FR | Regla |
|---|---|---|
| `documento`, `version`, `descripcion_cambio`, `fecha`, `redactado_por`, `revisado_por`, `aprobado_por` | FR-940 | Solo Administrador (FR-942, CA-903, research.md Decisión 4) |

## ControlCopias

| Campo | Origen FR | Regla |
|---|---|---|
| `documento`, `num_copia`, `usuario_id`, `fecha` | FR-941 | Solo Administrador (FR-942, CA-903) |

## Farmacia (adición — research.md Decisión 3)

| Campo | Origen FR | Regla |
|---|---|---|
| `umbral_dias_aviso_calidad` | FR-950 (Clarifications Q1) | INTEGER NOT NULL DEFAULT 7; único umbral para ambiental y limpieza (CA-904) |

## Fuera de este recorte

`Preparacion`/`SPD` (Spec 006, de donde vendría un `RegistroAmbiental.spd_id` no nulo) no existen
todavía y no se tocan aquí.
