# -*- coding: utf-8 -*-
"""Fuente única del plan de pruebas de aceptación. Genera el .md del repositorio y el HTML."""

# (titulo, nota, [(paso, comprobar, [CA...])])
BLOQUES = [
("0. Antes de empezar",
 "Se parte de cero: sin base de datos, para que el asistente de primer arranque entre en juego.",
 [("Borrar la base y arrancar: `rm -f src/Spd.Presentacion/bin/Debug/net8.0/spd.db` y `dotnet run --project src/Spd.Presentacion`",
   "Abre el asistente de primer arranque y no se puede llegar a ninguna otra pantalla sin completarlo.", ["CA-000"]),
  ("Mirar `logs/log-*.txt` junto al ejecutable",
   "La primera línea dice versión, sistema operativo y runtime. La segunda dice el tiempo de arranque con su límite: debe ser menor de 2000 ms.", ["CA-1504"]),
 ]),

("1. Farmacia y valores por defecto",
 "Configuración → Farmacia. Lo que se fije aquí condiciona todo lo demás.",
 [("Rellenar los datos de la farmacia y fijar prefijo de nº de ficha `F-`, día de retirada por defecto y días de antelación del listado",
   "Se guardan y se ven al reabrir la pantalla.", []),
  ("Más adelante, con un paciente ya creado con ficha `F-000001`, volver aquí y cambiar el prefijo a `PAC-`",
   "El paciente existente sigue mostrando `F-000001`; el siguiente alta usa `PAC-`.", ["CA-001 (000)"]),
  ("Con un paciente que tenga día de retirada fijado a mano, cambiar aquí el día por defecto",
   "El paciente conserva su día; el valor por defecto solo afecta a los nuevos.", ["CA-006 (000)"]),
 ]),

("2. Usuarios y sesión",
 "Administración → Usuarios. Conviene crear ya un segundo usuario Elaborador: hace falta para varias comprobaciones.",
 [("Intentar darse de baja siendo el único Administrador activo",
   "Lo impide y explica el motivo.", ["CA-002 (000)"]),
  ("Crear un usuario Elaborador y guardar su contraseña provisional", "Se crea y aparece en la lista.", []),
  ("Cerrar sesión y fallar la contraseña cinco veces seguidas; intentar una sexta",
   "El usuario queda bloqueado y solo un Administrador puede desbloquearlo.", ["CA-004 (000)"]),
  ("Entrar como Elaborador",
   "No aparece ninguna sección de administración en el menú, y no se puede llegar a ellas.", ["CA-1500", "CA-903"]),
  ("Al final de todo el recorrido: dar de baja al usuario que elaboró blísteres y consultarlos",
   "Los SPD siguen mostrando su nombre como elaborador.", ["CA-003 (000)"]),
 ]),

("3. Marco único, tema y ayuda",
 "Transversal. Se puede ir comprobando mientras se recorre el resto.",
 [("Recorrer las quince secciones del menú lateral",
   "Ninguna abre una ventana nueva: siempre una sola ventana. «← Atrás» vuelve a la sección anterior.", ["CA-1500"]),
  ("Cambiar el tema del sistema a oscuro y volver a claro",
   "Todo sigue legible en ambas variantes: ningún texto del tema claro sobre fondo oscuro ni al revés.", ["CA-1502"]),
  ("Pulsar F1 en Preparaciones, en Retirada y en cuatro pestañas distintas de un paciente",
   "Abre el apartado de procedimiento de esa pantalla, no el índice general.", ["CA-1503", "CA-1401"]),
  ("Abrir la Ayuda y mirar el índice",
   "Están separadas «Uso de la aplicación» y «Procedimiento del servicio SPD».", ["CA-1400"]),
  ("Buscar «verificador» en la ayuda",
   "Aparecen tanto el apartado de Uso de la verificación como el de Procedimiento.", ["CA-1404"]),
  ("Abrir Procedimiento → Documentación por momento del servicio, apartado «antes de preparar»",
   "Encuentra la lista de comprobación (consentimiento, idoneidad, tratamiento revisado…).", ["CA-1403"]),
 ]),

("4. Catálogo de medicamentos",
 "Crear al menos tres medicamentos: uno fraccionable, uno no apto y uno sin unidades por envase.",
 [("Dar de alta un medicamento con solo CN y nombre",
   "Se crea y queda marcado «descripción física pendiente» donde se use.", ["CA-300"]),
  ("Dar de alta otro introduciendo el CN y pulsando «Consultar CIMA»",
   "Se rellenan nombre, principio activo, laboratorio y forma farmacéutica.", ["CA-306"]),
  ("Pulsar «Consultar CIMA» con un CN inventado que no exista",
   "Informa de que no hay datos y deja seguir rellenando a mano.", ["CA-307"]),
  ("Intentar crear otro medicamento con un CN que ya existe",
   "Lo impide y muestra el existente.", ["CA-303"]),
  ("Coger un medicamento cuya forma lo marca apto y marcarlo como no apto",
   "Exige un motivo antes de guardar.", ["CA-302"]),
  ("Dar de baja un medicamento y luego volver a necesitar ese mismo CN",
   "Reactiva el registro existente en vez de crear un duplicado.", ["CA-305"]),
  ("Tras entregar un blíster (bloque 13): cambiar aquí la descripción física de un medicamento que iba en él y consultar ese blíster",
   "El blíster entregado sigue mostrando la descripción de cuando se elaboró. **Art. IV.3: es lo más importante de todo el catálogo.**", ["CA-301"]),
  ("Configuración → Nomenclátor → «Descargar ahora» (o «Importar al catálogo el último fichero descargado»). Buscar luego en el catálogo un medicamento que no hubieras dado de alta, uno de baja y uno de los que creaste a mano",
   "El mensaje da el recuento. El nuevo está, con aptitud «Sin confirmar»; el de baja está inactivo; los tuyos conservan su nombre, aptitud y motivo. Un efecto o accesorio (p. ej. una bolsa de ostomía) no aparece. Buscar con una sola letra no lista nada.", ["CA-308"]),
  ("Importar un nomenclátor que incluya un medicamento con descripción física ya completa",
   "La descripción física no cambia sin confirmación explícita.", ["CA-304"]),
 ]),

("5. Paciente nuevo, con su médico nuevo",
 "**El orden importa y es este**: se empieza por el paciente. El médico se da de alta desde su ficha cuando hace falta.",
 [("Pacientes → Nuevo paciente. Rellenar solo nombre y apellidos e intentar guardar",
   "No guarda, e indica que falta al menos uno de: DNI, CIP o fecha de nacimiento.", ["CA-002 (001)"]),
  ("Añadir el DNI. En «Médico de cabecera», escribir dos letras de un apellido que no exista",
   "No lo encuentra, pero ofrece «Nuevo médico…».", ["CA-004 (001)"]),
  ("Pulsar «Nuevo médico…», rellenar apellidos y nombre, y crear",
   "El panel se abre a la derecha con el apellido ya escrito, **la ficha de detrás se sigue viendo**, y al guardar queda seleccionado sin haber perdido nada de lo tecleado ni haber salido de la ficha.", ["CA-005 (001)"]),
  ("Guardar el paciente",
   "Recibe el número de ficha correlativo con el prefijo configurado, y el campo no es editable.", ["CA-001 (001)"]),
  ("Crear un segundo paciente con el mismo DNI",
   "Avisa con el nombre y nº de ficha del existente, y deja continuar o cancelar.", ["CA-003 (001)"]),
  ("Crear un paciente «José Núñez» y buscar «nunez» en el buscador de pacientes",
   "Aparece en los resultados: la búsqueda no distingue tildes ni mayúsculas.", ["CA-011"]),
  ("Con fecha de nacimiento, apellidos y sexo rellenos y el CIP vacío, pulsar «autocompletar CIP»",
   "Propone el CIP con las posiciones de control en blanco.", ["CA-014"]),
  ("Con un CIP ya puesto, cambiar el sexo y guardar",
   "Avisa de que el CIP no se corresponde, sin bloquear.", ["CA-015"]),
  ("Cambiar el teléfono de un paciente y guardar; después mirar la auditoría",
   "Hay una entrada EDITAR sobre Paciente con el detalle `telefono1: … → …`.", ["CA-013"]),
  ("Entrar como Elaborador y editar una ficha de paciente",
   "Puede editar y guardar todos los campos, igual que un Administrador.", ["CA-012"]),
 ]),

("5b. Mantenimiento del catálogo de médicos",
 "Sección «Catálogo de médicos» del menú. **No hace falta para dar de alta a nadie**: es la pantalla a la que se va de vez en cuando. Se prueba con los médicos ya creados desde las fichas.",
 [("Mirar la lista",
   "Están los médicos dados de alta desde las fichas, y la columna «Pacientes» cuenta los que los tienen de cabecera.", []),
  ("Crear un médico con los mismos nombre y apellidos que uno existente",
   "Avisa del posible duplicado **y lo crea igualmente**: pueden ser dos personas distintas, o la misma en dos centros. El aviso no bloquea.", []),
  ("Cambiar el teléfono de un médico que referencian tres pacientes, y abrir la ficha de los tres",
   "Los tres muestran el teléfono nuevo. **No hay copia de los datos del médico: es una referencia (Art. IV.1).**", ["CA-006 (001)"]),
  ("Intentar dar de baja a un médico que es cabecera de un paciente activo",
   "Lo impide **diciendo el nombre del paciente** al que hay que reasignar, no con un «no se puede» a secas.", ["CA-007 (001)"]),
  ("Dar de baja a un médico sin pacientes activos, y consultar un tratamiento antiguo que lo referencia",
   "El médico ya no sale en los selectores, pero el tratamiento antiguo lo sigue mostrando: no se borró.", []),
 ]),

("6. Contactos del paciente",
 "En la pestaña Datos, debajo de los datos. El botón está apagado hasta el primer guardado del paciente: es correcto, no un fallo.",
 [("Añadir un contacto de tipo Representante legal sin DNI",
   "No lo guarda e indica que el DNI es obligatorio para ese tipo.", ["CA-008 (001)"]),
  ("Añadir un familiar sin DNI y marcarlo como «retira la medicación»",
   "Al marcarlo, exige DNI.", ["CA-008 (001)"]),
  ("Marcar un segundo contacto también como quien retira",
   "El primero se desmarca solo: solo puede haber uno.", []),
  ("Con un contacto marcado como quien retira, con DNI, abrir el listado de retirada",
   "La columna DNI muestra el del contacto, no el del paciente.", ["CA-514"]),
  ("Quitar la marca a todos los contactos y volver al listado",
   "La columna DNI muestra el del propio paciente, y la ficha avisa de que será así.", ["CA-515"]),
  ("Crear un consentimiento de representante en un paciente sin ningún representante legal",
   "Ofrece crear el contacto antes de continuar.", ["CA-202"]),
  ("Dar de baja un contacto y marcar «Ver histórico»",
   "Sigue ahí con su fecha de baja, y ha perdido las marcas de principal y de retirada.", []),
 ]),

("7. Idoneidad y consentimiento",
 "Segunda pestaña del paciente. Es lo que lleva de EVALUACION a ACTIVO.",
 [("Marcar criterios hasta que la propuesta sea NO APTO, dejar las observaciones vacías e intentar guardar",
   "Lo impide: las observaciones son obligatorias en NO APTO.", ["CA-205"]),
  ("Registrar una evaluación NO APTO y después otra APTO",
   "Manda la última (APTO), y ambas siguen siendo consultables en el historial.", ["CA-204"]),
  ("Con evaluación APTO, crear el consentimiento y registrar su firma",
   "**La cabecera pasa a ACTIVO en el acto, sin cerrar ni reabrir nada.** Este era el defecto del diseño anterior.", ["CA-200", "CA-1521"]),
  ("Registrar una evaluación NO APTO sobre el paciente ya activo e intentar abrir sesión de preparación",
   "Lo impide.", ["CA-201"]),
  ("Revocar el consentimiento con fecha y motivo",
   "Sigue existiendo con las dos fechas, y el paciente deja de cumplir las condiciones de ACTIVO si no hay otro vigente.", ["CA-203"]),
 ]),

("8. Tratamiento",
 "Tercera pestaña. Dejar el paciente con al menos dos tratamientos en SPD, uno de ellos con pauta fraccionada.",
 [("Crear un tratamiento nuevo en un paciente que tiene médico de cabecera",
   "El campo prescriptor aparece prerrellenado con ese médico, y es editable.", ["CA-400"]),
  ("Teclear en desayuno `1+1/2`, en cena `1/3` y guardar; después teclear `0,5` en almuerzo",
   "Lo primero se guarda y se ve igual al cambiar la pauta. Con `0,5` aparece el aviso en rojo al escribir y al guardar dice la toma, lo tecleado y los catorce valores admitidos; no se guarda.", ["CA-403"]),
  ("En el tratamiento, buscar un medicamento que no esté (un CN inventado) y pulsar «Nuevo medicamento…»; darlo de alta y guardar el tratamiento",
   "Se da de alta sin salir de la ficha, queda elegido y el tratamiento se guarda con él. Elegir uno marcado «de baja» lo reactiva.", ["CA-407"]),
  ("Cambiar la pauta de 1-0-0-0 a 1-0-1-0",
   "La fila original queda FINALIZADO con fecha de fin de hoy, y hay una nueva ACTIVO con fecha de inicio de hoy y la pauta nueva.", ["CA-401"]),
  ("Cambiar la pauta una tercera vez y pulsar «Ver historial»",
   "Se ven los tres tramos con sus fechas de vigencia; ninguno oculto.", ["CA-402"]),
  ("Poner un tratamiento en PENDIENTE_REVISION e intentar abrir sesión de preparación",
   "Lo impide.", ["CA-404"]),
  ("En un tratamiento de pauta fraccionada, abrir el campo de ajuste manual de unidades",
   "Se ve el valor calculado junto al campo, y sobrescribirlo exige motivo.", ["CA-405"]),
  ("Finalizar un tratamiento que tiene un envase en custodia",
   "Muestra el envase y propone la salida a SIGRE.", ["CA-406", "CA-508"]),
 ]),

("9. Depósito y retirada de envases",
 "Cuarta pestaña del paciente, y sección «Retirada de envases» del menú. Es el bloque con más criterios: el cálculo de faltantes es el corazón del servicio.",
 [("Paciente con 2 blísteres, un tratamiento de 1 cápsula/día y un envase de 4 unidades. Abrir el listado en su ventana de antelación",
   "La fila muestra necesarias 14, disponibles 4, faltan 10.", ["CA-500"]),
  ("Con `días de antelación = 2`, paciente con retirada el jueves, mirar el listado un lunes y luego un martes",
   "El lunes no aparece; el martes sí.", ["CA-501"]),
  ("Desde la fila con «faltan 10», pulsar «Registrar envase» y guardar uno de 28 unidades",
   "La fila desaparece del listado y el envase está en el depósito del paciente con 28 restantes.", ["CA-503"]),
  ("Intentar registrar en otro paciente una serie que ya está en custodia",
   "Lo impide indicando qué paciente ya la tiene.", ["CA-504"]),
  ("Dos envases del mismo medicamento: E1 con 3 restantes y caducidad lejana, E2 con 28 y caducidad próxima. Preparar una línea de 7 unidades",
   "Se usa primero el sobrante E1 (3) y luego E2 (4). E1 queda agotado.", ["CA-505"]),
  ("Un envase de 28 y una línea de 7: ejecutar el descuento",
   "El envase queda EN CUSTODIA con 21 restantes, y no existe ninguna acción que lo descarte por sobrante.", ["CA-506"]),
  ("Pauta de 0,5/día todos los días (suma semanal 3,5)",
   "Las unidades a descontar son 4: entero más uno.", ["CA-507"]),
  ("Buscar, en cualquier envase y en cualquier estado, una acción que lo devuelva al stock o lo reasigne a otro paciente",
   "**No existe ninguna.** Art. III y trazabilidad.", ["CA-509"]),
  ("Medicamento con `en SPD` desmarcado: registrar un envase sin serie y marcarlo como entregado al paciente",
   "Queda ENTREGADO_PACIENTE, y no aparece en el listado de retirada ni en el cálculo de disponibles.", ["CA-510"]),
  ("Envase que caduca antes del fin de validez de la próxima preparación",
   "No cuenta como disponible, y aparece marcado «caduca antes de la próxima validez».", ["CA-511"]),
  ("Medicamento sin unidades por envase, en el listado",
   "La columna «envases a retirar» muestra «?», y las demás filas se calculan con normalidad.", ["CA-512"]),
  ("Paciente con un SPD ya verificado que cubre su próxima retirada",
   "No aparece en el listado aunque esté en ventana.", ["CA-502"]),
  ("Pulsar «Imprimir» en el listado y mirar la auditoría",
   "Queda registrada la acción IMPRIMIR.", ["CA-513"]),
  ("Panel «Importar tratamiento»: pegar una línea con el CN de un tratamiento que ya existe, serie nueva",
   "Crea el envase EN CUSTODIA para ese tratamiento **sin duplicar el tratamiento**. El depósito de detrás se sigue viendo mientras pegas.", ["CA-516"]),
  ("Pegar una línea con un CN para el que el paciente no tiene tratamiento",
   "Crea un tratamiento pendiente de posología y su envase, y el resumen lo lista como pendiente.", ["CA-517"]),
  ("Importar un fichero de 5 filas donde una serie ya existe en otro paciente",
   "Da de alta 4 envases, y la fila conflictiva sale en el resumen como no importada con el nombre del paciente que ya tiene esa serie.", ["CA-518"]),
 ]),

("10. Preparación: sesión, carril y rejilla",
 "Quinta pestaña del paciente. **Aquí es donde quiero que compares la rejilla con un blíster real.**",
 [("Paciente con faltantes pendientes: intentar abrir sesión",
   "No crea nada y enlaza al listado de retirada.", ["CA-601"]),
  ("Preparaciones → «Nueva preparación…»: buscar a un paciente activo sin sesión y pulsar «Sesión nueva»; repetir con uno que ya tenga una sesión abierta",
   "El primero abre la sesión y lleva a su pestaña de preparación. El segundo deja el motivo en el panel y no navega.", ["CA-614"]),
  ("Abrir la preparación de un paciente cuyos medicamentos llegaron del nomenclátor",
   "Aviso «Aptitud para SPD sin confirmar» con sus nombres. «Confirmo que todos son aptos para SPD» lo quita y no vuelve. Un medicamento marcado «no apto» en el catálogo se advierte en rojo y no se ofrece confirmar. Nada de esto impide seguir preparando.", ["CA-613"]),
  ("Resolver los faltantes y abrir «Nueva sesión de preparación» en un paciente con 2 blísteres",
   "Crea dos SPD con números correlativos y validez consecutiva.", ["CA-600"]),
  ("Mirar el carril de pasos de un blíster recién creado",
   "Los pasos hechos, el actual y los bloqueados se corresponden con su estado, y **cada bloqueado dice por qué**.", ["CA-1530"]),
  ("Sin material de acondicionamiento registrado, mirar el paso de llenado",
   "Está bloqueado, y el motivo dice que falta el material.", ["CA-1530"]),
  ("**Comparar la rejilla de alvéolos con el blíster físico que estás llenando**",
   "Cuatro filas (desayuno, almuerzo, cena, noche) por siete columnas. Una pauta ½-0-1-0 todos los días da siete alvéolos de desayuno con ½ y siete de cena con 1, y nada en almuerzo ni noche.", ["CA-1531"]),
  ("Mirar las cabeceras de columna de un blíster que **no** empieza en lunes",
   "La primera columna es el día real de inicio de validez, con su fecha. Un tratamiento de «solo lunes» cae en la columna del lunes.", ["CA-1531"]),
  ("Medicamento con envase A de 3 uds y envase B de 28, y una línea de 7 uds",
   "La línea muestra dos filas de envase: 3 de A y 4 de B.", ["CA-602"]),
  ("Línea sin saldo suficiente en ningún envase: intentar pasar el blíster a PREPARADO",
   "Lo impide, indica el medicamento y ofrece registrar un envase.", ["CA-603"]),
  ("Desde ese aviso, registrar un envase nuevo",
   "**El panel se cierra y la línea ya muestra el envase, sin recargar la pantalla.** La línea recalcula el reparto y el blíster ya puede pasar a PREPARADO.", ["CA-604", "CA-1523"]),
  ("Buscar en el modelo de datos una tabla de negocio «Sesión»",
   "No existe: la sesión es un identificador compartido, no una entidad. *(Revisión del modelo, no de pantalla.)*", ["CA-611"]),
 ]),

("11. Verificación",
 "Paso 4 del carril. Las ocho preguntas del Anexo I.G con su texto literal.",
 [("Sesión con dos blísteres: verificar solo el primero, marcando todos los ítems",
   "El primero pasa a VERIFICADO y el segundo sigue PREPARADO.", ["CA-605"]),
  ("Abrir el selector de verificador",
   "Se elige **una persona de una lista por su nombre, nunca un número**.", ["CA-1532"]),
  ("Elegir como verificador al mismo que elaboró, e intentar confirmar",
   "El aviso aparece **junto al botón**, exige motivo de excepción, y tiene su «¿Por qué?».", ["CA-1533", "CA-1402"]),
  ("Pulsar ese «¿Por qué?»",
   "Llega al apartado que explica la regla, no al índice general.", ["CA-1402"]),
 ]),

("12. Entrega",
 "Paso 5 del carril.",
 [("Dos blísteres VERIFICADOS: entregar solo el primero",
   "El primero pasa a ENTREGADO y el segundo sigue VERIFICADO, disponible para entregar aparte.", ["CA-607"]),
  ("Dos blísteres VERIFICADOS: entregarlos marcando ambos",
   "Los dos pasan a ENTREGADO con los mismos datos de entrega.", ["CA-606"]),
  ("Entregar marcando «refiere cambios de medicación»",
   "Avisa de las consecuencias, los tratamientos quedan pendientes de revisión, y **cambia a la pestaña de Comunicaciones con la carta preparada para el médico de cabecera**.", ["CA-803"]),
 ]),

("13. Continuidad y reelaboración",
 "«Preparar siguiente» y «Reelaborar». Es donde se comprueba que nada se reescribe.",
 [("Paciente cuyo tratamiento no ha cambiado y con envases con saldo: pulsar «Preparar siguiente»",
   "Genera la sesión siguiente sin avisos, con una sola fila de envase por línea.", ["CA-608"]),
  ("Modificar la posología de una línea y pulsar «Preparar siguiente»",
   "Solo esa línea aparece marcada como modificada.", ["CA-609"]),
  ("Envase agotado en la sesión anterior sin sustituto en custodia: generar la siguiente",
   "Esa línea aparece como pendiente de envase.", ["CA-610"]),
  ("Reelaborar un SPD VERIFICADO con nº 000041 v1, añadiendo un medicamento",
   "Sigue siendo 000041 y pasa a versión 2.", ["CA-6120"]),
  ("Tras reelaborar, mirar el envase de una línea que no cambió",
   "Sus unidades restantes no varían.", ["CA-6121"]),
  ("Línea que pasa de 7 a 10 unidades en la reelaboración",
   "El envase descuenta solo la diferencia (3).", ["CA-6122"]),
  ("Línea de 7 unidades eliminada en la reelaboración",
   "Las 7 unidades vuelven a las restantes de su envase.", ["CA-6123"]),
  ("Consultar el historial de modificaciones del SPD reelaborado",
   "Hay una copia completa de las líneas y envases de la versión 1, con el motivo y quién lo hizo.", ["CA-6124"]),
  ("Buscar «Reelaborar» en un SPD ya ENTREGADO",
   "No está disponible.", ["CA-6125"]),
  ("Mirar el estado de un SPD verificado que se acaba de reelaborar",
   "Está en PREPARADO y no se puede entregar hasta verificarlo de nuevo.", ["CA-6126"]),
 ]),

("14. Documentos impresos",
 "Desde el carril y desde la pestaña Documentos. **Abrir los PDF y mirarlos, no solo comprobar que el fichero existe.**",
 [("Generar la ficha de preparación de un paciente «María López Vidal»",
   "El nombre del fichero incluye paciente y fecha, y se guarda en la carpeta configurada.", ["CA-700"]),
  ("Generar un documento de un paciente con nombre y apellidos muy largos",
   "El nombre del fichero se acorta y sigue siendo válido.", ["CA-701"]),
  ("Abrir la ficha de preparación de un tratamiento con pauta D=½, A=0, C=½, N=0",
   "La posología impresa es `1/2 - 0 - 1/2 - 0`.", ["CA-709"]),
  ("Abrir la ficha de un tratamiento con dosis de 1/3 de comprimido",
   "Muestra exactamente `1/3`, nunca `0,33` ni `0,3`.", ["CA-710"]),
  ("Imprimir etiquetas e instrucciones desde el carril, y la ficha del paciente y el documento RGPD desde la pestaña Documentos",
   "Los cinco PDF se generan, se abren, y los datos de la farmacia y del paciente están rellenos.", []),
  ("Generar el lote desde la tabla de Preparaciones, marcando tres pacientes al día",
   "Resumen con generados, excluidos con su motivo y fallidos; un fallo no detiene al resto.", ["CA-702…708"]),
 ]),

("15. Comunicaciones con el médico",
 "Sexta pestaña del paciente.",
 [("Generar una carta de presentación en un paciente activado con médico de cabecera",
   "El médico y los datos del paciente vienen rellenos.", ["CA-800"]),
  ("Formulario de incidencia sin rellenar la propuesta: intentar guardar",
   "Lo impide.", ["CA-801"]),
  ("Registrar la respuesta del médico a una incidencia guardada días antes",
   "Conserva su fecha original y añade la respuesta con su propia fecha.", ["CA-802"]),
  ("Registrar una comunicación de tipo Teléfono",
   "No hay opción de generar un documento imprimible para ella.", ["CA-804"]),
 ]),

("16. Inicio, avisos y búsqueda global",
 "Se comprueba mejor al final, cuando ya hay datos que generen avisos de verdad.",
 [("Iniciar sesión con faltantes, un verificado sin entregar y una sesión a medias",
   "Los cuatro indicadores y los tres avisos se ven en la primera pantalla, sin abrir nada.", ["CA-1510"]),
  ("Mirar el menú lateral con tres pacientes con faltantes",
   "«Retirada de envases» muestra el contador 3.", ["CA-1501"]),
  ("Pulsar el aviso de faltantes de un paciente",
   "Lleva al listado de retirada **ya centrado en ese paciente**, y se puede quitar el filtro.", ["CA-1511"]),
  ("Pulsar un aviso de cada uno de los otros tipos",
   "Cada uno lleva al sitio donde se resuelve.", ["CA-1511"]),
  ("Ordenar la tabla de Preparaciones por la columna de validez, con dos filas marcadas para el lote",
   "Las filas se reordenan y **la selección se conserva**.", ["CA-1512"]),
  ("En la búsqueda de la cabecera: apellido de un paciente, número de un blíster, CN de un medicamento",
   "Aparece cada uno, y al elegirlo lleva a su pantalla. Con una sola letra no busca.", ["CA-1513"]),
 ]),

("17. Espacio del paciente, de conjunto",
 "Lo que engloba todo lo anterior. Vale la pena repetirlo de un tirón al final.",
 [("Recorrer las siete pestañas de un paciente y registrar un envase por el camino",
   "**No se abre ninguna ventana nueva en todo el recorrido.**", ["CA-1520"]),
  ("Paciente con faltantes en su depósito: mirar sus pestañas",
   "La de Depósito está marcada como pendiente. Igual con idoneidad, preparación y comunicaciones.", ["CA-1522"]),
  ("Mirar la cabecera fija mientras cambias de pestaña",
   "Nombre, ficha, estado, idoneidad, día de retirada, blísteres y las alergias en rojo siguen visibles siempre.", []),
 ]),

("18. Registros de calidad",
 "Sección «Registros de calidad».",
 [("Registrar una tercera formación a un usuario que ya tenía dos",
   "Las tres siguen consultables.", ["CA-902"]),
  ("Entrar como Elaborador e intentar acceder a Control documental",
   "Lo impide.", ["CA-903"]),
  ("Registrar una recogida de residuos SIGRE", "Queda registrada y consultable.", []),
 ]),

("19. Nomenclátor, perfiles e importación/exportación",
 "Administración → Nomenclátor y Perfiles de importación, y Exportar pacientes.",
 [("Poner en Nomenclátor una URL que no responda y pulsar «Descargar ahora»",
   "Se ve el motivo del fallo y **el resto de la aplicación sigue funcionando con normalidad**: una descarga fallida no bloquea nada.", ["CA-005 (000)"]),
  ("Importar un nomenclátor real y revisar la pantalla de revisión",
   "Las filas conflictivas se pueden revisar una a una antes de aplicarlas.", []),
("Crear un perfil de importación y recuperarlo por nombre para importar",
   "No vuelve a pedir el mapeo de columnas.", ["CA-1100"]),
  ("Probar un patrón de extracción contra varias filas de muestra antes de guardar",
   "Dice cuántas aciertan y cuántas fallan. *(Si la pantalla no lo ofrece, anótalo: el FR-1110 estaba pendiente de una muestra real del nomenclátor.)*", ["CA-1101"]),
  ("Exportar con un perfil que solo mapea nombre, apellidos y teléfono",
   "El CSV no contiene ningún otro campo.", ["CA-1103"]),
 ]),

("20. Baja y reactivación del paciente",
 "Deja esto para el final: cambia el estado del paciente de pruebas.",
 [("Dar de baja un paciente con tratamientos, envases y SPD, con fecha y motivo",
   "Estado BAJA, **todos los datos relacionados siguen existiendo y consultables**, y la baja con su motivo está en auditoría.", ["CA-009"]),
  ("Reactivar ese paciente",
   "Pasa a EVALUACION y la cabecera dice «sin consentimiento vigente».", ["CA-010"]),
 ]),

("21. Copia de seguridad y cifrado",
 "**Lo último, y con la base de pruebas, no con datos que te importen.** Activar el cifrado cambia el fichero de la base: si pierdes la contraseña maestra y la frase de recuperación, no hay vuelta atrás.",
 [("Cerrar la aplicación con cambios pendientes y mirar la carpeta de backup",
   "Hay un `.zip` fechado cuya base se abre y está íntegra.", ["CA-1000"]),
  ("Poner una ruta de backup inaccesible y cerrar la aplicación",
   "Avisa explícitamente antes de terminar de cerrarse.", ["CA-1001"]),
  ("Simular 40 backups diarios acumulados y generar el 41",
   "Quedan 30 diarios más los mensuales promovidos, sin tocar ningún mensual.", ["CA-1002"]),
  ("Activar el cifrado",
   "Genera la clave de recuperación y **no continúa hasta confirmar explícitamente que la has impreso o guardado**. Guárdala de verdad antes de seguir.", ["CA-1003"]),
  ("Con el cifrado ya activo, cambiar la contraseña maestra",
   "Es prácticamente instantáneo (rekey), no una exportación completa de la base.", ["CA-1004"]),
  ("Cerrar y reabrir la aplicación con el cifrado activo",
   "Pide la contraseña maestra, y acepta indistintamente esa o la frase de recuperación de 24 palabras.", []),
 ]),

("22. Purga",
 "**Destructiva por diseño**: es la única excepción al Art. III. Solo sobre pacientes de prueba.",
 [("Tres pacientes elegibles para purga: marcar solo uno y confirmar con la contraseña",
   "Se purga solo ese.", ["CA-1005"]),
  ("Consultar la auditoría de un paciente purgado",
   "La entrada de purga sobrevive al borrado, con su motivo y quién lo hizo.", ["CA-1006"]),
  ("Intentar purgar un paciente que todavía tiene un envase EN CUSTODIA",
   "Lo impide.", ["CA-1007"]),
 ]),
]

