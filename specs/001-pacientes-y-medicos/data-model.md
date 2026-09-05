# Data Model — Spec 001 (recorte específico de esta feature)

Fase 1 de `/speckit-plan`. Referencia global y autoritativa: [docs/data-model.md](../../docs/data-model.md) v0.6.

## Medico (catálogo)

| Campo | Origen FR | Regla |
|---|---|---|
| `nombre`, `apellidos` | FR-031 | Obligatorios |
| `colegiado`, `especialidad` (defecto "Medicina de familia"), `centro`, `telefono`, `email`, `direccion` | FR-030 | Opcionales |
| `activo` | FR-036 | Baja lógica; solo permitida si no es cabecera de ningún paciente ACTIVO/EVALUACION (CA-007) |
| `busqueda_normalizada` | FR-032 | Recalculada en cada alta/edición (Decisión 1) |

**Invariante FR-034** (aviso, no bloqueo): al crear, si existe otro médico activo con los mismos
apellidos+nombre (normalizado) o el mismo `colegiado`, se informa al usuario, que decide si
continúa.

**Invariante FR-035**: ninguna otra tabla copia los campos del médico. `Paciente.medico_id` es la
única referencia; editar el médico se refleja de inmediato en toda ficha que lo use (CA-006).

## Paciente

| Campo | Origen FR | Regla |
|---|---|---|
| `num_ficha` (calculado, inmutable) | FR-001 | `prefijo_num_ficha_vigente + correlativo_num_ficha.ToString("D6")`, una sola vez al crear (CA-001) |
| `correlativo_num_ficha` | FR-001 | `MAX(correlativo_num_ficha) + 1` en toda la tabla, independiente del prefijo (Decisión 3) |
| `nombre`, `apellidos` | FR-003 | Obligatorios |
| `sexo` | FR-002b | Obligatorio solo si `cip` informado |
| `dni`, `cip`, `fecha_nacimiento` | FR-003 | Al menos uno de los tres obligatorio (CA-002) |
| resto de campos de FR-002 | FR-002 | Opcionales |
| `dia_retirada`, `n_blisteres` | FR-002c | Prerrellenados desde `Farmacia.dia_retirada_defecto`/`n_blisteres_defecto` al crear; editables después sin volver a leer el valor por defecto (mismo patrón que Spec 000 CA-006) |
| `estado` | FR-006 | `EVALUACION` al crear; transiciones: `EVALUACION→ACTIVO`, `ACTIVO↔SUSPENDIDO`, cualquiera→`BAJA`, `BAJA→EVALUACION` |
| `fecha_baja`, `motivo_baja`, `motivo_baja_detalle` | FR-007 | Exigidos al pasar a `BAJA`; `motivo_baja_detalle` solo si `motivo_baja='OTRO'` |
| `busqueda_normalizada` | FR-010 | Recalculada en cada alta/edición (Decisión 1) |

**Invariante FR-004** (aviso, no bloqueo): al crear/editar, si existe otro paciente **activo** (no
de baja) con el mismo DNI o CIP, se informa con nombre y número de ficha del existente.

**Invariante FR-005/005b**: la validación de DNI/NIE y CIP vive en `Spd.Dominio`
(`ValidadorDni`/`ValidadorCip`, research.md Decisiones 4/5), siempre como aviso, nunca bloqueo.

**Invariante estado→baja (FR-007, CA-009)**: dar de baja no toca ninguna otra tabla (tratamientos,
envases, SPD de otras specs); estos siguen existiendo y consultables sin cambios.

## Contacto (0..n por Paciente)

| Campo | Origen FR | Regla |
|---|---|---|
| `paciente_id`, `tipo` (`FAMILIAR`/`REPRESENTANTE_LEGAL`/`PERSONA_AUTORIZADA`/`CUIDADOR`) | FR-020 | — |
| `nombre`, `apellidos`, `telefono`, `email` | FR-020 | Opcionales salvo lo indicado abajo |
| `dni` | FR-022/FR-021c | Obligatorio si `tipo` es `REPRESENTANTE_LEGAL`/`PERSONA_AUTORIZADA`, u obligatorio si `retira_medicacion=1` |
| `es_principal` | FR-021 | Como mucho un contacto por paciente con `es_principal=1` (CA constraint de aplicación, no de BD: se desmarca el anterior al marcar uno nuevo) |
| `retira_medicacion` | FR-021b | Como mucho un contacto por paciente con `retira_medicacion=1`; si ninguno lo tiene, el paciente retira su propia medicación (Spec 005 lo consume) |
| `activo` | FR-023 | Baja lógica; oculto de la ficha salvo "ver histórico" |

## Fuera de este recorte

`Tratamiento`, `Envase`, `SPD*`, `EvaluacionIdoneidad`, `Consentimiento` pertenecen a otras specs y
no se tocan aquí; se citan solo como consumidores futuros de `Paciente`/`Contacto`/`Medico`.
