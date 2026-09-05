# Feature Specification: Catálogo de medicamentos

**Feature Branch**: `003-catalogo-medicamentos`

**Created**: 2026-09-05

**Status**: Draft — pendiente de `/speckit-clarify`

**Constitución aplicable:** 2.1.0 (Artículos IV, V, VI — la spec original cita 2.1.0, sin contradicción con la constitución vigente)

**Depende de:** Ninguna funcional; Spec 000 (configuración inicial) para la URL del nomenclátor

**Requerida por:** Spec 004 (tratamiento), 005 (depósito), 006 (preparación)

**Input**: Especificación completa aportada literalmente por el propietario del producto (`spec-003-catalogo-medicamentos.md`, v0.1 — 2026-09-04). Se traslada tal cual: no se reinterpretan los requisitos, no se añaden requisitos nuevos, no se cambia la numeración FR-3xx ni los criterios de aceptación.

---

## Clarifications

### Session 2026-09-05

- Q: ¿Necesitas ampliar la lista cerrada de formas farmacéuticas de FR-300, o basta con la lista ya propuesta? → A: Se mantiene la lista tal cual (comprimido, comprimido de liberación prolongada, cápsula, cápsula de liberación prolongada, gragea, pastilla, píldora, otra no apta para SPD), sin añadir valores ahora.

---

## 1. Propósito

Mantener el catálogo de medicamentos por CN, con su descripción física reutilizable y editable en cualquier momento (Constitución Artículo IV), y la importación opcional de un nomenclátor de referencia.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Alta, edición, consulta del catálogo |

## 3. Escenarios de usuario

### E1 — Alta rápida al crear un tratamiento
Como elaborador, al dar de alta un tratamiento y no encontrar el medicamento, quiero crearlo con lo mínimo (CN, nombre, forma) sin salir de la pantalla, y completar la descripción física más tarde si hace falta.

### E2 — Completar la descripción física la primera vez que hace falta
Como elaborador, si preparo un blíster con un medicamento sin descripción física, quiero poder rellenarla ahí mismo y que quede guardada en el catálogo para todos los pacientes futuros.

### E3 — Corregir un dato del catálogo
Como elaborador, si el color de un genérico cambia de lote a lote del laboratorio, quiero poder cambiar la descripción física del CN sin que eso reescriba las etiquetas ya impresas de blísteres anteriores.

### E4 — Importar el nomenclátor
Como administrador, quiero descargar el nomenclátor desde la URL configurada (Spec 000) para tener nombres y CN de referencia, sabiendo que no sustituye la revisión manual porque el nomenclátor no siempre es fiable para todo.

## 4. Requisitos funcionales

### 4.1 Alta y edición

- **FR-300** Campos: CN (6 dígitos, único), nombre, principio activo (opcional), laboratorio (opcional), forma farmacéutica (catálogo cerrado: comprimido, comprimido de liberación prolongada, cápsula, cápsula de liberación prolongada, gragea, pastilla, píldora, otra no apta para SPD), fraccionable (sí/no), unidades por envase, GTIN (opcional, para Spec 012).
- **FR-301** `apto_spd` se deriva de la forma farmacéutica (formas líquidas, efervescentes, bucodispersables, parenterales, tópicas quedan fuera por defecto) pero es **editable manualmente** con motivo obligatorio (`motivo_no_apto` o razón de excepción), porque la aptitud real depende también de informes del laboratorio que el sistema no puede conocer.
- **FR-302** Alta mínima: solo CN y nombre son obligatorios para crear el registro (permite el flujo de alta rápida del Escenario E1); forma farmacéutica y descripción física pueden completarse después, con aviso visible mientras falten.
- **FR-303** Descripción física: forma, color, ranura, serigrafía, tamaño, más un campo de texto libre autogenerado a partir de los anteriores y editable a mano. Editar cualquiera de estos campos actualiza el catálogo para todo uso futuro (Artículo IV.2); no afecta a instantáneas ya congeladas en líneas de SPD existentes (Artículo IV.3).
- **FR-304** Cada cambio de descripción física añade una fila a `Medicamento_Hist` con la fecha de vigencia, sin borrar el estado anterior.
- **FR-305** Búsqueda por CN exacto o por fragmento de nombre, sin distinguir mayúsculas ni tildes.
- **FR-306** No se permite duplicar un CN. Un CN dado de baja (`activo = 0`) puede reactivarse si vuelve a comercializarse; no se crea uno nuevo con el mismo CN.

### 4.2 Unidades por envase

