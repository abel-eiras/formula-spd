# Feature Specification: Pacientes, contactos y catálogo de médicos

**Feature Branch**: `001-pacientes-y-medicos`

**Created**: 2026-09-05

**Status**: Draft — pendiente de `/speckit-clarify`

**Constitución aplicable:** 2.1.0 (Artículos III, IV, V, VII — la spec original cita 1.0.0, la constitución vigente es 2.1.0 tras las enmiendas ya registradas; no hay contradicción de contenido en los artículos citados)

**Depende de:** Spec 000 (configuración inicial y usuarios) — ya implementada

**Requerida por:** Spec 002 (idoneidad y consentimiento), 004 (tratamiento), 006 (preparación)

**Input**: Especificación completa aportada literalmente por el propietario del producto (`spec-001-pacientes-y-medicos.md`, v0.1 — 2026-09-04). Se traslada tal cual: no se reinterpretan los requisitos, no se añaden requisitos nuevos, no se cambia la numeración FR-xxx ni los criterios de aceptación.

---

## Clarifications

*(Se completa en `/speckit-clarify`.)*

---

## 1. Propósito

Dar de alta y mantener la ficha del paciente (Anexo 2, cara anterior) con sus contactos y su médico de cabecera, y mantener el catálogo de médicos reutilizable desde cualquier punto de la aplicación.

## 2. Actores

| Actor | Rol en esta spec |
|---|---|
| Elaborador | Crea, edita, da de baja y reactiva pacientes; crea y edita médicos — cualquier usuario que participe en el circuito del paciente, sin distinción de categoría profesional |
| Administrador | Todo lo anterior, más purga manual (fuera de esta spec: Spec 010) y configuración de valores por defecto que afectan a esta spec (día de retirada y nº de blísteres por defecto) |

## 3. Escenarios de usuario

### E1 — Alta de paciente tras la entrevista inicial
Como farmacéutico, tras la entrevista en la zona de atención personalizada, quiero registrar la ficha del paciente en una sola pantalla para que el número de ficha quede asignado y pueda continuar con la evaluación de idoneidad sin salir del contexto del paciente.

### E2 — Reutilizar un médico ya conocido
Como farmacéutico, al indicar el médico de cabecera de un paciente nuevo, quiero escribir parte del apellido y elegir un médico ya registrado, para no volver a teclear su nombre, centro y teléfono.

### E3 — Crear un médico sin abandonar la ficha
Como farmacéutico, si el médico no existe, quiero crearlo desde el mismo campo con los datos mínimos, para que quede en el catálogo y no tenga que ir a otra pantalla.

### E4 — Corregir datos de un médico para todos
Como farmacéutico, si un médico cambia de centro o de teléfono, quiero editarlo una vez y que todos los pacientes que lo referencian y las cartas futuras usen el dato nuevo.

### E5 — Registrar representante legal o persona autorizada
Como farmacéutico, para un paciente que no firma por sí mismo, quiero registrar a la persona que firmará el consentimiento 1b con su DNI, para que el documento se genere con esos datos.

### E6 — Baja y reactivación
Como farmacéutico, cuando un paciente abandona el servicio, quiero marcarlo como baja con fecha y motivo sin perder nada, y poder reactivarlo si vuelve.

### E7 — Localizar un paciente rápido
Como cualquier usuario, quiero encontrar un paciente escribiendo parte del nombre, apellidos, DNI o número de ficha, y ver primero los activos.

### Casos límite

- Paciente con dos representantes (tutela compartida): se permiten varios contactos de tipo REPRESENTANTE_LEGAL; el consentimiento 1b elige cuál firma (Spec 002).
- Dos pacientes convivientes con el mismo domicilio y apellidos: el identificador visual (FR-002) permite distinguir sus SPD; no se bloquea nada.
- Médico con mismo nombre en dos centros: FR-034 avisa; se permiten ambos porque son personas distintas o el mismo médico en dos consultas; el usuario decide.
- Paciente fallecido: baja con motivo FALLECIMIENTO; los envases en custodia pasan a estado RETIRADO con salida a SIGRE (Spec 005 define el flujo; aquí solo se registra la baja).
- Cambio de médico de cabecera: se sustituye la referencia; los tratamientos ya existentes conservan su prescriptor original (no se reasigna automáticamente).

## 4. Requisitos funcionales

### 4.1 Paciente

