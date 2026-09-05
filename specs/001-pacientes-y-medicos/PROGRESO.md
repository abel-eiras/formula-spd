# Plan y progreso — Spec 001: Pacientes, contactos y catálogo de médicos

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

## Fases del flujo Spec Kit

| # | Fase | Comando | Estado | Commit |
|---|---|---|---|---|
| 1 | Especificación | `/speckit-specify` | ✅ Hecho — 2026-09-05 | `cba86b9` |
| 2 | Aclaración | `/speckit-clarify` | ✅ Hecho — 2026-09-05 (Q1 y Q2 resueltas) | *(pendiente de commit)* |
| 3 | Plan de implementación | `/speckit-plan` | ✅ Hecho — 2026-09-05 | `0b5aa16` |
| 4 | Desglose de tareas | `/speckit-tasks` | ✅ Hecho — 2026-09-05 | *(pendiente de commit)* |
| 5 | Análisis de coherencia | `/speckit-analyze` | ✅ Hecho — 2026-09-05 (5 hallazgos remediados) | *(pendiente de commit)* |
| 6 | Implementación | `/speckit-implement` | ⏳ Pendiente | — |

## Preguntas abiertas resueltas en la fase 2 (Constitución Art. X.3)

| # | Pregunta | Respuesta |
|---|---|---|
| Q1 | ¿Segundo médico "especialista de referencia" en la ficha, o solo prescriptor por tratamiento? | Solo médico de cabecera; cada tratamiento fija su propio prescriptor |
| Q2 | ¿Estado SUSPENDIDO necesario en 1.0, o se cubre con observaciones? | Se mantiene como estado formal |

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
  bloqueado a propósito hasta la fase 2. Commit `cba86b9`.
- **2026-09-05** — Fase 2 completada. Dos preguntas planteadas una a una; el propietario del
  producto confirmó en ambas la opción recomendada (la propuesta por defecto ya apuntada en la spec
  original): Q1 → solo médico de cabecera; Q2 → SUSPENDIDO se mantiene como estado formal. `spec.md`
  actualizado con sección "Clarifications" y FR-006/nota de médico de cabecera resueltos sin
  marcadores pendientes. Checklist de calidad al 100 %.
- **2026-09-05** — Fase 3 completada. `plan.md` (Technical Context + Constitution Check con 9
  artículos), `research.md` (5 decisiones: normalización de búsqueda sin extensión SQLite,
  `motivo_baja` como TEXT+CHECK, `correlativo_num_ficha` independiente del prefijo, validación de
  DNI/NIE y de CIP gallego como reglas de Dominio puras). `data-model.md` (recorte de
  Paciente/Contacto/Medico), `contracts/servicios-aplicacion.md` (3 interfaces), `quickstart.md`.
  Efecto colateral: `docs/data-model.md` sube a v0.6 (añade `correlativo_num_ficha`,
  `motivo_baja_detalle`, `busqueda_normalizada` a Paciente; `busqueda_normalizada` a Medico).
- **2026-09-05** — Fase 4 completada. `tasks.md` generado: sin fase de Setup (no hay proyectos
  nuevos), Foundational (T001-T018: migración 0002, entidades, validadores, repositorios, tests de
  humo) y 3 user stories — US1 Ficha de paciente P1/MVP (T019-T034), US2 Catálogo de médicos P2
  (T035-T044), US3 Contactos P3 (T045-T051) — más Polish (T052-T054). A diferencia de la Spec 000,
  el test explícito de auditoría (Art. VII.6) se incluyó desde el diseño de cada user story
  (T029, T039, T049), no como remediación tardía de `/speckit-analyze`. 54 tareas en total.
- **2026-09-05** — Fase 5 completada. `/speckit-analyze` cruzó spec/plan/tasks/constitución y
  encontró 5 hallazgos, todos MEDIA/ALTA, ninguno crítico ni de violación de constitución.
  Remediados directamente en `tasks.md` (renumerado 54→59, seguro porque aún no hay código):
  - **F1** (MEDIA): CA-012 (Elaborador sin restricción de categoría) sin test automatizado →
    nuevo T031.
  - **F2** (MEDIA): FR-002b (sexo obligatorio si hay CIP) sin ninguna cobertura, ni test ni
    quickstart → nuevo T030.
  - **F3** (MEDIA-ALTA): CA-005 (nuevo médico en contexto, queda seleccionado sin cerrar la
    ficha) es una aserción de UI sin test Avalonia.Headless → nuevo T048.
  - **F4** (BAJA): FR-004 cubre duplicado por DNI *o* CIP pero solo se probaba DNI → T025
    (antes T025) ampliado a ambos casos.
  - **F5** (ALTA): `plan.md` fija Avalonia.Headless.XUnit como estrategia de test de
    Presentación citando expresamente el incidente de `UsuariosWindow` de Spec 000, pero
    `tasks.md` no programaba ningún test de ese tipo para las vistas nuevas con `ItemTemplate`
    (`BuscadorPacientesView`, `CatalogoMedicosView`) — el mismo patrón de bug que ya causó un
    cierre inesperado una vez. Nuevos T037 y T049.
  Total tras remediación: 59 tareas. Informe completo del análisis conservado en el historial de
  la conversación, no duplicado aquí.
