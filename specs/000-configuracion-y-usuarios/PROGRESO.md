# Plan y progreso — Spec 000: Configuración inicial, farmacia y usuarios

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

## Fases del flujo Spec Kit

| # | Fase | Comando | Estado | Commit |
|---|---|---|---|---|
| 1 | Especificación | `/speckit-specify` | ✅ Hecho — 2026-09-05 | `bff6a93` |
| 2 | Aclaración | `/speckit-clarify` | ⏳ Pendiente (bloqueada por Q1/Q2, ver abajo) | — |
| 3 | Plan de implementación | `/speckit-plan` | ⏳ Pendiente | — |
| 4 | Desglose de tareas | `/speckit-tasks` | ⏳ Pendiente | — |
| 5 | Análisis de coherencia | `/speckit-analyze` | ⏳ Pendiente | — |
| 6 | Implementación | `/speckit-implement` | ⏳ Pendiente | — |

## Preguntas abiertas que bloquean la fase 2 (Constitución Art. X.3)

| # | Pregunta | Respuesta |
|---|---|---|
| Q1 | FR-050: ¿URL de actualizaciones = GitHub Releases del proyecto, o servidor propio? | Pendiente de decidir con el propietario del producto |
| Q2 | FR-045: ¿bloqueo manual por Administrador, o bloqueo temporal automático (p. ej. 15 min)? | Pendiente de decidir con el propietario del producto |

## Criterios de aceptación de la spec y su tipo de test previsto

Formato Dado/Cuando/Entonces (Constitución Art. IX.2). Cada uno se implementa como test antes de
darse por completado (Art. IX.1); el tipo de test se confirma o ajusta en la fase de `/speckit-plan`.

| CA | Descripción | Capa / tipo de test previsto |
|---|---|---|
| CA-000 | Asistente obligatorio en primer arranque | Presentación/Aplicación — test de integración (arranque sin BD → asistente bloqueante) |
| CA-001 | Cambio de prefijo no afecta a numeración pasada | Dominio — test unitario (Art. IV.3, instantánea/numeración ya asignada) |
| CA-002 | Único administrador protegido (no autobaja) | Dominio — test unitario (FR-042) |
| CA-003 | Baja de usuario conserva histórico | Dominio/Infraestructura — test de integración (Art. III.1, consulta de SPD tras baja) |
| CA-004 | Bloqueo por intentos fallidos | Aplicación — test unitario/integración (FR-045; forma exacta depende de Q2) |
| CA-005 | Descarga de nomenclátor no bloquea la app | Infraestructura — test de integración con fallo de red simulado (Art. VI.3) |
| CA-006 | Cambio de valores por defecto no reescribe pacientes existentes | Dominio — test unitario (Art. V.3, FR-020) |

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
