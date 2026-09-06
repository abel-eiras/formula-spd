# Research: Rediseño de la interfaz

## Decisión 1 — Shell con región de contenido y el `ViewLocator` que ya existe

El proyecto arrastra desde la plantilla de Avalonia un `ViewLocator` (`src/Spd.Presentacion/ViewLocator.cs`)
que, dado un ViewModel, construye por convención de nombre su `View` y le asigna el DataContext. Nunca
se usó porque todo se abría con ventanas construidas a mano. Es exactamente el mecanismo que necesita
un marco único: la región central es un `ContentControl` cuyo `Content` es el ViewModel de la sección,
y el `ViewLocator` resuelve la vista.

Esto explica por qué el rediseño es barato: **las 29 vistas ya son `UserControl` independientes de su
`Window`** (cada `XWindow.axaml` no contiene más que `<XView />`), y toda la lógica está en los
ViewModels con tests. Migrar es sustituir el marco, no reescribir pantallas.

**Alternativa descartada**: un framework de navegación (ReactiveUI routing, Prism). Añade dependencia y
conceptos para resolver algo que aquí son treinta líneas.

## Decisión 2 — Construcción de ViewModels: fábrica en Presentación, no contenedor de inyección

`App.axaml.cs` construye a mano todos los servicios y los va pasando por los constructores de ventanas;
esa cascada es la que hace que añadir un servicio toque seis ficheros (ha pasado en cada spec). Con el
shell, la cascada desaparece: se introduce una **fábrica de ViewModels** (`FabricaViewModels`) en
Presentación que recibe una vez todos los servicios y expone `CrearInicio()`, `CrearPacientes()`,
`CrearPaciente(int id)`… El navegador pide el ViewModel a la fábrica.

No se introduce un contenedor de inyección de dependencias: sería un elemento de stack nuevo
(Art. VIII.2) para un problema que una fábrica explícita resuelve, y el proyecto ya tiene por norma la
construcción explícita y legible (Art. XI).

## Decisión 3 — Tipografías embebidas

El diseño usa IBM Plex Sans (interfaz) e IBM Plex Mono (códigos, series, lotes, pautas). Se embeben como
recurso de la aplicación (`Assets/Fuentes/*.ttf` + `FontFamily` con `avares://`), no se referencian por
nombre: un equipo de la farmacia no tiene por qué tenerlas instaladas, y el Art. VI (portabilidad: toda
la instalación es una carpeta) obliga a que la aplicación se lleve lo que necesita. Licencia: IBM Plex
es SIL Open Font License 1.1, redistribuible con la aplicación.

**Alternativa descartada**: usar Inter, que ya viene con `Avalonia.Fonts.Inter`. Es la tipografía por
defecto de medio ecosistema y no aporta la diferenciación entre texto y datos que sí da el par
Sans/Mono de una misma familia.

## Decisión 4 — Tablas: `Avalonia.Controls.DataGrid`, con alternativa sin dependencia

Los listados necesitan columnas alineadas, cabecera, ordenación y selección múltiple. `DataGrid` no
viene en el paquete base de Avalonia: es el paquete `Avalonia.Controls.DataGrid`, **del mismo fabricante
y en la misma versión (11.3.20)** que el stack ya fijado por el Art. VIII.2, más su hoja de estilos
(`avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml`, que se sobrescribe con el tema propio).

Se interpreta como parte del stack ya fijado ("Avalonia UI 11"), no como stack nuevo — pero **se somete
a la decisión del propietario** (spec.md §9, Q4) en vez de darlo por hecho.

**Alternativa si la respuesta es no**: tabla construida con `ItemsControl` y un `Grid` de anchos
compartidos (`SharedSizeGroup`), con ordenación resuelta en el ViewModel. Es más código y no da
virtualización de columnas, pero no añade ningún paquete y cubre lo que estos listados necesitan
(decenas de filas, no miles).

### Resuelto en la fase 2 (2026-09-06): se adopta `DataGrid`, en la versión 11.3.13

Al añadirlo apareció un detalle que la decisión daba por supuesto y no era cierto: **no existe
`Avalonia.Controls.DataGrid` 11.3.20**. El paquete no se publica en cada versión del núcleo; su rama
11.3 termina en la 11.3.13, y la siguiente publicada es ya la 12.0.0, que exige Avalonia 12 y por
tanto degradaría el núcleo fijado (el `restore` falla con NU1605, no en silencio).

Se fija **11.3.13**, que declara `Avalonia >= 11.3.13` y queda satisfecha por el 11.3.20 ya fijado:
misma rama del mismo fabricante, sin tocar la versión del núcleo. Esto **no** relaja el Art. VIII.2;
lo que se anota es que la regla "misma versión que Avalonia" no se puede cumplir literalmente para
este paquete, y que la comprobación correcta es "misma rama y sin degradar el núcleo".

## Decisión 5 — `PacienteContexto`: una carga, muchos suscriptores

Hoy cada ventana de paciente relee el paciente por su cuenta, y por eso la ficha se queda obsoleta
cuando la idoneidad lo activa (se parcheó al cerrar la ventana; con pestañas el parche ya no vale). El
espacio del paciente introduce un `PacienteContexto` observable en Presentación: contiene el `Paciente`
actual y notifica cuando cambia. La cabecera y las pestañas se suscriben; cualquier ViewModel que
modifique al paciente llama a `Recargar()`.

