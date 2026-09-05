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
| 4 | Desglose de tareas | `/speckit-tasks` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 5 | Análisis de coherencia | `/speckit-analyze` | ✅ Hecho — 2026-09-05 (sin hallazgos) | *(pendiente de commit)* |
| 6 | Implementación | `/speckit-implement` | 🔄 En curso — US1+US2+US3 hechas (2026-09-05) | *(pendiente de commit)* |

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
- **2026-09-05** — Fase 4 completada. `tasks.md` generado: Foundational (T001-T014: migración
  0002 + ALTER TABLE Farmacia, 6 entidades, 2 repositorios, tests de humo) y 4 user stories — US1
  Ambiental+Limpieza P1/MVP (T015-T023), US2 Formación+Residuos P2 (T024-T030), US3 Control
  documental P2 (T031-T038), US4 Avisos P3 (T039-T042, depende de US1) — más Polish (T043-T045).
  45 tareas en total. Auditoría y test Avalonia.Headless de regresión incluidos desde el diseño.
- **2026-09-05** — Fase 5 completada. Sin hallazgos: los 5 CA están cubiertos por tests
  dedicados, la auditoría se verificó entidad por entidad (evitando el hueco de Spec 003), y el
  diseño no expone ningún método de actualización/eliminación en los repositorios — todas las
  entidades son de solo alta, lo que satisface el Art. III sin necesitar un test dedicado. 100 %
  de cobertura FR→tarea.
- **2026-09-05** — Fase 6 (Implementación) iniciada. Foundational (T001-T014) completada: migración
  `0002_registros_calidad.sql` (6 tablas + `ALTER TABLE Farmacia`), 6 entidades y enum
  `TipoLimpieza`, 2 repositorios con `Fila` explícita. **Hallazgo relevante**: se descubrió un
  comportamiento real de Dapper 2.1.79 — la materialización automática de un `record` con un
  parámetro `long?` desde una columna snake_case puede fallar aunque
  `MatchNamesWithUnderscores` esté activo, dependiendo de la forma exacta del record (reproducido
  en aislamiento; no es simplemente "toda columna nula falla"). Corregido en
  `RepositorioRegistrosCalidad.ListarAmbiental()` con alias explícitos `SELECT col AS
  PascalCaseName`. Se ha dejado una tarea en segundo plano para revisar si `PacienteFila.MedicoId`
  (Spec 001, mismo patrón `long?`) tiene el mismo riesgo latente, aunque sus tests actuales pasan.
  `dotnet build` sin errores; 7+2+34 = 43 tests en verde.
- **2026-09-05** — User Story 1 (P1, MVP) completada: T015-T023. `ServicioRegistrosCalidad`
  (Ambiental/Limpieza) con `fuera_rango` congelado al registrar (CA-900) y limpieza de un clic con
  fecha/usuario automáticos (CA-901). Se añadió `Farmacia.UmbralDiasAvisoCalidad` para soportar
  FR-950 (US4). En Presentación: `RegistrosCalidadWindow` con pestañas Ambiental/Limpieza, botón
  en `MainWindow` visible para Elaborador y Administrador. Test Avalonia.Headless de regresión
  para ambos listados. `dotnet build` sin errores; 7+3+38 = 48 tests en verde.
- **2026-09-05** — User Story 2 (P2) completada: T024-T030. Formación acumulativa (tres
  formaciones consultables, CA-902) y recogida de residuos no SIGRE. Dos pestañas nuevas
  (Formación, Residuos) en `RegistrosCalidadWindow`. Se detectó que `Spd.Presentacion.csproj` no
  trae `System` implícito en algunos ficheros nuevos (a diferencia de los proyectos de test);
  corregido con `using System;` explícito en los dos ViewModels nuevos. `dotnet build` sin
  errores; 7+3+41 = 51 tests en verde.
- **2026-09-05** — User Story 3 (P2) completada: T031-T038. `ServicioControlDocumental` con
  comprobación de rol en la propia capa de Aplicación tanto para registrar como para listar
  (CA-903) — un Elaborador no puede ni escribir ni consultar. `ControlDocumentalWindow`, ventana
  separada visible solo para Administrador (mismo criterio que los botones de Configuración de
  Spec 000). Test Avalonia.Headless de regresión para ambos listados. `dotnet build` sin errores;
  7+4+46 = 57 tests en verde.
