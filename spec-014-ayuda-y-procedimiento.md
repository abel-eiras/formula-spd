# Spec 014 — Ayuda de la aplicación y guía de procedimiento

**Estado:** Borrador para revisión
**Versión:** 0.1 — 2026-09-04
**Constitución aplicable:** 2.1.0 (Artículos I, II, IX, XI)
**Depende de:** Ninguna (contenido, no requiere otras funcionalidades implementadas, aunque referencia sus pantallas)
**Requerida por:** Ninguna

---

## 1. Propósito

La documentación es el objetivo principal de la aplicación: facilitar la creación y el mantenimiento de toda la documentación que exige el servicio de SPD. El menú de ayuda no es un manual de botones — es la forma en que la aplicación transmite **el procedimiento completo** para prestar el servicio con garantías ante una inspección, apoyándose en la propia herramienta para explicarlo. La facilidad de uso (Fase 1, requisito original) es el objetivo secundario que esta ayuda también sirve.

## 2. Actores

| Actor | Puede |
|---|---|
| Cualquier usuario | Consultar toda la ayuda |
| Administrador | Nada adicional en esta spec: el contenido de ayuda viene con la aplicación, no se edita desde la interfaz |

## 3. Escenarios de usuario

### E1 — Un elaborador nuevo que no conoce el PNT
Como elaborador que empieza a trabajar con SPD, quiero abrir la ayuda y encontrar, en orden, todo el procedimiento que tengo que seguir con un paciente desde que entra por la puerta hasta que se le entrega el primer blíster, para no depender de que alguien me lo explique de memoria.

### E2 — Duda puntual sobre una pantalla
Como elaborador en mitad de la verificación, quiero pulsar el icono o la tecla de ayuda de esa pantalla y llegar directamente a la explicación de qué significa cada ítem del checklist y por qué existe, sin buscar en un índice.

### E3 — Prepararse para una inspección
Como titular, antes de una inspección, quiero repasar en la ayuda el listado completo de qué documentación debe existir y estar disponible en cada momento del servicio, para comprobar que no falta nada.

### E4 — Entender por qué la aplicación exige algo
Como elaborador, cuando la aplicación me bloquea una acción (p. ej. "verificador igual al elaborador exige motivo"), quiero poder ver de un clic la razón normativa de esa exigencia, no solo el mensaje de error.

## 4. Requisitos funcionales

### 4.1 Estructura de la ayuda

- **FR-1400** Menú de ayuda accesible desde cualquier pantalla (icono o tecla F1), con dos secciones diferenciadas:
  1. **Uso de la aplicación**: qué hace cada pantalla, botón y campo. Organizado igual que la navegación de la aplicación.
  2. **Procedimiento del servicio SPD**: el circuito completo — idoneidad, consentimiento, alta de tratamiento y depósito, preparación, verificación, entrega, continuidad, comunicación con el médico, registros de calidad, bajas — explicado como procedimiento de trabajo, con referencia expresa a qué documento se genera en cada paso y a qué exige el PNT en cada uno. No es una copia del PNT completo; es su traducción operativa a "qué hago en la aplicación en este paso y por qué".
- **FR-1401** Cada pantalla tiene un punto de entrada directo (F1 o icono "?") a la sección de la sección 2 (Procedimiento) que le corresponde, no solo a la sección 1 (Uso). Ejemplo: F1 en la pantalla de verificación abre "Procedimiento → Verificación", que explica el checklist y su fundamento, con un enlace cruzado a "Uso → Pantalla de verificación" si además hace falta ayuda de botones.
- **FR-1402** Cuando un mensaje de bloqueo o aviso de la aplicación tiene una razón normativa (verificador≠elaborador, validez máxima, consentimiento obligatorio, etc.), el propio mensaje incluye un enlace "¿Por qué?" que abre el punto correspondiente de la sección Procedimiento.
- **FR-1403** Índice de la sección Procedimiento organizado como una lista de comprobación de documentación por momento del servicio: qué debe existir en el alta, qué debe existir antes de preparar, qué debe existir para entregar, qué debe conservarse y durante cuánto tiempo. Este índice es lo que el titular usaría literalmente para autoevaluarse antes de una inspección (Escenario E3).
- **FR-1404** La ayuda se busca por texto libre (título y contenido), con resultados que muestran a qué sección pertenecen.
- **FR-1405** Todo el contenido de ayuda vive en ficheros de texto (Markdown) embebidos en la aplicación, no en una base de datos ni editable desde la interfaz — es documentación versionada junto con el código (Constitución Artículo XI.5), no un dato de usuario.

