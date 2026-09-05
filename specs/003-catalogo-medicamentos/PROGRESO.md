# Plan y progreso — Spec 003: Catálogo de medicamentos

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: esta spec se implementa en una rama independiente
(`003-catalogo-medicamentos`, creada desde `main`) mientras la Spec 001
(`001-pacientes-y-medicos`) espera la verificación manual del usuario. Spec 003 no depende
funcionalmente de Spec 001 (ver spec.md, "Depende de"), así que ambas ramas avanzan en paralelo
sin conflicto — Medicamento y Paciente son catálogos/entidades independientes.

## Fases del flujo Spec Kit

| # | Fase | Comando | Estado | Commit |
|---|---|---|---|---|
| 1 | Especificación | `/speckit-specify` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 2 | Aclaración | `/speckit-clarify` | ✅ Hecho — 2026-09-05 (Q1 resuelta) | *(pendiente de commit)* |
| 3 | Plan de implementación | `/speckit-plan` | ⏳ Pendiente | — |
| 4 | Desglose de tareas | `/speckit-tasks` | ⏳ Pendiente | — |
| 5 | Análisis de coherencia | `/speckit-analyze` | ⏳ Pendiente | — |
| 6 | Implementación | `/speckit-implement` | ⏳ Pendiente | — |

## Criterios de aceptación de la spec y su tipo de test previsto

Formato Dado/Cuando/Entonces (Constitución Art. IX.2). El tipo de test se confirma o ajusta en la
fase de `/speckit-plan`.

| CA | Descripción | Capa / tipo de test previsto |
|---|---|---|
| CA-300 | Alta mínima (solo CN + nombre), marcado "descripción física pendiente" | Aplicación — test unitario |
| CA-301 | Editar descripción física no reescribe instantáneas de SPD ya entregados | Aplicación — test de integración (Art. IV.3) |
| CA-302 | Marcar no apto SPD exige motivo | Dominio/Aplicación — test unitario |
| CA-303 | CN duplicado bloqueado | Aplicación — test unitario |
| CA-304 | Importación no sobrescribe descripción física sin confirmación | Aplicación — test de integración |
| CA-305 | Reactivación de CN dado de baja en vez de duplicar | Aplicación — test unitario |

## Invariantes de constitución que aplican a esta spec

- Art. IV (catálogo vivo, historia congelada): FR-303/FR-304 son el ejemplo canónico del Art. IV —
  editar la descripción física de un medicamento afecta a todo uso futuro, pero `Medicamento_Hist`
  conserva cada versión y las líneas de SPD ya creadas guardan su propia instantánea (a diferencia
  de Spec 001, donde Medico no tiene instantánea porque la referencia del paciente siempre es
  viva — aquí sí existe instantánea porque el consumidor, SPD_Linea, es un documento legal
  reproducible, Art. II.3).
- Art. V (un dato, una entrada): FR-303 (texto autogenerado a partir de los campos de descripción,
  editable pero no re-tecleado desde cero).
- Art. VI (aislamiento de red): FR-320 reutiliza la única conexión de red ya prevista para el
  nomenclátor (Spec 000); esta spec no abre ninguna conexión nueva.
- Art. IX (calidad): CA-300..CA-305 se traducen en tests antes de darse por completado.
- Art. X (simplicidad): FR-306 reactiva un CN existente en vez de crear un catálogo paralelo de
  "medicamentos retirados".

## Registro de avance

- **2026-09-05** — Fase 1 completada. `spec.md` trasladado literalmente desde
  `spec-003-catalogo-medicamentos.md`, con Q1 (lista de formas farmacéuticas) marcada
  `[NEEDS CLARIFICATION]`. Checklist de calidad generado; todo pasa salvo "sin NEEDS
  CLARIFICATION", bloqueado a propósito hasta la fase 2.
- **2026-09-05** — Fase 2 completada. Q1 planteada; el propietario del producto eligió la opción
  recomendada: mantener la lista de formas farmacéuticas de FR-300 tal cual, sin añadir valores.
  `spec.md` actualizado con sección "Clarifications" y §9 sin marcadores pendientes. Checklist de
  calidad al 100 %.
