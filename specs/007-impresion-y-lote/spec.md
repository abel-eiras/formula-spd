# Feature Specification: Impresión, generación en lote y documentación base

**Feature Branch**: `007-impresion-y-lote`

**Created**: 2026-09-06

**Status**: Draft — pendiente de `/speckit-clarify`

**Constitución aplicable:** 2.1.0 (Artículos I, II, IV, VIII.2, IX, X, XI)

**Depende de:** Spec 000 (configuración), Spec 001 (pacientes), Spec 005 (depósito y retirada),
Spec 006 (preparación/verificación/entrega), Spec 009 (registros de calidad) — todas ya mergeadas
en `main` salvo lo que se difiere explícitamente (ver Alcance de esta iteración).

**Requerida por:** Ninguna interna; es el motor de generación de documentos usado por el resto.

**Input**: Especificación completa aportada literalmente por el propietario del producto
(`spec-007-impresion-y-lote.md`, v0.1 — 2026-09-04). Se traslada tal cual salvo una corrección
formal obligatoria: no se reinterpretan los requisitos de negocio, no se añaden requisitos nuevos,
no se cambia la numeración FR-7xx ni los criterios de aceptación CA-7xx.

**Corrección formal (no de negocio) — FR-701/Q2**: el documento fuente (v0.1, anterior a la
fijación del stack en la Constitución) proponía generar todo en `.docx`. La Constitución 2.1.0,
posterior, fija el stack de la aplicación (Artículo VIII.2) y en él el motor de documentos es
**QuestPDF** (generación de PDF), no una librería de Word — cambiar el stack exige una enmienda
constitucional, no una decisión de spec. Mismo patrón que Spec 009 encontró con `FormacionPersonal`
(contenido de Fase 1 desactualizado tras una enmienda posterior de la Constitución). FR-701 se
reinterpreta como "todo documento se genera en formato PDF mediante QuestPDF", resolviendo también
la Q2 del documento fuente (que preguntaba PDF vs. .docx para las etiquetas) a favor de PDF para
todo, por coherencia con el motor único que exige el propio FR-701 ("un solo servicio de
generación por familia de documento, sin duplicar lógica de maquetación"). El resto de FR-701 (una
sola clase de servicio por familia de documento) se mantiene literal.

**Alcance de esta iteración**: el catálogo de documentos de FR-700 incluye tipos que dependen de
specs que no existen todavía en esta rama (`IDONEIDAD`/`CONSENT` de Spec 002) o de contenido
externo que el propietario ha indicado expresamente que se revisará más adelante con más
capacidad de análisis (plantillas/PNT reales, ver Assumptions). Esta iteración construye el motor
único (FR-710..713) completo y lo aplica a los documentos que sí son generables hoy con datos
reales de la aplicación: `FICHA`, `ETQ-A`, `ETQ-R`, `INSTR` (Spec 006) y `FICHA-PAC` (Spec 001).
FR-720..733 (lote y documentación base) y el resto del catálogo (`CARTA-PRES`/`CARTA-INC` de Spec
008, `RETIRADA` de Spec 005, `REG-*` de Spec 009, `IDONEIDAD`/`CONSENT` de Spec 002) quedan
diferidos — ver Fuera de alcance y Assumptions.

**Corrección 2026-09-06 (tras el cribado de `resources/`, ver `docs/analisis-resources.md`)**: la
primera implementación de `FICHA`, `ETQ-A`, `ETQ-R`, `INSTR` y `FICHA-PAC` era un esqueleto que no
contenía los elementos mínimos de sus anexos (Art. I.2). Con los PNT del COF de A Coruña (Decreto
87/2022) ya disponibles, los cinco se completan campo a campo según los Anexos I.G, I.F, I.H y I.E
del PNT I, y cada uno lleva un test que extrae el texto del PDF generado y comprueba la presencia
de esos elementos (Art. IX.3). Se añade un único documento nuevo al catálogo, `RGPD` (Anexo I.D,
información de protección de datos), por decisión del propietario: es el único de los anexos no
contemplados que la aplicación puede rellenar automáticamente con datos propios (farmacia y
paciente); el resto (calibración, incidencia ambiental, recepción de DDP, organigrama, firmas)
se cubre con el modelo oficial del Colegio en papel y **no** entra en el catálogo. Para el `RGPD`
se añaden a `Farmacia` los campos `dpo_nombre`/`dpo_contacto` (migración 0010).

---

## Clarifications

### Session 2026-09-06

