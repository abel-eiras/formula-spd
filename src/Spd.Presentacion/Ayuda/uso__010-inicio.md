# Inicio de sesión y pantalla principal

## Primer arranque

La primera vez, un asistente pide los datos de la farmacia y crea el primer usuario **Administrador**. Después, cada arranque empieza en la pantalla de inicio de sesión. Si la base de datos está cifrada, antes se pide la contraseña maestra o la frase de recuperación de 24 palabras.

## Inicio de sesión

Usuario y contraseña. Tras cinco intentos fallidos consecutivos el usuario queda bloqueado y solo un Administrador puede desbloquearlo. Quien entra queda registrado como autor de todo lo que haga (auditoría).

## Una sola ventana

Tras iniciar sesión todo ocurre en **una única ventana**. A la izquierda, la navegación; a la derecha, la sección abierta. Cambiar de sección no abre nada nuevo, y **← Atrás** vuelve a donde se estaba.

En la navegación, para cualquier usuario: **Inicio**, **Pacientes** ([[uso:pacientes]]), **Preparaciones** ([[uso:preparacion]]), **Retirada de envases** ([[uso:retirada-envases]]), **Catálogo de medicamentos** ([[uso:catalogo-medicamentos]]), **Registros de calidad** ([[uso:registros-calidad]]) y **Exportar pacientes**. Solo para Administrador: farmacia, usuarios, seguridad, control documental, nomenclátor, perfiles de importación y actualizaciones ([[uso:configuracion]]).

Las entradas con trabajo pendiente llevan un número al lado: los faltantes en Retirada y los blísteres a medias en Preparaciones.

## Inicio

Cuatro indicadores de un vistazo: pacientes con faltantes, blísteres verificados sin entregar, sesiones a medias y si la lectura ambiental está al día o cuántos días lleva sin registrarse.

Debajo, los **avisos del día**, los mismos que cuentan los indicadores: faltantes de envases antes de la próxima retirada, blísteres verificados sin entregar con la validez ya empezada, sesiones a medias (un blíster entregado y el otro no desde hace más de tres días) y días sin lectura ambiental. Son informativos, nunca bloquean nada, y **al pulsar uno se va a donde se resuelve**, ya centrado en ese paciente. "Actualizar" los recalcula.

## Buscar

Arriba a la derecha hay un buscador que vale para todo: escribe parte del nombre o el DNI de un paciente, el CN o el nombre de un medicamento, o el número de registro de un blíster, y lleva directamente al resultado. No distingue tildes ni mayúsculas y hacen falta al menos dos letras.

**Cerrar sesión** vuelve al inicio de sesión sin cerrar la aplicación. Cerrar la ventana con la X cierra la aplicación y, antes, genera una **copia de seguridad** automática; si falla, avisa y no cierra hasta que lo veas.

## Ayuda

**F1** en cualquier pantalla abre el apartado del procedimiento que le corresponde; el botón "Ayuda" abre el índice. Junto a los mensajes de bloqueo hay un botón "¿Por qué?" que explica la regla ([[procedimiento:porque-de-los-bloqueos]]).

Procedimiento relacionado: [[procedimiento:servicio-spd]].
