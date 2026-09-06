# Research: Idoneidad y consentimiento informado

## Decisión 1 — Criterios literales del PNT I §4.1 y resultado por criterio profesional con propuesta (FR-200/201)

El documento fuente dejaba el enunciado de los criterios como `[NEEDS CLARIFICATION]` para no
inventarlo. El PNT I del COF de A Coruña (`resources/`) enumera siete criterios de inclusión
orientativos y dos condiciones ("es importante que…"), y **no fija ninguna fórmula**: "su inclusión
debe quedar justificada en base al riesgo-beneficio". Se modelan las 9 casillas tal cual y el resultado
lo elige el farmacéutico. Para respetar el Art. V.1 (la aplicación propone, el usuario confirma), la
entidad `EvaluacionIdoneidad` expone `ResultadoPropuesto()` (regla pura, con test): NO_APTO si falta
alguna de las dos condiciones o no hay ningún criterio; APTO si no. FR-204 se extiende a "el resultado
elegido difiere de la propuesta" como caso de "criterios contradictorios".

**Alternativa descartada**: fórmula cerrada (p. ej. "≥2 criterios ⇒ APTO"). No tiene respaldo en el
PNT y convertiría un juicio clínico en un cálculo.

## Decisión 2 — Un único modelo de consentimiento (Anexo I.B) para PACIENTE y REPRESENTANTE (FR-210/212)

El Anexo I.B es un solo documento con la fórmula "en nombre propio, o como representante legal o
persona autorizada de…". No hacen falta dos plantillas (1a/1b): el generador `CONSENT` rellena el
firmante (paciente o contacto) y, si es representante, añade la relación y los datos del paciente
representado. Los diez compromisos del anexo se imprimen literales.

## Decisión 3 — Vigencia indefinida (Q2 del documento fuente)

Sin plazo de caducidad ni para la evaluación ni para el consentimiento; el PNT y la Guía del CIM no lo
exigen. "Vigente" = la evaluación más reciente; el consentimiento firmado y no revocado más reciente
(FR-202/FR-215).

## Decisión 4 — Alta inline del representante desde la pantalla de consentimiento (FR-211)

Spec 001 definió los contactos en Dominio/Infraestructura pero no construyó pantalla de gestión.
Para no bloquear FR-210 y no abrir aquí una pantalla general de contactos (fuera de alcance), el
servicio de esta spec expone `CrearRepresentante` (solo tipos `REPRESENTANTE_LEGAL` /
`PERSONA_AUTORIZADA`, DNI obligatorio por Spec 001 FR-022) y la pantalla ofrece el formulario en línea
cuando no hay elegibles (CA-202). Es el mismo patrón que Spec 001 FR-033 con los médicos.

## Decisión 5 — La activación automática pasa por `IServicioPacientes.CambiarEstado` (FR-213)

En vez de tocar `Paciente.Estado` directamente, `ServicioIdoneidadConsentimiento` delega en el
servicio de Spec 001, que ya valida la transición (`TransicionValida`) y audita. Así EVALUACION→ACTIVO
queda registrada con la misma traza que un cambio manual. La sugerencia de SUSPENDIDO (FR-214 y el caso
límite de NO_APTO sobre un ACTIVO) se devuelve como dato (`SugerirSuspension`) y la pantalla ofrece el
botón; nunca se suspende sin confirmación (Art. II: decisiones del profesional).

## Decisión 6 — `IDONEIDAD` no es un documento aparte; `CONSENT` sí (Spec 007 FR-700)

En el PNT la evaluación es un bloque del Anexo I.E (ficha del paciente). `FICHA-PAC` imprime la
evaluación vigente en ese bloque (criterios marcados, observaciones, resultado, fecha, farmacéutico) y,
si no hay ninguna, lo deja en blanco para cubrirlo a mano. El código `IDONEIDAD` reservado en Spec 007
queda sin uso, documentado en su catálogo. `CONSENT` se genera desde la pantalla de consentimiento y
marca `impreso_en`.

## Decisión 7 — El comprobador real sustituye al nulo sin tocar `ServicioPreparacion` (Spec 006)

`ComprobadorIdoneidadYConsentimientoReal` implementa el punto de extensión que Spec 006 dejó
documentado: `Aprobado(pacienteId)` = evaluación vigente APTO **y** consentimiento vigente.
`App.axaml.cs` lo registra en lugar del nulo; los tests de `ServicioPreparacion` siguen usando el nulo
(prueban la preparación, no la idoneidad) y CA-201 se prueba sobre el comprobador real con datos reales.
