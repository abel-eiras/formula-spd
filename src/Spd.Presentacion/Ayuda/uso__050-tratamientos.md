# Tratamientos del paciente

Pestaña **Tratamiento** del paciente: sus tratamientos vigentes a la izquierda y el formulario para añadir o cambiar a la derecha.

## Campos

Medicamento (del catálogo, por nombre o código nacional); **en SPD** (solo si el medicamento es apto); problema de salud; médico prescriptor —se busca **por nombre**, no por número, y se puede dar de alta ahí mismo ([[uso:medicos]])—; pauta **desayuno / almuerzo / cena / noche** en fracciones (0, ¼, ⅓, ½, ⅔, ¾, 1, 1½, 2…); días de la semana; vía y momento (p. ej. "con las comidas"); texto libre de pauta para los no incluidos; fechas de inicio y de prescripción inicial; tipo crónico o esporádico; conocimiento y cumplimiento, incidencias e intervención farmacéutica (PRM/RNM).

## Acciones

- **Guardar** un tratamiento nuevo.
- **Cambiar pauta**: cierra la fila vigente con fecha de fin y abre otra con la nueva pauta; el historial se conserva y la hoja de instrucciones muestra la fecha de la última modificación.
- **Finalizar / suspender** un tratamiento (no se borra).
- **Ver historial**: abre un panel con todo lo que este paciente ha tomado de ese medicamento, del tramo más reciente al más antiguo, con la pauta y los días que estuvieron en vigor en cada periodo y por qué terminó cada uno. Es la forma de responder a «¿qué tomaba y desde cuándo?» cuando llama el médico: como cada cambio de pauta cierra un tramo y abre otro, la ficha actual sola no lo cuenta.
- **Comunicar incidencia**: **cambia a la pestaña de Comunicaciones** con el paciente y el prescriptor ya puestos ([[uso:comunicaciones-medico]]); no se abre ninguna ventana y el tratamiento sigue a un clic.
- **Ajuste de unidades**: si la pauta es fraccionada, la aplicación descuenta "entero más uno" por semana; puedes sobrescribir el valor con motivo.

Un tratamiento en estado **pendiente de revisión** (cambio referido por el paciente sin prescripción) bloquea "Preparar siguiente" hasta que lo revises.

Procedimiento relacionado: [[procedimiento:ficha-y-tratamiento]].
