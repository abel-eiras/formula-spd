# Feature Specification: Rediseño de la interfaz — una sola ventana

**Feature Branch**: `015-rediseno-interfaz`

**Created**: 2026-09-06

**Status**: Plan aprobado por el propietario; implementación pendiente (cuatro fases)

**Constitución aplicable:** 2.1.0 (Artículos II, V, VIII.2, IX.4, XI)

**Depende de:** Specs 000–011 y 014 (todas fusionadas en `main`). No añade reglas de negocio.

**Requerida por:** Ninguna. Es una spec de presentación: cambia cómo se ve y se navega lo que ya existe.

**Input**: Propuesta de rediseño elaborada y presentada al propietario el 2026-09-06
(`docs/rediseno-interfaz.html`), revisada y aprobada por él en la misma sesión ("me encanta el diseño
propuesto"), con la petición expresa de detallar hitos y pruebas de aceptación por fase. Las
directrices del propietario, literales, son: **"que se vea lo más fluida y cómoda posible"**,
**"prefiero que se vea más información en pantalla"** y **"menos ventanas emergentes que tenga que
abrir y cerrar"**.

---

## 1. Propósito

La aplicación funciona y está probada (322 tests), pero se presenta como **27 ventanas** que se abren
unas encima de otras y una pantalla de inicio que es una columna de catorce botones sin un solo dato.
Esta spec convierte esa colección de ventanas en **un único espacio de trabajo**: navegación lateral
persistente, el paciente como una pantalla con pestañas, la preparación como un carril de pasos, y un
sistema visual propio (paleta, tipografías, pastillas de estado) aplicado de forma homogénea.

No cambia ninguna regla de negocio, ningún documento generado y ningún dato almacenado.

## 2. Actores

| Actor | Puede |
|---|---|
| Cualquier usuario | Todo lo que ya podía; con menos clics y sin perder el contexto |
| Administrador | Además, las secciones de administración, ahora en el mismo marco |

## 3. Escenarios de usuario (User Stories)

### US1 (P1) — Saber qué hay que hacer hoy
Como elaboradora, al iniciar sesión quiero ver en la propia pantalla los pacientes con faltantes, los
blísteres verificados sin entregar y las sesiones a medias, y poder ir a resolverlos desde ahí, en vez
de abrir tres pantallas para enterarme.

### US2 (P1) — Trabajar un paciente sin perderlo de vista
Como elaboradora, cuando atiendo a un paciente quiero pasar de sus datos a su tratamiento, su depósito
y su preparación sin abrir ventanas, viendo siempre arriba su estado, sus alergias y si tiene
idoneidad y consentimiento.

### US3 (P1) — Leer los listados como tablas
Como farmacéutica, quiero ver las preparaciones y el listado de retirada como tablas con columnas
alineadas que pueda ordenar, no como párrafos dentro de una lista.

### US4 (P2) — Preparar siguiendo el proceso
Como elaboradora, quiero que la pantalla de preparación me muestre en qué paso del procedimiento
estoy, qué falta para el siguiente y cómo queda el blíster que tengo delante.

### US5 (P2) — Que la aplicación se vea de la farmacia
Como titular, quiero que la aplicación tenga un aspecto cuidado y coherente, con los estados legibles
de un vistazo, porque es la herramienta que se enseña en una inspección.

## 4. Requisitos funcionales

### 4.1 Marco único (Fase 1)

- **FR-1500** Tras iniciar sesión existe **una sola ventana de trabajo**. Únicas excepciones: el
  asistente de primer arranque, el inicio de sesión y la petición de contraseña maestra (previos a la
  sesión, y excluyentes por naturaleza), y los diálogos modales de confirmación o aviso.
- **FR-1501** Navegación lateral permanente con las secciones de la aplicación, agrupadas en trabajo
  diario y administración (esta última solo para Administrador, misma regla de visibilidad que hoy),
  con **contador** en las secciones que tengan trabajo pendiente.
- **FR-1502** Navegar sustituye el contenido de la región central sin cerrar ni abrir ventanas; existe
  "atrás" al destino anterior.
- **FR-1503** Sistema visual propio aplicado a toda la aplicación: paleta con variante clara y oscura,
  tipografías embebidas en la aplicación (no dependientes de las instaladas en el equipo), y estilos de
  botón primario, secundario y de enlace.
- **FR-1504** Componentes de estado reutilizables: **pastilla** (estado de paciente, de blíster, de
  tratamiento, sí/no de envases al día), **franja de severidad** a la izquierda de una fila con
  problema, y **barra de mensaje** con su botón "¿Por qué?" (Spec 014 FR-1402).
- **FR-1505** F1 sigue abriendo la ayuda contextual, resuelta ahora por la **vista activa** en vez de
  por la ventana.
- **FR-1506** El arranque hasta login sigue por debajo de 2 s (Art. IX.4): las vistas se construyen al
  navegar a ellas, no al abrir la ventana.

### 4.2 Inicio accionable y tablas (Fase 2)

- **FR-1510** La pantalla de inicio muestra los indicadores del día (pacientes con faltantes, blísteres
  verificados sin entregar, sesiones a medias, última lectura ambiental) y la lista de avisos, con la
  información suficiente para decidir sin abrir nada.
- **FR-1511** Cada aviso es accionable: lleva al lugar donde se resuelve (depósito del paciente, su
  preparación, o la sección correspondiente).
- **FR-1512** Los listados de Preparaciones, Retirada de envases, Pacientes y Catálogo se presentan como
  **tabla**: columnas alineadas y con cabecera, ordenación por columna, selección múltiple donde ya
  existe (lote), y cifras con numeración tabular alineadas a la derecha.
- **FR-1513** Búsqueda global en la cabecera del marco: paciente (nombre, apellidos, nº de ficha, DNI,
  CIP), medicamento (nombre o código nacional) y blíster (nº de registro); el resultado navega a su
  destino.
- **FR-1514** Las acciones más frecuentes (nueva sesión de preparación, nuevo paciente, generar en lote)
  están accesibles desde el inicio.

### 4.3 Espacio del paciente (Fase 3)

- **FR-1520** Un paciente se trabaja en **una sola pantalla** con cabecera fija y pestañas: Datos,
  Idoneidad y consentimiento, Tratamiento, Depósito, Preparación, Comunicaciones y Documentos.
- **FR-1521** La cabecera muestra siempre nombre, nº de ficha, estado, alergias destacadas, si tiene
  idoneidad APTO y consentimiento vigente, día de retirada, nº de blísteres y médico de referencia, y
  **se actualiza en el momento** en que cualquier pestaña cambia esos datos (corrige el problema actual
  de la ficha desactualizada tras activar al paciente).
- **FR-1522** Cada pestaña indica si tiene algo pendiente (sin idoneidad o consentimiento, faltantes en
  depósito, blísteres sin entregar, comunicaciones sin respuesta).
- **FR-1523** Los formularios secundarios (registrar envase, alta de representante, respuesta del
  médico, importar tratamiento, revocación, reelaboración) se abren como **panel lateral dentro de la
  pantalla**, no como ventana, y se cierran al completar la acción.
- **FR-1524** Ninguna acción sobre un paciente abre una ventana nueva.
- **FR-1525** El contenido de ayuda de Spec 014 (sección Uso) se actualiza a la nueva navegación, y la
  tabla de ayuda contextual pasa a indexarse por vista.

### 4.4 Preparación como proceso (Fase 4)

- **FR-1530** La preparación muestra un **carril con los cinco pasos** del procedimiento (sesión;
  ambiente y material; llenado; verificación; entrega), señalando el paso actual, los completados y los
  bloqueados **con su motivo**. El estado se deriva del SPD; no se almacena nada nuevo.
- **FR-1531** El blíster se representa como **rejilla de alvéolos** (días de la semana × tomas), con la
  fracción y la identificación del medicamento en cada alvéolo, generada a partir de las líneas del SPD.
- **FR-1532** Verificación y entrega se cumplimentan como paneles del paso correspondiente, con las ocho
  preguntas del Anexo I.G y los datos de adherencia ya existentes.
- **FR-1533** Ningún campo de la interfaz pide un **identificador numérico** de una entidad: el
  verificador y el médico se eligen por nombre en un selector.
- **FR-1534** Los mensajes de bloqueo aparecen junto a la acción que los provoca, con su "¿Por qué?".

### 4.5 Invariantes de todas las fases

- **FR-1540** No cambia ninguna regla de negocio: las capas de Dominio y Aplicación solo se tocan para
  **exponer** datos que ya calculan (p. ej. el destino de un aviso) o para añadir consultas de lectura.
  Ninguna spec funcional anterior se reinterpreta.
- **FR-1541** Al terminar cada fase, la totalidad de los tests está en verde y la aplicación es usable:
  cada fase se fusiona a `main` por separado.
- **FR-1542** La cobertura de regresión de plantillas (`ListBox`/`ItemsControl` con `ItemTemplate` y
  comando de ancestro — regresión F5 de Spec 001) se conserva para toda vista migrada.

## 5. Entidades clave

Ninguna de negocio y ninguna migración: esta spec no crea, modifica ni borra tablas. Los tipos nuevos
son de presentación (ver `data-model.md`).

## 6. Criterios de aceptación

### Fase 1 — Marco único y sistema visual

**CA-1500 Una sola ventana** — Dado que he iniciado sesión, cuando recorro las secciones de navegación
principal, entonces el número de ventanas abiertas sigue siendo una.

**CA-1501 Navegación con contadores** — Dado que hay tres pacientes con faltantes, cuando miro el menú
lateral, entonces "Retirada de envases" muestra el contador 3.

**CA-1502 Paleta completa en ambas variantes** — Dado el conjunto de recursos de color del tema, cuando
se comprueba la variante clara y la oscura, entonces ninguna deja un recurso sin definir.

**CA-1503 F1 por vista** — Dado que estoy en la sección de Preparaciones, cuando pulso F1, entonces se
abre el apartado de procedimiento de preparación, no el índice.

**CA-1504 Arranque** — Dado el arranque de la aplicación, cuando se muestra el login, entonces el
tiempo registrado sigue siendo inferior a 2 s.

### Fase 2 — Inicio y tablas

**CA-1510 El inicio informa** — Dado un día con faltantes, un verificado sin entregar y una sesión a
medias, cuando inicio sesión, entonces veo los tres en la primera pantalla sin abrir nada.

**CA-1511 Aviso accionable** — Dado el aviso "faltan envases de Paracetamol para María López", cuando
lo pulso, entonces llego al depósito de esa paciente.

**CA-1512 Ordenación de tabla** — Dado el listado de preparaciones, cuando ordeno por la columna de
validez, entonces las filas se reordenan y la selección para el lote se conserva.

**CA-1513 Búsqueda global** — Dado que escribo "lópez" en la búsqueda de la cabecera, entonces aparece
María López Vidal; con "F-000041" aparece ese blíster; con "654321", el medicamento.

### Fase 3 — Espacio del paciente

**CA-1520 Siete pestañas, cero ventanas** — Dado un paciente abierto, cuando recorro sus siete
pestañas y registro un envase, entonces no se ha abierto ninguna ventana nueva.

**CA-1521 La cabecera no miente** — Dado un paciente en EVALUACION, cuando registro su evaluación APTO
y la firma del consentimiento desde su pestaña, entonces la cabecera pasa a ACTIVO **sin cerrar ni
reabrir la pantalla**.

**CA-1522 Pestaña con pendiente** — Dado un paciente con faltantes en su depósito, cuando miro sus
pestañas, entonces la de Depósito está marcada como pendiente.

**CA-1523 Panel lateral** — Dado que registro un envase desde una línea del blíster, cuando guardo,
entonces el panel se cierra y la línea ya muestra el envase, sin recargar la pantalla.

### Fase 4 — Preparación

**CA-1530 Carril fiel al estado** — Dado un SPD en PREPARADO, cuando abro su preparación, entonces los
pasos de sesión, ambiente y llenado están completados, verificación es el paso actual y entrega está
bloqueada con el motivo "requiere verificación".

**CA-1531 Rejilla fiel a la pauta** — Dado un blíster con una línea de pauta ½-0-1-0 todos los días,
cuando miro la rejilla, entonces hay siete alvéolos de desayuno con ½, siete de cena con 1, y ninguno
en almuerzo ni en noche.

**CA-1532 Verificador por nombre** — Dado el paso de verificación, cuando elijo verificador, entonces
selecciono una persona de una lista por su nombre, nunca un número.

**CA-1533 Bloqueo explicado en su sitio** — Dado que el verificador coincide con el elaborador, cuando
intento confirmar, entonces el aviso aparece junto al botón, con su "¿Por qué?".

## 7. Casos límite

- **Pantalla pequeña**: con menos de 1100 px de ancho, la navegación lateral se colapsa a iconos y las
  tablas permiten desplazamiento horizontal; el contenido nunca queda cortado sin posibilidad de verlo.
- **Sección sin permisos**: un Elaborador no ve las secciones de administración, igual que hoy; navegar
  a ellas por búsqueda tampoco es posible.
- **Trabajar con dos pacientes a la vez**: la propuesta reduce a una ventana; si se necesita comparar
  dos pacientes, se hace navegando entre ellos (el caso no aparece en el procedimiento y el propietario
  ha pedido expresamente menos ventanas). Si en la prueba manual resulta necesario, se resolverá con
  "abrir en ventana aparte" como excepción explícita, no revirtiendo el diseño.
- **Blísteres de 2 o 3 tomas**: la rejilla de alvéolos se dibuja según el número de tomas configurado;
  el modelo real de dispositivo está pendiente de que el propietario lo indique (ver §9).

## 8. Fuera de alcance de esta spec

- Cualquier cambio de regla de negocio, de documento generado o de esquema de datos.
- Modo táctil, accesibilidad avanzada (lector de pantalla) e internacionalización: fuera, como en el
  resto del proyecto.
- Impresión directa a impresora: se sigue generando el PDF y abriéndolo (Spec 007 sin cambios).

## 9. Decisiones pendientes del propietario

| # | Pregunta | Propuesta si no hay respuesta | Fase que bloquea |
|---|---|---|---|
| Q1 | Orden de las fases | 1 → 2 → 3 → 4 (cada una deja la app usable) | — |
| Q2 | Modelo real de dispositivo (tomas por día y días por blíster) | 7 días × 4 tomas (D/A/C/N), el estándar del PNT | Fase 4 (H4.2) |
| Q3 | Densidad de la interfaz: monitor normal o pantalla pequeña/táctil | 13 px base, pensado para 1366×768 o mayor | Fase 1 (H1.1) |
| Q4 | ¿Se acepta añadir `Avalonia.Controls.DataGrid` (mismo fabricante y versión que el stack ya fijado) para las tablas? | Sí; alternativa sin dependencia nueva en research.md Decisión 4 | Fase 2 (H2.3) |
