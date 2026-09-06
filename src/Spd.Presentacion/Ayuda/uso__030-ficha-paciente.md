# Ficha del paciente

Todo lo del paciente está en **un solo espacio**, con una cabecera fija y siete pestañas: Datos, Idoneidad y consentimiento, Tratamiento, Depósito, Preparación, Comunicaciones y Documentos. No se abre ninguna ventana: se cambia de pestaña, y la cabecera sigue delante en todas.

## Cabecera fija

Nombre, número de ficha (correlativo con el prefijo configurado; se asigna al guardar y no se edita), estado (EVALUACION → ACTIVO ↔ SUSPENDIDO; cualquiera → BAJA; BAJA → EVALUACION para reactivar), si la idoneidad y el consentimiento están en regla, día de retirada, blísteres por sesión, blísteres sin entregar y, en rojo y a la vista, las alergias.

Está siempre visible, se esté en la pestaña que se esté: es lo que no se puede perder de vista mientras se trabaja con este paciente.

## Pestañas

Cada pestaña lleva un número cuando tiene algo pendiente: la idoneidad si no está en regla, el depósito si hay faltantes, la preparación si hay blísteres sin entregar y las comunicaciones si hay alguna sin respuesta del médico. Así se ve lo que falta sin entrar a mirar una por una.

Lo que se hace en una pestaña se ve en el resto sin recargar nada: al registrar el consentimiento, la cabecera pasa a ACTIVO en el acto.

## Datos

Nombre y apellidos (obligatorios); sexo (obligatorio si hay CIP); DNI, fecha de nacimiento, nº de Seguridad Social, CIP (se valida y puede autocompletarse); dirección, código postal, población, teléfonos, email; enfermedades crónicas, alergias e intolerancias, observaciones; pictogramas de comidas e identificador visual (útiles cuando dos personas conviven); **día de retirada** y **número de blísteres** (1 o 2), que fijan el listado de retirada y cuántos SPD crea cada sesión.

Es obligatorio al menos uno de: DNI, CIP o fecha de nacimiento. Si el DNI o el CIP coinciden con otro paciente, la aplicación avisa y te deja continuar pulsando Guardar de nuevo.

**Guardar** crea o actualiza el paciente. Los cambios quedan en auditoría con el antes y el después.

En un paciente nuevo solo está la pestaña de Datos: hasta que no se guarda no hay ficha a la que colgar tratamientos, envases ni preparaciones. Al guardar por primera vez aparecen las otras seis, sin cerrar ni volver a abrir nada.

Las demás pestañas: [[uso:idoneidad-consentimiento]], [[uso:tratamientos]], [[uso:deposito]], [[uso:preparacion]], [[uso:comunicaciones-medico]] y **Documentos** (ficha del paciente e información de protección de datos, [[uso:documentos-generados]]).

Nada se elimina: la baja pide fecha y motivo y conserva todo.

Procedimiento relacionado: [[procedimiento:ficha-y-tratamiento]].
