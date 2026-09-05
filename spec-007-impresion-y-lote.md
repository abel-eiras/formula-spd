# Spec 007 — Impresión, generación en lote y documentación base

**Estado:** Borrador para revisión
**Versión:** 0.1 — 2026-09-04
**Constitución aplicable:** 2.1.0 (Artículos II, IV, IX, X, XI)
**Depende de:** Spec 000 (configuración), 001 (pacientes), 005 (depósito y retirada), 006 (preparación/verificación/entrega), 009 (registros de calidad)
**Requerida por:** Ninguna interna; es el motor de generación de documentos usado por el resto

---

## 1. Propósito

Definir el motor único de generación de documentos de la aplicación, sus tres modos de uso — impresión individual desde una pantalla (ya referenciada en Spec 006 §4.9), generación en lote sobre varios pacientes a la vez, y generación de la documentación base de la farmacia desde Configuración — y la convención de nombres y carpetas de salida común a los tres.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Generar documentos individuales y en lote |
| Administrador | Generar y regenerar la documentación base de la farmacia (Configuración) |

## 3. Escenarios de usuario

### E1 — Documentación base al configurar la farmacia
Como administrador, tras rellenar los datos de la farmacia en Configuración, quiero pulsar "Generar documentación base" y obtener, en una carpeta, todos los documentos y registros en blanco del PNT ya membretados con mis datos, listos para imprimir y archivar en la carpeta física de inspección.

### E2 — Diez pacientes listos de una vez
Como elaborador, con diez pacientes cuyos envases están al día y cuyo tratamiento no ha cambiado, quiero seleccionarlos todos en el listado de preparaciones, pulsar "Generar documentos", elegir qué documentos quiero (ficha de preparación, etiquetas, instrucciones) y obtener todos los ficheros de golpe en la carpeta de salida, sin repetir el proceso paciente a paciente.

### E3 — Saber quién está listo sin repasar uno a uno
Como elaborador, quiero ver en el listado de preparaciones una columna que me diga de un vistazo si los envases de cada paciente están al día para su próxima sesión, para poder filtrar y seleccionar solo esos.

### E4 — Nombre de fichero que no se rompe con nombres largos
Como elaborador, quiero que el nombre del fichero generado identifique claramente el documento y el paciente, y que si el nombre completo es demasiado largo para el sistema de archivos, el sistema use el código de ficha y una abreviatura sin que la generación falle.

## 4. Requisitos funcionales

### 4.1 Catálogo de documentos

- **FR-700** Catálogo cerrado de tipos de documento, cada uno con código de abreviatura para nombres de fichero largos:

| Documento | Código | Ámbito | Fuente |
|---|---|---|---|
| Ficha de preparación, control y entrega | `FICHA` | Por SPD (blíster) | Spec 006 |
| Etiqueta anverso | `ETQ-A` | Por SPD | Spec 006 |
| Etiqueta reverso | `ETQ-R` | Por SPD | Spec 006 |
| Hoja de instrucciones al paciente | `INSTR` | Por SPD (o por sesión si Spec 006 FR-682 lo permite) | Spec 006 |
| Ficha del paciente (Anexo 2) | `FICHA-PAC` | Por paciente | Spec 001 |
| Evaluación de idoneidad | `IDONEIDAD` | Por evaluación | Spec 002 |
| Consentimiento informado (1a o 1b) | `CONSENT` | Por paciente | Spec 002 |
| Carta de presentación al médico | `CARTA-PRES` | Por comunicación | Spec 008 |
| Carta de incidencias al médico | `CARTA-INC` | Por comunicación | Spec 008 |
| Listado de retirada de envases | `RETIRADA` | Por fecha de listado | Spec 005 |
| Registro de condiciones ambientales | `REG-AMB` | Por periodo | Spec 009 |
| Registro de limpieza | `REG-LIMP` | Por periodo | Spec 009 |
| Registro de formación del personal | `REG-FORM` | Por usuario/periodo | Spec 009 |
| Registro de recogida de residuos no SIGRE | `REG-RES` | Por periodo | Spec 009 |
| Documentación base (conjunto) | — | Farmacia, una vez | Este documento §4.4 |

