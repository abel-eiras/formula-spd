# Spec 008 — Comunicaciones al médico

**Estado:** Borrador para revisión
**Versión:** 0.1 — 2026-09-04
**Constitución aplicable:** 2.1.0 (Artículos I, II, V)
**Depende de:** Spec 001 (pacientes, médicos), Spec 004 (tratamiento)
**Requerida por:** Ninguna (hoja terminal); consumida opcionalmente por Spec 006 (entrega con cambios referidos)

---

## 1. Propósito

Cubrir la carta de presentación del servicio al médico (Anexo 3) y la comunicación de incidencias detectadas (Anexo 4), reutilizando el catálogo de médicos.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Crear y consultar comunicaciones |

## 3. Escenarios de usuario

### E1 — Presentar el servicio al incorporar un paciente
Como elaborador, al activar el servicio SPD para un paciente nuevo, quiero generar la carta de presentación para su médico de cabecera, con los datos de la farmacia y del paciente ya rellenos.

### E2 — Comunicar una incidencia detectada
Como elaborador, si detecto una posible interacción o un problema durante la revisión del tratamiento, quiero generar la comunicación de incidencia con lo detectado y mi propuesta, dirigida al médico correspondiente.

### E3 — Registrar la respuesta del médico
Como elaborador, cuando el médico contesta (por teléfono, por escrito), quiero registrar su respuesta y la fecha, para que quede trazado el cierre de la incidencia.

### E4 — Partir de un cambio referido en la entrega
Como elaborador, cuando una entrega (Spec 006) señala cambios de medicación referidos por el paciente, quiero poder crear una comunicación de incidencia directamente desde ese aviso, sin volver a escribir los datos del paciente.

## 4. Requisitos funcionales

- **FR-800** Tipo de comunicación: `PRESENTACION` (Anexo 3), `INCIDENCIA` (Anexo 4), `TELEFONO` (registro libre de una llamada, sin plantilla formal).
- **FR-801** Campos comunes: paciente, médico (selector sobre Spec 001, prerrellenado con el médico de cabecera o con el prescriptor del tratamiento si se origina desde una incidencia concreta), fecha.
- **FR-802** `INCIDENCIA`: incidencias detectadas (texto), propuesta del farmacéutico (texto), ambos obligatorios.
- **FR-803** Registro de respuesta: texto libre y fecha, opcional hasta que el médico conteste; el sistema no fuerza un plazo, solo permite marcar la comunicación como "pendiente de respuesta" o "resuelta" a criterio del usuario.
- **FR-804** Creación desde un tratamiento (Spec 004): un botón "Comunicar incidencia" en la ficha de tratamiento prerrellena paciente y médico prescriptor.
- **FR-805** Creación desde una entrega con cambios referidos (Spec 006 FR-663): un enlace directo desde el aviso de "tratamiento pendiente de revisión" ofrece crear la incidencia con el paciente y su médico de cabecera prerrellenados, dejando el texto de incidencias en blanco para que el elaborador lo redacte.
- **FR-806** El documento se genera mediante Spec 007 (`CARTA-PRES` / `CARTA-INC`); una comunicación tipo `TELEFONO` no genera documento imprimible, solo queda como registro interno.
- **FR-807** Ninguna comunicación se borra ni se edita tras guardarse, salvo el campo de respuesta y su fecha, que puede completarse más tarde (Artículo III: las comunicaciones son un hecho registrado, no un borrador).

## 5. Entidades clave

| Entidad | Referencia |
|---|---|
| ComunicacionMedico | data-model.md |

## 6. Criterios de aceptación

**CA-800 Presentación prerrellenada**
Dado un paciente activado con médico de cabecera asignado, cuando genero una carta de presentación, entonces el médico y los datos del paciente aparecen ya rellenos.

**CA-801 Incidencia exige propuesta**
Dado el formulario de incidencia sin rellenar el campo de propuesta, cuando intento guardar, entonces el sistema lo impide.

**CA-802 Respuesta se añade sin reabrir el resto**
Dado una incidencia ya guardada, cuando registro la respuesta del médico dos semanas después, entonces la comunicación conserva su fecha original y añade fecha y texto de respuesta.

**CA-803 Desde cambio referido en entrega**
Dado un aviso de tratamiento pendiente de revisión originado en una entrega, cuando pulso "Comunicar incidencia" desde ese aviso, entonces se abre el formulario con paciente y médico ya seleccionados.

**CA-804 Llamada telefónica no genera documento**
Dado un registro tipo TELEFONO, cuando lo reviso, entonces no hay opción de generar un documento imprimible para él.

## 7. Casos límite

- Médico prescriptor distinto del médico de cabecera para el medicamento que originó la incidencia: FR-801 permite elegir cualquier médico del catálogo, no fuerza el de cabecera.
- Incidencia sin respuesta nunca registrada: queda "pendiente" indefinidamente; no hay purga ni cierre automático.
- Paciente dado de baja con comunicaciones abiertas: las comunicaciones se conservan igual que cualquier otro dato del paciente (Artículo III).

## 8. Fuera de alcance de esta spec

- Maquetación exacta de las cartas (Spec 007).
- Envío electrónico de la comunicación (fax, email) al médico: la aplicación genera el documento; el envío es responsabilidad manual de la farmacia (Constitución Artículo VI: sin conexiones de red salvo las dos excepciones tasadas, que no incluyen esta).

## 9. Preguntas abiertas

Ninguna bloqueante.
