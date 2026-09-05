# Data Model — Spec 000 (recorte específico de esta feature)

Fase 1 de `/speckit-plan`. Este fichero es el recorte de esta feature; la referencia global y
autoritativa sigue siendo [docs/data-model.md](../../docs/data-model.md) v0.5 — si algo difiere,
gana el documento global y se corrige este recorte.

## Farmacia (fila única)

Entidad de configuración global, una sola fila en toda la instalación (Art. IV.1). No tiene ciclo
de vida de alta/baja: existe desde que el asistente de primer arranque (FR-000) la crea.

| Campo | Origen FR | Regla |
|---|---|---|
| `codigo_sanitario`, `nombre`, `titular_o_comunidad_bienes`, `cif`, `direccion`, `poblacion`, `cp`, `telefono` | FR-010 | Obligatorios en el asistente y en Configuración |
| `logo`, `fax`, `email`, `whatsapp`, `titular_colegiado` | FR-010 | Opcionales |
| `logo` (histórico) | FR-011 | Al sustituir, el fichero anterior se copia a `logos_historico/<fecha>.<ext>` dentro de la carpeta de instalación; no hay tabla de negocio para el histórico |
| `responsable_datos`, `direccion_derechos`, `email_derechos` | FR-012 | Opcionales; si están vacíos, la capa de Aplicación resuelve el valor efectivo como `titular_o_comunidad_bienes`/`direccion` al generar el texto de consentimiento (Spec 002) — no se copian físicamente a estos campos |
| `prefijo_num_ficha`, `prefijo_num_spd` | FR-013 | Cambiarlos no reescribe `num_ficha`/`num_registro` ya asignados (CA-001) |
| `dia_retirada_defecto`, `n_blisteres_defecto`, `dias_antelacion_listado` | FR-020 | Cambiarlos no reescribe `Paciente.dia_retirada`/`n_blisteres` ya personalizados (CA-006) |
| `temp_min/max`, `hr_min/max` | FR-021 | Por defecto 15/25/40/60 |
| `umbral_reutilizacion_lectura_ambiental_horas` | FR-022 | Por defecto 2 |
| `ruta_backup` | FR-030 | Validada al guardar: existe + escribible; si no, aviso no bloqueante |
| `ruta_documentos_generados` | FR-031 | Misma validación que `ruta_backup` |
| `url_nomenclator` | FR-051 | Editable en cualquier momento, sin validar formato de URL más allá de sintaxis básica |

**Regla transversal FR-032**: si `ruta_backup` o `ruta_documentos_generados` resuelven (`Path.GetFullPath`) dentro de la carpeta de instalación de la app, se muestra un aviso al guardar; no se impide guardar.

## Usuario

| Campo | Origen FR | Regla |
|---|---|---|
| `nombre`, `apellidos`, `login` (único) | FR-040 | — |
| `rol` | FR-040 | `ADMINISTRADOR` \| `ELABORADOR` |
| `cargo_pnt`, `colegiado` | FR-040 | Opcionales, texto libre |
| `activo` | FR-043 | Baja lógica; nunca `DELETE` |
| `hash_password` | FR-041/044 | Argon2id (Decisión 2 de `research.md`) |
| `debe_cambiar_password` | FR-041/044 | Se pone a 1 al crear el usuario o al resetear contraseña; el login lo comprueba y fuerza cambio antes de continuar |
| `intentos_fallidos_consecutivos` | FR-045 | +1 por login fallido; reset a 0 en login correcto |
| `bloqueado` | FR-045 | Se pone a 1 al alcanzar el umbral (por defecto 5); solo un Administrador lo desbloquea (sin expiración automática, Q2) |

**Invariante FR-042** (verificado en Dominio, no en Aplicación ni en UI): una operación que
desactivaría o degradaría al último `ADMINISTRADOR` `activo=1` se rechaza antes de tocar la base de
datos, con un motivo explicable al usuario (CA-002).

## Rol

Sin tabla propia — `TEXT` con dominio cerrado `{ADMINISTRADOR, ELABORADOR}` validado en Dominio
(Art. VII.4). No se modela categoría profesional.

## Fuera de este recorte

`Paciente`, `Envase`, `SPD*`, `Medicamento`, etc. pertenecen a otras specs y no se tocan aquí; se
citan solo como consumidores de los valores que esta spec fija (p. ej. `Paciente.dia_retirada` lee
`Farmacia.dia_retirada_defecto` como valor inicial, Spec 001).
