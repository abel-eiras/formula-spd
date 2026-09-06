# Feature Specification: Import/export con programas de gestión

**Feature Branch**: `011-import-export`

**Created**: 2026-09-06

**Status**: Draft — pendiente de `/speckit-clarify`

**Constitución aplicable:** 2.1.0 (Artículos V, VI)

**Depende de:** Spec 000 (URL del nomenclátor), Spec 001 (pacientes), Spec 003 (medicamentos),
Spec 005 (envases, importación de tratamiento) — todas ya mergeadas en `main`.

**Requerida por:** Ninguna directamente; da soporte a Spec 003 (nomenclátor) y Spec 005
(importación de tratamiento).

**Input**: Especificación completa aportada literalmente por el propietario del producto
(`spec-011-import-export.md`, v0.1 — 2026-09-04). Se traslada tal cual: no se reinterpretan los
requisitos, no se añaden requisitos nuevos, no se cambia la numeración FR-11xx ni los criterios de
aceptación CA-11xx.

**Alcance de esta iteración**: FR-1100, FR-1110, FR-1111, FR-1120 (ya cubierto por
`PerfilImportacionTratamiento` de Spec 005, sin cambios), FR-1130 y FR-1131 completos. FR-1101
(perfiles de fábrica precargados para Farmatic/Nixfarma/Unycop) y FR-1102 (asistente interactivo
de mapeo con detección de cabeceras) quedan diferidos — ver alcance y Assumptions.

---

## Clarifications

Ninguna: el documento fuente no dejaba preguntas bloqueantes (§9 "Ninguna bloqueante"). Se detectó
un punto que sí necesita una decisión explícita, documentado en Assumptions en vez de resuelto por
adivinanza.

---

## 1. Propósito

Definir los perfiles de importación/exportación reutilizables que alimentan el nomenclátor
(Spec 003) y la importación de tratamiento+envase (Spec 005 §4.8), admitiendo los formatos de
columnas de distintos programas de gestión de farmacia sin acoplar la aplicación a ninguno en
concreto.

## 2. Actores

| Actor | Puede |
|---|---|
| Administrador | Crear y editar perfiles de importación |
| Elaborador / Administrador | Usar un perfil ya creado para importar |

## 3. Escenarios de usuario (User Stories)

### US1 (P1) — Definir un perfil genérico reutilizable (E1)
Como administrador, mapeo las columnas de un fichero de ejemplo a los campos de la aplicación y
guardo ese mapeo con un nombre, para no repetir el proceso cada vez.

**Prueba independiente**: crear un `PerfilImportacion` de tipo `PACIENTES` con un mapeo de 3
columnas y verificar que se puede recuperar por nombre.

### US2 (P2) — Extraer unidades por envase con una regla probada (E3)
Como administrador, defino una expresión regular que localiza el número de unidades dentro de una
columna de texto, la pruebo contra una muestra antes de guardar, y veo cuántas filas aciertan y
cuántas fallan.

**Prueba independiente**: probar un patrón contra 10 filas de muestra y verificar el recuento de
aciertos/fallos sin guardar nada (CA-1101).

### US3 (P2) — Exportar pacientes con un mapeo configurable (FR-1130)
Como administrador, exporto los pacientes activos a CSV con solo las columnas que un perfil de
exportación mapea explícitamente, para llevar un listado a otro sistema sin filtrar datos que no
correspondían.

**Prueba independiente**: exportar con un perfil que solo mapea nombre/apellidos/teléfono y
verificar que el CSV resultante no contiene ningún otro campo (CA-1103).

## 4. Requisitos funcionales

### 4.1 Perfiles genéricos

- **FR-1100** `PerfilImportacion` con tipo `PACIENTES`, `DISPENSACIONES`, `MEDICAMENTOS` o
  `NOMENCLATOR`; cada uno define separador, codificación, si tiene cabecera, y el mapeo columna
  origen → campo destino (JSON).
- **FR-1101** Perfiles precargados de fábrica para los programas de gestión de farmacia más
  habituales en Galicia (Farmatic, Nixfarma, Unycop), editables y no bloqueados — diferido en esta
  iteración (ver Assumptions): no hay muestras reales de sus columnas.
- **FR-1102** Un perfil nuevo se crea desde un asistente: se pega o carga un fichero de ejemplo,
  la aplicación detecta cabeceras candidatas, y el usuario asigna cada columna a un campo destino
  de una lista cerrada según el tipo de perfil. Diferido en esta iteración (ver Assumptions): se
  implementa el perfil y su mapeo (FR-1100) sin la detección automática de cabeceras candidatas;
  el usuario indica el índice de columna directamente, igual que ya hace `PerfilImportacionTratamiento`
  de Spec 005.

### 4.2 Importación de nomenclátor (soporte a Spec 003)

