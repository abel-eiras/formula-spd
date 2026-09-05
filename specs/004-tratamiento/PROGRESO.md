# Plan y progreso — Spec 004: Tratamiento del paciente

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: esta rama se bifurca de `main` **después** de fusionar Specs 001, 003, 009 y
010 (2026-09-06) — el esquema ya tiene Paciente/Médico (001), Medicamento (003), Formación/
Residuos/Control documental (009) y sin migración nueva de 010. La migración de esta spec se
numera `0005` sin colisión, siguiendo directamente a la 0004 de Spec 009.

- **2026-09-06** — Ciclo completo `/speckit-specify` → `/speckit-plan` → `/speckit-tasks` →
  `/speckit-implement`, ejecutado de forma autónoma. FR-421/422/430 dependen de Specs 005/006, que
  no existen en esta rama — documentado como diferido en spec.md ("Fuera de alcance"), no como
  `NEEDS CLARIFICATION` sin resolver (Art. X.3): se implementan los estados/campos que esas specs
  futuras necesitarán enganchar, sin inventar su lógica.

- **Foundational**: `Tratamiento` versiona en la propia tabla (sin `Tratamiento_Hist` separada,
  research.md Decisión 1 — a diferencia de Medicamento_Hist de Spec 003, aquí no hace falta porque
  cada fila ya pertenece a un único paciente). `FraccionDosis` (enum, 9 valores cerrados,
  research.md Decisión 2) en vez de un decimal validado.

- **User Story 1 (P1, MVP) — Alta y reutilización**: `ServicioTratamientos.Crear` prerrellena el
  médico de cabecera del paciente si no se indica otro (CA-400); acepta `en_spd=0` con
  `pauta_texto` libre (FR-401); `ListarVigentesDePaciente` filtra por `fecha_fin IS NULL` y
  `estado != Finalizado`. `TratamientoView` (selector de medicamento por CN, formulario de alta)
  accesible desde un botón "Tratamientos" en `FichaPacienteView` (Spec 001), visible solo cuando el
  paciente ya está guardado (`PuedeAbrirTratamientos`).

- **User Story 2 (P2) — Inmutabilidad, estados y ajuste manual**: `CambiarPauta` cierra la fila
  vigente (`Finalizado`, `fecha_fin`) y crea una nueva `Activo`, heredando
  `fecha_prescripcion_inicial` de la original (CA-401). `ActualizarCamposNoClinicos` edita en el
  sitio sin cerrar/abrir fila (FR-411). `CambiarEstado` para transiciones administrativas
  (Suspendido↔Activo) sin cambio clínico. `ListarHistorialDeMedicamento` devuelve todas las
  versiones (CA-402), verificado a nivel de servicio.

  **Gap conocido, pendiente de sesión posterior**: no se construyó una pantalla de línea temporal
  para el historial (T025) — `ListarHistorialDeMedicamento` existe y está testeado, pero solo
  `TratamientoView` muestra los vigentes, no el historial completo con sus fechas de vigencia. No
  bloquea el resto de la spec (CA-402 está verificado a nivel de servicio, solo falta exponerlo en
  la UI).

  El botón "usar cálculo automático" de FR-430 tampoco tiene control dedicado en la vista todavía
  (el campo `AjusteUnidadesManual` se puede fijar/limpiar vía `ActualizarCamposNoClinicos`, ya
  testeado, pero sin un botón explícito en `TratamientoView`) — coherente con que el propio "valor
  calculado" de referencia también está diferido a Spec 005 (research.md Decisión 3).

  `dotnet build` sin errores; **44+10+107 = 161 tests en verde**.

## Pendiente

- Vista de línea temporal del historial (T025).
- Botón "usar cálculo automático" explícito en `TratamientoView` (el campo ya funciona vía
  `ActualizarCamposNoClinicos`).
- FR-421 (disparo automático de PENDIENTE_REVISION desde Spec 006), FR-422 (propuesta SIGRE desde
  Spec 005) y el valor de referencia calculado de FR-430: implementables cuando existan esas specs.
- Prueba manual real (`dotnet run`) por el usuario.
