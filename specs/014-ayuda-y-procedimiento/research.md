# Research: Ayuda de la aplicación y guía de procedimiento

## Decisión 1 — Markdown embebido, sin dependencia externa de render (FR-1405)

Los ficheros `.md` van como `EmbeddedResource` de `Spd.Presentacion` (`Ayuda/*.md`) y se leen del
ensamblado al arrancar la ventana. Para mostrarlos no se añade ninguna librería de Markdown: un
renderizador propio de ~100 líneas cubre lo que usa el contenido (títulos `#`/`##`/`###`, párrafos,
listas `-`, negrita `**x**`, enlaces internos `[[seccion:id]]`). Alternativa descartada: un paquete
NuGet de Markdown para Avalonia — más superficie de dependencia (Art. VIII: stack fijado) para una
necesidad muy acotada.

## Decisión 2 — Identificadores estables por nombre de fichero

`{seccion}__{orden}-{id}.md` (p. ej. `procedimiento__070-verificacion.md`). El `id` es el contrato
que usan el F1 contextual (`AyudaContextual`), los enlaces cruzados y los botones "¿Por qué?"; el
`orden` fija la posición en el índice; el título es la primera línea `# …` del fichero. Un test
comprueba que todos los ids referenciados existen.

## Decisión 3 — F1 por ventana con un registro de una línea (FR-1401)

`AyudaContextual.Registrar(this)` en el constructor de cada ventana: una tabla ventana→id de
Procedimiento; F1 abre esa sección (y la sección tiene el enlace cruzado a su "Uso"). Alternativas:
clase base común de ventana (cambia el XAML raíz de todas las vistas) o `KeyBinding` en cada XAML
con comando en cada ViewModel (más código repetido). Las ventanas previas al inicio de sesión
(asistente, login, contraseña maestra, aviso) no se registran.

## Decisión 4 — "¿Por qué?" como botón junto al mensaje (FR-1402)

Los mensajes de bloqueo son texto en `Mensaje`; añadir enlaces dentro del texto exigiría cambiar el
contrato de todas las pantallas. Se añade un botón "¿Por qué? (F1)" junto a la barra de mensaje en
las pantallas con bloqueos normativos (preparación, idoneidad/consentimiento) que abre la sección
"El porqué de los bloqueos", con un apartado por regla y su origen (constitución Art. I.3, PNT I).

## Decisión 5 — Búsqueda sin tildes, título antes que contenido (FR-1404)

Se reutiliza `Normalizador.QuitarTildesYMayusculas` (Spec 001). Coincidencia en el título puntúa
más que en el contenido; el resultado indica la sección (Uso/Procedimiento).

## Decisión 6 — Contenido: fuentes y voz

Procedimiento se redacta a partir del PNT I–VII del COF de A Coruña, la Guía del CIM y el curso del
CGCOF (`docs/analisis-resources.md` §3), siempre en segunda persona operativa ("registra…", "pulsa…")
y con tres bloques fijos por apartado (FR-1411): *Qué exige el PNT*, *Qué hago en la aplicación* y
*Qué pasa si se omite*. No se reproduce el PNT completo; sí se citan literales cortos donde el texto
exacto importa (advertencias de etiqueta, compromisos del consentimiento).
