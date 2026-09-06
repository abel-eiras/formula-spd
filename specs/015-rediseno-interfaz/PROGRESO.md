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
| 1 — Marco único y sistema visual | **Hecha** (2026-09-06) | 27 → **11** | sí |
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

## Fase 1 — Marco único y sistema visual (2026-09-06)

Hecha entera y fusionada. **27 → 11 ventanas**; las once que quedan son el marco, las tres previas a
la sesión (asistente, login, contraseña maestra), el diálogo de aviso y las seis de paciente que
absorbe la fase 3. **329 tests en verde** (68 Dominio + 36 Presentación + 225 Aplicación).

- **H1.1 Tema**: IBM Plex Sans y Mono embebidas (SIL OFL, `Assets/Fuentes`), paleta en variante clara
  y oscura, escala tipográfica y estilos de botón, campo, lista y pestaña. `App.axaml` los aplica
  sobre `FluentTheme`.
- **H1.2 Componentes**: `Pastilla` (cinco variantes) y `FranjaSeveridad`, con plantilla propia.
- **H1.3 Marco**: `AppShell` con navegación lateral (grupos, contadores, atrás, ayuda, cerrar sesión)
  y región de contenido resuelta por el `ViewLocator`.
- **H1.4 Fábrica**: `ServiciosAplicacion` reúne los 26 servicios una vez y `FabricaViewModels`
  construye cada sección a demanda. Desaparece la cascada de constructores entre ventanas: `App` pasó
  de enhebrar 26 servicios por cinco niveles de ventanas a construir uno solo.
- **H1.5 Migración**: catorce secciones dentro del marco; sus ventanas, `MainWindow` y `MainViewModel`
  eliminados. `RegistrosCalidad` y `ControlDocumental` necesitaban vista propia (su contenido vivía en
  la ventana): se extrajeron a `RegistrosCalidadView` y `ControlDocumentalView`.
- **H1.6 Ayuda y tests**: `AyudaContextual` indexa por vista; `AnfitrionDeVista` aloja cada vista en
  una ventana para conservar la regresión F5; catorce ficheros de test migrados; `MainWindowTests`
  sustituido por `AppShellTests` (navegación a todas las secciones, visibilidad por rol, contadores,
  atrás, y que toda vista alcanzable tenga apartado de ayuda).

**Tres hallazgos anotados en research.md** (Decisiones 10 y 11) para no volver a descubrirlos: se
estilan propiedades y no plantillas de Fluent (sin pantalla no se puede validar una plantilla propia);
Avalonia no sintetiza pesos de fuente embebida, hay que embeber cada uno; y el dibujo simulado de
headless no carga fuentes, así que los tests pasan a renderizar con Skia.

**Pendiente de la prueba manual**: los estados de hover y pulsado de los botones siguen derivando de
Fluent; la densidad (13 px) está por confirmar en el monitor real.
