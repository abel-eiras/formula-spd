Esta versión beta mejora la lectura de las cajas, añade herramientas de seguridad y calidad y facilita el trabajo con residencias.

**Lector de códigos**
- El código nacional sale del propio código de la caja aunque el fabricante no lo incluya: se deduce del código de barras español. Solo las cajas extranjeras sin ese dato piden el CN a mano.
- Si el lector pierde el separador entre el lote y el número de serie y el código admite varias lecturas, la aplicación no adivina: enseña las posibles y eliges la que coincide con la caja.
- Nueva pantalla Administración → Lector de códigos para probar el lector con una caja (qué caracteres llegan y qué se entiende) y para indicar el símbolo que tu lector manda como separador.

**Preparación**
- La rejilla del blíster se puede ver igual que el blíster que usáis: días por orden o de lunes a domingo, de izquierda a derecha o al revés, y desayuno o noche arriba (Configuración → Farmacia).
- Aviso cuando dos medicamentos del mismo alvéolo tienen el mismo aspecto (forma y color), para distinguirlos por la serigrafía.
- En el catálogo, la descripción física se puede proponer pegando el texto de la ficha técnica.

**Calidad y seguridad**
- Alertas de calidad: al registrar la retirada de un lote, la aplicación dice qué cajas lo contienen y qué blísteres, pendientes o ya entregados, llevan unidades de ese lote. Las alertas activas aparecen las primeras en Inicio.
- La auditoría queda sellada: cada entrada enlaza con la anterior y en Configuración → Seguridad se puede comprobar que nadie la ha modificado por fuera de la aplicación.
- Las actualizaciones solo se descargan de GitHub.

**Residencias y reposición**
- Centros y residencias: agrupa a los pacientes de un centro e imprime un albarán con sus blísteres listos para entregar, para que lo firme quien los recibe. La entrega de cada blíster se sigue registrando como siempre.
- Reposición: lista de los medicamentos que se agotan en los próximos días con lo que hay en custodia, para pedir a tiempo. Se puede filtrar por centro e imprimir.

**Actualizar:** desde la 0.3.0-beta, un administrador puede instalarla desde Configuración → Actualizaciones (se hace una copia de seguridad antes). Desde versiones anteriores, a mano: haz una copia de seguridad, descarga el paquete de tu sistema y sigue las instrucciones incluidas. Los datos se conservan y la base de datos se actualiza al arrancar.

La aplicación continúa en pruebas. Configura las copias de seguridad y conserva una copia antes de actualizar.