- Q: FR-701/Q2 — ¿todos los documentos en `.docx`, o PDF para que no se descuadren al abrirlas en
  otro equipo? → A: PDF para todos, vía QuestPDF (el motor ya fijado en la Constitución Artículo
  VIII.2) — no es una preferencia de maquetación, es una consecuencia obligada del stack ya fijado.
- Q: FR-710/Q1 — ¿120 caracteres es razonable para el nombre de fichero? → A: Sí, la propuesta
  única del documento fuente (120 para el nombre; formato corto si la ruta completa supera 200).

---

## 1. Propósito

Definir el motor único de generación de documentos de la aplicación, sus tres modos de uso —
impresión individual desde una pantalla (ya referenciada en Spec 006 §4.9), generación en lote
sobre varios pacientes a la vez, y generación de la documentación base de la farmacia desde
Configuración — y la convención de nombres y carpetas de salida común a los tres.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Generar documentos individuales y en lote |
| Administrador | Generar y regenerar la documentación base de la farmacia (Configuración) |

## 3. Escenarios de usuario (User Stories)

### US1 (P1) — Imprimir la ficha, etiquetas e instrucciones de un blíster (E-implícito de Spec 006 §4.9)
Como elaborador, tras pasar un blíster a PREPARADO, quiero generar su ficha de
preparación-control-entrega, sus etiquetas y su hoja de instrucciones en PDF, con nombre de
fichero identificable, sin repetir a mano ningún dato ya introducido.

**Prueba independiente**: generar `FICHA` de un SPD real y comprobar que el PDF resultante existe,
tiene el nombre esperado (CA-700), y que la posología aparece en fracción, nunca en decimal
(CA-709/710).

### US2 (P1) — Imprimir la ficha del paciente (Anexo 2)
Como elaborador, desde la ficha de un paciente, quiero generar su ficha en PDF con sus datos
básicos, para el expediente físico.

**Prueba independiente**: generar `FICHA-PAC` de un paciente real y comprobar que el PDF existe
con los datos del paciente.

### US3 (P1) — Nombre de fichero que no se rompe con nombres largos (E4)
Como elaborador, quiero que el nombre del fichero identifique claramente el documento y el
paciente, y que si el nombre completo es demasiado largo, el sistema use el código de ficha y una
abreviatura sin que la generación falle.

**Prueba independiente**: generar un documento para un paciente con nombre muy largo y comprobar
que el fichero usa el formato corto (CA-701).

## 4. Requisitos funcionales

### 4.1 Catálogo de documentos

- **FR-700** Catálogo cerrado de tipos de documento, cada uno con código de abreviatura para
  nombres de fichero largos:

| Documento | Código | Ámbito | Fuente | Esta iteración |
|---|---|---|---|---|
| Ficha de preparación, control y entrega | `FICHA` | Por SPD (blíster) | Spec 006 | Sí |
| Etiqueta anverso | `ETQ-A` | Por SPD | Spec 006 | Sí |
| Etiqueta reverso | `ETQ-R` | Por SPD | Spec 006 | Sí |
| Hoja de instrucciones al paciente | `INSTR` | Por SPD (o por sesión si Spec 006 FR-682 lo permite) | Spec 006 | Sí |
| Ficha del paciente (Anexo 2) | `FICHA-PAC` | Por paciente | Spec 001 | Sí |
| Información sobre protección de datos (Anexo I.D) | `RGPD` | Por paciente | Este documento (corrección 2026-09-06) | Sí — desde la ficha del paciente |
| Evaluación de idoneidad | `IDONEIDAD` | Por evaluación | Spec 002 | Sin documento aparte: se imprime dentro de `FICHA-PAC` (Anexo I.E; Spec 002, 2026-09-06) |
| Consentimiento informado (Anexo I.B, paciente o representante) | `CONSENT` | Por consentimiento | Spec 002 | Sí — desde la pantalla de idoneidad y consentimiento (2026-09-06) |
| Carta de presentación al médico | `CARTA-PRES` | Por comunicación | Spec 008 | Diferido |
| Carta de incidencias al médico | `CARTA-INC` | Por comunicación | Spec 008 | Diferido |
| Listado de retirada de envases | `RETIRADA` | Por fecha de listado | Spec 005 | Diferido |
| Registro de condiciones ambientales | `REG-AMB` | Por periodo | Spec 006/009 | Diferido |
| Registro de limpieza | `REG-LIMP` | Por periodo | Spec 009 | Diferido |
| Registro de formación del personal | `REG-FORM` | Por usuario/periodo | Spec 009 | Diferido |
| Registro de recogida de residuos no SIGRE | `REG-RES` | Por periodo | Spec 009 | Diferido |
| Documentación base (conjunto) | — | Farmacia, una vez | Este documento §4.4 | Diferido |

