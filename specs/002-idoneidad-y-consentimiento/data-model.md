# Fase 1 — Modelo de datos: Idoneidad y consentimiento informado

Migración `0011_idoneidad_consentimiento.sql`.

## EvaluacionIdoneidad (solo INSERT — FR-202, Art. III)

| Campo | Tipo | Notas |
|---|---|---|
| id | INTEGER PK | |
| paciente_id | FK Paciente | |
| fecha | TEXT | Fecha-hora del registro; la más reciente es la vigente |
| farmaceutico_id | FK Usuario, nulo | Quien evalúa |
| criterio_1 … criterio_7 | INTEGER 0/1 | Los siete criterios de inclusión del PNT I §4.1 (spec.md Clarifications) |
| condicion_motivacion, condicion_destreza | INTEGER 0/1 | Las dos condiciones "es importante que" |
| observaciones | TEXT | Obligatorio si NO_APTO o si el resultado difiere de la propuesta (FR-204) |
| resultado | TEXT | `APTO` / `NO_APTO` — decisión del farmacéutico |

Regla pura (Dominio): `ResultadoPropuesto()` = NO_APTO si falta alguna condición o ningún criterio; APTO si no.

## Consentimiento (INSERT + UPDATE de firma/revocación/impresión — nunca DELETE)

| Campo | Tipo | Notas |
|---|---|---|
| id | INTEGER PK | |
| paciente_id | FK Paciente | |
| tipo | TEXT | `PACIENTE` / `REPRESENTANTE` |
| contacto_id | FK Contacto, nulo | Obligatorio si `REPRESENTANTE`: contacto activo de tipo REPRESENTANTE_LEGAL o PERSONA_AUTORIZADA con DNI |
| fecha_creacion | TEXT | |
| fecha_firma | TEXT (fecha), nulo | Se informa cuando el papel está firmado (FR-212) |
| fecha_revocacion | TEXT (fecha), nulo | FR-214 |
| motivo_revocacion | TEXT | Obligatorio al revocar |
| impreso_en | TEXT, nulo | Última generación del documento `CONSENT` |

`Vigente` = `fecha_firma` informada y `fecha_revocacion` nula. El vigente del paciente es el más reciente que cumple eso (FR-215).

## Transiciones de Paciente (Spec 001 FR-006) disparadas aquí

- EVALUACION → ACTIVO: automática al cumplirse "evaluación vigente APTO ∧ consentimiento vigente" (FR-213), vía `IServicioPacientes.CambiarEstado`.
- ACTIVO → SUSPENDIDO: nunca automática; la pantalla la ofrece cuando `SugerirSuspension` (FR-214, caso límite NO_APTO).
