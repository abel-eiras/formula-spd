# Tratamientos del paciente

Lista de tratamientos vigentes del paciente y formulario para añadir o cambiar.

## Campos

Medicamento (del catálogo, por nombre o código nacional); **en SPD** (solo si el medicamento es apto); problema de salud; médico prescriptor (prerrellenado con el de cabecera); pauta **desayuno / almuerzo / cena / noche** en fracciones (0, ¼, ⅓, ½, ⅔, ¾, 1, 1½, 2…); días de la semana; vía y momento (p. ej. "con las comidas"); texto libre de pauta para los no incluidos; fechas de inicio y de prescripción inicial; tipo crónico o esporádico; conocimiento y cumplimiento, incidencias e intervención farmacéutica (PRM/RNM).

## Acciones

- **Guardar** un tratamiento nuevo.
- **Cambiar pauta**: cierra la fila vigente con fecha de fin y abre otra con la nueva pauta; el historial se conserva y la hoja de instrucciones muestra la fecha de la última modificación.
- **Finalizar / suspender** un tratamiento (no se borra).
- **Comunicar incidencia**: abre Comunicaciones con paciente y prescriptor prerrellenados ([[uso:comunicaciones-medico]]).
- **Ajuste de unidades**: si la pauta es fraccionada, la aplicación descuenta "entero más uno" por semana; puedes sobrescribir el valor con motivo.

Un tratamiento en estado **pendiente de revisión** (cambio referido por el paciente sin prescripción) bloquea "Preparar siguiente" hasta que lo revises.

Procedimiento relacionado: [[procedimiento:ficha-y-tratamiento]].
