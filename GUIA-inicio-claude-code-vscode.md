# Guía — De esta documentación a un proyecto real en Claude Code (VS Code)

Versión 0.1 — 4 de septiembre de 2026. Pensada para el extension de Claude Code en VS Code, no para el uso por terminal puro.

---

## 0. Una advertencia honesta sobre la extensión de VS Code

La extensión de Claude Code para VS Code es el mismo motor que la CLI — mismo modelo, mismo `CLAUDE.md`, mismos hooks, MCP y skills — pero a fecha de esta guía tiene una limitación conocida: **la lista de comandos slash que se ve al escribir `/` en el panel de la extensión no siempre tiene paridad completa con la CLI**, y los comandos de proyecto que crea Spec Kit (`/speckit.specify`, `/speckit.plan`, etc., guardados en `.claude/commands/`) pueden no aparecer de forma fiable en el autocompletado de la extensión.

La solución práctica, sin salir de VS Code: usa la **terminal integrada de VS Code** (`Ctrl+ñ` / `Ctrl+backtick`) para el arranque del proyecto y para lanzar los comandos de Spec Kit la primera vez; usa el **panel de Claude Code** para todo el trabajo conversacional de revisión, aclaración e implementación. Ambos comparten el mismo repositorio abierto, así que no pierdes contexto por alternar entre uno y otro — es la misma carpeta, el mismo `CLAUDE.md`, la misma sesión de Git.

Si en algún momento un comando `/speckit.*` no aparece en el panel, ejecútalo desde la terminal integrada con `claude` (la CLI) apuntando a la misma carpeta: es exactamente el mismo agente, solo cambia el marco de la ventana.

---

## 1. Instalación (una sola vez)

En la terminal integrada de VS Code:

```bash
# Spec Kit (herramienta que scaffolda .specify/ y los comandos slash)
uv tool install specify-cli
# (si no tienes uv: pipx install specify-cli, o pip install specify-cli --user)

# Extensión de Claude Code en VS Code, si no la tienes ya:
# Ctrl+Shift+X -> buscar "Claude Code" -> Install
```

## 2. Estructura de carpetas recomendada

```
spd-farmacia/                          <- raíz del repositorio git
├── .specify/                          <- creado por `specify init`
│   ├── memory/
│   │   └── constitution.md            <- pega aquí tu constitution.md tal cual
│   ├── templates/                     <- plantillas internas de Spec Kit, no tocar
│   └── scripts/
├── .claude/
│   ├── commands/                      <- comandos /speckit.* generados por specify init
│   └── skills/                        <- skills de proyecto (§5 de esta guía)
├── CLAUDE.md                          <- instrucciones permanentes para Claude Code en este repo
├── specs/                             <- una carpeta por funcionalidad, creada por /speckit.specify
│   ├── 000-configuracion-y-usuarios/
│   │   ├── spec.md
│   │   ├── plan.md
│   │   ├── data-model.md
│   │   └── tasks.md
│   ├── 001-pacientes-y-medicos/
│   └── ...
├── docs/
│   ├── data-model.md                  <- tu modelo de datos consolidado (referencia global)
│   ├── arquitectura.md                <- resumen de una página (Constitución Artículo XI.5)
│   ├── legislacion/                   <- ver §6
│   │   ├── PNT-SPD-Pontevedra-2022.pdf
│   │   └── Decreto-87-2022-Xunta.pdf
│   └── plantillas-word-originales/    <- ver §6
│       ├── Anexo_01a_...docx
│       ├── Anexo_02_Ficha_del_paciente.xlsx
│       └── ...
├── src/
│   ├── Spd.Dominio/                   <- Artículo VIII: entidades y reglas, sin dependencias
│   ├── Spd.Aplicacion/                <- servicios de caso de uso
│   ├── Spd.Infraestructura/           <- SQLite/SQLCipher, QuestPDF, ficheros
│   └── Spd.Presentacion/              <- Avalonia, MVVM
├── tests/
│   ├── Spd.Dominio.Tests/
│   └── Spd.Aplicacion.Tests/
└── SPD.sln
```