- **FR-701** Todo documento se genera en formato **PDF** mediante el motor de generación de
  documentos (QuestPDF, Constitución Artículo VIII.2 — corrección formal, ver cabecera de este
  documento), con una sola clase de servicio de generación por familia de documento, sin duplicar
  lógica de maquetación entre pantallas (Artículo XI: legibilidad).

### 4.2 Nombre de fichero y carpeta de salida

- **FR-710** Nombre de fichero por defecto: `"<Nombre del documento>. <Nombre> <Apellido1>
  <Apellido2>.<ddmmaaaa>.pdf"`, donde `<ddmmaaaa>` es la fecha de generación. Ejemplo: `Hoja de
  elaboración. María López Vidal.04092026.pdf`.
- **FR-711** Si el nombre resultante supera 120 caracteres, o si el paciente no tiene apellidos
  completos, o si dos documentos generados en el mismo lote producirían el mismo nombre, el
  sistema usa el formato corto: `"<CODIGO>_<num_ficha o num_registro>_<ddmmaaaa>.pdf"`. Ejemplo:
  `FICHA_F-000123_04092026.pdf`.
- **FR-712** Carpeta de salida configurable en Configuración (`Farmacia.RutaDocumentosGenerados`,
  ya existente desde Spec 000). Estructura por defecto:
  ```
  <ruta_documentos_generados>/
    <aaaa-mm-dd de la generación>/
      <ficheros generados en esa tanda>
  ```
  Una generación individual y una generación en lote ambas caen en la carpeta del día en que se
  generaron, no del periodo de validez del SPD.
- **FR-713** Cada fichero generado deja traza en auditoría (tipo de documento, entidad de origen,
  ruta final, usuario, fecha-hora) — Constitución Artículo VII.6.

### 4.3 Generación en lote

- **FR-720** Columna "Envases al día" en el listado de Preparaciones (Spec 006 FR-690, diferido en
  esta iteración junto con el resto de FR-720..725 — ver Fuera de alcance).
- **FR-721** — diferido.
- **FR-722** — diferido.
- **FR-723** — diferido.
- **FR-724** — diferido.
- **FR-725** — diferido.

### 4.4 Documentación base de la farmacia

- **FR-730** a **FR-733** — diferidos (ver Fuera de alcance).

### 4.5 Formato de posología en documentos impresos

- **FR-740** En todo documento impreso que muestre posología (ficha de preparación, etiquetas,
  hoja de instrucciones), las dosis D/A/C/N se representan **siempre en fracciones**, nunca en
  decimales. Ejemplo: medio comprimido en desayuno y en cena se imprime `1/2 - 0 - 1/2 - 0`, no
  `0,5 - 0 - 0,5 - 0`.
- **FR-741** El vocabulario de fracciones admitido es cerrado (Spec 004, `FraccionDosis`), no una
  conversión decimal→fracción por aproximación. Ya implementado desde Spec 004: `Cero, UnCuarto,
  UnTercio, Media, DosTercios, TresCuartos, Uno, UnoYCuarto, UnoYMedio`, con `Texto()` devolviendo
  la representación exacta ("1/2", "1 1/4", …).
- **FR-742** Internamente el sistema guarda el valor numérico equivalente (Spec 004
  `FraccionDosis.Valor()`) para poder sumar y calcular unidades semanales (Spec 005 FR-522), pero
  la interfaz de introducción de dosis (Spec 004) ya es un selector sobre el vocabulario cerrado.
  Sin cambios en esta spec.
- **FR-743** En pantalla se puede mostrar la fracción, el decimal, o ambos; lo único obligatorio
  por esta spec es que **lo impreso sea siempre fracción**: los generadores de documento llaman a
  `FraccionDosis.Texto()`, nunca a `Valor()`, para representar una dosis.

## 5. Entidades clave

Esta spec no añade tablas de negocio nuevas; usa `Farmacia` (Spec 000/005), `SPD` y sus líneas
(Spec 006), `Paciente` (Spec 001), y añade una entrada de Auditoría por fichero generado (FR-713).

## 6. Criterios de aceptación

