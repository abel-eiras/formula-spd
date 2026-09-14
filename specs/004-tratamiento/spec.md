# Feature Specification: Tratamiento del paciente

**Feature Branch**: `004-tratamiento`

**Created**: 2026-09-06

**Status**: Draft — pendiente de `/speckit-clarify`

**Constitución aplicable:** 2.1.0 (Artículos I.3, IV, V, XI)

**Depende de:** Spec 001 (pacientes, médicos), Spec 003 (catálogo de medicamentos) — ambas ya
mergeadas en `main`.

**Requerida por:** Spec 005 (depósito), 006 (preparación)

**Input**: Especificación completa aportada literalmente por el propietario del producto
(`spec-004-tratamiento.md`, v0.1 — 2026-09-04). Se traslada tal cual: no se reinterpretan los
requisitos, no se añaden requisitos nuevos, no se cambia la numeración FR-4xx ni los criterios de
aceptación.

---

## Clarifications

### Session 2026-09-06

- Q: FR-410 — ¿un cambio de vía o de médico prescriptor (sin cambiar posología) también debe
  cerrar/abrir fila, o basta con editar en el sitio? → A: Cierra/abre fila igual que la posología
  (la propuesta por defecto de la propia spec, sin alternativa razonable mejor): cualquier dato que
  aparecería distinto en una instantánea de SPD pasada debe quedar congelado correctamente
  (Art. IV.3).

---

## 1. Propósito

Mantener la lista de medicamentos que toma cada paciente, dentro y fuera del blíster, con posología estructurada, de forma inmutable (Constitución Artículo IV.4: un cambio cierra la fila y abre otra), reutilizando medicamento y médico de los catálogos.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Alta, edición (mediante cierre y apertura), finalización de tratamientos |

## 3. Escenarios de usuario

### E1 — Añadir un tratamiento nuevo
Como elaborador, quiero buscar el medicamento por CN o nombre, que se prerrellene con los datos del catálogo, y solo tener que indicar la posología, el médico (ya prerrellenado con el de cabecera) y las fechas.

### E2 — Posología con fracciones exactas
Como elaborador, quiero indicar "medio comprimido" eligiendo de una lista de fracciones, no escribiendo un decimal, para que la etiqueta salga siempre exacta.

### E3 — Cambiar la pauta sin perder el historial
Como elaborador, cuando el médico cambia la dosis, quiero registrar el cambio y que el sistema conserve cuándo empezó y terminó la pauta anterior, sin tener que anotarlo yo a mano.

### E4 — Ajustar el redondeo de un tratamiento fraccionado
Como elaborador, si el cálculo automático de unidades a descontar del envase no encaja con la realidad de un paciente concreto, quiero poder fijar manualmente cuántas unidades se descuentan cada semana para ese tratamiento.

### E5 — Medicamento fuera del blíster
Como elaborador, quiero registrar un medicamento que el paciente toma pero que no va en el SPD (una crema, algo "a demanda"), con una pauta en texto libre si no encaja en D/A/C/N.

### E6 — Marcar un tratamiento para revisar
Como elaborador, cuando la entrega de un SPD (Spec 006) señala cambios de medicación referidos por el paciente, quiero ver el tratamiento marcado como pendiente de revisión hasta que lo confirme.

## 4. Requisitos funcionales

### 4.1 Alta y reutilización

- **FR-400** Campos: medicamento (selector sobre Spec 003 por nombre o CN, con alta rápida inline si no existe —CN y nombre, con «Consultar CIMA»— en un panel lateral, sin salir de la ficha; elegir uno de baja lo reactiva, Spec 003 CA-305), `en_spd` (sí/no), problema de salud/indicación, médico prescriptor (selector sobre Spec 001, prerrellenado con el médico de cabecera del paciente), pauta D/A/C/N, días de la semana, vía, momento de administración, fecha de inicio, fecha de fin, tipo (crónico/esporádico).
- **FR-401** Si `en_spd = 0`, la posología puede introducirse como texto libre (`pauta_texto`) en vez de D/A/C/N, para pautas que no encajan en cuatro tomas diarias (p. ej. "cada 12 horas", "a demanda si dolor").
- **FR-402** Dosis (D, A, C, N) **tecleada** y restringida al vocabulario cerrado de Spec 007 FR-741: `0, 1, 2, 3, 1/4, 1/2, 1/3, 3/4, 2/3, 1+1/2, 1+1/4, 1+1/3, 1+2/3, 1+3/4` (revisado el 2026-09-14; antes era una lista desplegable). Vacío es «sin toma». Cualquier otro texto —un decimal, «0,5»— se advierte mientras se escribe y no se guarda, con un mensaje que dice la toma, lo tecleado y los valores admitidos. Así la impresión en fracción (Spec 007) sigue siendo siempre exacta.
- **FR-403** Días de la semana: selector de checkboxes L-M-X-J-V-S-D, todos marcados por defecto.

