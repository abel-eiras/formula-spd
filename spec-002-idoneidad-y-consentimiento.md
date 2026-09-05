# Spec 002 — Idoneidad y consentimiento informado

**Estado:** Borrador para revisión
**Versión:** 0.1 — 2026-09-04
**Constitución aplicable:** 2.1.0 (Artículos I.3, II, III, V)
**Depende de:** Spec 001 (pacientes y contactos)
**Requerida por:** Spec 006 (preparación exige consentimiento vigente e idoneidad APTO)

---

## 1. Propósito

Cubrir el Anexo 9 (evaluación de idoneidad) y los Anexos 1a/1b (consentimiento informado), y la transición del paciente de `EVALUACION` a `ACTIVO` cuando ambos están en regla.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Registrar evaluación y consentimiento |

## 3. Escenarios de usuario

### E1 — Evaluar a un paciente nuevo
Como elaborador, tras la entrevista, quiero rellenar los 7 criterios del Anexo 9 y que el sistema me diga si el resultado es APTO o si algún criterio lo impide.

### E2 — Consentimiento del propio paciente
Como elaborador, con un paciente APTO y capaz de decidir por sí mismo, quiero generar el documento 1a para que lo firme, y registrar la fecha de la firma en papel.

### E3 — Consentimiento por representante
Como elaborador, con un paciente que no puede firmar por sí mismo, quiero elegir su representante legal ya registrado como contacto y generar el 1b con sus datos, sin volver a teclearlos.

### E4 — Revocación
Como elaborador, si el paciente o su representante retira el consentimiento, quiero registrar la revocación con fecha, dejando constancia sin borrar el consentimiento anterior.

### E5 — Reevaluar tras un cambio relevante
Como elaborador, si cambia significativamente la situación del paciente (nuevo diagnóstico, cambio de capacidad), quiero poder registrar una nueva evaluación de idoneidad sin perder la anterior.

## 4. Requisitos funcionales

### 4.1 Evaluación de idoneidad

- **FR-200** Formulario con los 7 criterios del Anexo 9, cada uno con respuesta Sí/No y observaciones opcionales. `[NEEDS CLARIFICATION: el enunciado exacto de los 7 criterios debe tomarse literal del Anexo 9 del PNT al redactar la pantalla — no se reproduce aquí para evitar una transcripción incorrecta; quien implemente esta spec debe copiarlos del documento original]`.
- **FR-201** Resultado `APTO` / `NO_APTO`, calculado según la combinación de criterios que define el PNT (algunos criterios son excluyentes por sí solos; otros se valoran en conjunto) — regla exacta a fijar en el mismo punto que FR-200, con test unitario en Dominio (Constitución Artículo IX.1).
- **FR-202** Un paciente puede tener varias evaluaciones a lo largo del tiempo; la más reciente es la vigente. Ninguna se sobrescribe ni se borra (Artículo III).
- **FR-203** Evaluación `NO_APTO`: el paciente no puede pasar a `ACTIVO` ni se le puede preparar un SPD (Spec 006 FR-602), pero su ficha permanece completa por si se reevalúa más adelante.
- **FR-204** Campo de observaciones obligatorio cuando el resultado es `NO_APTO` o cuando hay criterios contradictorios, para justificar la decisión profesional (Artículo I.2: trazabilidad de la decisión).

### 4.2 Consentimiento informado

