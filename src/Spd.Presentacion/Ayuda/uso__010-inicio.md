# Inicio de sesión y pantalla principal

## Primer arranque

La primera vez, un asistente pide los datos de la farmacia y crea el primer usuario **Administrador**. Después, cada arranque empieza en la pantalla de inicio de sesión. Si la base de datos está cifrada, antes se pide la contraseña maestra o la frase de recuperación de 24 palabras.

## Inicio de sesión

Usuario y contraseña. Tras cinco intentos fallidos consecutivos el usuario queda bloqueado y solo un Administrador puede desbloquearlo. Quien entra queda registrado como autor de todo lo que haga (auditoría).

## Pantalla principal

Arriba, los **avisos del día**: pacientes con faltantes de envases antes de su próxima retirada, blísteres verificados sin entregar con la validez ya empezada, sesiones a medias (un blíster entregado y el otro no desde hace más de tres días) y días sin lectura ambiental. Son informativos; "Actualizar avisos" los recalcula.

Botones para cada área. Para cualquier usuario: **Pacientes** ([[uso:pacientes]]), **Preparaciones** ([[uso:preparacion]]), **Retirada de envases** ([[uso:retirada-envases]]), **Exportar pacientes**, **Catálogo de medicamentos** ([[uso:catalogo-medicamentos]]), **Registros de calidad** ([[uso:registros-calidad]]) y **Ayuda**. Solo para Administrador: usuarios, farmacia, actualizaciones, nomenclátor, control documental, seguridad y perfiles de importación ([[uso:configuracion]]).

**Cerrar sesión** vuelve al inicio de sesión sin cerrar la aplicación. Cerrar la ventana principal con la X cierra la aplicación y, antes, genera una **copia de seguridad** automática; si falla, avisa y no cierra hasta que lo veas.

## Ayuda

**F1** en cualquier pantalla abre el apartado del procedimiento que le corresponde; el botón "Ayuda" abre el índice. Junto a los mensajes de bloqueo hay un botón "¿Por qué?" que explica la regla ([[procedimiento:porque-de-los-bloqueos]]).

Procedimiento relacionado: [[procedimiento:servicio-spd]].