### 4.2 Inmutabilidad

- **FR-410** Un tratamiento **no se edita en el sitio** cuando cambia algo clínicamente relevante (posología, días, momento, medicamento, vía, médico prescriptor — Clarifications 2026-09-06): la edición cierra la fila actual (`estado = FINALIZADO`, `fecha_fin` = hoy) y crea una fila nueva con `fecha_inicio` = hoy y los valores nuevos, enlazando `fecha_prescripcion_inicial` a la fila original si se desea conservar la fecha de la primera prescripción de ese medicamento para ese paciente.
- **FR-411** Campos que **sí** pueden editarse en el sitio sin cerrar la fila, por no ser clínicamente relevantes para la instantánea de un SPD: observaciones, incidencias, conocimiento del cumplimiento, `ajuste_unidades_manual`.
- **FR-412** El historial completo de un medicamento para un paciente es consultable en una vista de línea temporal, mostrando cada versión con sus fechas de vigencia.

### 4.3 Estados

- **FR-420** Estados: `ACTIVO`, `SUSPENDIDO` (pausa temporal sin finalizar, p. ej. mientras dura un ingreso), `FINALIZADO` (con `fecha_fin`), `PENDIENTE_REVISION`.
- **FR-421** `PENDIENTE_REVISION` se activa automáticamente desde Spec 006 (FR-663: cambios referidos en la entrega) y bloquea la creación de una nueva sesión de preparación para ese paciente (Spec 006 FR-674) hasta que un usuario lo revise y lo devuelva a `ACTIVO` o lo cierre con `FINALIZADO`. `[NEEDS CLARIFICATION: Spec 006 no existe todavía en esta rama — el disparador automático desde la entrega queda fuera de esta iteración; ver "Fuera de alcance"]`
- **FR-422** Finalizar un tratamiento con envases en custodia dispara la propuesta de salida a SIGRE (Spec 005 FR-541). `[NEEDS CLARIFICATION: Spec 005 (Envase) no existe todavía en esta rama — la propuesta de salida a SIGRE queda fuera de esta iteración; ver "Fuera de alcance"]`

### 4.4 Ajuste manual de unidades

- **FR-430** Campo `ajuste_unidades_manual` (Spec 005 FR-522), editable desde la ficha de tratamiento, con un botón "usar cálculo automático" que lo vuelve a poner a nulo. Se muestra junto al valor calculado por la fórmula para que el usuario vea la diferencia antes de decidir sobrescribir. `[NEEDS CLARIFICATION: el "valor calculado por la fórmula" es la fórmula de consumo de Spec 005, que no existe todavía en esta rama — se implementa el campo y el botón, sin el cálculo automático de referencia; ver "Fuera de alcance"]`

## 5. Entidades clave

| Entidad | Referencia |
|---|---|
| Tratamiento | [docs/data-model.md](../../docs/data-model.md) §Tratamiento, incluye `ajuste_unidades_manual` |

## 6. Criterios de aceptación

**CA-400 Prerrelleno de prescriptor**
Dado un paciente con médico de cabecera "Fernández Souto", cuando creo un tratamiento nuevo, entonces el campo prescriptor aparece prerrellenado con ese médico, editable.

**CA-401 Cambio de pauta cierra y abre**
Dado un tratamiento activo con pauta 1-0-0-0, cuando lo cambio a 1-0-1-0, entonces la fila original pasa a FINALIZADO con fecha_fin = hoy, y existe una fila nueva ACTIVO con fecha_inicio = hoy y la pauta nueva.

**CA-402 Historial consultable**
Dado un medicamento con tres versiones de pauta a lo largo del tiempo, cuando abro su línea temporal, entonces veo las tres con sus fechas de vigencia, ninguna oculta.

