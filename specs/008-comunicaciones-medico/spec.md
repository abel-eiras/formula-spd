# Feature Specification: Comunicaciones al médico

**Feature Branch**: `008-comunicaciones-medico`

**Created**: 2026-09-06

**Status**: Draft — pendiente de `/speckit-clarify`

**Constitución aplicable:** 2.1.0 (Artículos I, II, V)

**Depende de:** Spec 001 (pacientes, médicos), Spec 004 (tratamiento) — ambas ya mergeadas en
`main`.

**Requerida por:** Ninguna (hoja terminal); consumida opcionalmente por Spec 006 (entrega con
cambios referidos).

**Input**: Especificación completa aportada literalmente por el propietario del producto
(`spec-008-comunicaciones-medico.md`, v0.1 — 2026-09-04). Se traslada tal cual: no se
reinterpretan los requisitos, no se añaden requisitos nuevos, no se cambia la numeración FR-8xx ni
los criterios de aceptación CA-8xx.

**Alcance de esta iteración**: FR-800 a FR-807 completos, salvo el punto de entrada de FR-805/
CA-803 ("desde una entrega con cambios referidos, Spec 006 FR-663"), que depende de una pantalla de
entrega que no existe todavía en esta rama. Se construye y prueba el método de creación
prerrellenada (`CrearDesdeAvisoCambioReferido`) como servicio invocable, con el mismo patrón que
research.md documenta para el resto de puntos de extensión hacia specs futuras en este proyecto
(Spec 005 research.md Decisiones 3 y 5); Spec 006 lo enganchará desde su propio aviso sin cambiar
esta firma. FR-806 (generación del documento imprimible vía Spec 007) también queda como punto de
extensión: esta spec registra el tipo de comunicación y expone qué comunicaciones son imprimibles
(`PRESENTACION`/`INCIDENCIA`, no `TELEFONO`), sin generar el documento en sí.

---

## Clarifications

Ninguna: el documento fuente no dejaba preguntas bloqueantes (§9 "Ninguna bloqueante") y no se
detectó ninguna ambigüedad adicional al trasladar los requisitos.

---

## 1. Propósito

Cubrir la carta de presentación del servicio al médico (Anexo 3) y la comunicación de incidencias
detectadas (Anexo 4), reutilizando el catálogo de médicos de Spec 001.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Crear y consultar comunicaciones |

## 3. Escenarios de usuario (User Stories)

### US1 (P1) — Presentar el servicio al incorporar un paciente (E1)
Como elaborador, al activar el servicio SPD para un paciente nuevo, genero la carta de
presentación para su médico de cabecera, con los datos de la farmacia y del paciente ya rellenos.

**Prueba independiente**: crear una comunicación `PRESENTACION` para un paciente con médico de
cabecera asignado y verificar que el médico se prerrellena sin indicarlo (CA-800).

### US2 (P1) — Comunicar una incidencia y registrar la respuesta (E2, E3)
Como elaborador, genero la comunicación de incidencia con lo detectado y mi propuesta, dirigida al
médico correspondiente, y más adelante registro su respuesta sin perder la fecha original.

**Prueba independiente**: crear una incidencia sin propuesta y verificar que el sistema lo impide
(CA-801); crear una completa, añadir la respuesta días después y verificar que la fecha original
no cambia (CA-802).

### US3 (P2) — Crear desde el tratamiento o desde un aviso de entrega (E4)
Como elaborador, desde la ficha de tratamiento (Spec 004) o desde un aviso de "tratamiento
pendiente de revisión" originado en una entrega (Spec 006, diferido), abro el formulario de
incidencia con paciente y médico ya seleccionados.

**Prueba independiente**: invocar el método de creación prerrellenada desde un tratamiento y
verificar que paciente y médico prescriptor quedan fijados (CA-803, adaptado al punto de extensión
de esta iteración).

## 4. Requisitos funcionales

- **FR-800** Tipo de comunicación: `PRESENTACION` (Anexo 3), `INCIDENCIA` (Anexo 4), `TELEFONO`
  (registro libre de una llamada, sin plantilla formal).
- **FR-801** Campos comunes: paciente, médico (selector sobre Spec 001, prerrellenado con el
  médico de cabecera o con el prescriptor del tratamiento si se origina desde una incidencia
  concreta), fecha.
- **FR-802** `INCIDENCIA`: incidencias detectadas (texto), propuesta del farmacéutico (texto),
  ambos obligatorios.
