# Spec 000 — Configuración inicial, farmacia y usuarios

**Estado:** Borrador para revisión
**Versión:** 0.1 — 2026-09-04
**Constitución aplicable:** 2.1.0 (Artículos V, VI, VII, VIII)
**Depende de:** Ninguna — es la base de todo lo demás
**Requerida por:** Todas las specs restantes

---

## 1. Propósito

Primer arranque de la aplicación: datos de la farmacia, primer usuario administrador, valores por defecto que el resto de specs reutilizan (día de retirada, nº de blísteres, antelación del listado, rutas de backup y de documentos generados, rangos ambientales), y gestión continuada de usuarios.

## 2. Actores

| Actor | Puede |
|---|---|
| Nadie (primer arranque) | Asistente de configuración inicial obligatorio antes de poder usar la aplicación |
| Administrador | Todo lo de esta spec en cualquier momento posterior |
| Elaborador | Solo consulta de los datos de farmacia que aparecen en documentos; no accede a Configuración |

## 3. Escenarios de usuario

### E1 — Primer arranque
Como quien instala la aplicación por primera vez, quiero que un asistente me pida los datos imprescindibles (farmacia, primer usuario administrador, si quiero cifrar) para no encontrarme una pantalla vacía sin saber por dónde empezar.

### E2 — Cambiar un dato de la farmacia meses después
Como administrador, quiero poder editar el logo, el teléfono o el WhatsApp de la farmacia en cualquier momento desde Configuración, y que se refleje en todos los documentos generados a partir de ese momento.

### E3 — Dar de alta a un compañero
Como administrador, quiero crear un usuario Elaborador con una contraseña provisional que tenga que cambiar en su primer acceso.

### E4 — Cambiar los valores por defecto de retirada
Como administrador, quiero cambiar el día de retirada por defecto o el número de blísteres por defecto para pacientes nuevos, sin que afecte a los pacientes ya configurados individualmente.

## 4. Requisitos funcionales

### 4.1 Asistente de primer arranque

- **FR-000** Al arrancar la aplicación sin base de datos existente, se lanza un asistente obligatorio, en este orden: (1) elegir cifrado sí/no y contraseña maestra si aplica (Spec 010); (2) datos de la farmacia (FR-010); (3) primer usuario, con rol Administrador forzado; (4) valores por defecto de retirada y blísteres; (5) rutas de backup y documentos generados.
- **FR-001** El asistente no permite avanzar sin los campos obligatorios de cada paso (FR-010, FR-020). Se puede volver a un paso anterior antes de finalizar. Una vez finalizado, todos los valores son editables individualmente desde Configuración, sin necesidad de repetir el asistente.

### 4.2 Datos de la farmacia

- **FR-010** Campos obligatorios: nombre de la farmacia, código sanitario/siglas, titular o comunidad de bienes, CIF, dirección, población, código postal, teléfono. Opcionales: logo (imagen), fax, email, WhatsApp, nº de colegiado del titular (para documentos que lo requieran).
- **FR-011** El logo se almacena como fichero dentro de la carpeta de instalación (Artículo VI: todo en una carpeta), no como ruta externa; si se sustituye, el anterior se conserva en una subcarpeta de histórico con fecha, sin generar una tabla de negocio para ello.
- **FR-012** Campos de responsable de datos y dirección/email de derechos ARCO, para el texto legal del consentimiento (Spec 002): opcionales, con valor por defecto igual al titular y dirección si no se rellenan aparte.
- **FR-013** Prefijos de numeración configurables: `prefijo_num_ficha` (pacientes, Spec 001) y `prefijo_num_spd` (blísteres, Spec 006). Cambiar el prefijo no afecta a los números ya asignados; solo a los siguientes.

### 4.3 Valores por defecto

- **FR-020** `dia_retirada_defecto`, `n_blisteres_defecto`, `dias_antelacion_listado` (Spec 005). Editables en cualquier momento; el cambio solo afecta a pacientes nuevos o a los que no hayan personalizado su propio valor.
- **FR-021** Rangos ambientales `temp_min/max`, `hr_min/max` (por defecto 15–25 °C, 40–60 % HR), usados por Spec 006/009 para el aviso de lectura fuera de rango.
- **FR-022** Umbral de reutilización de lectura ambiental reciente (Spec 006 FR-630), por defecto 2 horas, configurable.

### 4.4 Rutas

- **FR-030** `ruta_backup`: carpeta local o de red donde se escriben las copias de seguridad (Spec 010). Se valida que la ruta existe y es escribible al guardar; si no, se avisa sin bloquear el resto de la configuración.
- **FR-031** `ruta_documentos_generados`: carpeta de salida de todo documento generado (Spec 007). Misma validación que FR-030.
- **FR-032** Ninguna ruta puede coincidir con la carpeta de instalación de la aplicación si se quiere evitar que una reinstalación las borre; el sistema solo avisa, no impide, porque el propietario puede tener razones válidas.

### 4.5 Usuarios