- **FR-210** Tipo `PACIENTE` (Anexo 1a) o `REPRESENTANTE` (Anexo 1b). Si `REPRESENTANTE`, exige seleccionar un contacto del paciente marcado como `REPRESENTANTE_LEGAL` o `PERSONA_AUTORIZADA` (Spec 001 FR-020) con DNI informado (Spec 001 FR-022).
- **FR-211** Si el paciente no tiene ningún contacto de ese tipo dado de alta, el sistema ofrece crearlo en el momento (mismo patrón que Spec 001 FR-033 para médicos), sin salir de la pantalla de consentimiento.
- **FR-212** El documento se genera (Spec 007, código `CONSENT`) y se registra `fecha_firma` cuando el usuario confirma que el papel ya está firmado — la aplicación no firma nada, solo registra la fecha del hecho (Constitución Artículo II).
- **FR-213** Un paciente pasa de `EVALUACION` a `ACTIVO` (Spec 001 FR-006) automáticamente cuando existen a la vez: una evaluación vigente `APTO` y un consentimiento vigente con `fecha_firma` informada y sin `fecha_revocacion`.
- **FR-214** Revocación: registra `fecha_revocacion` y motivo. El consentimiento no se borra; simplemente deja de ser vigente. Si no hay otro consentimiento vigente, el sistema avisa y ofrece pasar al paciente a `SUSPENDIDO` (Spec 001 FR-006).
- **FR-215** Un nuevo consentimiento del mismo tipo sustituye al anterior como vigente (el anterior queda con fecha, sin revocar explícitamente, simplemente no es el más reciente); esto cubre la renovación periódica si la farmacia la practica, sin necesitar una revocación previa.

## 5. Entidades clave

| Entidad | Referencia |
|---|---|
| EvaluacionIdoneidad | data-model.md |
| Consentimiento | data-model.md |
| Contacto | Spec 001 |

## 6. Criterios de aceptación

**CA-200 Resultado APTO habilita**
Dado un paciente con evaluación APTO y consentimiento firmado sin revocar, cuando se consulta su estado, entonces es ACTIVO.

**CA-201 NO_APTO bloquea preparación**
Dado un paciente con última evaluación NO_APTO, cuando se intenta crear una sesión de preparación (Spec 006), entonces el sistema lo impide.

**CA-202 Consentimiento 1b exige contacto con DNI**
Dado un paciente sin contactos de tipo REPRESENTANTE_LEGAL, cuando intento generar un 1b, entonces el sistema me ofrece crear el contacto antes de continuar.

**CA-203 Revocación no borra**
Dado un consentimiento firmado el 1 de enero, cuando se revoca el 1 de junio, entonces sigue existiendo con ambas fechas, y el paciente pasa a no cumplir las condiciones de ACTIVO si no hay otro vigente.

**CA-204 Varias evaluaciones, la última manda**
Dado dos evaluaciones de un paciente, la primera NO_APTO y la segunda (posterior) APTO, cuando se consulta el estado vigente, entonces es APTO y ambas evaluaciones siguen siendo consultables.

**CA-205 Observaciones obligatorias en NO_APTO**
Dado un formulario de evaluación con resultado NO_APTO y sin observaciones, cuando intento guardar, entonces el sistema lo impide.

## 7. Casos límite

- Paciente que recupera capacidad y pasa de representante a firmar él mismo: se genera un nuevo consentimiento tipo PACIENTE; el anterior tipo REPRESENTANTE queda en su historial sin revocarse necesariamente (puede coexistir como histórico).
- Representante que deja de serlo (p. ej. cambio de tutela): se da de baja el contacto (Spec 001), el consentimiento firmado por él en su día sigue siendo válido como hecho histórico; solo un nuevo consentimiento requeriría un representante vigente.
- Evaluación registrada por error: no se borra (Artículo III); se registra una nueva evaluación correcta, que pasa a ser la vigente.

## 8. Fuera de alcance de esta spec

- Generación y maquetación exacta de los documentos 1a/1b y del Anexo 9 (Spec 007 define el motor; el contenido literal se copia del PNT al implementar).
- Bajas y purga del paciente (Spec 001, Spec 010).

## 9. Preguntas abiertas

| # | Pregunta | Propuesta si no hay respuesta |
|---|---|---|
| Q1 | FR-200/201: enunciado exacto y regla de combinación de los 7 criterios del Anexo 9 | Copiar literal del PNT al implementar; no inventar redacción |
| Q2 | ¿Existe un plazo de caducidad del consentimiento/idoneidad que obligue a reevaluar periódicamente, o es indefinido mientras no cambie la situación del paciente? | Indefinido; reevaluación solo ante cambio relevante, a criterio profesional |