- **FR-310** `unidades_envase` es editable manualmente en cualquier momento (Spec 005 FR-513 depende de este valor). Puede rellenarse a mano o mediante la regla de extracción del nomenclátor (Spec 011).
- **FR-311** `unidades_envase_origen` registra si el valor viene de `MANUAL` o `IMPORTADO_REGEX`, solo a título informativo — no bloquea la edición manual en ningún caso.

### 4.3 Importación del nomenclátor

- **FR-320** Desde Configuración (Spec 000 FR-051) se descarga el fichero del nomenclátor. Esta spec define qué se hace con ese fichero una vez descargado: se ofrece una pantalla de revisión que compara el nomenclátor con el catálogo actual y muestra, por CN, qué medicamentos son nuevos, cuáles tienen nombre distinto al registrado, y (si el perfil de importación lo mapea, Spec 011) qué valor de `unidades_envase` se extraería por regex.
- **FR-321** La importación **nunca sobrescribe automáticamente** la descripción física ni la aptitud SPD de un medicamento existente — esos campos son exclusivamente de responsabilidad manual del profesional (Artículo I.2 y I.3: aptitud SPD y descripción son decisiones clínicas, no datos de nomenclátor). Solo puede rellenar `unidades_envase` cuando está vacío, o actualizarlo si el usuario lo confirma explícitamente fila a fila.
- **FR-322** El usuario decide, medicamento a medicamento, si acepta la creación o el dato propuesto; no hay una importación masiva sin revisión para los campos sensibles.

## 5. Entidades clave

| Entidad | Descripción | Referencia |
|---|---|---|
| Medicamento | Catálogo por CN, con descripción física editable | [docs/data-model.md](../../docs/data-model.md) §Medicamento |
| Medicamento_Hist | Historial de descripción física (no borra, versiona) | [docs/data-model.md](../../docs/data-model.md) §Medicamento_Hist |

## 6. Criterios de aceptación

**CA-300 Alta mínima**
Dado que solo introduzco CN y nombre, cuando guardo, entonces el medicamento se crea y aparece marcado "descripción física pendiente" en cualquier pantalla que lo use.

**CA-301 Edición no reescribe instantáneas**
Dado un medicamento usado en un SPD entregado con descripción "comprimido blanco", cuando cambio la descripción a "comprimido amarillo", entonces el SPD entregado sigue mostrando "comprimido blanco" en su consulta e impresión.

**CA-302 Aptitud editable con motivo**
Dado un medicamento cuya forma farmacéutica lo marca como apto por defecto, cuando lo marco como no apto, entonces el sistema exige un motivo antes de guardar.

**CA-303 CN duplicado bloqueado**
Dado un medicamento con CN 654321, cuando intento crear otro con el mismo CN, entonces el sistema lo impide y muestra el existente.

**CA-304 Importación no sobrescribe descripción física**
Dado un medicamento con descripción física ya completa, cuando se importa un nomenclátor que lo incluye, entonces la descripción física no cambia sin confirmación explícita.

**CA-305 Reactivación de CN dado de baja**
Dado un medicamento con CN 111111 dado de baja, cuando se vuelve a necesitar, entonces se reactiva el registro existente en vez de crear uno duplicado.

## 7. Casos límite

- Medicamento con varias presentaciones (20 mg y 40 mg) del mismo principio activo: son CN distintos, catálogos independientes; no hay relación automática entre ellos salvo el campo opcional `principio_activo` para búsquedas.
- Medicamento fraccionable a la mitad pero no a cuartos: `fraccionable` es un booleano simple en 1.0; fraccionamientos parciales por denominador se dejan para una versión posterior si hiciera falta.
- Nomenclátor con un CN que ya no es apto SPD según el fabricante: la importación no cambia `apto_spd` (FR-321); es responsabilidad profesional revisarlo.

## 8. Fuera de alcance de esta spec

- Descarga del fichero en sí y perfiles de mapeo columna a columna (Spec 011).
- Lectura de GTIN por escáner (Spec 012); aquí el campo `gtin` solo se almacena.

## 9. Preguntas abiertas

Ninguna pendiente — Q1 resuelta, ver sección "Clarifications" al inicio del documento.

## Assumptions

- Todos los campos y reglas no marcados `[NEEDS CLARIFICATION]` se toman literalmente de la especificación original del propietario del producto, sin inferencias adicionales.
- Los criterios de aceptación (§6, formato Dado/Cuando/Entonces) son los que exige el Artículo IX.2 de la constitución y se usan tal cual como base de los tests de `/speckit-tasks`.
- FR-320/FR-321/FR-322 (importación del nomenclátor) asumen que la descarga del fichero (Spec 011) y el servicio de descarga ya existente de Spec 000 (`IServicioNomenclator`) son la única fuente de datos externos; esta spec solo define la pantalla de revisión y las reglas de qué se sobrescribe.
