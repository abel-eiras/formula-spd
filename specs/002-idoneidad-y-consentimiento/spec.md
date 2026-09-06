# Feature Specification: Idoneidad y consentimiento informado

**Feature Branch**: `002-idoneidad-y-consentimiento`

**Created**: 2026-09-06

**Status**: Implementada en la misma sesión (ver PROGRESO.md)

**Constitución aplicable:** 2.1.0 (Artículos I.3, II, III, V)

**Depende de:** Spec 001 (pacientes y contactos), Spec 007 (motor de documentos) — ambas en `main`.

**Requerida por:** Spec 006 (preparación exige consentimiento vigente e idoneidad APTO — hoy tapado con
`ComprobadorIdoneidadYConsentimientoNulo`, que esta spec sustituye por la implementación real).

**Input**: Especificación aportada literalmente por el propietario del producto
(`spec-002-idoneidad-y-consentimiento.md`, v0.1 — 2026-09-04). Se traslada tal cual: no se
reinterpretan requisitos, no se cambia la numeración FR-2xx ni CA-2xx. Sus dos `[NEEDS CLARIFICATION]`
(enunciado de los criterios y regla de combinación) se resuelven con el contenido literal del PNT I
del COF de A Coruña (`resources/`, analizado en `docs/analisis-resources.md` §2.7).

**Nota sobre numeración de anexos**: el documento fuente habla de "Anexo 9" (idoneidad) y "Anexos
1a/1b" (consentimiento) — numeración del PNT de Pontevedra citado en la constitución, que no está
disponible. En el PNT de A Coruña la evaluación de idoneidad es el bloque "EVALUACIÓN IDONEIDAD PARA
LA INCLUSIÓN EN EL SERVICIO" del **Anexo I.E** (ficha del paciente) y el consentimiento es un único
modelo, el **Anexo I.B**, que cubre tanto al paciente ("en nombre propio") como al representante legal
o persona autorizada. Esta spec usa el contenido de A Coruña y conserva los códigos de documento que
Spec 007 ya había reservado (`CONSENT`; `IDONEIDAD` se imprime dentro de `FICHA-PAC`, no como documento
aparte — research.md Decisión 6).

---

## Clarifications

### Session 2026-09-06

- Q: FR-200 — ¿cuál es el enunciado exacto de los criterios? → A: Los siete criterios de inclusión
  orientativos del PNT I §4.1 (literal): (1) polimedicados, especialmente con pautas posológicas
  complejas; (2) personas mayores que viven solas o dependientes sin cuidador o persona de referencia;
  (3) deficiencia cognitiva o demencia que impida seguir el tratamiento; (4) problemas de adherencia
  terapéutica; (5) programas concertados con la Administración que incluyan el SPD; (6) dificultades
  expresadas por la propia persona para gestionar su medicación; (7) a criterio del médico prescriptor
  o del farmacéutico. Más las dos condiciones que el PNT marca como importantes: motivación y
  disposición a gestionar la medicación (por sí mismo o vía cuidador); capacidad de manejar el DDP
  (destreza manual y agudeza visual suficientes, paciente o cuidador).
- Q: FR-201 — ¿qué regla combina los criterios en APTO/NO_APTO? → A: El PNT no fija fórmula: la
  idoneidad "quedará justificada en base al riesgo-beneficio" y el resultado lo decide el farmacéutico.
  La aplicación **propone** (Art. V.1): NO_APTO si no se cumple alguna de las dos condiciones o no se
  marca ningún criterio; APTO en caso contrario. El farmacéutico confirma o corrige; si su resultado
  difiere de la propuesta, o es NO_APTO, las observaciones son obligatorias (FR-204).
- Q: Q2 — ¿caduca el consentimiento o la idoneidad? → A: No; indefinidos mientras no cambie la
  situación. Reevaluación o nuevo consentimiento solo a criterio profesional (FR-202/FR-215).

---

## 1. Propósito

Cubrir la evaluación de idoneidad y el consentimiento informado, y la transición del paciente de
`EVALUACION` a `ACTIVO` cuando ambos están en regla — el invariante del Art. I.3 "no se prepara un SPD
para un paciente sin consentimiento vigente y evaluación de idoneidad APTO".

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Registrar evaluación y consentimiento |

## 3. Escenarios de usuario (User Stories)

### US1 (P1) — Evaluar a un paciente nuevo
Como elaborador, tras la entrevista, quiero rellenar los criterios de idoneidad y que el sistema me
proponga si el resultado es APTO o si algún criterio lo impide.

### US2 (P1) — Consentimiento del propio paciente
Como elaborador, con un paciente APTO y capaz de decidir por sí mismo, quiero generar el documento de
consentimiento para que lo firme, y registrar la fecha de la firma en papel.

### US3 (P1) — Consentimiento por representante
Como elaborador, con un paciente que no puede firmar por sí mismo, quiero elegir su representante legal
ya registrado como contacto y generar el consentimiento con sus datos, sin volver a teclearlos.

### US4 (P2) — Revocación
Como elaborador, si el paciente o su representante retira el consentimiento, quiero registrar la
revocación con fecha, dejando constancia sin borrar el consentimiento anterior.

### US5 (P2) — Reevaluar tras un cambio relevante
Como elaborador, si cambia significativamente la situación del paciente, quiero poder registrar una
nueva evaluación de idoneidad sin perder la anterior.

## 4. Requisitos funcionales

### 4.1 Evaluación de idoneidad

