# Preparación y listado de preparaciones

## Preparaciones (pantalla principal → "Preparaciones")

Todos los blísteres de todos los pacientes, el más reciente arriba, con filtro por paciente y estado y la casilla "solo pendientes". Cada fila indica **envases al día**: SÍ cuando la próxima sesión del paciente puede prepararse sin intervención (activo, con idoneidad, tratamiento revisado y saldo suficiente); si NO, el motivo. "Abrir preparación" lleva a la pantalla del paciente.

**Lote**: marca los pacientes (o "Seleccionar los que están al día"), elige ficha, etiquetas e instrucciones, y "Generar documentos": para cada paciente la aplicación localiza su sesión pendiente o crea una nueva, la pasa a PREPARADO con el primer material activo y la última lectura ambiental reciente, y genera los documentos. El resumen indica generados, excluidos (con el motivo) y fallidos; un fallo no detiene al resto.

## Preparación del paciente

Se abre desde la ficha del paciente o desde el listado. Muestra todos los blísteres (SPD) del paciente, el más reciente arriba, con su número de registro, estado, versión y periodo de validez.

## Barra superior

- **Nueva sesión de preparación**: crea 1 o 2 blísteres de 7 días consecutivos con las líneas del tratamiento activo. Exige paciente ACTIVO con idoneidad y consentimiento, tratamiento revisado y sin faltantes en el listado de retirada.
- **Preparar siguiente**: copia la última sesión y avisa de líneas modificadas, nuevas, eliminadas o sin envase.
- **Imprimir instrucciones de la sesión**: una sola hoja para los dos blísteres si tienen el mismo contenido (PNT I §4.4.1: la hoja es "en cada entrega").
- **Entrega**: "Entregado a", primera entrega, SPD anterior recogido, unidades no administradas, observaciones de adherencia y "refiere cambios de medicación"; "Entregar todos los pendientes" entrega de una vez los VERIFICADOS. Si se marcan cambios referidos, el tratamiento queda pendiente de revisión y se abre la comunicación al médico prerrellenada.

## Condiciones ambientales y material

Temperatura y humedad (si las dejas vacías y hay una lectura reciente, se reutiliza); selector de material de acondicionamiento y alta rápida de material nuevo (descripción y lote).

## Por cada blíster

- Líneas con medicamento, unidades por envase y estado; **Registrar envase** en la línea si falta saldo.
- **Pasar a preparado**: valida saldo en todas las líneas y descuenta de los envases.
- **Verificar**: las ocho preguntas del Anexo I.G, verificador y motivo de excepción si coincide con el elaborador.
- **Reelaborar**: origen (paciente, familiar, médico) y motivo; nueva versión, pendiente de verificar.
- **Imprimir ficha / etiquetas / instrucciones**: genera los PDF y registra la hora de impresión.

Los mensajes de bloqueo llevan el botón **"¿Por qué? (F1)"** ([[procedimiento:porque-de-los-bloqueos]]).

Procedimientos relacionados: [[procedimiento:preparacion]], [[procedimiento:verificacion]], [[procedimiento:entrega]].
