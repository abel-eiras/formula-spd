# Feature Specification: Registros de calidad

**Feature Branch**: `009-registros-calidad`

**Created**: 2026-09-05

**Status**: Draft — pendiente de `/speckit-clarify`

**Constitución aplicable:** 2.1.0 (Artículos I, II, III, VII — la spec original cita 2.1.0, sin contradicción con la constitución vigente)

**Depende de:** Spec 000 (usuarios) — ya implementada

**Requerida por:** Spec 006 (registro ambiental enlazado a la preparación), Spec 007 (generación de estos documentos)

**Input**: Especificación completa aportada literalmente por el propietario del producto (`spec-009-registros-calidad.md`, v0.1 — 2026-09-04). Se traslada tal cual: no se reinterpretan los requisitos, no se añaden requisitos nuevos, no se cambia la numeración FR-9xx ni los criterios de aceptación.

---

## Clarifications

### Session 2026-09-05

- Q: ¿7 días es razonable como umbral del aviso de FR-950 para ambos registros (ambiental y limpieza), o prefieres umbrales distintos? → A: 7 días para ambos, como un único valor configurable (no dos independientes) — resuelto de forma autónoma adoptando la propuesta por defecto de la spec original, porque el usuario pidió avanzar en desarrollo que no requiera su intervención.

### Session 2026-09-05 (tras prueba manual)

- Q: Tras probar la aplicación, el propietario del producto indicó que el registro ambiental y de
  limpieza (E1/E2, FR-900/FR-901/FR-902/FR-910/FR-911) **no son necesarios en absoluto** como
  pantallas independientes de esta spec. La temperatura y humedad se incluirán en la
  documentación impresa (hoja de elaboración del blíster, Spec 006/007): esa hoja permitirá
  rellenar esos valores en el momento de generarla, pero si quedan en blanco y se rellenan a mano
  en papel no es un problema — especialmente porque las hojas de un día se generan todas juntas
  pero los blísteres se preparan a lo largo de la jornada con temperatura/humedad distintas, así
  que no tiene sentido un único registro ambiental "suelto" por día. → A: Se retiran de esta spec
  las pantallas y los servicios de registro ambiental y de limpieza (FR-900/901/902/910/911) y,
  por depender de ellos, el aviso de registro atrasado (FR-950, CA-904 y el propio Escenario E5).
  FR-920..FR-942 (formación, residuos, control documental) no se ven afectados. La captura de
  temperatura/humedad ligada a la hoja de elaboración queda pendiente de diseñarse en Spec 006/007,
  no aquí. Ver PROGRESO.md para el detalle completo de la decisión y qué código se retiró.

---

## 1. Propósito

Cubrir los registros de calidad que exige el PNT al margen del circuito de cada paciente: condiciones ambientales, limpieza de la zona de preparación, formación del personal, recogida de residuos no SIGRE, control de cambios del propio PNT y control de copias de los documentos maestros.

## 2. Actores

| Actor | Puede |
|---|---|
| Elaborador / Administrador | Registrar entradas de ~~ambiental, limpieza~~ (retirados), formación, residuos |
| Administrador | Control de cambios del PNT y control de copias |

## 3. Escenarios de usuario

### E1 — Registro ambiental rutinario ⚠️ RETIRADO (ver Clarifications, sesión 2026-09-05 tras prueba manual)
Como elaborador, al empezar el día en el obrador, quiero registrar la temperatura y humedad sin que tenga que estar ligado a ninguna preparación concreta.

### E2 — Limpieza antes y después de preparar ⚠️ RETIRADO (ver Clarifications, sesión 2026-09-05 tras prueba manual)
Como elaborador, quiero registrar que he limpiado la zona antes y después de una tanda de preparaciones, con un clic, sin rellenar un formulario largo.

### E3 — Formación del personal
Como administrador, quiero registrar los cursos o formaciones que ha recibido cada usuario, para tener constancia ante una inspección.

### E4 — Residuos no SIGRE
Como elaborador, quiero registrar cuándo se recoge el material de acondicionamiento sobrante y otros residuos no farmacéuticos, con la empresa gestora.

### E5 — Ver de un vistazo si falta algún registro ⚠️ RETIRADO (dependía de E1/E2, ver Clarifications)
Como titular, quiero ver en el panel de inicio si llevamos más de X días sin registrar limpieza o ambiental, para no descubrirlo en la inspección.

## 4. Requisitos funcionales

### 4.1 Registro ambiental ⚠️ RETIRADO DE ESTA SPEC (ver Clarifications, sesión 2026-09-05 tras prueba manual)

> No se implementan pantallas ni servicio de registro ambiental independiente. La temperatura y
> humedad se capturarán, si acaso, al generar la hoja de elaboración del blíster (Spec 006/007);
> ese diseño no está definido todavía y no es parte de esta spec.

- **FR-900** Campos: fecha, hora, temperatura, humedad, usuario, observaciones, `spd_id` opcional (si se originó desde una preparación, Spec 006 FR-630) o nulo (si es una lectura rutinaria independiente).
- **FR-901** `fuera_rango` se calcula y se guarda en el momento del registro comparando con `Farmacia.temp_min/max, hr_min/max` (Spec 000): si cambian los rangos de configuración después, las lecturas pasadas conservan el resultado con el que se registraron (Artículo IV: instantánea, no recálculo retroactivo).
- **FR-902** Alta rápida desde el panel de inicio o desde cualquier pantalla, con fecha/hora prerrellenadas a "ahora", editables.

