# Ficha del paciente

## Cabecera

Número de ficha (correlativo con el prefijo configurado; se asigna al guardar y no se edita), estado (EVALUACION → ACTIVO ↔ SUSPENDIDO; cualquiera → BAJA; BAJA → EVALUACION para reactivar), edad y un aviso rojo si hay alergias.

## Datos

Nombre y apellidos (obligatorios); sexo (obligatorio si hay CIP); DNI, fecha de nacimiento, nº de Seguridad Social, CIP (se valida y puede autocompletarse); dirección, código postal, población, teléfonos, email; enfermedades crónicas, alergias e intolerancias, observaciones; pictogramas de comidas e identificador visual (útiles cuando dos personas conviven); **día de retirada** y **número de blísteres** (1 o 2), que fijan el listado de retirada y cuántos SPD crea cada sesión.

Es obligatorio al menos uno de: DNI, CIP o fecha de nacimiento. Si el DNI o el CIP coinciden con otro paciente, la aplicación avisa y te deja continuar pulsando Guardar de nuevo.

## Botones

- **Guardar**: crea o actualiza. Los cambios quedan en auditoría con el antes y el después.
- **Idoneidad y consentimiento**: [[uso:idoneidad-consentimiento]].
- **Tratamientos**: [[uso:tratamientos]]. **Depósito**: [[uso:deposito]]. **Comunicaciones**: [[uso:comunicaciones-medico]]. **Preparación**: [[uso:preparacion]].
- **Imprimir ficha**: genera la Ficha del paciente (Anexo I.E) con familiar/cuidador, médico, salud, evaluación de idoneidad vigente, medicamentos incluidos y no incluidos y control de adherencia.
- **Imprimir información de protección de datos (RGPD)**: el documento que se entrega con el consentimiento.

Los documentos se guardan en la carpeta configurada ([[uso:documentos-generados]]). Nada se elimina: la baja pide fecha y motivo y conserva todo.

Procedimiento relacionado: [[procedimiento:ficha-y-tratamiento]].
