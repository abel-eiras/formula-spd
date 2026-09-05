# Plan y progreso — Spec 000: Configuración inicial, farmacia y usuarios

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

## Fases del flujo Spec Kit

| # | Fase | Comando | Estado | Commit |
|---|---|---|---|---|
| 1 | Especificación | `/speckit-specify` | ✅ Hecho — 2026-09-05 | `bff6a93` |
| 2 | Aclaración | `/speckit-clarify` | ✅ Hecho — 2026-09-05 (Q1 y Q2 resueltas) | `b1422cb` |
| 3 | Plan de implementación | `/speckit-plan` | ✅ Hecho — 2026-09-05 | `f57ee1f` |
| 4 | Desglose de tareas | `/speckit-tasks` | ✅ Hecho — 2026-09-05 (52 tareas, 5 user stories) | `f3ad51c` |
| 5 | Análisis de coherencia | `/speckit-analyze` | ✅ Hecho — 2026-09-05 (1 CRITICAL + 4 mejoras, remediadas) | *(pendiente de commit)* |
| 6 | Implementación | `/speckit-implement` | ⏳ Pendiente | — |

## Preguntas abiertas resueltas en la fase 2 (Constitución Art. X.3)

| # | Pregunta | Respuesta |
|---|---|---|
| Q1 | FR-050: ¿URL de actualizaciones = GitHub Releases del proyecto, o servidor propio? | GitHub Releases del repositorio `abel-eiras/spd` |
| Q2 | FR-045: ¿bloqueo manual por Administrador, o bloqueo temporal automático (p. ej. 15 min)? | Solo desbloqueo manual por un Administrador, sin expiración automática |

## Criterios de aceptación de la spec y su tipo de test previsto

Formato Dado/Cuando/Entonces (Constitución Art. IX.2). Cada uno se implementa como test antes de
darse por completado (Art. IX.1); el tipo de test se confirma o ajusta en la fase de `/speckit-plan`.

| CA | Descripción | Test previsto (tasks.md, renumerado tras `/speckit-analyze`) | Estado |
|---|---|---|---|
| CA-000 | Asistente obligatorio en primer arranque | T021, T022, T023 | ⏳ Pendiente de implementar |
| CA-001 | Cambio de prefijo no afecta a numeración pasada | T038 | ⏳ Pendiente de implementar |
| CA-002 | Único administrador protegido (no autobaja) | T028 | ⏳ Pendiente de implementar |
| CA-003 | Baja de usuario conserva histórico | T029 | ⏳ Pendiente de implementar |
| CA-004 | Bloqueo por intentos fallidos | T030, T031 | ⏳ Pendiente de implementar |
| CA-005 | Descarga de nomenclátor no bloquea la app | T049, T050 | ⏳ Pendiente de implementar |
| CA-006 | Cambio de valores por defecto no reescribe pacientes existentes | T046 | ⏳ Pendiente de implementar |

## Invariantes de constitución que aplican a esta spec

- Art. I.3 (documentación de paciente ≥ 1 año tras baja): no se ejercita en esta spec en sí (no hay
  pacientes todavía), pero `fecha_baja` de usuario sigue el mismo patrón de baja del Art. III.1.
- Art. III (nada se borra): FR-043 — baja de usuario, no eliminación física.
- Art. V (un dato, una entrada): FR-020, FR-013 — valores por defecto y prefijos no se piden dos veces.
- Art. VI (aislamiento y portabilidad): FR-050/FR-051/FR-052 — únicas dos excepciones de red permitidas.
- Art. VII (seguridad y acceso): FR-040..FR-045 — roles, hash Argon2id, bloqueo por intentos.
- Art. VIII (arquitectura): el plan de la fase 3 debe derivar de las 4 capas fijadas, sin proponer stack alternativo.

## Registro de avance

- **2026-09-05** — Fase 1 completada. `spec.md` trasladado literalmente desde
  `spec-000-configuracion-y-usuarios.md`, con Q1/Q2 marcadas `[NEEDS CLARIFICATION]`. Checklist de
  calidad de la especificación (`checklists/requirements.md`) generado; todo pasa salvo el punto de
  "sin NEEDS CLARIFICATION", bloqueado a propósito hasta la fase 2. Commit `bff6a93`.
- **2026-09-05** — Fase 2 completada. Dos preguntas planteadas una a una; el propietario del
  producto confirmó en ambas la opción recomendada (la propuesta por defecto ya apuntada en la spec
  original): Q1 → GitHub Releases del repositorio del proyecto; Q2 → solo desbloqueo manual por un
  Administrador, sin expiración automática. `spec.md` actualizado con sección "Clarifications" y
  FR-045/FR-050 resueltos sin marcadores pendientes. Checklist de calidad al 100 %. Detectada y
  corregida sobre la marcha una desviación propia: al trasladar la spec en la fase 1 se había
  eliminado sin avisar la mención "Anthropic/" de FR-050 (texto original ambiguo) — se restauró
  antes de resolver Q1, en vez de dejarla editada en silencio.
- **2026-09-05** — Fase 3 completada. `plan.md` (Technical Context + Constitution Check con los 9
  artículos aplicables), `research.md` (5 decisiones técnicas, ninguna deja NEEDS CLARIFICATION),
  `data-model.md` (recorte Farmacia/Usuario de esta feature), `contracts/servicios-aplicacion.md`
  (5 interfaces de Aplicación) y `quickstart.md` (guía de validación por CA). Efecto colateral
  documentado: `docs/data-model.md` sube a v0.5 (añade `ruta_documentos_generados`,
  `url_nomenclator`, `umbral_reutilizacion_lectura_ambiental_horas` a Farmacia;
  `intentos_fallidos_consecutivos`, `bloqueado` a Usuario) porque esos campos los exige esta spec y
  no estaban en el documento consolidado.
- **2026-09-05** — Fase 4 completada. `tasks.md` con 52 tareas en 8 fases (Setup, Foundational,
  5 user stories por prioridad P1-P5, Polish). Cada user story mapeada a su escenario de la spec y
  a sus CA-xxx (tabla al inicio de `tasks.md`). Tests incluidos por decisión explícita (Art. IX.1
  exige test de Dominio antes de dar una regla por implementada). US1 (asistente) marcado como MVP.
- **2026-09-05** — Fase 5 completada. `/speckit-analyze` cruzó spec.md/plan.md/tasks.md contra la
  constitución: 1 hallazgo **CRITICAL** (C1 — Art. VII.6, traza de auditoría no cubierta
  explícitamente en la mayoría de escrituras) y 4 mejoras (E1 test de "nunca automático al
  arrancar" Art. VI.3; U1 comportamiento del paso de cifrado sin Spec 010; U2 test de valor por
  defecto FR-012; U3 test de FR-041/FR-044). Cobertura previa: 21/21 FR y 7/7 CA con ≥1 tarea, pero
  solo 18/21 FR con test dedicado. Remediación aplicada directamente en `tasks.md`: 8 tareas nuevas
  (T023, T032, T033, T041, T042, T051, T052, y notas de auditoría añadidas a T024, T035, T043,
  T047, T053, T054) — `tasks.md` pasa de 52 a 59 tareas, renumerado íntegramente porque ninguna
  tenía código escrito todavía. Tabla CA→tarea de este documento actualizada con la nueva
  numeración.