**CA-700 Nombre por defecto**
Dado un paciente "María López Vidal" y una ficha de preparación generada el 4 de septiembre de
2026, cuando se genera el documento, entonces el fichero se llama `Ficha de preparación. María
López Vidal.04092026.pdf`.

**CA-701 Nombre acortado por longitud**
Dado un paciente con nombre y apellidos que producirían un nombre de más de 120 caracteres, cuando
se genera el documento, entonces el fichero usa el formato `<CODIGO>_<num_ficha>_<fecha>.pdf`.

**CA-702 a CA-708** — diferidos junto con la generación en lote y la documentación base (FR-720..733).

**CA-709 Posología impresa en fracción**
Dado un tratamiento con pauta D=1/2, A=0, C=1/2, N=0, cuando se genera la ficha de preparación,
entonces la posología impresa es `1/2 - 0 - 1/2 - 0`, no `0,5 - 0 - 0,5 - 0`.

**CA-710 Vocabulario cerrado evita aproximaciones**
Dado un tratamiento con dosis 1/3 de comprimido, cuando se imprime, entonces la ficha muestra
exactamente `1/3`, no `0,33` ni `0,3`.

## 7. Casos límite

- Un paciente incluido en el lote cuya sesión pasa a requerir revisión: diferido junto con FR-720..725.
- Documentación base solicitada sin logo cargado: diferido junto con FR-730..733.
- Dos pacientes con el mismo nombre y apellidos completos, generados en momentos distintos (no en
  el mismo lote): no hay colisión real porque cada fichero cae en la carpeta del día y un nombre
  repetido simplemente coexiste; FR-711 solo resuelve la colisión dentro de un mismo lote.
- Envase con múltiples filas en una línea (Spec 006 FR-611): la ficha y la etiqueta reverso deben
  imprimir tantas filas de ese medicamento como envases hayan intervenido (Spec 006 CA-602).

## 8. Fuera de alcance de esta spec

- FR-720..733 (generación en lote y documentación base de la farmacia): quedan diferidas —
  requieren la columna "Envases al día" (que a su vez depende de recorrer todas las sesiones
  pendientes de todos los pacientes, una pantalla nueva no construida todavía) y, en el caso de la
  documentación base, un catálogo de textos legales fijos que conviene revisar junto con las
  plantillas y el PNT reales que el propietario ha añadido para un análisis posterior (ver
  Assumptions) — construirlos ahora sin ese material sería el mismo riesgo de contenido que ya se
  evitó con el Anexo 9 de Spec 002.
- `IDONEIDAD`/`CONSENT`: Spec 002 no existía al escribir esto; construida el mismo día (ver catálogo FR-700 actualizado).
- `CARTA-PRES`/`CARTA-INC`/`RETIRADA`/`REG-*`: sus servicios de origen (Specs 005/008/009) ya
  registran el punto de extensión de impresión (`RegistrarImpresion`/similar) pero esta iteración
  no construye su maquetación PDF concreta — se puede añadir en una sesión posterior reutilizando
  el mismo motor sin cambiar su contrato.
- El contenido y maquetación **exactos** de cada Anexo oficial del PNT: el Artículo I.2 de la
  Constitución permite rediseñar libremente el documento siempre que contenga como mínimo los
  elementos que el PNT exige; esta iteración construye una maquetación funcional con los campos
  que la propia especificación ya describe, no una réplica pixel a pixel de un Anexo que no se ha
  analizado todavía (ver Assumptions).
- Impresión física (enviar a la impresora del sistema operativo): fuera de alcance de la
  aplicación; el usuario abre el PDF generado y lo imprime con su lector habitual.
- Firma electrónica o digital de cualquier documento (Constitución Artículo II).

## 9. Assumptions

- El usuario ha añadido una carpeta `resources/` con documentación de PNT y plantillas reales, y
  ha pedido explícitamente que no se analice ese contenido todavía (se hará en una sesión
  posterior con más capacidad). Esta spec no abre ni referencia esa carpeta: los generadores de
  documento de esta iteración usan una maquetación propia basada en los campos que las specs de
  origen (001, 006) ya describen textualmente, dejando la revisión contra las plantillas reales
  como trabajo futuro documentado, no como una tarea silenciosamente omitida.
- FR-701 se reinterpreta como PDF/QuestPDF en vez de `.docx` por ser una corrección formal exigida
  por un artefacto posterior (la Constitución) al documento fuente, no una decisión de producto —
  ver cabecera de este documento.