- **FR-001** El sistema asigna el número de ficha automáticamente al guardar por primera vez, con el prefijo configurado y un correlativo de 6 dígitos. Un número asignado nunca se reutiliza, ni tras purga.
- **FR-002** Campos de la ficha: nombre, apellidos, sexo (mujer/hombre), DNI, fecha de nacimiento, nº Seguridad Social, CIP, dirección, código postal, población, teléfono 1, teléfono 2, email, médico de cabecera, enfermedades crónicas, alergias e intolerancias, observaciones, identificador visual, pictogramas de comidas (sí/no), día de retirada del SPD, número de blísteres (1 o 2).
- **FR-002b** `sexo` es obligatorio si se informa CIP (necesario para validar el dígito de sexo, FR-005). En cualquier otro caso es opcional.
- **FR-002c** `dia_retirada` y `n_blisteres` se prerrellenan con los valores por defecto de Configuración (Spec 000 / Spec 005 FR-500) y son editables por paciente. Ver Spec 005 para su uso en el listado de retirada y las sesiones de preparación.
- **FR-003** Obligatorios para guardar: nombre, apellidos, y al menos uno de {DNI, CIP, fecha de nacimiento}. Todo lo demás puede completarse después.
- **FR-004** El sistema advierte (sin bloquear) si existe otro paciente activo con el mismo DNI o CIP.
- **FR-005** El sistema valida el formato del DNI/NIE (letra de control) y del CIP gallego cuando ambos, CIP y los datos que lo componen, están informados. Regla de construcción del CIP gallego (14 caracteres):
  - Posiciones 1–6: fecha de nacimiento en formato `aammdd`.
  - Posiciones 7–8: inicial del primer apellido + inicial del segundo apellido.
  - Posiciones 9–10: segunda letra del primer apellido + segunda letra del segundo apellido.
  - Posición 11: sexo, `0` mujer / `1` hombre.
  - Posiciones 12–14: tres dígitos (asignados por el sistema sanitario, no calculables).

  Validación: formato de 14 caracteres, fecha de las posiciones 1–6 coincide con `fecha_nacimiento`, posiciones 7–10 coinciden con las iniciales/segundas letras de `apellidos` (primer y segundo apellido, sin tildes), posición 11 coincide con `sexo`. Las posiciones 12–14 no se validan (son aleatorias). Formato o correspondencia inválidos = aviso, no bloqueo — apellidos compuestos, con partícula o de un solo apellido pueden no ajustarse a la regla estándar.

  **FR-005b** Si `fecha_nacimiento`, `apellidos` y `sexo` están informados y `cip` está vacío, el sistema propone autocompletar las posiciones 1–11 y deja las posiciones 12–14 en blanco para completar a mano.
- **FR-006** Estados del paciente: `EVALUACION` (recién creado), `ACTIVO` (tras consentimiento vigente e idoneidad APTO — lo fija la Spec 002), `SUSPENDIDO` (pausa temporal: hospitalización, viaje) [NEEDS CLARIFICATION: Q2 — ¿es necesario el estado SUSPENDIDO en esta primera versión, o basta con registrarlo como observación de texto libre? Propuesta si no hay respuesta: mantenerlo como estado formal, coste de implementación bajo.], `BAJA`. Transiciones permitidas: EVALUACION→ACTIVO, ACTIVO↔SUSPENDIDO, cualquiera→BAJA, BAJA→EVALUACION (reactivación, exige nueva evaluación y consentimiento).
- **FR-007** La baja exige fecha y motivo de una lista (`FALLECIMIENTO`, `RENUNCIA`, `TRASLADO`, `HOSPITALIZACION_PROLONGADA`, `CRITERIO_FARMACEUTICO`, `OTRO` con texto). No elimina ningún dato relacionado.
- **FR-008** La pantalla del paciente muestra en cabecera permanente: número de ficha, nombre completo, edad, estado, alertas (alergias en rojo si existen; "sin consentimiento vigente" si aplica).
- **FR-009** Desde la ficha se accede por pestañas a: Datos, Contactos, Idoneidad y consentimiento (Spec 002), Tratamiento (Spec 004), Depósito (Spec 005), Preparaciones (Spec 006), Comunicaciones (Spec 008). Esta spec define solo Datos y Contactos.
- **FR-010** Búsqueda global de pacientes: coincidencia parcial, sin distinguir mayúsculas ni tildes, sobre nombre, apellidos, DNI, CIP y número de ficha. Resultados ordenados: activos primero, luego evaluación, suspendidos, bajas; dentro de cada grupo por apellidos.
- **FR-011** El listado de pacientes permite filtrar por estado y por médico de cabecera, y muestra por defecto solo activos y en evaluación.

**Nota sobre médico de cabecera** [NEEDS CLARIFICATION: Q1 — ¿se necesita un segundo médico "especialista de referencia" en la ficha del paciente, además del médico de cabecera, o basta con el prescriptor que se fija por cada tratamiento (Spec 004)? Propuesta si no hay respuesta: no añadir un segundo campo; solo prescriptor por tratamiento.]