`docs/data-model.md` es tu documento consolidado de referencia global; cada `specs/NNN-.../data-model.md` que genere `/speckit.plan` es el recorte específico de esa funcionalidad — no son el mismo fichero, y está bien que no lo sean.

## 3. Orden de trabajo y dependencias

Sigue este orden; cada funcionalidad depende de que la anterior esté mergeada:

```
000 configuración y usuarios          (base de todo)
 └─ 001 pacientes, contactos, médicos
 └─ 003 catálogo de medicamentos
      └─ 004 tratamiento
           └─ 002 idoneidad y consentimiento
                └─ 005 depósito y retirada
                     └─ 009 registros de calidad
                          └─ 006 preparación, verificación, entrega
                               └─ 007 impresión, lote, documentación base
                                    └─ 008 comunicaciones al médico
                                    └─ 010 backup, cifrado, purga
                                    └─ 011 import/export
                                         └─ 012 lectura DataMatrix
                                    └─ 014 ayuda y procedimiento
```

014 (ayuda) puede escribirse en paralelo desde el principio si quieres ir documentando cada pantalla a medida que se construye, en vez de dejarla para el final.

## 4. Los comandos, paso a paso, con los prompts exactos

### 4.1 Arrancar el proyecto (terminal integrada)

```bash
specify init spd-farmacia --ai claude
cd spd-farmacia
code .          # si no está ya abierto
```

Abre la carpeta en VS Code si `code .` no funciona directamente.

### 4.2 Constitución (panel de Claude Code o terminal)

Abre el panel de Claude Code y escribe:

```
/speckit.constitution
```

Cuando pregunte qué constitución quieres, pega el contenido íntegro de tu `constitution.md` (versión 2.1.0) y añade:

> Usa este documento literalmente. No reformules los artículos, no añadas principios que no estén aquí, no "mejores" la redacción. Guárdalo en `.specify/memory/constitution.md` sustituyendo cualquier plantilla por defecto.

### 4.3 Cada funcionalidad — cuatro comandos por spec

Para cada una de las specs, en el orden de la sección 3:

**Paso 1 — `/speckit.specify`**
```
/speckit.specify
```
Pega el contenido íntegro del `.md` de la spec correspondiente (por ejemplo, todo `spec-000-configuracion-y-usuarios.md`) y añade:

> Esta es la especificación completa de la funcionalidad, ya redactada. Úsala tal cual: no reinterpretes los requisitos, no inventes otros nuevos, no cambies la numeración FR-xxx ni los criterios de aceptación. Si encuentras algo genuinamente ambiguo que no esté ya marcado `[NEEDS CLARIFICATION]`, márcalo tú mismo así en vez de asumir una solución. Crea la rama y `specs/NNN-nombre/spec.md` según la convención de Spec Kit.

**Paso 2 — `/speckit.clarify`**
```
/speckit.clarify
```
Deja que te pregunte. Aquí es donde resuelves en vivo cualquier `[NEEDS CLARIFICATION]` que quedara en la spec, incluidas las preguntas abiertas de la sección 9 de cada documento. Si alguna pregunta ya la resolviste en la conversación conmigo, dile directamente la respuesta que ya decidiste.

**Paso 3 — `/speckit.plan`**
```
/speckit.plan
```
Antes de lanzarlo, pega también el contenido de `docs/data-model.md` (o al menos la sección de la entidad principal de esta spec) y di:

> El modelo de datos y el stack ya están decididos en `docs/data-model.md` y en la constitución — no propongas un stack alternativo ni un modelo de datos distinto. El plan debe derivarse de estos, no inventarlos. Incluye explícitamente la sección "Constitution Check" citando qué artículos aplica esta funcionalidad.

**Paso 4 — `/speckit.tasks`**
```
/speckit.tasks
```
Sin prompt adicional necesario salvo que quieras acotar el tamaño de las tareas.

**Paso 5 — `/speckit.analyze`** (antes de implementar, no lo saltes)
```
/speckit.analyze
```
Cruza spec, plan y tareas contra la constitución. Si señala una incoherencia, resuélvela antes de seguir — es más barato aquí que a mitad de implementación.

