# Plan y progreso — Spec 015: Rediseño de la interfaz

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: el propietario pidió el 2026-09-06, tras cerrar la opción A del plan de trabajo
(preparaciones global, avisos, lote, entrega y purga), un rediseño con tres directrices literales: "que
se vea lo más fluida y cómoda posible", "más información en pantalla" y "menos ventanas emergentes".
Se le presentó una propuesta visual (`docs/rediseno-interfaz.html`) con maquetas de inicio, espacio del
paciente, preparación y listado; la aprobó ("me encanta el diseño propuesto") y pidió el plan detallado
por fases con hitos y pruebas de aceptación, que es lo que documenta esta spec.

- **2026-09-06 — Planificación completa, sin implementación.** Ciclo `/speckit-specify` → `plan` →
  `tasks` sobre la propuesta aprobada. Cuatro fases independientes y fusionables por separado, cada una
  con sus hitos, sus tests automáticos y su guion de prueba manual (plan.md).

- **Punto de partida verificado**: 27 ventanas, 29 vistas ya separadas como `UserControl`, 27
  ViewModels, 322 tests en verde. Hallazgo que abarata el trabajo: el proyecto arrastra sin usar el
  `ViewLocator` de la plantilla de Avalonia, que resuelve ViewModel → Vista por convención — es
  exactamente el mecanismo que necesita la región de contenido del marco único (research.md Decisión 1).

- **Ninguna regla de negocio cambia** (FR-1540). Dominio y Aplicación solo se amplían con dos consultas
  de lectura en la fase 2: el destino de un aviso y la búsqueda global.

## Estado por fases

| Fase | Estado | Ventanas al terminar | Fusión |
|---|---|---|---|
| 1 — Marco único y sistema visual | Pendiente | 27 → 11 | — |
| 2 — Inicio accionable, tablas y búsqueda | Pendiente | 11 | — |
| 3 — Espacio del paciente | Pendiente | 11 → 4 | — |
| 4 — Preparación como carril y blíster | Pendiente | 4 | — |

## Pendiente (documentado, no fabricado)

- **Decisiones del propietario** (spec.md §9): Q1 orden de las fases (propuesta 1→2→3→4), Q2 modelo real
  de dispositivo para la rejilla de alvéolos (bloquea H4.2), Q3 densidad de la interfaz (bloquea H1.1),
  Q4 si se acepta `Avalonia.Controls.DataGrid` del mismo fabricante y versión (bloquea H2.3).
- **Caso "dos pacientes a la vez"**: la propuesta lo elimina al pasar a una sola ventana. No aparece en
  el procedimiento y el propietario pidió menos ventanas; si la prueba manual demuestra que hace falta,
  se resolverá con "abrir en ventana aparte" como excepción explícita.
- **Prueba manual del ciclo completo** (T061): sigue pendiente desde el principio del proyecto y es la
  condición de terminado de la fase 4.
