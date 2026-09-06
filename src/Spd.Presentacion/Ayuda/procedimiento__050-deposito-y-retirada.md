# Depósito de envases, custodia y listado de retirada

## Qué exige el PNT (PNT I §4.4, Guía §5.2)

Los medicamentos dispensados de cada paciente se **custodian en la zona de almacenamiento para SPD**, separados por paciente e identificados con su nombre o un código unívoco, en sus envases originales y en sus condiciones de conservación. La cantidad dispensada debe ser acorde con la que se va a preparar. La **trazabilidad** va desde el envase original (código nacional, lote, caducidad y número de serie) hasta cada blíster entregado.

## Qué hago en la aplicación

- [[uso:deposito]]: registra cada envase que entra en custodia (serie, lote, caducidad, unidades). Puedes importarlos desde un fichero del programa de gestión con un perfil de importación ([[uso:configuracion]]).
- La aplicación calcula las **unidades que necesita cada blíster** a partir de la pauta y descuenta del envase al preparar; nunca las pide dos veces (constitución Art. V). Con pautas fraccionadas descuenta "entero más uno".
- [[uso:retirada-envases]]: el **listado de retirada** avisa, con la antelación configurada, de los pacientes cuyo próximo blíster no tiene envases suficientes en custodia, para pedir las recetas a tiempo.
- La medicación sobrante se guarda siempre en su envase original, por paciente. Un envase agotado o retirado se marca como tal; nada se borra.

**Documento**: Listado de retirada (pendiente de impresión); las series y lotes usados aparecen en la Ficha de preparación y en la etiqueta del reverso.

## Qué pasa si se omite

Sin envases suficientes en custodia, "Pasar a preparado" se rechaza para todo el blíster (la aplicación valida todas las líneas antes de descontar ninguna). Puedes registrar el envase que falta desde la propia línea del blíster.

Siguiente paso: [[procedimiento:preparacion]].