NO_APLICAN = [
 ("CA-900, CA-901, CA-904", "Retirados de la Spec 009: la pantalla de lectura ambiental rutinaria y la de limpieza se retiraron de esa iteración. El aviso de ambiental atrasado sí se comprueba en el bloque 16, con los indicadores de inicio."),
 ("CA-1102", "Perfiles de fábrica (Farmatic, Nixfarma, Unycop) no existen: esperan una fila real de cada programa que no tenemos. No hay nada que editar todavía."),
 ("CA-1502", "Además de la comprobación visual del bloque 3, está cubierto por `TemaTests`, que recorre todas las claves de color en ambas variantes. Un ojo humano no puede garantizar que ninguna falte."),
]

# --------------------------------------------------------------------------- generación del .md
import re as _re
import sys as _sys
import unicodedata as _ud
from pathlib import Path as _Path


def _ancla(titulo):
    sin_tildes = "".join(c for c in _ud.normalize("NFD", titulo.lower()) if _ud.category(c) != "Mn")
    return _re.sub(r"\s+", "-", _re.sub(r"[^a-z0-9\s-]", "", sin_tildes).strip())


def _criterios_citados():
    citados = {c for _, _, pasos in BLOQUES for _, _, cas in pasos for c in cas}
    return citados | {x.strip() for grupo, _ in NO_APLICAN for x in grupo.split(",")}


