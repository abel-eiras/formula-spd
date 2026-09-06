# Feature Specification: Preparación, verificación y entrega del SPD

**Feature Branch**: `006-preparacion-verificacion-entrega`

**Created**: 2026-09-06

**Status**: Draft — pendiente de `/speckit-clarify`

**Constitución aplicable:** 2.1.0 (Artículos I.3, II, III, IV, V, VII, VIII, IX)

**Depende de:** Spec 001 (pacientes), Spec 003 (catálogo de medicamentos), Spec 004
(tratamiento), Spec 005 (depósito de envases y listado de retirada) — todas ya mergeadas en
`main`. Spec 002 (idoneidad y consentimiento) y Spec 009 (registro ambiental como pantalla
independiente) están fuera de esta rama — ver Alcance de esta iteración.

**Requerida por:** Spec 007 (impresión), Spec 008 (comunicaciones al médico).

**Input**: Especificación completa aportada literalmente por el propietario del producto
(`spec-006-preparacion-verificacion-entrega.md`, v0.2 — 2026-09-04). Se traslada tal cual: no se
reinterpretan los requisitos, no se añaden requisitos nuevos, no se cambia la numeración FR-6xx ni
los criterios de aceptación CA-6xx.

**Alcance de esta iteración**: FR-600 a FR-695 y FR-6120 a FR-6127 completos, con dos puntos de
extensión documentados en vez de fabricados:

