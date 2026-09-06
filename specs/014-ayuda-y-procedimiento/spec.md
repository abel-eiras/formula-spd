# Feature Specification: Ayuda de la aplicación y guía de procedimiento

**Feature Branch**: `014-ayuda-y-procedimiento`

**Created**: 2026-09-06

**Status**: Implementada en la misma sesión (ver PROGRESO.md)

**Constitución aplicable:** 2.1.0 (Artículos I, II, IX, XI)

**Depende de:** Ninguna (contenido; referencia las pantallas de las specs 000–011).

**Input**: Especificación aportada literalmente por el propietario del producto
(`spec-014-ayuda-y-procedimiento.md`, v0.1 — 2026-09-04). Se traslada tal cual, sin cambiar la
numeración FR-14xx ni CA-14xx. El contenido de la sección Procedimiento se redacta a partir de los PNT
del COF de A Coruña, la Guía de implantación del CIM (junio 2022) y el curso del CGCOF analizados en
`docs/analisis-resources.md` §3, en clave operativa ("qué hago en la aplicación en este paso y por qué").

---

## Clarifications

### Session 2026-09-06

- Q: Q1 — ¿ventana dentro de la aplicación o navegador con HTML? → A: Ventana Avalonia dentro de la
  aplicación (propuesta del documento fuente; coherente con el aislamiento del Art. VI).
- Q: Q2 — ¿se imprime el checklist de documentación por momento del servicio? → A: Diferido; el
  apartado existe en la ayuda (FR-1403) y puede imprimirse más adelante como documento del catálogo de
  Spec 007 (`CHECKLIST-PROC`) si el propietario lo pide.

---

## 1. Propósito

La documentación es el objetivo principal de la aplicación. El menú de ayuda no es un manual de
botones — es la forma en que la aplicación transmite **el procedimiento completo** para prestar el
servicio con garantías ante una inspección, apoyándose en la propia herramienta para explicarlo.

## 2. Actores

| Actor | Puede |
|---|---|
| Cualquier usuario | Consultar toda la ayuda |
| Administrador | Nada adicional: el contenido viene con la aplicación, no se edita desde la interfaz |

## 3. Escenarios de usuario (User Stories)

### US1 (P1) — Un elaborador nuevo que no conoce el PNT
Quiero abrir la ayuda y encontrar, en orden, todo el procedimiento desde que el paciente entra por la
puerta hasta que se le entrega el primer blíster.

### US2 (P1) — Duda puntual sobre una pantalla
En mitad de la verificación, quiero pulsar F1 y llegar directamente a la explicación de qué significa
cada ítem del checklist y por qué existe.

### US3 (P2) — Prepararse para una inspección
Como titular, quiero repasar el listado completo de qué documentación debe existir en cada momento del
servicio.

### US4 (P2) — Entender por qué la aplicación exige algo
Cuando la aplicación me bloquea una acción, quiero ver de un clic la razón normativa.

## 4. Requisitos funcionales

### 4.1 Estructura de la ayuda

- **FR-1400** Menú de ayuda accesible desde cualquier pantalla (F1 o botón), con dos secciones
  diferenciadas: **Uso de la aplicación** (qué hace cada pantalla, botón y campo, organizado como la
  navegación) y **Procedimiento del servicio SPD** (el circuito completo explicado como procedimiento de
  trabajo, con qué documento se genera en cada paso y qué exige el PNT). No es una copia del PNT; es su
  traducción operativa.
- **FR-1401** Cada pantalla tiene un punto de entrada directo (F1) a la sección de Procedimiento que le
  corresponde, con enlace cruzado a "Uso → pantalla".
- **FR-1402** Cuando un bloqueo o aviso tiene razón normativa, la pantalla ofrece "¿Por qué?" que abre el
  punto correspondiente de Procedimiento.
- **FR-1403** Índice de Procedimiento con una lista de comprobación de documentación por momento del
  servicio (alta, antes de preparar, para entregar, conservación y plazos).
- **FR-1404** Búsqueda por texto libre (título y contenido), con resultados que muestran su sección.
- **FR-1405** Todo el contenido vive en ficheros Markdown embebidos en la aplicación (Art. XI.5), no en
  base de datos ni editable desde la interfaz.

### 4.2 Alcance del contenido

- **FR-1410** La sección Procedimiento cubre, como mínimo, un apartado por cada spec funcional (001 a
  011 y sucesivas), en lenguaje de procedimiento de trabajo.
- **FR-1411** Cada apartado de Procedimiento indica: qué documento(s) se genera(n), qué constitución/PNT
  lo exige, y qué pasa si se omite.
- **FR-1412** La sección Uso cubre toda pantalla que exista en la aplicación.

## 5. Entidades clave

Ninguna de negocio. Contenido estático embebido (FR-1405).

## 6. Criterios de aceptación

**CA-1400 Dos secciones accesibles** — Dado el menú de ayuda, cuando lo abro, entonces veo separadas
"Uso de la aplicación" y "Procedimiento del servicio SPD".

**CA-1401 F1 contextual** — Dado que estoy en la pantalla de verificación, cuando pulso F1, entonces se
abre el apartado de Procedimiento sobre verificación, no el índice general.

**CA-1402 Enlace "¿Por qué?" en un bloqueo** — Dado el aviso "verificador igual al elaborador, se exige
motivo", cuando pulso "¿Por qué?", entonces llego al apartado que explica la regla y su origen.

**CA-1403 Checklist de inspección** — Dado que abro "Procedimiento → Documentación por momento del
servicio", cuando reviso "antes de preparar", entonces encuentro la lista (consentimiento vigente,
idoneidad APTO, tratamiento activo) con referencia a dónde se ve cada uno en la aplicación.

**CA-1404 Búsqueda por texto** — Dado que busco "verificador", cuando reviso los resultados, entonces
aparecen tanto el apartado de Uso de la pantalla de verificación como el de Procedimiento.

## 7. Casos límite

- Spec nueva sin apartado de ayuda: no bloquea; es deuda de calidad (FR-1410/1412). Un test comprueba
  que cada ventana de la aplicación tiene su apartado de Uso.
- Contenido desactualizado tras un cambio de regla: es texto versionado con el código; se revisa en el
  mismo commit (disciplina de proceso, no regla de la aplicación).

## 8. Fuera de alcance

- Edición del contenido desde la interfaz; traducción; vídeos.
- Impresión del checklist de documentación (Q2).

## 9. Assumptions

- Numeración de anexos según el PNT del COF de A Coruña disponible en `resources/` (ver
  `docs/analisis-resources.md` §2.1); si la fuente canónica cambia, solo cambian referencias en el texto.