- **FR-040** CRUD de usuarios: nombre, apellidos, login único, rol (`ADMINISTRADOR` / `ELABORADOR`), cargo_pnt (texto libre para lo que se imprime en el registro de firmas reconocidas, Spec 009), colegiado (opcional), activo.
- **FR-041** Alta de usuario: contraseña provisional generada o introducida por el administrador; el usuario debe cambiarla en su primer inicio de sesión (`debe_cambiar_password`).
- **FR-042** Un usuario no puede autodesactivarse ni autodegradarse de Administrador si es el único Administrador activo del sistema — la aplicación no puede quedarse sin ningún administrador.
- **FR-043** Baja de usuario: se marca `activo = 0`, nunca se elimina (Artículo III); conserva su histórico de acciones en auditoría y sigue apareciendo como autor de preparaciones, verificaciones y entregas pasadas.
- **FR-044** Cambio de contraseña propio, y reseteo por un Administrador (genera contraseña provisional, marca `debe_cambiar_password`).
- **FR-045** Bloqueo tras N intentos fallidos de inicio de sesión consecutivos (por defecto 5), desbloqueable solo por un Administrador. Registrado en auditoría (`LOGIN_FALLIDO`).

### 4.6 Actualizaciones y nomenclátor (excepciones de red)

- **FR-050** Pantalla "Actualizaciones" en Configuración: botón "Comprobar actualizaciones" que consulta una URL fija de Anthropic/proveedor del software (no configurable por el usuario) y, si hay una versión nueva, ofrece descargarla. Nunca automático al arrancar sin que el usuario lo pida (Constitución Artículo VI.3).
- **FR-051** Pantalla "Nomenclátor" en Configuración: campo `url_nomenclator` editable, y botón "Descargar ahora" que obtiene el fichero Excel/CSV de esa URL para alimentar Spec 003/011. Si la URL cambia de formato o desaparece, el campo sigue siendo editable manualmente sin necesitar una nueva versión de la aplicación.
- **FR-052** Ambas acciones muestran el resultado (éxito, fecha de la última comprobación/descarga, o el motivo del fallo) y no impiden el uso del resto de la aplicación si fallan.

## 5. Entidades clave

| Entidad | Referencia |
|---|---|
| Farmacia | data-model.md — una sola fila |
| Usuario, Rol | data-model.md |

## 6. Criterios de aceptación

**CA-000 Asistente obligatorio en primer arranque**
Dado que no existe base de datos, cuando abro la aplicación, entonces se muestra el asistente y no puedo acceder a ninguna otra pantalla hasta completarlo.

**CA-001 Prefijo no afecta a numeración pasada**
Dado un paciente con num_ficha "F-000041", cuando cambio el prefijo a "PAC-", entonces ese paciente sigue mostrando "F-000041" y el siguiente alta es "PAC-000042".

**CA-002 Único administrador protegido**
Dado un sistema con un solo usuario Administrador activo, cuando ese usuario intenta darse de baja a sí mismo, entonces el sistema lo impide explicando el motivo.

**CA-003 Baja de usuario conserva histórico**
Dado un usuario dado de baja que fue elaborador de diez SPD, cuando consulto esos SPD, entonces siguen mostrando su nombre como elaborador.

**CA-004 Bloqueo por intentos fallidos**
Dado 5 intentos de contraseña incorrecta consecutivos, cuando se intenta un sexto, entonces el usuario queda bloqueado y solo un Administrador puede desbloquearlo.

**CA-005 Descarga de nomenclátor no bloquea la app**
Dado que la URL del nomenclátor no responde, cuando pulso "Descargar ahora", entonces veo el motivo del fallo y el resto de la aplicación sigue funcionando con normalidad.

**CA-006 Cambio de valores por defecto no reescribe pacientes existentes**
Dado un paciente con `dia_retirada = MARTES` fijado individualmente, cuando cambio `dia_retirada_defecto` a JUEVES, entonces ese paciente sigue en MARTES.

## 7. Casos límite

- Farmacia sin WhatsApp ni fax: campos opcionales, los documentos que los mencionan simplemente omiten la línea.
- Ruta de backup en una unidad de red que se desconecta: el backup al cerrar (Spec 010) debe fallar visiblemente, no en silencio; se avisa al usuario en ese momento, no solo en Configuración.
- Dos administradores, uno se autodesactiva mientras queda el otro activo: permitido, FR-042 solo protege el caso de quedarse a cero.

## 8. Fuera de alcance de esta spec

- Backup, cifrado y purga en detalle (Spec 010): aquí solo se fija la ruta y el disparo del asistente inicial.
- Import/export y perfiles de mapeo (Spec 011).
- Contenido de la ayuda (Spec 014).

## 9. Preguntas abiertas

| # | Pregunta | Propuesta si no hay respuesta |
|---|---|---|
| Q1 | FR-050: ¿la URL de actualizaciones apunta a un repositorio propio (GitHub Releases del proyecto GPLv3) o a un servidor propio de distribución? | GitHub Releases del repositorio del proyecto, coherente con la licencia GPLv3 |
| Q2 | FR-045: ¿5 intentos y bloqueo manual son razonables, o prefieres un bloqueo temporal automático (p. ej. 15 minutos) en vez de exigir intervención del administrador? | Bloqueo manual — es una farmacia pequeña, un administrador siempre está localizable |
