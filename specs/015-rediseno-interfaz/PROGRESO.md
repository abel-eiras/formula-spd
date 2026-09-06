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
| 2 — Inicio accionable, tablas y búsqueda | **Hecha** (2026-09-06) | 11 | — |
| 3 — Espacio del paciente | **Hecha** (2026-09-06) | 11 → 4 | — |
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

## Fase 2 — Inicio accionable, tablas y búsqueda global (2026-09-06)

Hecha entera y fusionada. **336 tests en verde** (68 Dominio + 227 Aplicación + 41 Presentación).

- **H2.1 Indicadores**: `IServicioAvisosInicio.ObtenerIndicadores` devuelve faltantes, verificados
  sin entregar, sesiones a medias y días sin lectura ambiental. Se derivan de los **mismos** avisos
  que se listan debajo, no de un cálculo aparte: así el número del indicador y las líneas que el
  usuario lee no pueden discrepar. El cuarto es un plazo, no un recuento, y `null` significa "al día".
- **H2.2 Avisos accionables**: `AvisoInicio` gana `Area` (`Retirada` / `Preparaciones` / `Calidad`)
  y el nombre del paciente. `Area` es un concepto de aplicación —dónde se arregla el aviso—, no una
  pantalla: la traducción a secciones vive solo en `InicioViewModel.FilaAviso`. Pulsar un aviso abre
  la pantalla **ya centrada en ese paciente**: la retirada por `PacienteId` (el filtro ya existía en
  `FiltrosListadoRetirada`) y las preparaciones por nombre. El filtro se anuncia en pantalla y se
  puede quitar, para que nadie crea que faltan filas.
- **H2.3 Tablas**: `Avalonia.Controls.DataGrid` en Preparaciones, Retirada, Pacientes y Catálogo,
  con `Estilos/Tabla.axaml` (cifras tabulares, bandas alternas, rejilla solo horizontal, filas de
  30 px). La selección múltiple del lote sigue viviendo en cada fila, no en el índice de la rejilla.
- **H2.4 Búsqueda global**: `IServicioBusquedaGlobal` sobre pacientes, medicamentos y blísteres,
  reutilizando las búsquedas que ya existían (Art. V) en vez de escribir otra consulta; así encuentra
  exactamente lo mismo que la pantalla correspondiente. Campo en la cabecera del marco, resultados
  desplegados sobre el contenido y navegación al elegir uno.
- **H2.5 Acciones rápidas**: cuatro atajos en el inicio a las secciones de uso diario.

**Dos cosas anotadas por el camino**: la versión del `DataGrid` (research.md Decisión 4, resuelta —
no existe 11.3.20 de ese paquete; se fija 11.3.13, misma rama y sin degradar el núcleo), y un aviso
`CS8604` que la fase 1 había introducido en `App.axaml.cs`, ya corregido.

**Pendiente de la prueba manual**: la densidad de la tabla (30 px de fila, cuerpo de 13 px) y si el
ancho de la búsqueda global en la cabecera estorba en pantallas estrechas.

## Fase 3 — Espacio del paciente (2026-09-06)

Hecha entera y fusionada. **11 → 4 ventanas**: solo quedan las tres previas a la sesión (asistente,
login, contraseña maestra) y el diálogo de aviso. **345 tests en verde** (68 Dominio + 227 Aplicación
+ 50 Presentación).

- **H3.1/H3.2 Cabecera y contexto**: `PacienteContexto` carga el paciente una vez y avisa a quien lo
  muestre. Con eso desaparece un defecto real del diseño anterior: la ficha seguía diciendo
  EVALUACION después de que la ventana de idoneidad hubiera activado al paciente, y solo se corregía
  al cerrarla (`RecargarPaciente` + `ventana.Closed +=`, ya borrados). La cabecera fija lleva nombre,
  ficha, estado, idoneidad, día de retirada, blísteres, sin entregar y las alergias en rojo.
- **H3.3 Siete pestañas**: Datos, Idoneidad, Tratamiento, Depósito, Preparación, Comunicaciones y
  Documentos. Un alta nueva empieza solo con Datos —sin `paciente_id` no hay nada que registrar— y
  gana las otras seis al guardar por primera vez, sin cerrar ni reabrir nada.
- **H3.4 Indicadores por pestaña**: idoneidad no en regla, faltantes, blísteres sin entregar y
  comunicaciones sin respuesta.
- **H3.5 Paneles laterales**: `PanelLateral` (plantilla propia) y la importación de tratamiento del
  depósito ya no es una ventana. Los saltos entre pestañas con argumento (la comunicación al médico
  prerrellenada desde tratamiento o desde preparación) sustituyen a las otras aperturas de ventana.
- **H3.6 Simplificación**: `FichaPacienteViewModel` pasa de nueve servicios a uno más el contexto;
  `BuscadorPacientesViewModel`, de nueve a dos. Los dos existían así solo para poder construir
  ventanas.
- **H3.7 Ayuda**: `uso__010-inicio` reescrito al marco único, la búsqueda global y los avisos
  accionables; `uso__030-ficha-paciente` a cabecera fija y pestañas; añadidos los apartados de tabla
  en pacientes y de panel lateral en depósito.

**Un defecto propio, encontrado por los tests y no por la prueba manual**: guardar un alta nueva
disparaba una recursión infinita (guardar → avisar al contexto → reconstruir pestañas → construir el
ViewModel de idoneidad → que avisaba al contexto al cargarse → …). Corregido en su raíz —avisar solo
tras las operaciones que cambian al paciente, no al construirse— y además con un cerrojo de
reentrada en `ConstruirPestanas`.

**Decisión anotada**: `PacienteWorkspaceView` y su cabecera quedan **fuera** de la tabla de ayuda
contextual a propósito. Si estuvieran, F1 daría siempre la misma ayuda genérica; al no estarlo, la
primera vista documentada que se encuentra es la de la pestaña abierta.

**Pendiente de la prueba manual**: si siete pestañas caben cómodas en el monitor real y si el panel
lateral de 380 px deja ver bastante depósito por detrás.