### 4.2 Alcance del contenido

- **FR-1410** La sección Procedimiento cubre, como mínimo, un apartado por cada spec funcional del proyecto (001 a 010 y sucesivas), escrito en lenguaje de procedimiento de trabajo, no en lenguaje de especificación técnica.
- **FR-1411** Cada apartado de Procedimiento indica explícitamente: qué documento(s) se genera(n) en ese paso, qué constitución/PNT exige, y qué pasa si se omite (consecuencia práctica, no solo normativa).
- **FR-1412** La sección Uso cubre toda pantalla que exista en la aplicación; no puede quedar una pantalla sin su entrada de ayuda correspondiente — se trata como un criterio de "terminado" de cada spec funcional (a añadir como nota en el Artículo IX de la constitución si se considera necesario, fuera de esta spec).

## 5. Entidades clave

Ninguna de negocio. Contenido estático embebido; ver FR-1405.

## 6. Criterios de aceptación

**CA-1400 Dos secciones accesibles**
Dado el menú de ayuda, cuando lo abro, entonces veo separadas "Uso de la aplicación" y "Procedimiento del servicio SPD".

**CA-1401 F1 contextual**
Dado que estoy en la pantalla de verificación, cuando pulso F1, entonces se abre el apartado de Procedimiento sobre verificación, no el índice general.

**CA-1402 Enlace "¿Por qué?" en un bloqueo**
Dado el aviso "verificador igual al elaborador, se exige motivo", cuando pulso "¿Por qué?", entonces llego al apartado de Procedimiento que explica la regla y su origen normativo.

**CA-1403 Checklist de inspección**
Dado que abro "Procedimiento → Documentación por momento del servicio", cuando reviso el apartado "antes de preparar", entonces encuentro la lista de qué debe existir (consentimiento vigente, idoneidad APTO, tratamiento activo) con referencia a dónde se ve cada uno en la aplicación.

**CA-1404 Búsqueda por texto**
Dado que busco "verificador" en la ayuda, cuando reviso los resultados, entonces aparecen tanto el apartado de Uso de la pantalla de verificación como el apartado de Procedimiento correspondiente.

## 7. Casos límite

- Una spec nueva se añade al proyecto sin su apartado de ayuda correspondiente: no es un bloqueo técnico de la aplicación, pero sí un criterio de calidad incompleto (FR-1410/1412); se señala como deuda pendiente, no impide el uso de la funcionalidad.
- Contenido de ayuda desactualizado tras un cambio de regla de negocio: al ser texto embebido versionado junto al código (Constitución Artículo XI.5), el cambio de spec y el cambio de ayuda se revisan en el mismo commit — es una disciplina de proceso de desarrollo, no una regla que la aplicación pueda hacer cumplir por sí sola.

## 8. Fuera de alcance de esta spec

- Edición de contenido de ayuda desde la interfaz de usuario: la ayuda es documentación del producto, no un dato configurable por la farmacia.
- Traducción a otros idiomas (Spec 001 ya fijó castellano como único idioma de interfaz).
- Vídeos o contenido multimedia: solo texto e imágenes estáticas si hacen falta capturas de pantalla.

## 9. Preguntas abiertas

| # | Pregunta | Propuesta si no hay respuesta |
|---|---|---|
| Q1 | ¿La ayuda se muestra en una ventana dentro de la aplicación o abre el navegador con ficheros HTML locales? | Ventana dentro de la aplicación (Avalonia), coherente con el aislamiento de red del Artículo VI |
| Q2 | ¿Se exporta/imprime el checklist de documentación por momento del servicio como documento aparte (útil para pegar en la pared del obrador)? | Sí, como uno más del catálogo de Spec 007, código `CHECKLIST-PROC` |
