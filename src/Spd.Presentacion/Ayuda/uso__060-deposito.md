# Depósito de envases e importación

## Depósito

Envases del paciente en custodia, con medicamento, **serie**, **lote**, **caducidad**, unidades iniciales y restantes, origen (manual o importado) y estado (en custodia, agotado, residuo SIGRE, entregado al paciente).

- **Registrar envase**: alta manual.
- **Salida**: marcar como residuo SIGRE o entregado al paciente (nunca se borra; la aplicación no tiene "devolver al stock").
- Las unidades restantes bajan automáticamente al pasar un blíster a PREPARADO, con la serie y el lote anotados en la línea del SPD.

## Importar tratamiento y envases

Desde un fichero CSV exportado por el programa de gestión, con un **perfil de importación** que indica qué columna es cada dato ([[uso:configuracion]]). La aplicación muestra qué líneas se crearán y cuáles tienen errores antes de confirmar.

Procedimientos relacionados: [[procedimiento:deposito-y-retirada]], [[procedimiento:residuos]].
