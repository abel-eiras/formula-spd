# Plan y progreso — Spec 001: Pacientes, contactos y catálogo de médicos

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

## Fases del flujo Spec Kit

| # | Fase | Comando | Estado | Commit |
|---|---|---|---|---|
| 1 | Especificación | `/speckit-specify` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 2 | Aclaración | `/speckit-clarify` | ⏳ Pendiente (bloqueada por Q1/Q2) | — |
| 3 | Plan de implementación | `/speckit-plan` | ⏳ Pendiente | — |
| 4 | Desglose de tareas | `/speckit-tasks` | ⏳ Pendiente | — |
| 5 | Análisis de coherencia | `/speckit-analyze` | ⏳ Pendiente | — |
| 6 | Implementación | `/speckit-implement` | ⏳ Pendiente | — |

## Preguntas abiertas que bloquean la fase 2 (Constitución Art. X.3)

| # | Pregunta | Respuesta |
|---|---|---|
| Q1 | ¿Segundo médico "especialista de referencia" en la ficha, o solo prescriptor por tratamiento? | Pendiente de decidir con el propietario del producto |
| Q2 | ¿Estado SUSPENDIDO necesario en 1.0, o se cubre con observaciones? | Pendiente de decidir con el propietario del producto |

## Criterios de aceptación de la spec y su tipo de test previsto

Formato Dado/Cuando/Entonces (Constitución Art. IX.2). El tipo de test se confirma o ajusta en la
fase de `/speckit-plan`.

| CA | Descripción | Capa / tipo de test previsto |
|---|---|---|
| CA-001 | Número de ficha correlativo con prefijo, no editable | Dominio/Aplicación — test unitario (numeración, Art. V.3) |
| CA-002 | Mínimos para guardar (nombre+apellidos+uno de DNI/CIP/fecha nac.) | Dominio — test unitario |
| CA-003 | Aviso de duplicado por DNI (no bloqueante) | Aplicación — test de integración |
| CA-004 | Autocompletado de médico por fragmento | Aplicación — test de integración (búsqueda) |
| CA-005 | Alta de médico en contexto desde el selector | Aplicación — test de integración |
| CA-006 | Edición de médico propaga a todos los pacientes que lo referencian | Aplicación — test de integración (referencia por id, sin copia — Art. IV.1) |
| CA-007 | Baja de médico bloqueada si es cabecera de paciente activo | Aplicación — test unitario/integración |
| CA-008 | Representante legal/persona autorizada exige DNI | Dominio — test unitario |
| CA-009 | Baja de paciente conserva todo (Art. III) | Aplicación — test de integración |
| CA-010 | Reactivación exige nuevo consentimiento (pasa a EVALUACION) | Dominio — test unitario |
| CA-011 | Búsqueda sin distinguir tildes | Aplicación — test de integración |
| CA-012 | Elaborador edita sin restricción de categoría profesional | Aplicación — test unitario (permisos, Art. VII.4) |
| CA-013 | Auditoría de edición con detalle antes/después | Aplicación — test de integración (Art. VII.6) |
| CA-014 | Autocompletado de CIP gallego | Dominio — test unitario (regla de construcción FR-005) |
| CA-015 | Aviso de CIP no correspondiente (no bloqueante) | Dominio — test unitario |

## Invariantes de constitución que aplican a esta spec

- Art. III (nada se borra): FR-007/FR-023 — baja de paciente y de contacto, ambas lógicas.
- Art. IV (catálogo vivo): médico es catálogo (FR-035, editar afecta a todas las referencias, sin
  copia); el "vigente" del paciente (médico de cabecera) no es una instantánea — a diferencia de
  Medicamento en SPD_Linea, aquí la referencia siempre apunta al dato actual del médico.
- Art. V (un dato, una entrada): FR-002c (valores por defecto prerrellenados desde Farmacia),
  FR-032/033 (selector con autocompletado en vez de reteclear datos del médico).
- Art. VII (seguridad y acceso): FR-040 (Elaborador y Administrador sin distinción de categoría
  profesional), FR-042 (auditoría de toda escritura).
- Art. VIII (arquitectura): validación de DNI/NIE y CIP como reglas de Dominio puras, sin
  dependencias externas.
- Art. IX (calidad): CA-001..CA-015 se traducen en tests antes de darse por completado.

## Registro de avance

- **2026-09-05** — Fase 1 completada. `spec.md` trasladado literalmente desde
  `spec-001-pacientes-y-medicos.md`, con Q1/Q2 marcadas `[NEEDS CLARIFICATION]`. Checklist de
  calidad de la especificación generado; todo pasa salvo el punto de "sin NEEDS CLARIFICATION",
  bloqueado a propósito hasta la fase 2.