- **FR-1110** `regex_unidades_envase`: patrón de expresión regular aplicado sobre una columna de
  texto del nomenclátor, con un grupo de captura que da el número de unidades. Se prueba contra
  una muestra de filas en el propio asistente antes de guardarse, mostrando aciertos y fallos.
- **FR-1111** El resultado de aplicar la regla se usa como propuesta (Spec 003 FR-320), nunca se
  escribe directamente sin pasar por la pantalla de revisión fila a fila.

### 4.3 Importación de tratamiento y envase (soporte a Spec 005 §4.8)

- **FR-1120** `PerfilImportacionTratamiento`: mapeo mínimo a CN, número de serie, lote, caducidad,
  con origen `PORTAPAPELES` o `FICHERO`. Ya implementado en Spec 005 (`Spd.Dominio.PerfilImportacionTratamiento`,
  `Spd.Aplicacion.ServicioImportacionTratamientoEnvase`); esta spec no lo modifica.

### 4.4 Exportación

- **FR-1130** Exportación de pacientes activos a CSV con columnas configurables (mismo mecanismo
  de perfil, en sentido inverso: campo de la aplicación → columna de salida), por si la farmacia
  necesita llevar un listado a otro sistema.
- **FR-1131** Exportación no incluye nunca campos bajo `[NEEDS CLARIFICATION]` de privacidad no
  resueltos; se limita a los campos que el perfil mapea explícitamente, nunca "todo el registro"
  por defecto.

## 5. Entidades clave

| Entidad | Referencia |
|---|---|
| PerfilImportacion | docs/data-model.md §PerfilImportacion |
| PerfilImportacionTratamiento | Ya implementada en Spec 005, sin cambios |

## 6. Criterios de aceptación

**CA-1100 Perfil reutilizable**
Dado un perfil ya creado, cuando lo recupero por nombre para importar, entonces no se me pide
mapear columnas de nuevo.

**CA-1101 Regex probada antes de guardar**
Dado un patrón de extracción de unidades, cuando lo pruebo contra 10 filas de muestra, entonces
veo cuántas aciertan y cuántas fallan antes de guardar el perfil.

**CA-1102 Perfil de fábrica editable**
*(Diferido — ver Assumptions: sin perfiles de fábrica en esta iteración, no hay nada que editar
todavía. El mecanismo de edición de un `PerfilImportacion` ya creado por el usuario sí se prueba:
editar su mapeo y verificar que conserva el mismo nombre e id.)*

**CA-1103 Exportación respeta el mapeo**
Dado un perfil de exportación que solo mapea nombre, apellidos y teléfono, cuando exporto,
entonces el CSV resultante no contiene ningún otro campo del paciente.

## 7. Casos límite

- Fichero con codificación distinta de UTF-8 (habitual en programas de gestión antiguos,
  Windows-1252): el perfil registra la codificación; leerla de verdad con esa codificación queda
  para cuando exista un caso real de importación de fichero completo con este perfil genérico
  (hoy el único consumidor de ficheros, Spec 005, ya lee CSV en UTF-8).
- Columna de unidades del nomenclátor con formato inconsistente entre filas: la regla regex puede
  no acertar en todas; el contador de aciertos/fallos de CA-1101 es precisamente para detectar
  esto antes de guardar el perfil.

## 8. Fuera de alcance de esta spec

- Qué se hace con cada fila importada de tratamiento (Spec 005 §4.8) o de nomenclátor (Spec 003
  §4.3): esta spec solo define el mecanismo de mapeo y extracción reutilizable.
- Sincronización automática o programada con ningún programa externo (Constitución Artículo VI:
  sin conexiones de red salvo las dos excepciones tasadas; import/export de ficheros locales no es
  una conexión de red).
- Integrar `regex_unidades_envase` dentro del flujo real de importación del nomenclátor (Spec 003,
  `LectorNomenclatorCsv`): esa columna de texto ya tiene un lector afinado contra el fichero real
  de la AEMPS sin un campo de "descripción con unidades" conocido; conectarlo exige una muestra
  real de esa columna (ver Assumptions).

## 9. Assumptions

- FR-1101 (perfiles de fábrica) y la integración de FR-1110 con el lector real del nomenclátor
  (Spec 003) requieren datos reales (muestras de columnas de Farmatic/Nixfarma/Unycop, o de la
  columna de descripción del nomenclátor con unidades) que no están disponibles en esta iteración.
  Inventar esas columnas sería el mismo riesgo que fabricar contenido clínico (Spec 002) — se
  documenta como pendiente de datos, no se adivina.
- FR-1102 (detección automática de cabeceras candidatas) se difiere: el usuario mapea por índice
  de columna, igual que `PerfilImportacionTratamiento` de Spec 005 (mismo patrón ya aceptado).