### 4.2 Contactos

- **FR-020** Un paciente tiene 0..n contactos con tipo (`FAMILIAR`, `REPRESENTANTE_LEGAL`, `PERSONA_AUTORIZADA`, `CUIDADOR`), nombre, apellidos, DNI, teléfono, email, marca "principal" y marca "retira la medicación".
- **FR-021** Solo un contacto puede ser principal; es el que se imprime como "Familiar próximo" en el Anexo 2.
- **FR-021b** Solo un contacto puede llevar la marca "retira la medicación". Si ningún contacto la lleva, se entiende que el paciente retira su propia medicación y el listado de retirada (Spec 005) usa el DNI del propio paciente. La misma persona (mismo DNI) puede marcarse como responsable de retirar en varios pacientes distintos; el sistema no lo impide ni lo advierte, es una situación esperada (p. ej. un cuidador de varios pacientes de una residencia).
- **FR-021c** El contacto marcado como "retira la medicación" exige DNI para poder marcarse.
- **FR-022** Un contacto de tipo `REPRESENTANTE_LEGAL` o `PERSONA_AUTORIZADA` exige DNI (necesario para el Anexo 1b).
- **FR-023** Los contactos se dan de baja, no se eliminan, y no se muestran en la ficha una vez dados de baja salvo que se pida "ver histórico".

### 4.3 Catálogo de médicos

- **FR-030** Campos: nombre, apellidos, nº colegiado, especialidad (por defecto "Medicina de familia"), centro, teléfono, email, dirección, activo.
- **FR-031** Obligatorios: apellidos y nombre. El resto opcional.
- **FR-032** Todo campo de la aplicación que referencia un médico (médico de cabecera del paciente, prescriptor de un tratamiento, destinatario de una comunicación) es un **selector con autocompletado** que busca por fragmento de apellidos, nombre o centro, sin tildes, mostrando "Apellidos, Nombre — Centro". Mínimo 2 caracteres para buscar.
- **FR-033** El selector ofrece siempre la acción "Nuevo médico…", que abre un diálogo con los campos del FR-030, guarda en el catálogo y deja el nuevo médico seleccionado en el campo de origen.
- **FR-034** Al crear un médico, el sistema advierte si existe otro activo con los mismos apellidos y nombre (sin tildes) o el mismo nº colegiado; el usuario decide.
- **FR-035** Editar un médico afecta a todas las referencias. No existe "copia" de los datos del médico en paciente ni en tratamiento. (Las comunicaciones ya impresas se reproducen con la traza de impresión, no con los datos del médico; ver Spec 008.)
- **FR-036** Un médico se puede dar de baja solo si no es médico de cabecera de ningún paciente activo o en evaluación. Si lo es, el sistema lista esos pacientes y no permite la baja.
- **FR-037** Pantalla de catálogo de médicos: listado con búsqueda, número de pacientes activos que lo tienen de cabecera, y acceso a la ficha del médico.
- **FR-038** Al crear un tratamiento (Spec 004), el prescriptor se prerrellena con el médico de cabecera del paciente.

### 4.4 Permisos y auditoría

- **FR-040** Cualquier usuario Elaborador o Administrador realiza todas las operaciones de esta spec; no hay distinción de sólo-lectura por categoría profesional.
- **FR-041** Solo Administrador accede a la utilidad de purga (Spec 010) y a los valores por defecto de día de retirada y nº de blísteres (Spec 000).
- **FR-042** Toda creación, edición, baja y reactivación de paciente, contacto o médico deja traza en auditoría con los campos cambiados (antes/después) y el usuario que la hizo.

## 5. Entidades clave

| Entidad | Descripción | Referencia |
|---|---|---|
| Paciente | Ficha del Anexo 2 cara anterior más estado y baja | [docs/data-model.md](../../docs/data-model.md) §Paciente |
| Contacto | Persona vinculada al paciente | [docs/data-model.md](../../docs/data-model.md) §Contacto |
| Medico | Catálogo compartido | [docs/data-model.md](../../docs/data-model.md) §Medico |

## 6. Criterios de aceptación

**CA-001 Número de ficha**
Dado un sistema con prefijo "F-" y último correlativo 000041, cuando guardo un paciente nuevo, entonces recibe "F-000042" y el campo no es editable.

**CA-002 Mínimos para guardar**
Dado un formulario con nombre y apellidos pero sin DNI, CIP ni fecha de nacimiento, cuando pulso guardar, entonces el sistema no guarda e indica que falta al menos uno de los tres.