**Paso 6 — `/speckit.implement`**
```
/speckit.implement
```
Aquí Claude Code escribe código, ejecuta `dotnet build` y `dotnet test`. Revisa el diff en el panel de VS Code (la vista de diferencias lado a lado es justamente el punto fuerte de la extensión frente a la terminal) antes de aceptar.

**Al terminar cada funcionalidad:**
```bash
git add -A
git commit -m "feat(NNN): implementa <nombre de la funcionalidad>"
```
Y solo entonces pasas a la siguiente spec de la lista de la sección 3.

### 4.4 Recordatorio permanente en `CLAUDE.md`

Crea `CLAUDE.md` en la raíz con, como mínimo:

```markdown
# spd-farmacia

Antes de cualquier cambio, lee `.specify/memory/constitution.md`.
Los Artículos III (nada se borra) y IV (catálogo vivo, instantánea congelada)
no admiten excepciones silenciosas: si una tarea parece requerir romperlos,
detente y pregunta en vez de improvisar una solución.

Este proyecto es software SPD para farmacias gallegas. El dominio está en
castellano (Paciente, Tratamiento, SPD); la mecánica técnica genérica está
en inglés (Repository, ILogger). No mezclar ambos en un mismo nombre.

Antes de escribir código en `src/Spd.Dominio`, comprueba si ya existe una
entidad o regla equivalente en otra spec — el Artículo V prohíbe duplicar
lo que ya existe en un catálogo.
```

## 5. Skills recomendadas

### 5.1 Oficiales de Anthropic (`anthropics/skills`)

```
claude plugin marketplace add anthropics/skills
claude plugin install document-skills@anthropic-agent-skills
```

