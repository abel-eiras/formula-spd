# Plan y progreso — Spec 002: Idoneidad y consentimiento informado

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: esta spec estaba bloqueada desde el 4-sep por contenido ("copiar literal del
PNT los 7 criterios del Anexo 9"). El cribado de `resources/` (`docs/analisis-resources.md` §2.7)
aportó el PNT I del COF de A Coruña con los criterios de inclusión, el consentimiento (Anexo I.B) y el
bloque de idoneidad (Anexo I.E) literales, y el usuario dio vía libre para seguir avanzando con lo
aprendido. Se bifurca de `main` tras la corrección 007b (documentos con elementos mínimos). Migración
`0011_idoneidad_consentimiento.sql`.

- **2026-09-06** — Ciclo `/speckit-specify` → `plan` → `tasks` → `implement` de forma autónoma.
  Las dos preguntas abiertas del documento fuente se resuelven con el PNT: los 7 criterios + 2
  condiciones son literales del §4.1 y **no hay fórmula** — el resultado es criterio profesional; la
  aplicación solo **propone** (research.md Decisión 1, Art. V.1) y exige observaciones si el
  farmacéutico contradice la propuesta o decide NO_APTO (FR-204 ampliado).

- **Un único modelo de consentimiento** (Anexo I.B) cubre paciente y representante; el generador
  `CONSENT` rellena firmante, relación y paciente representado, imprime los once compromisos literales
  y marca `impreso_en`. `IDONEIDAD` no es documento aparte: `FICHA-PAC` imprime la evaluación vigente
  en el bloque del Anexo I.E (o lo deja en blanco si no la hay).

- **El invariante del Art. I.3 es real**: `ComprobadorIdoneidadYConsentimientoReal` sustituye al nulo
  en `App.axaml.cs`; `ServicioPreparacion` no cambia (el punto de extensión de Spec 006 funcionó como
  estaba diseñado). CA-201 se prueba sobre el comprobador real con datos reales.

- **Activación automática** EVALUACION→ACTIVO (FR-213) vía `IServicioPacientes.CambiarEstado`, para que
  quede auditada como cualquier cambio de estado. Suspensión nunca automática: el servicio devuelve
  `SugerirSuspension` (revocación sin otro vigente; NO_APTO sobre un ACTIVO) y la pantalla ofrece el
  botón.

- **Alta inline del representante** (FR-211): Spec 001 no construyó pantalla de contactos; aquí solo
  el alta de REPRESENTANTE_LEGAL / PERSONA_AUTORIZADA con DNI obligatorio, desde la propia pantalla.

- Pantalla `IdoneidadConsentimientoWindow` desde la ficha del paciente (botón "Idoneidad y
  consentimiento"): evaluación con propuesta en vivo, historial, consentimientos con imprimir / registrar
  firma / revocar, alta de representante, sugerencia de suspensión. Test headless con datos reales.

  `dotnet build` sin errores; **68 (Dominio) + 18 (Presentación) + 211 (Aplicación) = 297 tests en verde**.

## Pendiente (documentado, no fabricado)

- La ficha del paciente (`FichaPacienteView`) muestra el estado cargado al abrirla; tras activar al
  paciente desde la ventana de idoneidad hay que reabrir la ficha para verlo — refresco automático no
  construido.
- Fuente canónica de anexos (Pontevedra vs A Coruña): si el PNT de Pontevedra formula el "Anexo 9" con
  otros enunciados, cambian solo los textos de `EvaluacionIdoneidad.TextosCriterios`.
- Prueba manual real (`dotnet run`) por el usuario: el circuito alta → idoneidad → consentimiento →
  ACTIVO → preparación nunca se ha ejecutado en la app real, solo en tests.