def renderizar():
    pasos = sum(len(p) for _, _, p in BLOQUES)
    ultimo = "Lo que no se comprueba aquí, y por qué"
    lineas = [
        "# Pruebas de aceptación — recorrido manual completo",
        "",
        f"Los **{len(_criterios_citados())} criterios de aceptación** de las catorce especificaciones, ordenados en el orden en que",
        "se ejecutan de verdad, no por número de spec. Generado desde una única fuente para que ningún",
        "criterio se quede fuera: la comprobación de cobertura falla si alguno no aparece.",
        "",
        f"**{pasos} pasos en {len(BLOQUES)} bloques.** No hace falta hacerlo de una sentada, pero el orden importa:",
        "cada bloque deja el sistema en el estado que necesita el siguiente. Los tres últimos son",
        "destructivos y van al final por eso.",
        "",
        "Cuando algo falle, anota **el paso, lo que esperabas y lo que pasó**, y adjunta",
        "`logs/log-*.txt`: desde la corrección del 12-09 ahí queda cualquier caída con su traza, y la",
        "primera línea identifica la compilación y el sistema.",
        "",
        "## Índice",
        "",
    ]
    lineas += [f"- [{t}](#{_ancla(t)}) — {len(p)} pasos" for t, _, p in BLOQUES]
    lineas += [f"- [{ultimo}](#{_ancla(ultimo)})", ""]
    for titulo, nota, filas in BLOQUES:
        lineas += [f"## {titulo}", "", nota, "", "| ✓ | Qué hacer | Qué debe pasar | Criterio |", "|---|---|---|---|"]
        for paso, comprobar, cas in filas:
            criterio = ", ".join(f"`{c}`" for c in cas) if cas else "—"
            lineas.append(f"| ☐ | {paso} | {comprobar} | {criterio} |")
        lineas.append("")
    lineas += [
        f"## {ultimo}",
        "",
        "No se omite nada por descuido. Estos criterios no son comprobables a mano hoy, y conviene saberlo",
        "para no perder tiempo buscándolos:",
        "",
    ]
    lineas += [f"- **{grupo}** — {motivo}" for grupo, motivo in NO_APLICAN]
    lineas += [
        "",
        "## Cuando termines",
        "",
        "Anota el resultado en `specs/001-pacientes-y-medicos/PROGRESO.md` (T057) y en",
        "`specs/015-rediseno-interfaz/PROGRESO.md` (T061), que son las dos tareas que esta prueba cierra.",
        "Después ya tiene sentido el paquete de Windows y la primera release.",
    ]
    return "\n".join(lineas) + "\n"