- **FR-200** Formulario con los 7 criterios de inclusión y las 2 condiciones del PNT I §4.1 (ver
  Clarifications), cada uno con respuesta Sí/No, y observaciones opcionales.
- **FR-201** Resultado `APTO` / `NO_APTO`: la aplicación propone según la regla de Clarifications y el
  farmacéutico decide; la propuesta es una regla pura de Dominio con test unitario (Art. IX.1).
- **FR-202** Un paciente puede tener varias evaluaciones a lo largo del tiempo; la más reciente es la
  vigente. Ninguna se sobrescribe ni se borra (Artículo III).
- **FR-203** Evaluación `NO_APTO`: el paciente no puede pasar a `ACTIVO` ni se le puede preparar un SPD
  (Spec 006 FR-602), pero su ficha permanece completa por si se reevalúa más adelante.
- **FR-204** Observaciones obligatorias cuando el resultado es `NO_APTO` o cuando el resultado elegido
  difiere de la propuesta (criterios contradictorios), para justificar la decisión profesional
  (Artículo I.2: trazabilidad de la decisión).

### 4.2 Consentimiento informado

- **FR-210** Tipo `PACIENTE` o `REPRESENTANTE`. Si `REPRESENTANTE`, exige seleccionar un contacto del
  paciente de tipo `REPRESENTANTE_LEGAL` o `PERSONA_AUTORIZADA` (Spec 001 FR-020) con DNI informado
  (Spec 001 FR-022).
- **FR-211** Si el paciente no tiene ningún contacto de ese tipo, el sistema ofrece crearlo en el
  momento, sin salir de la pantalla de consentimiento.
- **FR-212** El documento se genera (Spec 007, código `CONSENT`, Anexo I.B) y se registra `fecha_firma`
  cuando el usuario confirma que el papel ya está firmado — la aplicación no firma nada, solo registra
  la fecha del hecho (Constitución Artículo II).
- **FR-213** Un paciente pasa de `EVALUACION` a `ACTIVO` (Spec 001 FR-006) automáticamente cuando existen
  a la vez una evaluación vigente `APTO` y un consentimiento vigente con `fecha_firma` informada y sin
  `fecha_revocacion`.
- **FR-214** Revocación: registra `fecha_revocacion` y motivo. El consentimiento no se borra; deja de
  ser vigente. Si no hay otro vigente, el sistema avisa y ofrece pasar al paciente a `SUSPENDIDO`.
- **FR-215** Un nuevo consentimiento sustituye al anterior como vigente (el anterior queda con fecha,
  sin revocar explícitamente); cubre la renovación periódica sin necesitar revocación previa.

## 5. Entidades clave

| Entidad | Referencia |
|---|---|
| EvaluacionIdoneidad | data-model.md |
| Consentimiento | data-model.md |
| Contacto | Spec 001 |

## 6. Criterios de aceptación

**CA-200 Resultado APTO habilita** — Dado un paciente con evaluación APTO y consentimiento firmado sin
revocar, cuando se consulta su estado, entonces es ACTIVO.

**CA-201 NO_APTO bloquea preparación** — Dado un paciente con última evaluación NO_APTO, cuando se
intenta crear una sesión de preparación (Spec 006), entonces el sistema lo impide.

**CA-202 Consentimiento por representante exige contacto con DNI** — Dado un paciente sin contactos de
tipo REPRESENTANTE_LEGAL, cuando intento crear un consentimiento de representante, entonces el sistema
me ofrece crear el contacto antes de continuar.

**CA-203 Revocación no borra** — Dado un consentimiento firmado el 1 de enero, cuando se revoca el 1 de
junio, entonces sigue existiendo con ambas fechas, y el paciente pasa a no cumplir las condiciones de
ACTIVO si no hay otro vigente.

**CA-204 Varias evaluaciones, la última manda** — Dado dos evaluaciones, la primera NO_APTO y la segunda
APTO, cuando se consulta el estado vigente, entonces es APTO y ambas siguen siendo consultables.

**CA-205 Observaciones obligatorias en NO_APTO** — Dado un formulario con resultado NO_APTO y sin
observaciones, cuando intento guardar, entonces el sistema lo impide.

## 7. Casos límite

- Paciente que recupera capacidad y pasa de representante a firmar él mismo: nuevo consentimiento tipo
  PACIENTE; el anterior queda en el historial sin revocarse.
- Representante que deja de serlo: se da de baja el contacto (Spec 001); el consentimiento firmado por
  él en su día sigue siendo válido como hecho histórico.
- Evaluación registrada por error: no se borra (Artículo III); se registra una nueva, que pasa a ser la
  vigente.
- Nueva evaluación NO_APTO sobre un paciente ya ACTIVO: no se cambia el estado automáticamente (podría
  tener SPD en curso); la preparación de nuevos SPD queda bloqueada por FR-203 y la pantalla ofrece pasar
  a SUSPENDIDO, igual que en FR-214.

## 8. Fuera de alcance de esta spec

- Maquetación exacta del Anexo I.B más allá de sus elementos mínimos (Spec 007, Art. I.2).
- Bajas y purga del paciente (Spec 001, Spec 010).
- Gestión general de contactos del paciente (Spec 001 no construyó pantalla; aquí solo el alta inline
  de un representante o persona autorizada, FR-211).

## 9. Assumptions

- La numeración de anexos sigue la del PNT de A Coruña disponible en `resources/` (ver nota de cabecera).
- Los textos de los criterios se toman literales del PNT I §4.1; si el PNT de Pontevedra formulase el
  "Anexo 9" con otros enunciados, se sustituyen los textos sin cambiar la estructura (9 casillas).