- **FR-701** Todo documento se genera en formato `.docx` (Word), editable, mediante el motor de generación de documentos (Artículo XI: legibilidad — una sola clase de servicio de generación por familia de documento, sin duplicar lógica de maquetación entre pantallas).

### 4.2 Nombre de fichero y carpeta de salida

- **FR-710** Nombre de fichero por defecto: `"<Nombre del documento>. <Nombre> <Apellido1> <Apellido2>.<ddmmaaaa>.docx"`, donde `<ddmmaaaa>` es la fecha de generación. Ejemplo: `Hoja de elaboración. María López Vidal.04092026.docx`.
- **FR-711** Si el nombre resultante supera 120 caracteres, o si el paciente no tiene apellidos completos, o si dos documentos generados en el mismo lote producirían el mismo nombre, el sistema usa el formato corto: `"<CODIGO>_<num_ficha>_<ddmmaaaa>.docx"`. Ejemplo: `FICHA_F-000123_04092026.docx`.
- **FR-712** Carpeta de salida configurable en Configuración (`Farmacia.ruta_documentos_generados`, ver Spec 000). Estructura por defecto:
  ```
  <ruta_documentos_generados>/
    <aaaa-mm-dd de la generación>/
      <ficheros generados en esa tanda>
  ```
  Una generación individual y una generación en lote ambas caen en la carpeta del día en que se generaron, no del periodo de validez del SPD.
- **FR-713** Cada fichero generado deja traza en auditoría (tipo de documento, entidad de origen, ruta final, usuario, fecha-hora) — Constitución Artículo VII.6.

### 4.3 Generación en lote

- **FR-720** Columna "Envases al día" en el listado de Preparaciones (Spec 006 FR-690), calculada por paciente/sesión pendiente: `SÍ` si, para la próxima sesión de ese paciente, todas las líneas del tratamiento activo tienen saldo suficiente en custodia (Spec 005) y el tratamiento no está `PENDIENTE_REVISION`; `NO` en caso contrario, con el motivo (qué falta) visible al pasar el cursor o en columna adicional.
- **FR-721** El listado admite filtrar por "Envases al día = SÍ" y selección múltiple de filas (pacientes o sesiones pendientes de crear).
- **FR-722** Acción "Generar documentos" sobre la selección. Dos casos según el estado de las sesiones seleccionadas:
  - Si el paciente aún no tiene sesión creada para el periodo (BORRADOR no existe): el sistema crea la sesión (Spec 006 FR-600) y la lleva hasta PREPARADO automáticamente para todos los seleccionados con "Envases al día = SÍ", usando las asignaciones de envase por defecto (sin intervención manual, porque por definición no hay líneas pendientes de decisión).
  - Si la sesión ya existe en PREPARADO o VERIFICADO: se usa tal cual.
  Un paciente con "Envases al día = NO" no puede incluirse en la generación en lote; el sistema lo excluye de la selección con aviso, sin bloquear al resto.
- **FR-723** Tras preparar (o localizar) las sesiones, un diálogo permite elegir qué tipos de documento generar de entre `FICHA`, `ETQ-A`, `ETQ-R`, `INSTR` (los aplicables a una preparación; el resto de la tabla FR-700 se generan desde su propia pantalla, no en este lote).
- **FR-724** El sistema genera todos los documentos elegidos para todos los pacientes/blísteres seleccionados en una sola operación, con barra de progreso, y al terminar muestra un resumen: generados, con aviso (p. ej. nombre acortado), y fallidos con motivo. Un fallo en un paciente no detiene el resto.
- **FR-725** Los documentos generados en lote no se marcan como impresos automáticamente en el sentido de "entregado": `impreso_*_en` se registra en el momento de la generación del fichero, igual que si se generara individualmente (generar el fichero es la acción relevante para la traza, no el envío a la impresora física).

### 4.4 Documentación base de la farmacia