### 4.2 Limpieza ⚠️ RETIRADO DE ESTA SPEC (ver Clarifications, sesión 2026-09-05 tras prueba manual)

- **FR-910** Campos: fecha, usuario, tipo (`PRE_PREPARACION`, `POST_PREPARACION`, `RUTINARIA`), observaciones opcionales.
- **FR-911** Botones de un clic "Registrar limpieza pre-preparación" y "post-preparación" visibles desde la pantalla de preparación (Spec 006), además del acceso general desde el menú de registros.

### 4.3 Formación del personal

- **FR-920** Campos: usuario, nombre del curso o formación, entidad organizadora, fecha, acreditado (sí/no). Sin distinción de categoría profesional (Constitución Artículo VII.4: la app no modela farmacéutico/técnico); el campo es texto libre para el tipo de formación, sin ramas condicionales por rol.
- **FR-921** Un usuario puede tener varias formaciones registradas a lo largo del tiempo; ninguna se sustituye, todas se acumulan (Artículo III).

### 4.4 Recogida de residuos no SIGRE

- **FR-930** Campos: fecha, empresa gestora, usuario, observaciones. Cubre material de acondicionamiento sobrante y otros residuos no farmacéuticos; los residuos de medicamentos van a SIGRE y se gestionan desde Spec 005 (salida de envase), no aquí.

### 4.5 Control de cambios del PNT y control de copias

- **FR-940** `ControlCambiosPNT`: documento (código del PNT o del anexo afectado), versión, descripción del cambio, fecha, quién lo redactó, quién lo revisó, quién lo aprobó. Es un registro administrativo de cuándo cambia el procedimiento en papel de la farmacia, no del código de la aplicación.
- **FR-941** `ControlCopias`: documento, número de copia, usuario que la recibió, fecha. Cubre la trazabilidad de a quién se entregó cada copia controlada del PNT en papel.
- **FR-942** Ambos registros son de solo Administrador, por ser control documental de alcance de farmacia, no del día a día de un paciente.

### 4.6 Avisos ⚠️ RETIRADO DE ESTA SPEC (dependía del registro ambiental/limpieza, ver Clarifications)

- **FR-950** Panel de inicio: días transcurridos desde el último registro ambiental rutinario y desde la última limpieza `RUTINARIA`, con umbral configurable (por defecto, aviso a partir de 7 días sin ambiental y 7 días sin limpieza rutinaria). El aviso no bloquea nada, es informativo.

## 5. Entidades clave

| Entidad | Referencia |
|---|---|
| ~~RegistroAmbiental, RegistroLimpieza~~ (retiradas), FormacionPersonal, RecogidaResiduos, ControlCambiosPNT, ControlCopias | data-model.md |

## 6. Criterios de aceptación

**CA-900 Ambiental fuera de rango se marca al registrar** ⚠️ RETIRADO DE ESTA SPEC
Dado un rango configurado 15–25 °C, cuando registro 27 °C, entonces la entrada queda marcada fuera de rango; si luego cambio el rango a 15–30 °C, esa entrada sigue marcada fuera de rango.

**CA-901 Limpieza de un clic** ⚠️ RETIRADO DE ESTA SPEC
Dado que estoy en la pantalla de preparación, cuando pulso "Registrar limpieza pre-preparación", entonces se crea el registro con fecha/hora actual y usuario actual sin más pasos.

**CA-902 Formación acumulativa**
Dado un usuario con dos formaciones ya registradas, cuando añado una tercera, entonces las tres siguen siendo consultables.

**CA-903 Control de cambios solo administrador**
Dado un usuario Elaborador, cuando intenta acceder a Control de cambios del PNT, entonces el sistema se lo impide.

**CA-904 Aviso de registro atrasado** ⚠️ RETIRADO DE ESTA SPEC
Dado que no hay ningún registro ambiental rutinario en los últimos 8 días y el umbral es 7, cuando abro el panel de inicio, entonces veo el aviso correspondiente.

## 7. Casos límite

- ~~Registro ambiental duplicado el mismo día por error: no se elimina (Artículo III); ambos quedan, no hay problema porque no es un dato único por día.~~ (no aplica: retirado)
- Formación acreditada que luego se descubre incorrecta: se añade una entrada aclaratoria; no se edita ni se borra la original.

## 8. Fuera de alcance de esta spec

- Generación de los documentos de estos registros (Spec 007, códigos `REG-FORM`, `REG-RES`).
- **Registro ambiental y de limpieza en cualquier forma** (FR-900/901/902/910/911/950, CA-900/901/904, Escenarios E1/E2/E5): retirados de esta spec tras prueba manual del usuario (Clarifications, sesión 2026-09-05). La captura de temperatura/humedad, si se hace, se diseñará como parte de la hoja de elaboración del blíster en Spec 006/007, no como un registro independiente.

## 9. Preguntas abiertas

Ninguna pendiente — Q1 resuelta, ver sección "Clarifications" al inicio del documento.

## Assumptions

- Todos los campos y reglas no marcados `[NEEDS CLARIFICATION]` se toman literalmente de la especificación original del propietario del producto, sin inferencias adicionales.
- Los criterios de aceptación (§6, formato Dado/Cuando/Entonces) son los que exige el Artículo IX.2 de la constitución y se usan tal cual como base de los tests de `/speckit-tasks`.
- `RegistroAmbiental.spd_id` (FR-900) referencia una entidad de Spec 006 que todavía no existe; en esta spec la columna se modela como nullable y se deja siempre nula (todo registro de esta spec es una "lectura rutinaria independiente"), sin romper el contrato para cuando Spec 006 exista.