- **`docx`** — útil no para la aplicación en sí (que genera `.docx` con OpenXML/QuestPDF en C#), sino para cuando le pidas a Claude, en una conversación normal, que te *maquete un boceto* de cómo debería verse la ficha de preparación o la etiqueta antes de que Claude Code lo traduzca a código de generación. Acelera la fase de diseño visual.
- **`pdf`** — para cuando necesites extraer texto o tablas del PNT o del Decreto 87/2022 en PDF al redactar el contenido literal de Spec 002 (los 7 criterios del Anexo 9) o el contenido de la ayuda (Spec 014).
- **`skill-creator`** — la usarás para crear las dos skills de proyecto de la sección 5.2.

### 5.2 Skills propias del proyecto (creadas con `skill-creator`)

Estas no existen en ningún repositorio; las creas tú una vez y quedan en `.claude/skills/` del propio repositorio, así que se activan solas en cualquier sesión de Claude Code sobre este proyecto sin que tengas que repetir instrucciones.

**`spd-dominio`** — encapsula el vocabulario y las reglas del PNT (qué es un SPD, qué es la instantánea legal, la regla "entero más uno" de fraccionables, verificador≠elaborador) para que cualquier tarea de código que toque `Spd.Dominio` la consulte automáticamente en vez de que tengas que recordárselo en cada prompt. Pídele a Claude:

```
Usa skill-creator para crear una skill de proyecto llamada "spd-dominio"
que se active cuando se trabaje con código o especificaciones de
Spd.Dominio. Debe resumir: qué es un SPD (siempre un blíster, nunca una
hoja para dos), la regla de instantánea del Artículo IV, la regla de
fraccionables "entero más uno" de la Spec 005, y la lista de estados de
Tratamiento y SPD. Básate en constitution.md y en docs/data-model.md.
```

**`spd-estilo-codigo`** — el Artículo XI de la constitución en forma de skill: nombres en castellano para el dominio, inglés para mecánica técnica, métodos cortos, comentario de cabecera por clase, comentarios que explican el porqué citando el artículo del PNT. Sin esto, un agente de código tiende a derivar hacia sus convenciones por defecto (inglés genérico, abstracciones .NET idiomáticas) en cuanto la conversación se alarga.

```
Usa skill-creator para crear una skill de proyecto llamada
"spd-estilo-codigo" a partir del Artículo XI de constitution.md, que se
active en cualquier tarea de escritura o revisión de código C#/Avalonia
en este repositorio.
```

### 5.3 Comunidad (skills.sh, opcionales)

`skills.sh` es un directorio de skills de terceros instalable con `bunx skills add <owner/repo>`. Antes de instalar cualquiera, revisa el `SKILL.md` del repositorio — una skill se ejecuta con los mismos permisos que el resto de la sesión, así que vale la misma cautela que con cualquier dependencia externa.

Dos paquetes que encajan con el tipo de trabajo de este proyecto (flujo de ingeniería disciplinado, no específico de .NET):

```bash
bunx skills add mhattingpete/claude-skills-marketplace --list
# git automation, test-fixing, code-review, feature-planning
```

No hay, a fecha de esta guía, una skill de comunidad específica para Avalonia/QuestPDF lo bastante consolidada como para recomendarla a ciegas — es un hueco real del ecosistema. Si en algún punto del desarrollo notas que repites las mismas instrucciones sobre patrones de Avalonia MVVM o de plantillas QuestPDF, ese es el momento de crear una tercera skill propia (`spd-avalonia` o `spd-questpdf`) con `skill-creator`, en vez de buscar una de terceros que probablemente no encaje con las convenciones ya fijadas en el Artículo XI.

No instales skills de diseño web, testing con Playwright, ni nada orientado a frontend web — no aplican a una aplicación de escritorio Avalonia y solo añaden ruido a lo que Claude Code considera relevante en cada tarea.

## 6. Legislación y plantillas: sí, guárdalas, pero fuera del build

Guarda ambas cosas en el repositorio, en `docs/`, **nunca dentro de lo que termina empaquetado en el ejecutable portable** — el Artículo VI exige que la instalación final sea solo exe + base de datos + configuración + logs + backups; los PDF y los `.docx` originales son material de referencia para quien mantiene el proyecto, no un activo en tiempo de ejecución.

```
docs/legislacion/
├── PNT-SPD-Pontevedra-junio-2022.pdf
└── Decreto-87-2022-Xunta-Galicia.pdf

docs/plantillas-word-originales/
├── Anexo_01a_...docx
├── Anexo_01b_...docx
├── Anexo_02_Ficha_del_paciente.xlsx
├── Anexo_05_Ficha_de_preparacion...xlsx
├── Anexo_06_Etiqueta...xlsx
├── Anexo_07_Hoja_instrucciones...xlsx
└── Procedimiento_general_SPD.docx
```

Para qué sirve cada carpeta, en la práctica:

- **`legislacion/`** — fuente de verdad al redactar Spec 002 (los 7 criterios exactos del Anexo 9, que dejé como `[NEEDS CLARIFICATION]` a propósito para no transcribirlos de memoria), y al escribir el contenido de Spec 014 (la ayuda cita el artículo del PNT del que procede cada regla — necesitas el texto original a mano para citarlo bien, no de memoria). También es la referencia si en algún momento cambia la normativa y hay que revisar qué specs quedan afectadas.
- **`plantillas-word-originales/`** — referencia de maquetación exacta para quien implemente Spec 007: qué campos lleva cada anexo, en qué orden, con qué formato de tabla. El código de generación (QuestPDF/OpenXML) no lee estos ficheros en tiempo de ejecución; un desarrollador (o Claude Code) los abre para replicar la estructura al escribir la plantilla en C#.

Cuando llegues a implementar Spec 007, un prompt útil:

```
Abre docs/plantillas-word-originales/Anexo_05_Ficha_de_preparacion...xlsx
y usa su estructura de columnas y cabeceras como referencia exacta para
la plantilla QuestPDF de la Ficha de preparación, control y entrega.
No inventes campos que no estén en el original; si algo de la spec 006
no encaja con el original, dímelo antes de continuar en vez de decidir tú.
```

## 7. Qué hacer si algo no encaja durante la implementación

Si `/speckit.analyze` o `/speckit.implement` detectan una contradicción entre una spec y la constitución, o entre dos specs entre sí, no dejes que el agente la resuelva por su cuenta con una suposición razonable — pídele explícitamente que pare y te lo plantee. El Artículo X.3 de tu propia constitución lo dice: un `[NEEDS CLARIFICATION]` bloquea, no se resuelve con una suposición del implementador. Eso vale igual para lo que descubras a mitad de código que para lo que ya estaba escrito en la spec.
