# Plan y progreso — Spec 009: Registros de calidad

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: rama independiente (`009-registros-calidad`, creada desde `main`) mientras
las Specs 001 y 003 esperan verificación manual del usuario. Spec 009 no depende funcionalmente de
ninguna de las dos (solo de Spec 000, ya mergeada) — el usuario pidió continuar con desarrollo que
no requiera su intervención.

**Nota sobre `/speckit-clarify`**: la única pregunta de esta spec (Q1) tiene una propuesta por
defecto documentada en la spec original. Siguiendo la instrucción explícita del usuario de avanzar
sin necesitar su intervención, se resuelve adoptando esa propuesta directamente en vez de esperar
respuesta, y se deja registrado aquí como una decisión autónoma, no como una respuesta del usuario.

## Fases del flujo Spec Kit

| # | Fase | Comando | Estado | Commit |
|---|---|---|---|---|
| 1 | Especificación | `/speckit-specify` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 2 | Aclaración | `/speckit-clarify` | ✅ Hecho — 2026-09-05 (Q1 resuelta de forma autónoma) | *(pendiente de commit)* |
| 3 | Plan de implementación | `/speckit-plan` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 4 | Desglose de tareas | `/speckit-tasks` | ⏳ Pendiente | — |
| 5 | Análisis de coherencia | `/speckit-analyze` | ⏳ Pendiente | — |
| 6 | Implementación | `/speckit-implement` | ⏳ Pendiente | — |

## Criterios de aceptación de la spec y su tipo de test previsto

Formato Dado/Cuando/Entonces (Constitución Art. IX.2). El tipo de test se confirma o ajusta en la
fase de `/speckit-plan`.

| CA | Descripción | Capa / tipo de test previsto |
|---|---|---|
| CA-900 | Ambiental fuera de rango se marca al registrar, no se recalcula si cambia la configuración | Aplicación — test unitario (Art. IV, instantánea) |
| CA-901 | Limpieza de un clic con fecha/hora y usuario actual | Aplicación — test unitario |
| CA-902 | Formación acumulativa, ninguna sustituye a otra | Aplicación — test unitario (Art. III) |
| CA-903 | Control de cambios del PNT solo Administrador | Aplicación — test unitario (permisos, Art. VII.4) |
| CA-904 | Aviso de registro atrasado en el panel de inicio | Aplicación — test unitario |

## Invariantes de constitución que aplican a esta spec

- Art. I.3 (invariantes del PNT): "toda preparación registra temperatura y humedad" (RegistroAmbiental
  es la entidad que lo soporta, aunque su vínculo directo con una preparación es de Spec 006).
- Art. III (nada se borra): FR-921 (formación acumulativa, nunca se sustituye), duplicados
  ambientales no se eliminan (§7 casos límite).
- Art. IV (catálogo vivo, historia congelada): FR-901 es el ejemplo de instantánea de esta spec —
  `fuera_rango` se congela con los rangos vigentes en el momento del registro, no se recalcula si
  cambia `Farmacia.temp_min/max`.
- Art. VII (seguridad y acceso): FR-942 (Control de cambios/copias solo Administrador); FR-920 sin
  distinción de categoría profesional.
- Art. IX (calidad): CA-900..CA-904 se traducen en tests antes de darse por completado.

## Registro de avance

- **2026-09-05** — Fase 1 completada. `spec.md` trasladado literalmente desde
  `spec-009-registros-calidad.md`, con Q1 (umbral de días del aviso) marcada
  `[NEEDS CLARIFICATION]`. Checklist de calidad generado; todo pasa salvo "sin NEEDS
  CLARIFICATION", bloqueado a propósito hasta la fase 2.
- **2026-09-05** — Fase 2 completada. Q1 resuelta de forma **autónoma** (sin plantearla al
  usuario): se adopta la propuesta por defecto de la spec original, 7 días como umbral único para
  ambos avisos (ambiental y limpieza rutinaria). Esto se aparta del patrón habitual de Specs
  000/001/003 (donde cada pregunta se planteó una a una); el motivo es la instrucción explícita del
  usuario de seguir avanzando en desarrollo que no requiera su intervención. `spec.md` actualizado
  con sección "Clarifications" y §9 sin marcadores pendientes. Checklist de calidad al 100 %.
- **2026-09-05** — Fase 3 completada. `plan.md` (Technical Context + Constitution Check con 9
  artículos), `research.md` (4 decisiones: `FormacionPersonal` sin distinción de categoría
  profesional —corrige una discrepancia real con el documento de Fase 1, que quedó desactualizado
  tras la enmienda 2.0.0 de la constitución—, cálculo congelado de `fuera_rango`, umbral único
  configurable en `Farmacia.umbral_dias_aviso_calidad`, y comprobación de rol en la capa de
  Aplicación para Control documental). `data-model.md` (recorte de las 6 entidades + adición a
  Farmacia), `contracts/servicios-aplicacion.md` (2 interfaces separadas por permisos),
  `quickstart.md`. Misma nota de coordinación de numeración de migración ya dejada en Specs 001 y
  003 (las tres ramas parten de `main` y numeran su propia migración `0002`).
