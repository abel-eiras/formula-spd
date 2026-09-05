# Data Model — Spec 003 (recorte específico de esta feature)

Fase 1 de `/speckit-plan`. Referencia global y autoritativa: [docs/data-model.md](../../docs/data-model.md) v0.5.

## Medicamento (catálogo, clave natural = CN)

| Campo | Origen FR | Regla |
|---|---|---|
| `cn` | FR-300/FR-306 | Único, 6 dígitos, no editable tras el alta |
| `nombre` | FR-300/FR-302 | Obligatorio junto con `cn` (alta mínima, CA-300) |
| `principio_activo`, `laboratorio` | FR-300 | Opcionales |
| `forma_farmaceutica` | FR-300 | Enum cerrado de 8 valores (research.md Decisión 1); opcional en el alta mínima |
| `nombre_normalizado` | FR-305 (adición) | No viene en `docs/data-model.md`; se añade igual que `busqueda_normalizada` en Spec 001 (misma Decisión 1 de esa spec) porque SQLite no puede normalizar tildes/mayúsculas en la propia consulta. Se recalcula en cada alta/edición de `nombre`. **Nota de coordinación entre ramas**: el `Normalizador` de Dominio se duplica idéntico al de la rama `001-pacientes-y-medicos` (todavía sin mergear) para no bloquear esta spec; al mergear ambas ramas en `main` quedará un solo fichero. |
| `apto_spd` | FR-301 | Derivado de `forma_farmaceutica` (research.md Decisión 2), editable a mano |
| `motivo_no_apto` | FR-301 | Texto libre; obligatorio solo si `apto_spd` se fija distinto del derivado (CA-302) |
| `fraccionable` | FR-300 | Booleano simple |
| `unidades_envase` | FR-310 | Editable a mano en cualquier momento |
| `unidades_envase_origen` | FR-311 | `MANUAL` / `IMPORTADO_REGEX`, informativo, nunca bloquea la edición |
| `desc_forma`, `desc_color`, `desc_ranura`, `desc_serigrafia`, `desc_tamano` | FR-303 | Opcionales; cualquier cambio dispara el versionado de Decisión 4 |
| `desc_texto` | FR-303 | Se propone (research.md Decisión 3), nunca se sobrescribe sola |
| `desc_vigente_desde` | FR-304 (adición, research.md Decisión 4) | Fecha desde la que el `desc_*` actual es vigente; no viene en `docs/data-model.md`, se añade para poder versionar |
| `gtin` | FR-300 | Opcional, solo almacenamiento (consumo real en Spec 012) |
| `activo` | FR-306 | Baja lógica; un alta con un `cn` ya existente y de baja reactiva la fila (CA-305), nunca crea otra |

**Invariante FR-306** (bloqueo real, no aviso): un `cn` no se duplica nunca; si ya existe **de
baja**, el alta lo reactiva; si ya existe **activo**, el alta se rechaza mostrando el existente
(CA-303).

**Invariante Art. IV.3**: esta tabla y `Medicamento_Hist` son el historial del *catálogo*; no
tienen relación con las instantáneas de `SPD_Linea` (Spec 006, fuera de esta spec) — cada línea de
SPD sigue congelando su propia copia al crearse, independiente de cuántas veces cambie después el
catálogo (CA-301).

## Medicamento_Hist (0..n por Medicamento)

| Campo | Origen FR | Regla |
|---|---|---|
| `medicamento_id`, `desc_forma`, `desc_color`, `desc_ranura`, `desc_serigrafia`, `desc_tamano`, `desc_texto` | FR-304 | Copia de los valores **anteriores** al cambio |
| `vigente_desde`, `vigente_hasta` | FR-304 | El periodo cerrado que describe esa fila (research.md Decisión 4) |

## Fuera de este recorte

`SPD_Linea` y su instantánea (Spec 006), `PerfilImportacion` configurable (Spec 011) no se tocan
aquí; se citan solo como consumidores/sustitutos futuros.