- **FR-803** Registro de respuesta: texto libre y fecha, opcional hasta que el médico conteste; el
  sistema no fuerza un plazo, solo permite marcar la comunicación como "pendiente de respuesta" o
  "resuelta" a criterio del usuario.
- **FR-804** Creación desde un tratamiento (Spec 004): un botón "Comunicar incidencia" en la ficha
  de tratamiento prerrellena paciente y médico prescriptor.
- **FR-805** Creación desde una entrega con cambios referidos (Spec 006 FR-663, diferido en esta
  iteración — ver alcance): un enlace directo desde el aviso de "tratamiento pendiente de
  revisión" ofrece crear la incidencia con el paciente y su médico de cabecera prerrellenados,
  dejando el texto de incidencias en blanco para que el elaborador lo redacte.
- **FR-806** El documento se genera mediante Spec 007 (`CARTA-PRES` / `CARTA-INC`, diferido en
  esta iteración — ver alcance); una comunicación tipo `TELEFONO` no genera documento imprimible,
  solo queda como registro interno.
- **FR-807** Ninguna comunicación se borra ni se edita tras guardarse, salvo el campo de respuesta
  y su fecha, que puede completarse más tarde (Artículo III: las comunicaciones son un hecho
  registrado, no un borrador).

## 5. Entidades clave

| Entidad | Referencia |
|---|---|
| ComunicacionMedico | docs/data-model.md §ComunicacionMedico |

## 6. Criterios de aceptación

**CA-800 Presentación prerrellenada**
Dado un paciente activado con médico de cabecera asignado, cuando genero una carta de
presentación, entonces el médico y los datos del paciente aparecen ya rellenos.

**CA-801 Incidencia exige propuesta**
Dado el formulario de incidencia sin rellenar el campo de propuesta, cuando intento guardar,
entonces el sistema lo impide.

**CA-802 Respuesta se añade sin reabrir el resto**
Dado una incidencia ya guardada, cuando registro la respuesta del médico dos semanas después,
entonces la comunicación conserva su fecha original y añade fecha y texto de respuesta.

**CA-803 Desde cambio referido en entrega**
*(Adaptado al punto de extensión de esta iteración — ver Assumptions.)* Dado el método de
preparación invocado con un paciente y su médico de cabecera, cuando se ejecuta, entonces
devuelve los datos de alta con paciente y médico ya fijados y el texto de incidencias vacío, sin
guardar nada todavía (FR-807).

**CA-804 Llamada telefónica no genera documento**
Dado un registro tipo TELEFONO, cuando lo reviso, entonces no hay opción de generar un documento
imprimible para él.

## 7. Casos límite

- Médico prescriptor distinto del médico de cabecera para el medicamento que originó la
  incidencia: FR-801 permite elegir cualquier médico del catálogo, no fuerza el de cabecera.
- Incidencia sin respuesta nunca registrada: queda "pendiente" indefinidamente; no hay purga ni
  cierre automático.
- Paciente dado de baja con comunicaciones abiertas: las comunicaciones se conservan igual que
  cualquier otro dato del paciente (Artículo III).

## 8. Fuera de alcance de esta spec

- Maquetación exacta de las cartas (Spec 007): esta spec solo expone qué comunicaciones son
  imprimibles, no genera el documento.
- Envío electrónico de la comunicación (fax, email) al médico: la aplicación genera el documento;
  el envío es responsabilidad manual de la farmacia (Constitución Artículo VI: sin conexiones de
  red salvo las dos excepciones tasadas, que no incluyen esta).
- El aviso real de "tratamiento pendiente de revisión" originado en una entrega (Spec 006): no
  existe todavía en esta rama; se deja el punto de extensión (FR-805).

## 9. Assumptions

- FR-805/CA-803 se implementan como un método de servicio invocable
  (`CrearDesdeAvisoCambioReferido`) que recibe directamente el paciente y su médico de cabecera ya
  resueltos, en vez de depender del aviso real de Spec 006 (que no existe en esta rama). Cuando
  Spec 006 exista, invocará este mismo método desde su propio aviso sin que cambie la firma
  pública (mismo patrón que Spec 005 aplicó para sus puntos de extensión hacia Spec 006).
- FR-806 (generación real del documento) se deja como dato expuesto (`EsImprimible`) más que como
  acción: Spec 007 decidirá cómo maquetar `CARTA-PRES`/`CARTA-INC` cuando exista.
