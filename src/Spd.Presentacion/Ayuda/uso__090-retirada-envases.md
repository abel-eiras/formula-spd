# Listado de retirada de envases

Pacientes cuyo próximo blíster empieza dentro de los días de antelación configurados y a los que **falta saldo** en custodia para alguna línea del tratamiento. Por cada uno: apellidos, nombre y **CIP**, medicamento que falta, unidades necesarias frente a disponibles, quién retira la medicación (el paciente o el contacto marcado "retira la medicación") y su teléfono.

**Registrar envase** en una fila abre el alta ya con el paciente y el medicamento. Las **unidades** vienen del catálogo (el tamaño del envase); si el catálogo no lo sabe, escríbelas y marca la casilla para guardarlas como tamaño de envase, solo si el envase está completo. Escanea el DataMatrix en el campo de escaneo: se rellenan serie, lote y caducidad y el cursor salta a las unidades; **Intro** guarda. También se puede escanear desde el buscador de arriba sin buscar antes la fila ([[uso:inicio]]).

Sirve para pedir las recetas y preparar la dispensación con tiempo. Un paciente cuya semana ya está cubierta por un SPD preparado o verificado no aparece. La antelación y el día de retirada por defecto se configuran en [[uso:configuracion]]; el día de cada paciente, en su ficha.

Procedimiento relacionado: [[procedimiento:deposito-y-retirada]].


## Reposición

**Reposición** dice qué medicamentos en SPD se agotan en los próximos días (14 por defecto, hasta 120) con lo que hay en custodia: unidades que quedan, días que cubren y fecha en que se acaban. Sirve para pedir a tiempo la dispensación o una receta nueva; el listado de retirada solo mira la próxima preparación. Se puede filtrar por centro e imprimir.

## Centros y residencias

En **Centros y residencias** se dan de alta las residencias o centros de día y se les asignan sus pacientes. Al elegir un centro aparecen los blísteres **verificados** de sus pacientes; **Imprimir albarán** genera el documento que acompaña a los blísteres, con espacio para la firma de quien los recibe. El albarán no registra la entrega: cada blíster se sigue entregando desde su preparación, porque el procedimiento pide los datos de cada entrega. Un centro con pacientes no se puede dar de baja.