**CA-003 Duplicado por DNI**
Dado un paciente activo con DNI 12345678Z, cuando creo otro con el mismo DNI, entonces veo un aviso con el nombre y número de ficha del existente y puedo continuar o cancelar.

**CA-004 Autocompletado de médico**
Dado un catálogo con "Fernández Souto, Ana — CS A Estrada", cuando escribo "fern" en el campo médico de cabecera, entonces aparece esa entrada y al seleccionarla el campo queda enlazado por id.

**CA-005 Nuevo médico en contexto**
Dado el campo médico de cabecera de un paciente, cuando elijo "Nuevo médico…", relleno apellidos y nombre y guardo, entonces el médico existe en el catálogo y queda seleccionado en el campo sin haber salido de la ficha del paciente.

**CA-006 Edición propaga**
Dado un médico referenciado por tres pacientes, cuando cambio su teléfono, entonces la ficha de los tres pacientes muestra el teléfono nuevo.

**CA-007 Baja bloqueada**
Dado un médico que es cabecera de un paciente activo, cuando intento darlo de baja, entonces el sistema lo impide y muestra el nombre del paciente.

**CA-008 Representante sin DNI**
Dado un contacto de tipo REPRESENTANTE_LEGAL sin DNI, cuando guardo, entonces el sistema no guarda e indica que el DNI es obligatorio para ese tipo.

**CA-009 Baja de paciente conserva todo**
Dado un paciente con tratamientos, envases y SPD, cuando lo doy de baja con motivo, entonces su estado es BAJA, todos los datos relacionados siguen existiendo y consultables, y aparece en auditoría la baja con motivo.

**CA-010 Reactivación exige nuevo consentimiento**
Dado un paciente en BAJA, cuando lo reactivo, entonces pasa a EVALUACION y la cabecera muestra "sin consentimiento vigente".

**CA-011 Búsqueda sin tildes**
Dado un paciente "José Núñez", cuando busco "nunez", entonces aparece en resultados.

**CA-012 Elaborador edita sin restricción de categoría**
Dado un usuario con rol Elaborador, cuando abre una ficha de paciente, entonces puede editar y guardar todos los campos de esta spec, igual que un Administrador.

**CA-013 Auditoría de edición**
Dado un paciente con teléfono 600000001, cuando lo cambio a 600000002 y guardo, entonces existe una entrada de auditoría con acción EDITAR, entidad Paciente, id del paciente, y detalle que incluye telefono1: 600000001 → 600000002.

**CA-014 Autocompletado de CIP**
Dado un paciente con fecha de nacimiento 1991-04-10, apellidos "Eiras Espiño" y sexo hombre, cuando el CIP está vacío y pulso "autocompletar CIP", entonces el sistema propone `910410EEIS1` con las posiciones 12–14 en blanco para completar.

**CA-015 Aviso de CIP no correspondiente**
Dado un paciente con fecha de nacimiento 1991-04-10, apellidos "Eiras Espiño", sexo hombre y CIP `910410EEIS1014`, cuando cambio el sexo a mujer y guardo, entonces el sistema muestra un aviso de correspondencia (posición 11 esperaría 0, tiene 1) sin bloquear el guardado.

## 7. Fuera de alcance de esta spec

- Idoneidad, consentimiento y paso a estado ACTIVO (Spec 002).
- Importación de pacientes desde programas de gestión (Spec 011).
- Purga de bajas > 5 años (Spec 010).
- Fotografía del paciente: descartada por decisión del propietario.
- Historial de médicos de cabecera anteriores del paciente: no se guarda; solo el vigente.

## 8. Preguntas abiertas

| # | Pregunta | Bloquea | Propuesta si no hay respuesta |
|---|---|---|---|
| Q1 | ¿Se necesita un segundo médico "especialista de referencia" en la ficha, o basta con el prescriptor por tratamiento? | Nada | Solo prescriptor por tratamiento |
| Q2 | ¿Estado SUSPENDIDO es necesario en 1.0 o se cubre con observaciones? | Nada | Mantenerlo, coste bajo |

## Assumptions

- Todos los campos y reglas no marcados `[NEEDS CLARIFICATION]` se toman literalmente de la especificación original del propietario del producto, sin inferencias adicionales.
- Los criterios de aceptación (§6, formato Dado/Cuando/Entonces) son los que exige el Artículo IX.2 de la constitución y se usan tal cual como base de los tests de `/speckit-tasks`.
- La validación de DNI/NIE (letra de control) y CIP (FR-005) se implementa como reglas de Dominio puras (sin dependencias externas), coherente con el Artículo VIII.1.