- **FR-730** Desde Configuración, acción "Generar documentación base", disponible solo para Administrador. Genera, en un lote, la versión en blanco (sin datos de paciente) de cada documento de la tabla FR-700 que tenga una parte fija basada en los datos de la farmacia: cabeceras con nombre, código sanitario, logo, dirección, CIF, teléfono; los registros de calidad (`REG-AMB`, `REG-LIMP`, `REG-FORM`, `REG-RES`) como plantillas de tabla vacía lista para rellenar a mano si hiciera falta; los modelos de carta al médico con la cabecera de la farmacia y el cuerpo en blanco.
- **FR-731** Esta generación no depende de ningún paciente ni SPD; usa exclusivamente los datos de Configuración (Spec 000) y el catálogo de textos legales fijos (motivo por el que existe el Artículo I.2 de la constitución: cada plantilla contiene los elementos exigidos por el PNT).
- **FR-732** Salida en `<ruta_documentos_generados>/documentacion-base/<aaaa-mm-dd>/`, con nombre `"<Nombre del documento base>.docx"` (sin necesidad de fecha de paciente en el propio nombre, ya está en la carpeta).
- **FR-733** Regenerable en cualquier momento (p. ej. tras cambiar el logo o la dirección de la farmacia); cada generación crea una carpeta nueva fechada, sin sobrescribir la anterior — permite comparar versiones o recuperar una plantilla previa.

### 4.5 Formato de posología en documentos impresos

- **FR-740** En todo documento impreso que muestre posología (ficha de preparación, etiquetas, hoja de instrucciones), las dosis D/A/C/N se representan **siempre en fracciones**, nunca en decimales. Ejemplo: medio comprimido en desayuno y en cena se imprime `1/2 - 0 - 1/2 - 0`, no `0,5 - 0 - 0,5 - 0`.
- **FR-741** El vocabulario de fracciones admitido es cerrado, no una conversión decimal→fracción por aproximación: `0`, `1/4`, `1/3`, `1/2`, `2/3`, `3/4`, `1`, `1 1/4`, `1 1/2`, `1 3/4`, `2`, `2 1/2`, `3`… Cada valor de dosis que puede introducirse en el tratamiento (Spec 004) pertenece a este vocabulario cerrado, precisamente para que la conversión a fracción impresa sea exacta y no una aproximación de un decimal en coma flotante (`0,33` no reconstruye de forma fiable `1/3`).
- **FR-742** Internamente el sistema guarda el valor numérico equivalente (`0,5`, `0,333…`) para poder sumar y calcular unidades semanales (Spec 005 FR-522), pero la interfaz de introducción de dosis (Spec 004) es un selector sobre el vocabulario cerrado, no un campo de texto libre — así se evita que el usuario escriba un decimal que no tenga fracción exacta representable.
- **FR-743** En pantalla (no en documento impreso) se puede mostrar la fracción, el decimal, o ambos, según se defina en Spec 004; lo único obligatorio por esta spec es que **lo impreso sea siempre fracción**.

## 5. Entidades clave

Esta spec no añade tablas de negocio nuevas; usa `Farmacia` (Spec 000/005), `SPD` y sus líneas (Spec 006), y añade una entrada de Auditoría por fichero generado (FR-713).

## 6. Criterios de aceptación

**CA-700 Nombre por defecto**
Dado un paciente "María López Vidal" y una ficha de preparación generada el 4 de septiembre de 2026, cuando se genera el documento, entonces el fichero se llama `Ficha de preparación. María López Vidal.04092026.docx`.

**CA-701 Nombre acortado por longitud**
Dado un paciente con nombre y apellidos que producirían un nombre de más de 120 caracteres, cuando se genera el documento, entonces el fichero usa el formato `<CODIGO>_<num_ficha>_<fecha>.docx`.

**CA-702 Columna envases al día**
Dado un paciente con todas las líneas de su próximo blíster cubiertas por envases en custodia, cuando abro el listado de Preparaciones, entonces su fila muestra "Envases al día = SÍ"; si le falta un envase, muestra "NO" con el medicamento que falta.

**CA-703 Lote excluye a quien no está listo**
Dado diez pacientes seleccionados, ocho con "Envases al día = SÍ" y dos con "NO", cuando pulso "Generar documentos", entonces el sistema genera para los ocho y avisa de los dos excluidos sin detener la operación.

