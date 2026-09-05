# Research — Spec 009: Registros de calidad

Fase 0 de `/speckit-plan`. Decisiones técnicas concretas dentro del stack ya fijado (Art. VIII).

## Decisión 1 — `FormacionPersonal` sin distinción de categoría profesional (FR-920)

**Contexto**: `docs/data-model.md` remite a `SPD_Diseno_Tecnico_Fase1_Arquitectura_y_Datos.md`
(líneas 441-446) para el detalle de estas 6 entidades ("sin cambios respecto a la Fase 1"). Esa
descripción de Fase 1 dice: `FormacionPersonal.tipo (FARMACEUTICO: curso+entidad / TECNICO:
formador_id)` — una distinción de categoría profesional con ramas de campos distintas por rama.

**Decisión**: se ignora ese campo `tipo`/`formador_id` de Fase 1 y se sigue FR-920 literal: "Sin
distinción de categoría profesional (Constitución Artículo VII.4: la app no modela
farmacéutico/técnico); el campo es texto libre para el tipo de formación, sin ramas condicionales
por rol." `FormacionPersonal` tiene un único conjunto de campos para cualquier usuario:
`usuario_id`, `nombre_curso`, `entidad_organizadora`, `fecha`, `acreditado`. Sin `formador_id`.

**Motivo del conflicto**: la Fase 1 (documento de arquitectura original) es anterior a la
enmienda 2.0.0 de la constitución (2026-09-04, "roles simplificados a Administrador/Elaborador...
la app no modela categoría profesional"). `docs/data-model.md` quedó desactualizado en este punto
concreto porque su nota "sin cambios respecto a la Fase 1" no se revisó campo a campo tras la
enmienda. La propia spec 009 (v0.1, posterior a la enmienda) ya corrige esto en su texto — se seguía
la spec, no el documento de Fase 1, exactamente igual que en Spec 001 cuando `docs/data-model.md`
no detallaba un campo del todo.

**Alternativas consideradas**: mantener `tipo`/`formador_id` "por si acaso se necesita en el
futuro" — rechazada por Art. X.1 (no se construye lo que la spec no pide) y porque violaría
directamente Art. VII.4.

## Decisión 2 — Cálculo de `fuera_rango` (FR-901, CA-900)

**Decisión**: `fuera_rango = temperatura < Farmacia.temp_min OR temperatura > Farmacia.temp_max OR
humedad < Farmacia.hr_min OR humedad > Farmacia.hr_max`, evaluado una sola vez al crear el
registro con los valores vigentes de `Farmacia` en ese instante, y guardado como columna
(`INTEGER`, 0/1). Nunca se recalcula al leer ni al cambiar la configuración (Art. IV: instantánea).

**Alternativas consideradas**: calcularlo al vuelo en cada lectura comparando con la configuración
actual — rechazada porque es exactamente lo que CA-900 prohíbe explícitamente.

## Decisión 3 — Umbral del aviso de registro atrasado (FR-950, Clarifications Q1)

**Contexto**: Q1 se resolvió con "7 días para ambos, como un único valor configurable".

**Decisión**: se añade `Farmacia.umbral_dias_aviso_calidad` (INTEGER NOT NULL DEFAULT 7) mediante
un `ALTER TABLE` aditivo en la migración de esta spec (Art. VIII.3: una migración nunca destruye
datos, solo puede añadir). Un único umbral se aplica tanto al aviso de ambiental como al de
limpieza rutinaria, editable desde Configuración (reutilizando el mismo patrón de
`dia_retirada_defecto`/`n_blisteres_defecto` de Spec 000). Se documenta como adición a
`docs/data-model.md` (`Farmacia`).

**Alternativas consideradas**: dos columnas independientes (`umbral_dias_ambiental`,
`umbral_dias_limpieza`) — rechazada por Art. X.2 (más piezas móviles que la propia clarificación
no pidió); si en el futuro se necesitan umbrales distintos, es una migración aditiva trivial.

## Decisión 4 — Permisos de `ControlCambiosPNT`/`ControlCopias` (FR-942, CA-903)

**Decisión**: comprobación explícita en la capa de Aplicación (`ServicioControlDocumental`): si
`usuarioQueEjecutaId` no corresponde a un usuario con `Rol.Administrador`, se lanza
`ErrorValidacionException`. Mismo patrón ya usado en `ServicioUsuarios.DesbloquearUsuario` de
Spec 000 (comprobación de rol en el servicio, nunca solo en la pantalla — Art. VII.4: "ninguna
pantalla asume permisos: cada acción los comprueba en la capa de Aplicación").

**Alternativas consideradas**: ninguna — es la única forma ya establecida en este proyecto de
aplicar una restricción de "solo Administrador".

---

**Output**: todas las incógnitas técnicas de esta feature quedan resueltas; ninguna arrastra
`NEEDS CLARIFICATION` a `tasks.md`. La Decisión 1 corrige una discrepancia real entre
`docs/data-model.md`/Fase 1 y la constitución vigente — se documenta aquí para que quede
trazabilidad de por qué el código no sigue al pie de la letra el documento de Fase 1 en ese punto.
