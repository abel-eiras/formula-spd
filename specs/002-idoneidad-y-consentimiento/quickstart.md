# Quickstart — validación de Spec 002

## Escenario 1 — Alta completa hasta ACTIVO (US1+US2, CA-200)

1. Paciente en EVALUACION. Abrir "Idoneidad y consentimiento" desde su ficha.
2. Marcar criterios y las dos condiciones → la app propone APTO; confirmar y guardar.
3. Crear consentimiento tipo PACIENTE → "Imprimir consentimiento" genera el PDF (Anexo I.B).
4. Registrar la fecha de firma → el paciente pasa a ACTIVO automáticamente (mensaje en pantalla).

## Escenario 2 — Representante sin contacto (US3, CA-202)

1. Paciente sin contactos de tipo representante. Elegir tipo REPRESENTANTE → el selector está vacío y
   aparece el formulario "Nuevo representante" (nombre, apellidos, DNI obligatorio).
2. Crear el contacto → queda seleccionado → crear consentimiento → imprimir → firmar.

## Escenario 3 — Revocación (US4, CA-203)

1. Revocar el consentimiento vigente con motivo → sigue en el historial con fecha de firma y de
   revocación; aparece el aviso y el botón "Pasar a SUSPENDIDO".

## Escenario 4 — Bloqueo de preparación (CA-201)

1. Registrar una nueva evaluación NO_APTO (con observaciones). Ir a Preparación → "Nueva sesión" se
   rechaza por idoneidad/consentimiento.

## Escenario 5 — Ficha del paciente

1. "Imprimir ficha" desde la ficha del paciente: el bloque de idoneidad muestra la evaluación vigente.