**CA-704 Lote crea y prepara sesiones automáticamente**
Dado un paciente sin sesión creada aún pero con "Envases al día = SÍ", cuando se incluye en una generación en lote, entonces el sistema crea su sesión, la asigna y la deja en PREPARADO antes de generar los documentos elegidos.

**CA-705 Resumen de lote con fallos parciales**
Dado un lote de diez pacientes donde uno falla por un error de plantilla, cuando termina la generación, entonces el resumen muestra nueve generados correctamente y uno fallido con el motivo, y los nueve ficheros existen en la carpeta de salida.

**CA-706 Documentación base con datos de farmacia**
Dado la Configuración con nombre "Farmacia Ejemplo", código sanitario "OU-045-F" y logo cargado, cuando el administrador genera la documentación base, entonces todos los documentos con cabecera institucional llevan ese nombre, código y logo.

**CA-707 Regenerar documentación base no sobrescribe**
Dado una documentación base generada el 1 de septiembre, cuando se genera de nuevo el 4 de septiembre tras cambiar el logo, entonces existen dos carpetas fechadas distintas, ambas accesibles.

**CA-708 Traza de generación en lote**
Dado un lote de cinco documentos generados, cuando reviso auditoría, entonces existen cinco entradas de tipo GENERAR_DOCUMENTO, cada una con su ruta final y el usuario que lanzó el lote.

**CA-709 Posología impresa en fracción**
Dado un tratamiento con pauta D=1/2, A=0, C=1/2, N=0, cuando se genera la ficha de preparación, entonces la posología impresa es `1/2 - 0 - 1/2 - 0`, no `0,5 - 0 - 0,5 - 0`.

**CA-710 Vocabulario cerrado evita aproximaciones**
Dado un tratamiento con dosis 1/3 de comprimido, cuando se imprime, entonces la ficha muestra exactamente `1/3`, no `0,33` ni `0,3`.

## 7. Casos límite

- Un paciente incluido en el lote cuya sesión pasa a requerir revisión (tratamiento `PENDIENTE_REVISION`) entre que se marcó "Envases al día = SÍ" y que se ejecuta el lote: el sistema lo revalida justo antes de generar y lo excluye si ya no cumple, en vez de fiarse del estado mostrado en pantalla segundos antes.
- Generación en lote de una sesión con dos blísteres: cuenta como una unidad de selección (el paciente), pero genera el doble de documentos (uno por blíster) automáticamente.
- Documentación base solicitada sin logo cargado: se genera igualmente, con el espacio del logo en blanco, sin bloquear.
- Dos pacientes con el mismo nombre y apellidos completos, generados en momentos distintos (no en el mismo lote): no hay colisión real porque cada fichero cae en la carpeta del día y un nombre repetido simplemente coexiste; FR-711 solo resuelve la colisión dentro de un mismo lote.

## 8. Fuera de alcance de esta spec

- El contenido y maquetación de cada plantilla concreta (qué campos lleva la ficha de preparación, cómo se ve la etiqueta): se define en la spec de cada funcionalidad de origen (006, 001, 002, 005, 008, 009).
- Impresión física (enviar a la impresora del sistema operativo): fuera de alcance de la aplicación; el usuario abre el `.docx` generado y lo imprime con su editor habitual, como ya se asumía en la Fase 1 (impresión en A4 con impresora normal).
- Firma electrónica o digital de cualquier documento (Constitución Artículo II: el papel es la base legal).

## 9. Preguntas abiertas

| # | Pregunta | Propuesta si no hay respuesta |
|---|---|---|
| Q1 | FR-710: ¿120 caracteres es razonable, dado que la ruta completa (carpeta + fichero) también tiene un límite en Windows? | 120 para el nombre de fichero; si la ruta completa supera 200, se usa igualmente el formato corto |
| Q2 | FR-701: ¿todos los documentos en `.docx`, o las etiquetas (formato A5/A6 apaisado) mejor como PDF para que no se descuadren al abrirlas en otro equipo? | Etiquetas y ficha en PDF; el resto en `.docx` editable — a confirmar |