def criterios_sin_cubrir(raiz):
    """Todo `**CA-…**` de cada spec debe aparecer en el plan. Un número repetido en dos specs se cita
    con la spec entre paréntesis («CA-001 (000)») para la que no es su dueña natural."""
    citados = set(_criterios_citados())
    for cita in list(citados):
        rango = _re.fullmatch(r"CA-(\d+)…(\d+)", cita)
        if rango:
            citados |= {f"CA-{n}" for n in range(int(rango.group(1)), int(rango.group(2)) + 1)}
    por_id = {}
    for spec in sorted(_Path(raiz, "specs").glob("[0-9][0-9][0-9]-*/spec.md")):
        numero = spec.parent.name[:3]
        for ca in set(_re.findall(r"^\*\*(CA-\d+)", spec.read_text(encoding="utf-8"), _re.M)):
            por_id.setdefault(ca, []).append(numero)
    faltan = []
    for ca, specs in sorted(por_id.items()):
        for numero in specs:
            con_spec = f"{ca} ({numero})" in citados
            # Sin paréntesis vale para un número que solo existe en una spec, o para la spec dueña del
            # rango cuando se repite (la 000 comparte CA-000..CA-006 con la 001 y se cita con «(000)»).
            sin_spec = ca in citados and (len(specs) == 1 or numero != "000")
            if not (con_spec or sin_spec):
                faltan.append(f"{ca} ({numero})")
    return faltan


if __name__ == "__main__":
    raiz = _Path(__file__).resolve().parent.parent
    faltan = criterios_sin_cubrir(raiz)
    if faltan:
        print("Criterios sin cubrir: " + ", ".join(faltan))
    destino = raiz / "specs" / "PRUEBAS-ACEPTACION.md"
    destino.write_text(renderizar(), encoding="utf-8")
    print(f"Escrito {destino.relative_to(raiz)}")
    _sys.exit(1 if faltan else 0)
