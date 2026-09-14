# Preparación y listado de preparaciones

## Preparaciones (sección «Preparaciones» de la navegación)

Una **tabla** con todos los blísteres de todos los pacientes: se ordena pulsando en la cabecera de cualquier columna, y hay filtro por paciente y estado y la casilla «solo pendientes». Lo que marques para el lote **no se pierde al reordenar**: la selección va con la fila, no con su posición. Cada fila indica **envases al día**: SÍ cuando la próxima sesión del paciente puede prepararse sin intervención (activo, con idoneidad, tratamiento revisado y saldo suficiente); si NO, el motivo. «Abrir» lleva a la pestaña de preparación de ese paciente.

**Nueva preparación…** abre un panel para buscar al paciente (dos letras bastan) sin pasar por su ficha. Cada paciente encontrado muestra su última preparación; **Preparar siguiente (desde la última)** aparece cuando la última está entregada, y **Sesión nueva** abre una en blanco. Si la sesión se abre, vas directamente a su pestaña de preparación; si no se puede (paciente no activo, sin consentimiento, sin tratamiento en SPD, faltantes…), el motivo se queda en el panel.

**Lote**: marca los pacientes (o "Seleccionar los que están al día"), elige ficha, etiquetas e instrucciones, y "Generar documentos": para cada paciente la aplicación localiza su sesión pendiente o crea una nueva, la pasa a PREPARADO con el primer material activo y la última lectura ambiental reciente, y genera los documentos. El resumen indica generados, excluidos (con el motivo) y fallidos; un fallo no detiene al resto.

## Preparación del paciente

Es la quinta pestaña del paciente. Muestra sus blísteres (SPD), el más reciente arriba, con su número de registro, estado, versión y periodo de validez.

### El carril de pasos

Cada blíster lleva encima un carril con sus cinco pasos: **llenado, etiquetado, instrucciones, verificación y entrega**. Cada paso está hecho, en curso o bloqueado, y **si está bloqueado dice por qué** —«requiere verificación», «no hay material de acondicionamiento registrado»— antes de que lo intentes, en vez de dejarte pulsar y devolverte un error. Los botones de esos pasos se apagan cuando no toca.

### La rejilla de alvéolos

Debajo, el blíster **como se ve**: cuatro filas (desayuno, almuerzo, cena y noche) por siete columnas, una por día. En cada alvéolo, la inicial del medicamento y su fracción (`E 1/2`). Es lo que evita traducir mentalmente «½-0-1-0, de lunes a viernes» a la posición física de cada hueco, que es justo donde se equivoca una persona cansada.

Las columnas son los **días reales de validez del blíster**, con su fecha: si empieza en jueves, la primera columna es el jueves. Un tratamiento de «solo lunes» aparece en la columna del lunes, no en la primera.

## Aptitud para SPD sin confirmar

Si algún medicamento de un blíster que todavía se va a emblistar tiene la aptitud **sin confirmar** —lo normal en los que llegan del nomenclátor, que no trae ese dato—, arriba aparece un aviso con sus nombres. **Confirmo que todos son aptos para SPD** los marca aptos a la vez y deja registrado quién lo confirmó. Asegurar que lo que se emblista es apto es responsabilidad del farmacéutico: el aviso **no bloquea** nada. Un medicamento marcado «no apto» en el catálogo se advierte aparte, en rojo, y no se cambia desde aquí.

## Barra superior

- **Nueva sesión de preparación**: crea 1 o 2 blísteres de 7 días consecutivos con las líneas del tratamiento activo. Exige paciente ACTIVO con idoneidad y consentimiento, tratamiento revisado y sin faltantes en el listado de retirada.
- **Preparar siguiente**: copia la última sesión y avisa de líneas modificadas, nuevas, eliminadas o sin envase.
- **Imprimir instrucciones de la sesión**: una sola hoja para los dos blísteres si tienen el mismo contenido (PNT I §4.4.1: la hoja es "en cada entrega").
- **Entrega**: "Entregado a", primera entrega, SPD anterior recogido, unidades no administradas, observaciones de adherencia y "refiere cambios de medicación"; "Entregar todos los pendientes" entrega de una vez los VERIFICADOS. Si se marcan cambios referidos, los tratamientos quedan pendientes de revisión y **se cambia a la pestaña de Comunicaciones** con la carta ya preparada para el **médico de cabecera** del paciente.

## Condiciones ambientales y material

Temperatura y humedad (si las dejas vacías y hay una lectura reciente, se reutiliza); selector de material de acondicionamiento y alta rápida de material nuevo (descripción y lote).

## Por cada blíster

- Líneas con medicamento, unidades por envase y estado; **Registrar envase** en la línea si falta saldo.
- **Pasar a preparado**: valida saldo en todas las líneas y descuenta de los envases.
- **Verificar**: las ocho preguntas del Anexo I.G con su texto literal, el **verificador elegido por nombre** de la lista de usuarios activos (quién firma una verificación es dato legal: elegirlo de una lista y no tecleando un número evita atribuir la firma a quien no verificó) y el motivo de excepción si coincide con el elaborador.
- **Reelaborar**: origen (paciente, familiar, médico) y motivo; nueva versión, pendiente de verificar.
- **Imprimir ficha / etiquetas / instrucciones**: genera los PDF y registra la hora de impresión.

Los mensajes de bloqueo llevan el botón **"¿Por qué? (F1)"** ([[procedimiento:porque-de-los-bloqueos]]).

Procedimientos relacionados: [[procedimiento:preparacion]], [[procedimiento:verificacion]], [[procedimiento:entrega]].