**CA-403 Dosis tecleada con vocabulario cerrado, no decimal libre**
Dado el campo de dosis de desayuno, cuando escribo `1+1/2` se guarda la fracción exacta; cuando escribo `0,5` se me avisa con los valores admitidos y el tratamiento no se guarda.

**CA-407 Alta del medicamento desde el tratamiento**
Dado un medicamento que no está en el catálogo, cuando en el tratamiento lo busco, no aparece y pulso «Nuevo medicamento…», entonces lo doy de alta con CN y nombre sin salir de la ficha, queda elegido y el tratamiento se guarda con él.

**CA-404 Pendiente de revisión bloquea preparación** ⚠️ DIFERIDO (Spec 006 no existe en esta rama; se deja el estado y su efecto de bloqueo consultable, no el disparo automático desde la entrega)
Dado un tratamiento en PENDIENTE_REVISION, cuando se intenta crear una sesión de preparación para ese paciente (Spec 006), entonces el sistema lo impide indicando el motivo.

**CA-405 Ajuste manual visible junto al cálculo** ⚠️ PARCIAL (sin el cálculo automático de referencia de Spec 005; ver FR-430)
Dado un tratamiento fraccionado cuyo cálculo automático da 4 unidades, cuando abro el campo de ajuste manual, entonces veo "calculado: 4" junto al campo donde puedo fijar otro valor.

**CA-406 Finalizar propone SIGRE** ⚠️ DIFERIDO (Spec 005/Envase no existe en esta rama)
Dado un tratamiento con un envase de 12 unidades en custodia, cuando lo finalizo, entonces el sistema muestra ese envase y propone la salida a SIGRE (Spec 005).

## 7. Casos límite

- Dos cambios de pauta el mismo día: se permiten varias filas con la misma `fecha_inicio`/`fecha_fin` de un día si hace falta corregir un error; el orden se distingue por `creado_en`.
- Tratamiento suspendido que se reactiva sin cambios: pasa de SUSPENDIDO a ACTIVO sin cerrar/abrir fila, porque no hay cambio clínico, solo una pausa administrativa.
- Medicamento con posología distinta en días distintos más allá de "días de la semana" (p. ej. dosis decreciente día a día): fuera de alcance de 1.0, según lo acordado en la Fase 1 (pautas complejas quedan para una versión posterior); se registra como `pauta_texto` si no encaja en D/A/C/N con días.

## 8. Fuera de alcance de esta spec

- Cálculo de unidades a descontar del envase (Spec 005 FR-522, que consume estos datos) — no existe todavía en esta rama.
- Importación de tratamientos por copiar/pegar o fichero (Spec 005 §4.8).
- Comunicación de incidencias al médico (Spec 008).
- Disparo automático de `PENDIENTE_REVISION` desde la entrega de un SPD (Spec 006, FR-421) — el estado existe y su efecto de bloqueo es consultable/asignable manualmente, pero el disparador automático se implementa cuando exista Spec 006.
- Propuesta de salida a SIGRE al finalizar con envases en custodia (FR-422) — Spec 005/Envase no existe todavía en esta rama.
- Valor de referencia "calculado automáticamente" junto al ajuste manual (FR-430) — depende de la fórmula de consumo de Spec 005.

## 9. Preguntas abiertas

Ninguna pendiente — Q1 resuelta, ver sección "Clarifications" al inicio del documento. Las
dependencias de Spec 005/006 (FR-421, FR-422, FR-430) quedan documentadas como diferidas, no como
preguntas abiertas: su respuesta ya se conoce (se implementarán cuando existan esas specs), no
requieren decisión del usuario.

## Assumptions

- El vocabulario cerrado de fracciones (FR-402) se implementó con los valores enumerados en esta
  spec (`0, 1/4, 1/3, 1/2, 2/3, 3/4, 1, 1 1/4, 1 1/2`) y el 2026-09-14 el propietario lo amplió a
  catorce (`2, 3, 1+1/3, 1+2/3, 1+3/4`). Los valores se guardan por nombre, así que solo se añaden,
  nunca se renombran; la suma semanal se hace en doceavos para que tres tercios sumen exactamente uno.
- FR-421/FR-422/FR-430 se implementan hasta donde sus datos y estados lo permiten sin Spec 005/006
  (campos, estados, UI), documentando explícitamente el punto de extensión donde esas specs
  futuras engancharán su lógica, sin inventar una versión provisional de esa lógica.