1. **Precondición de idoneidad/consentimiento (FR-602, Constitución Art. I.3)**: Spec 002 no existe
   en esta rama. Se implementa `IComprobadorIdoneidadYConsentimiento` (research.md Decisión 1),
   con una implementación por defecto que siempre aprueba, exactamente como Spec 005 hizo con
   `IComprobadorCoberturaSpd` hacia esta misma spec. La regla en sí (Art. I.3, "no se prepara sin
   consentimiento vigente y evaluación APTO") queda con test unitario sobre la interfaz, tal como
   exige la Constitución, sin inventar la lógica clínica de Spec 002.
2. **Registro ambiental y material de acondicionamiento**: Spec 009 retiró estas pantallas de su
   propio alcance explícitamente para esta spec (ver `src/Spd.Infraestructura/Migraciones/0004_registros_calidad.sql`,
   nota de alcance 2026-09-05: "la temperatura/humedad se rellenará más adelante... al generar la
   hoja de elaboración del blíster"). Esta spec crea `RegistroAmbiental` y
   `MaterialAcondicionamiento` como entidades propias (FR-630..632), tal como esa nota anticipaba.

FR-680/681 (impresión real) quedan como acción expuesta que registra `impreso_*_en` y audita, sin
generar ningún documento — la generación real es de Spec 007, que no existe todavía en esta rama.

---

## Clarifications

### Session 2026-09-06

- Q: FR-682 — ¿una hoja de instrucciones vale para los dos blísteres de una quincena o hace falta
  una por semana? → A: Una por blíster hasta confirmar (la propuesta única de la propia spec,
  Q1 de su §9, sin alternativa razonable mejor sin el PNT a mano).

---

## 1. Propósito

Cubrir el Anexo 5 para un blíster semanal 7×4: cada blíster tiene su propia ficha de
preparación-control-entrega con número de registro propio, aunque la pantalla permita generar los
dos blísteres de un paciente (si `n_blisteres = 2`) en una sola sesión de trabajo sin repetir
datos.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Crear, preparar, verificar (si no es quien preparó), entregar, anular, consultar — sin distinción de categoría profesional. La app exige verificador ≠ elaborador por usuario (Artículo I.3 y VII.5 de la Constitución), no por rol |

## 3. Escenarios de usuario (User Stories)

### US1 (P1) — Crear y llenar una sesión de preparación (E1, E2, E3)
Como farmacéutico, con el paciente activo, envases suficientes en depósito y tratamiento
revisado, abro "Nueva sesión de preparación" y el sistema crea automáticamente 1 o 2 SPD (según
`n_blisteres`), con sus líneas y filas de envase repartidas según la regla de consumo de Spec 005.

**Prueba independiente**: crear una sesión para un paciente con `n_blisteres=2` y verificar que se
crean dos SPD con validez consecutiva de 7 días y sus líneas correctas (CA-600); verificar que un
medicamento que necesita dos envases genera dos filas de envase en su línea (CA-602).

### US2 (P2) — Alta de envase sin salir de la preparación (E7)
Como elaborador, si al preparar descubro que un envase se agotó, lo registro desde la propia
pantalla y la línea recalcula el reparto sin perder lo ya introducido en otras líneas.

**Prueba independiente**: con una línea sin saldo suficiente, registrar un envase nuevo desde el
punto de extensión de preparación y verificar que la línea queda cubierta (CA-603, CA-604).

### US3 (P1) — Verificar cada blíster por separado (E4)
Como verificador, verifico el blíster 1 y el blíster 2 de forma independiente, con un checklist de
cinco ítems obligatorios y motivo si verificador = elaborador.

**Prueba independiente**: verificar el blíster 1 de una sesión de dos y comprobar que el blíster 2
no cambia de estado (CA-605).

### US4 (P1) — Entregar juntos o por separado (E5)
Como quien entrega, registro una entrega que cubre uno o los dos blísteres de un paciente, con los
mismos datos de entrega si se entregan juntos.

**Prueba independiente**: entregar los dos blísteres verificados de una sesión a la vez (CA-606) y,
en otro caso, entregar solo el primero dejando el segundo disponible (CA-607).

### US5 (P2) — Continuidad sin repetir datos (E6)
Como farmacéutico, con un paciente cuyo tratamiento no ha cambiado, "preparar siguiente" me abre
la hoja ya rellena y solo pide la lectura ambiental.

**Prueba independiente**: preparar la sesión siguiente de un paciente sin cambios y verificar que
solo se pide temperatura/humedad para poder pasar a PREPARADO (CA-608, CA-608b); con una línea
modificada, verificar que solo esa línea exige revisión (CA-609); con un envase agotado sin
sustituto, verificar que solo esa línea queda `ENVASE_PENDIENTE` (CA-610).

### US6 (P3) — Reelaborar antes de entregar (§4.12)
Como farmacéutico, si el paciente, un familiar o el médico piden un cambio de medicación antes de
la recogida, reelaboro el blíster sin tirarlo: mismo número de registro, versión siguiente,
historial íntegro de lo anterior.

**Prueba independiente**: reelaborar un SPD verificado añadiendo un medicamento y verificar que
conserva su número de registro, sube de versión y vuelve a PREPARADO (CA-6120); verificar que una
línea sin cambios no mueve envases (CA-6121) y que una línea eliminada devuelve unidades
(CA-6123).

## 4. Requisitos funcionales

### 4.1 Sesión de preparación

- **FR-600** Una **sesión de preparación** es un contexto de pantalla, no una entidad persistida
  más allá de agrupar los SPD que crea. Se abre para un paciente y genera 1 o 2 SPD según
  `paciente.n_blisteres` (Spec 005 FR-501).
- **FR-601** Cada SPD generado en la sesión tiene: número de registro propio y correlativo (p. ej.
  000041 y 000042), `validez_desde`/`validez_hasta` de 7 días exactos, consecutivos entre sí (el
  segundo empieza el día siguiente al fin del primero).
- **FR-602** Precondiciones para abrir sesión (iguales para todos los blísteres que genere):
  paciente ACTIVO, consentimiento vigente, idoneidad APTO, al menos un tratamiento activo
  `en_spd = 1`, y **sin faltantes pendientes** en el listado de retirada (Spec 005) para ese
  paciente — si los hay, el sistema no crea la sesión y enlaza al listado.
- **FR-603** La pantalla de sesión muestra los blísteres en pestañas o columnas paralelas. Los
  siguientes datos son **compartidos** y se introducen una vez para toda la sesión: lectura
  ambiental (FR-630), material de acondicionamiento por blíster (puede diferir si cambia de lote
  entre uno y otro, pero se prerrellena igual en los dos). El elaborador se prerrellena igual en
  ambos blísteres y es editable por separado si, por ejemplo, cambia el turno a media sesión.
- **FR-604** Cada blíster mantiene sus propias líneas, checklist de verificación, entrega y
  estado. No existe combinación de datos legales entre blísteres: son dos SPD independientes a
  efectos de documento.

### 4.2 Líneas y envases (multi-envase explícito)

- **FR-610** Al generar cada SPD de la sesión se crea una línea por tratamiento activo
  `en_spd = 1`, con instantánea de nombre, CN, pauta, días, momento y descripción física. Se
  calculan dos valores distintos (data-model.md §SPD_Linea): `unidades_dosis` (suma semanal real
  de tomas del paciente, puede ser fraccionaria, es la que aparece en etiqueta e instrucciones) y
  `unidades_envase` (unidades enteras a descontar del envase según la regla de Spec 005 FR-522,
  que nunca coincide con `unidades_dosis` cuando hay fracciones).
- **FR-611** Si las unidades de una línea no caben en un único envase con saldo suficiente, el
  sistema añade **una fila de envase adicional bajo la misma línea**, con su propia serie, lote y
  caducidad, repartiendo las unidades según la regla de consumo de Spec 005 FR-520 (se agota
  primero el envase con menos unidades restantes). La ficha y la etiqueta imprimen tantas filas de
  ese medicamento como envases haya intervenido.
- **FR-612** Si no hay saldo suficiente en ningún envase para completar una línea, el sistema no
  permite pasar el blíster a PREPARADO y remite al listado de retirada / al alta rápida de envase
  (FR-620), indicando medicamento, unidades que faltan.
- **FR-614** El elaborador puede excluir una línea de un blíster concreto (no de toda la sesión)
  con motivo; queda registrada como excluida, no eliminada.

### 4.3 Alta de envase desde la preparación

- **FR-620** Desde cualquier línea con saldo insuficiente, el botón "Registrar envase" abre el
  alta de Spec 005 FR-510 prerrellenada con paciente y medicamento, tanto si el tratamiento ya
  existía como si es nuevo (creado por FR-621). El usuario solo rellena los campos que el sistema
  no puede deducir: serie, lote, caducidad y, si el catálogo no lo trae, unidades iniciales. Al
  guardar, el envase pasa a custodia, la línea recalcula el reparto automáticamente y la pantalla
  de preparación no se recarga ni pierde lo ya introducido en otras líneas.
- **FR-621** Si el medicamento no tiene tratamiento activo para ese paciente (caso de un
  medicamento nuevo detectado durante la propia preparación), "Registrar envase" ofrece primero un
  alta mínima de tratamiento (medicamento + posología) antes de pedir los datos del envase, sin
  salir de la pantalla de preparación.

### 4.4 Condiciones ambientales y material

- **FR-630** La sesión muestra la última lectura ambiental. Si tiene menos de N horas
  (configurable, por defecto 2, `Farmacia.UmbralReutilizacionLecturaAmbientalHoras` ya existente
  desde Spec 000) se reutiliza con un clic; si no, se introduce una nueva, que se guarda una sola
  vez y se enlaza a todos los SPD de la sesión.
- **FR-631** Lectura fuera de rango (`Farmacia.TempMin/TempMax/HrMin/HrMax`, ya existentes): aviso,
  no bloquea.
- **FR-632** Material de acondicionamiento: selector por blíster con el último lote usado
  preseleccionado. Obligatorio por blíster antes de pasar a PREPARADO.

### 4.5 Llenado asistido

- **FR-640** Vista de llenado por blíster: cuadrícula 7×4, medicamento a medicamento. Cuando una
  línea tiene más de una fila de envase, la cuadrícula indica qué unidades proceden de cada envase
  (p. ej. mediante color o etiqueta discreta), aunque la casilla del alveolo sea una sola.
- **FR-641** Casilla "colocado" por línea; aviso si queda alguna sin marcar al intentar pasar de
  estado, sin bloquear.

### 4.6 Verificación

- **FR-650** Por blíster, no por sesión. Precondición: ese SPD en PREPARADO.
- **FR-651** Checklist de cinco ítems: integridad del blíster; datos de la etiqueta coinciden con
  la ficha; periodo de validez en la etiqueta; hoja de instrucciones preparada; cada alveolo
  contiene lo que le corresponde. Los cinco obligatorios, por blíster.
  > **Corrección 2026-09-06 (el PNT manda, Art. I.1)**: el Anexo I.G del PNT I (COF A Coruña,
  > Decreto 87/2022; ver `docs/analisis-resources.md` §2.8) fija **ocho** preguntas SÍ/NO, no
  > cinco. Se añaden: instrucciones del fabricante y PNT seguidas; etiqueta coincide con la ficha
  > del paciente a fecha de hoy; trazabilidad envase original → DDP. Las ocho obligatorias
  > (`ChecklistVerificacion`, migración 0010). La app pre-marca trazabilidad (garantizada por
  > construcción) y hoja de instrucciones (si consta generada); el verificador puede desmarcarlas.
- **FR-652** Verificador = elaborador de ese blíster ⇒ motivo de excepción obligatorio (≥10
  caracteres), registrado en la ficha y en auditoría.
- **FR-653** El resultado y el checklist se guardan por blíster; verificar el blíster 1 no verifica
  el 2.

### 4.7 Entrega

- **FR-660** La entrega se registra **por sesión** cuando los blísteres se entregan juntos (caso
  habitual), o por blíster si se entregan en momentos distintos (el paciente recoge el primero y
  vuelve luego por el segundo). La pantalla ofrece "entregar todos los blísteres pendientes de
  este paciente" preseleccionados; se pueden deseleccionar.
- **FR-661** Campos por entrega: fecha, entregado a, SPD anterior recogido vacío, unidades no
  administradas por línea del SPD anterior, observaciones de adherencia, pregunta por cambios de
  medicación, checklist de primera entrega si aplica. Si se entregan dos blísteres a la vez, estos
  datos se introducen una sola vez y se copian a los dos SPD.
- **FR-662** El documento de entrega (si se imprime resumen) identifica ambos números de registro
  cuando la entrega cubre dos blísteres.
- **FR-663** Cambios de medicación referidos ⇒ tratamiento "pendiente de revisión", bloquea la
  siguiente sesión de preparación hasta que se retire.

### 4.8 Sesión siguiente (continuidad)

- **FR-670** Acción "Preparar siguiente" sobre el paciente, disponible cuando todos los SPD de su
  última sesión están en ENTREGADO. Crea una nueva sesión con el mismo número de blísteres que la
  anterior (editable si `paciente.n_blisteres` cambió entre tanto).
- **FR-671** Cada nuevo SPD se genera **copiando la hoja anterior correspondiente**: mismas líneas
  con la misma posología, mismo material. Los envases se reasignan según saldo actual (Spec 005
  FR-520): si el mismo envase sigue con saldo, continúa usándose sin que el usuario haga nada.
- **FR-672** Regla general de continuidad: si para cada línea copiada existen en custodia el
  envase o los envases necesarios para cubrir sus unidades (una fila de envase o varias,
  indistintamente — FR-611), y el tratamiento activo no ha cambiado respecto a la sesión anterior,
  **la única entrada que el sistema pide es la lectura ambiental** (FR-630) para poder pasar a
  PREPARADO. No se piden de nuevo posología, descripción física, serie, lote ni caducidad: ya
  están en la línea copiada y en los envases de custodia.
- **FR-673** Si hay diferencia, la línea afectada se marca y exige revisión solo en esa línea; el
  resto no pide nada:
  - `MODIFICADA` — pauta, días o momento distintos del tratamiento activo.
  - `NUEVA` — tratamiento activo sin línea equivalente en la sesión anterior.
  - `ELIMINADA` — línea de la sesión anterior sin tratamiento activo correspondiente; se muestra en
    una lista aparte, no bloquea.
  - `ENVASE_PENDIENTE` — la línea no ha cambiado, pero el envase que la cubría se agotó y no hay
    otro con saldo suficiente en custodia; se resuelve con "Registrar envase" (FR-620) sin salir
    de la pantalla.
- **FR-674** Si el tratamiento está "pendiente de revisión" (FR-663 o Spec 004), no se puede abrir
  la sesión siguiente hasta que un farmacéutico la retire.

### 4.9 Impresión

- **FR-680** Cada blíster imprime su propia ficha de preparación-control-entrega, su etiqueta
  anverso, su etiqueta reverso y su hoja de instrucciones. Con dos blísteres, son dos juegos
  completos de documentos. *(Diferido a Spec 007: aquí solo se expone la acción y se registra el
  timestamp — ver Alcance de esta iteración.)*
- **FR-681** Documentos generados desde instantáneas de línea (incluidas las filas de envase
  múltiple) y nunca desde el catálogo actual. Cada impresión registra fecha-hora y usuario en su
  SPD.
- **FR-682** La hoja de instrucciones al paciente puede imprimirse una sola vez por sesión si el
  contenido es idéntico para ambos blísteres; el sistema lo detecta y ofrece "imprimir una sola
  hoja para los dos blísteres" o una por blíster. Resuelto en Clarifications: una por blíster
  hasta confirmar.

### 4.10 Listados y avisos

- **FR-690** Pantalla "Preparaciones" con filtro por sesión (agrupa los blísteres de un mismo
  paciente y misma fecha de creación), paciente, elaborador, estado.
- **FR-691** Avisos de inicio: pacientes con faltantes en el listado de retirada antes de su
  próxima sesión (enlaza a Spec 005); SPD verificados sin entregar con validez ya iniciada;
  sesiones a medias (un blíster ENTREGADO y el otro no, más de 3 días).

### 4.11 Auditoría

- **FR-695** Cada transición de estado, alta de envase desde preparación, verificación, entrega e
  impresión deja traza por blíster (SPD_id) en auditoría.

### 4.12 Reelaboración antes de entregar (reemblistado)

- **FR-6120** Acción "Reelaborar" disponible sobre un SPD en estado PREPARADO o VERIFICADO (nunca
  ENTREGADO ni ANULADO). Cubre el caso de un cambio de medicación pedido por el paciente, un
  familiar o el médico entre la preparación y la recogida. El blíster físico no se tira: se reabre
  y se rellena de nuevo con los cambios.
- **FR-6121** Al reelaborar, el sistema exige: origen de la solicitud (`PACIENTE` / `FAMILIAR` /
  `MEDICO` / `FARMACEUTICO` / `OTRO`) y motivo en texto libre. Se guarda una fila en
  `SPD_Modificacion` con una copia íntegra de las líneas y filas de envase anteriores
  (Constitución Artículo III.4): nada se pierde, todo queda reconstruible.
- **FR-6122** El SPD conserva su **mismo número de registro**; se incrementa `version` (1 → 2 →
  …). No se crea un nuevo SPD ni se anula el existente: es el mismo documento, en su siguiente
  versión.
- **FR-6123** El estado vuelve a PREPARADO (si estaba VERIFICADO, la verificación anterior queda
  archivada en `SPD_Verificacion` con su fecha; el checklist se limpia y hay que verificar de
  nuevo antes de poder entregar).
- **FR-6124** Recalculo de envases al reelaborar, línea a línea, comparando la versión anterior
  con la nueva:
  - Línea sin cambios: ningún movimiento de envase. Las unidades ya salieron de custodia con la
    versión anterior y siguen físicamente en el blíster.
  - Línea con más unidades que antes (dosis aumentada o día añadido): se consume solo la
    **diferencia** de envases en custodia, con la misma regla de asignación de Spec 005 (se agota
    primero el envase con menos saldo). Si no hay saldo, se ofrece "Registrar envase" sin salir de
    la pantalla (FR-620/FR-621).
  - Línea con menos unidades que antes (dosis reducida) o eliminada: la diferencia de unidades
    **vuelve a `unidades_restantes`** del envase o envases de los que salió (Spec 005), porque
    esas unidades concretas no llegaron a usarse en el blíster final y no hay razón para tratarlas
    como residuo.
  - Línea nueva (medicamento que no estaba en la versión anterior): se trata como un alta normal
    de línea (FR-610–FR-612), consumiendo o pidiendo envase según corresponda.
- **FR-6125** Las impresiones de la versión anterior quedan marcadas como obsoletas en la pantalla
  (no se destruyen: siguen en `impreso_ficha_en`, etc., de esa versión archivada) y el sistema
  exige reimprimir ficha, etiquetas e instrucciones de la nueva versión antes de poder entregar.
- **FR-6126** Un SPD puede reelaborarse más de una vez mientras no se entregue; cada vez incrementa
  `version` y añade una fila a `SPD_Modificacion`.
- **FR-6127** Reelaborar un SPD que forma parte de una sesión con dos blísteres afecta solo al
  blíster indicado; el otro no se toca.

## 5. Entidades clave

| Entidad | Notas |
|---|---|
| SPD | docs/data-model.md §SPD. Siempre "un blíster = un SPD" |
| SPD_Linea | docs/data-model.md §SPD_Linea |
| SPD_Linea_Envase | 1..n filas por línea con normalidad; cada fila lleva serie/lote/caducidad copiados del envase en el momento del descuento |
| SPD_Verificacion | Siempre por SPD (blíster), nunca por sesión |
| SPD_Modificacion | Historial de reelaboraciones, ver data-model.md |
| RegistroAmbiental | Nueva en esta spec (ver Alcance de esta iteración) |
| MaterialAcondicionamiento | Nueva en esta spec (catálogo simple, ver Alcance de esta iteración) |
| Sesión | No es una tabla de negocio; es un agrupador calculado (`sesion_id` GUID en SPD) |

## 6. Criterios de aceptación

**CA-600 Dos blísteres, dos hojas**
Dado un paciente con `n_blisteres = 2`, cuando abro "Nueva sesión de preparación", entonces se
crean dos SPD con números correlativos y validez consecutiva de 7 días cada uno.

**CA-601 Faltantes bloquean la sesión**
Dado un paciente con faltantes pendientes en el listado de retirada, cuando intento abrir sesión,
entonces el sistema no crea nada y enlaza al listado de retirada de ese paciente.

**CA-602 Envase repartido en dos filas**
Dado un medicamento con envase A (3 uds) y envase B (28 uds) y una línea de 7 uds, cuando se
genera el blíster, entonces la línea muestra dos filas: una con serie/lote/caducidad de A y 3
uds, otra con los de B y 4 uds; la etiqueta reverso imprime ambas.

**CA-603 Sin saldo, no se puede preparar**
Dado una línea sin saldo suficiente en ningún envase, cuando intento pasar el blíster a
PREPARADO, entonces el sistema lo impide, indica el medicamento y las unidades que faltan, y
ofrece "Registrar envase".

**CA-604 Alta de envase sin salir de la preparación**
Dado el aviso de CA-603, cuando registro un envase nuevo desde ese botón, entonces la línea
recalcula el reparto y el blíster puede pasar a PREPARADO sin recargar la pantalla desde otra
parte.

**CA-605 Verificación independiente**
Dado una sesión con dos blísteres, cuando verifico el blíster 1 con los cinco ítems APTO,
entonces el blíster 1 pasa a VERIFICADO y el blíster 2 permanece en su estado anterior.

**CA-606 Entrega conjunta**
Dado dos blísteres VERIFICADOS del mismo paciente, cuando registro una entrega marcando ambos,
entonces los dos pasan a ENTREGADO con los mismos datos de entrega y el documento resultante
lista ambos números de registro.

**CA-607 Entrega parcial**
Dado dos blísteres VERIFICADOS, cuando entrego solo el primero, entonces el primero pasa a
ENTREGADO y el segundo permanece VERIFICADO, disponible para entregarse después.

**CA-608 Continuidad sin cambios, una fila de envase**
Dado un paciente cuyo tratamiento no ha cambiado desde su última sesión entregada y cuyos
envases tienen saldo suficiente, cuando pulso "Preparar siguiente" e introduzco la temperatura y
humedad, entonces puedo pasar directamente a PREPARADO sin que el sistema pida nada más.

**CA-608b Continuidad sin cambios, dos filas de envase**
Dado un paciente en la misma situación que CA-608 pero con una línea que se cubre con dos envases
distintos (dos filas), cuando pulso "Preparar siguiente" e introduzco la temperatura y humedad,
entonces el sistema resuelve ambas filas de envase automáticamente desde la custodia y puedo
pasar a PREPARADO sin introducir serie, lote ni caducidad de ninguna de las dos.

**CA-609 Continuidad con cambio parcial**
Dado un paciente con una línea de posología modificada desde la última sesión, cuando pulso
"Preparar siguiente", entonces solo esa línea aparece marcada MODIFICADA exigiendo revisión; las
demás líneas están listas sin intervención.

**CA-610 Envase agotado en continuidad**
Dado un envase que se agotó en la sesión anterior sin sustituto en custodia, cuando genero la
sesión siguiente, entonces esa línea aparece como ENVASE_PENDIENTE y bloquea solo esa línea, no
las demás.

**CA-611 Sesión no crea entidad de negocio espuria**
Dado que reviso el modelo de datos, cuando busco la sesión como entidad, entonces no existe una
tabla de negocio "Sesión" con su propio ciclo de vida; solo un agrupador técnico sobre SPD.

**CA-6120 Reelaborar conserva número, sube versión**
Dado un SPD VERIFICADO con num_registro 000041 y version 1, cuando lo reelaboro añadiendo un
medicamento, entonces sigue siendo 000041, version pasa a 2, y el estado vuelve a PREPARADO.

**CA-6121 Línea sin cambios no mueve envase**
Dado un SPD reelaborado donde una línea no cambia, cuando se guarda la reelaboración, entonces
las unidades restantes de su envase no varían.

**CA-6122 Línea aumentada consume solo la diferencia**
Dado una línea que pasó de 7 a 10 unidades y un envase con saldo suficiente, cuando se guarda la
reelaboración, entonces el envase descuenta 3 unidades adicionales, no 10.

**CA-6123 Línea eliminada devuelve unidades**
Dado una línea de 7 unidades que se elimina en la reelaboración, cuando se guarda, entonces las 7
unidades vuelven a `unidades_restantes` del envase del que salieron.

**CA-6124 Historial íntegro**
Dado un SPD reelaborado, cuando consulto `SPD_Modificacion`, entonces encuentro una copia completa
de las líneas y envases de la versión 1, el motivo y quién lo solicitó.

**CA-6125 No se puede reelaborar tras entrega**
Dado un SPD en estado ENTREGADO, cuando busco la acción "Reelaborar", entonces no está disponible.

**CA-6126 Exige nueva verificación**
Dado un SPD VERIFICADO reelaborado, cuando reviso su estado, entonces está en PREPARADO y no puede
entregarse hasta verificarse de nuevo.

## 7. Casos límite

- Paciente que cambia de `n_blisteres = 2` a `1` entre sesiones: la sesión siguiente genera un
  único SPD; los envases con saldo siguen disponibles para la próxima.
- Un blíster de la sesión se anula (defecto de fabricación) y el otro no: se anula solo ese SPD
  (Artículo III, no se borra); el otro sigue su ciclo normal; el listado de retirada no vuelve a
  pedir el medicamento salvo que haga falta un envase adicional para reponer el anulado.
- Elaborador distinto en cada blíster de la misma sesión (cambio de turno a media preparación):
  permitido, FR-603 solo prerrellena, no obliga a que coincidan.
- Instrucciones al paciente para 14 días en una sola hoja (FR-682): pendiente de confirmación
  normativa; hasta entonces se imprime una por blíster por defecto.

## 8. Fuera de alcance de esta spec

- Diseño de los documentos impresos (Spec 007).
- Cálculo del listado de retirada (Spec 005; aquí solo se consume su resultado como precondición).
- Registro de limpieza como registro general (Spec 009); el registro ambiental sí se construye
  aquí (ver Alcance de esta iteración).
- Comunicación de incidencias al médico (Spec 008; ya implementada, se consume su punto de
  extensión `PrepararDesdeAvisoCambioReferido` para FR-663 si se decide enganchar la UI).
- Idoneidad y consentimiento (Spec 002): la comprobación real de sus criterios, no el punto de
  extensión que la sustituye en esta iteración.

## 9. Assumptions

- FR-602 (precondición de idoneidad/consentimiento) se implementa contra
  `IComprobadorIdoneidadYConsentimiento`, con `ComprobadorIdoneidadYConsentimientoNulo` como
  implementación por defecto que siempre aprueba — research.md Decisión 1. La regla constitucional
  (Art. I.3) queda con test unitario sobre la interfaz, satisfaciendo la exigencia de "regla
  implementada en Dominio con test unitario" sin fabricar los criterios clínicos de Spec 002.
- `RegistroAmbiental` y `MaterialAcondicionamiento` se crean en esta spec porque Spec 009
  explícitamente las retiró de su propio alcance a favor de esta (nota de alcance ya presente en
  `0004_registros_calidad.sql`).
- FR-680/681 (impresión real) se implementan como acción que registra el timestamp
  `impreso_*_en` y audita, sin generar ningún fichero — Spec 007 construirá el motor real sobre
  este mismo punto de extensión.