Es estado de interfaz, no de negocio: vive en Presentación y no altera ningún servicio.

## Decisión 6 — Ayuda contextual por vista, no por ventana

`AyudaContextual` mapea hoy *nombre de ventana* → apartado. Con una sola ventana eso deja de tener
sentido: pasa a mapear *nombre de vista* (`PreparacionView`, `DepositoView`…) y el shell resuelve el
apartado de la vista activa. El test que cruza ambas tablas (Spec 014, FR-1412) se mantiene con las
claves nuevas, y sigue garantizando que ninguna pantalla queda sin documentar.

## Decisión 7 — Los tests de vista pasan por un anfitrión común

Los tests de Presentación construyen hoy `new XWindow(servicios…)` y llaman a `Show()` para forzar la
realización de plantillas (regresión F5 de Spec 001: `ListBox` + `ItemTemplate` + comando de ancestro
provocó un SIGABRT real). Al desaparecer las ventanas, se introduce
`AnfitrionDeVista.Mostrar(Control vista)`: crea una ventana anfitriona, le pone la vista como contenido
y la muestra. **La cobertura de regresión se conserva íntegra** — sigue habiendo una ventana real que
realiza las plantillas — y cada test se simplifica a construir la vista con su ViewModel.

**Alternativa descartada**: conservar las ventanas como envoltorios muertos solo para los tests. Deja 27
clases sin uso en producción y una segunda forma de construir cada pantalla, justo lo que el Art. XI
(legibilidad, una sola forma de hacer las cosas) desaconseja.

## Decisión 8 — Migración por fases, cada una fusionable

Cada fase deja la aplicación completa y probada, y se fusiona a `main` por separado (FR-1541). Durante
las fases 1 y 2 conviven el marco único y las ventanas de paciente todavía sin migrar (las abre el
shell); la fase 3 las absorbe. Es deliberado: evita una rama larga y permite que el propietario pruebe
a mano el resultado de cada fase antes de seguir.

Riesgo aceptado y vigilado: durante ese intervalo hay dos patrones de navegación conviviendo. Se acota
documentándolo aquí y cerrándolo en la fase 3, no dejándolo indefinido.

## Decisión 10 — Se estilan propiedades, no se reescriben plantillas de controles estándar

Añadida durante la fase 1. Hay dos formas de aplicar un tema propio en Avalonia: ajustar propiedades
(color, tipo, radio, espaciado) con `Style`, o sustituir el `ControlTheme` completo de cada control.
La segunda da control total sobre los estados (hover, pulsado, foco), pero un error en una plantilla
no produce un fallo: produce un control **invisible**, y esta sesión no tiene pantalla con la que
comprobarlo. Se elige la primera, más otras dos palancas:

- **Sobrescribir `SystemAccentColor` y sus seis derivados** en la paleta: es de donde Fluent saca
  selecciones, focos y marcas de verificación, así que los controles estándar adoptan el acento de la
  farmacia sin tocar sus plantillas.
- **Plantilla propia solo para los controles propios** (`Pastilla`, `FranjaSeveridad`), donde no hay
  nada de Fluent que romper.

Consecuencia asumida y anotada para la prueba manual: los estados de hover y pulsado de los botones
siguen derivando de los colores de Fluent hasta que se validen en pantalla. Es un matiz, no un
defecto; y es reversible sin tocar nada más que `Estilos/Controles.axaml`.

## Decisión 11 — Los tests headless renderizan con Skia

El dibujo simulado de `Avalonia.Headless` (`UseHeadlessDrawing = true`, el valor por defecto) no carga
fuentes embebidas: en cuanto la aplicación pasó a usar IBM Plex, **todas** las vistas fallaban con
"Could not create glyphTypeface", por una limitación del entorno de prueba y no por un defecto del
código. `TestAppBuilder` pasa a `UseSkia()` con `UseHeadlessDrawing = false`, con lo que los tests
ejercitan el mismo camino de texto que el usuario.

Dos detalles aprendidos por el camino, que conviene no volver a descubrir:

- Avalonia **no sintetiza pesos** en una familia embebida: si el XAML usa `FontWeight="Bold"` y el
  fichero Bold no está, falla. Hay que embeber cada peso y estilo que se use (aquí: Regular, Medium,
  SemiBold, Bold e Italic de Sans; Regular, Medium, SemiBold y Bold de Mono).
- La carpeta que se referencia como familia (`avares://…/Assets/Fuentes#IBM Plex Sans`) debe contener
  **solo** ficheros de fuente; la licencia se guarda en `Assets/IBM-Plex-LICENSE.txt`, fuera.

## Decisión 9 — Sin animaciones más allá de las transiciones del propio tema

El propietario ha pedido fluidez, que en una herramienta de trabajo significa que no haya esperas ni
pasos innecesarios, no que haya movimiento. Se usan las transiciones que ya trae el tema (hover, foco) y
se evita animar la navegación: en una pantalla que se usa cien veces al día, una animación de 200 ms es
un impuesto, no un adorno. `prefers-reduced-motion` no aplica en escritorio Avalonia, pero el criterio
es el mismo.
