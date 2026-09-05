# Spec 011 — Import/export con programas de gestión

**Estado:** Borrador para revisión
**Versión:** 0.1 — 2026-09-04
**Constitución aplicable:** 2.1.0 (Artículos V, VI)
**Depende de:** Spec 000 (URL del nomenclátor), 001 (pacientes), 003 (medicamentos), 005 (envases, importación de tratamiento)
**Requerida por:** Ninguna directamente; da soporte a Spec 003 (nomenclátor) y Spec 005 (importación de tratamiento)

---

## 1. Propósito

Definir los perfiles de importación/exportación reutilizables que alimentan el nomenclátor (Spec 003) y la importación de tratamiento+envase (Spec 005 §4.8), admitiendo los formatos de columnas de distintos programas de gestión de farmacia sin acoplar la aplicación a ninguno en concreto.

## 2. Actores

| Actor | Puede |
|---|---|
| Administrador | Crear y editar perfiles de importación |
| Elaborador / Administrador | Usar un perfil ya creado para importar |

## 3. Escenarios de usuario

### E1 — Primer uso con un programa nuevo
Como administrador, la primera vez que importo desde un programa de gestión, quiero mapear sus columnas a los campos de la aplicación y guardar ese mapeo con un nombre, para no repetir el proceso cada vez.

### E2 — Uso habitual con un perfil ya creado
Como elaborador, quiero elegir el perfil "Farmatic — pegado desde dispensación" de una lista y que la importación funcione sin preguntarme nada más.

### E3 — Extraer unidades por envase del nomenclátor con una regla
Como administrador, quiero definir una expresión que localice el número de unidades dentro de una columna de texto del nomenclátor (p. ej. "COMP 30 mg 28 UDS" → 28), para no rellenar `unidades_envase` a mano medicamento a medicamento.

## 4. Requisitos funcionales

### 4.1 Perfiles genéricos

- **FR-1100** `PerfilImportacion` con tipo `PACIENTES`, `DISPENSACIONES`, `MEDICAMENTOS` o `NOMENCLATOR`; cada uno define separador, codificación, si tiene cabecera, y el mapeo columna origen → campo destino (JSON).
- **FR-1101** Perfiles precargados de fábrica para los programas de gestión de farmacia más habituales en Galicia (Farmatic, Nixfarma, Unycop), editables y no bloqueados — si el formato de un programa cambia de versión, el administrador ajusta el perfil sin depender de una actualización de la aplicación.
- **FR-1102** Un perfil nuevo se crea desde un asistente: se pega o carga un fichero de ejemplo, la aplicación detecta cabeceras candidatas, y el usuario asigna cada columna a un campo destino de una lista cerrada según el tipo de perfil.

### 4.2 Importación de nomenclátor (soporte a Spec 003)

- **FR-1110** `regex_unidades_envase`: patrón de expresión regular aplicado sobre una columna de texto del nomenclátor, con un grupo de captura que da el número de unidades. Se prueba contra una muestra de filas en el propio asistente antes de guardarse, mostrando aciertos y fallos.
- **FR-1111** El resultado de aplicar la regla se usa como propuesta (Spec 003 FR-320), nunca se escribe directamente sin pasar por la pantalla de revisión fila a fila.

### 4.3 Importación de tratamiento y envase (soporte a Spec 005 §4.8)

- **FR-1120** `PerfilImportacionTratamiento`: mapeo mínimo a CN, número de serie, lote, caducidad, con origen `PORTAPAPELES` o `FICHERO`. Reutiliza el mecanismo general de perfiles (FR-1100) especializado a estos cuatro campos.

### 4.4 Exportación

- **FR-1130** Exportación de pacientes activos a CSV con columnas configurables (mismo mecanismo de perfil, en sentido inverso: campo de la aplicación → columna de salida), por si la farmacia necesita llevar un listado a otro sistema.
- **FR-1131** Exportación no incluye nunca campos bajo `[NEEDS CLARIFICATION]` de privacidad no resueltos; se limita a los campos que el perfil mapea explícitamente, nunca "todo el registro" por defecto.

## 5. Entidades clave

| Entidad | Referencia |
|---|---|
| PerfilImportacion, PerfilImportacionTratamiento | data-model.md |

## 6. Criterios de aceptación

**CA-1100 Perfil reutilizable**
Dado un perfil "Farmatic — pegado desde dispensación" ya creado, cuando importo una nueva línea, entonces no se me pide mapear columnas de nuevo.

**CA-1101 Regex probada antes de guardar**
Dado un patrón de extracción de unidades, cuando lo pruebo contra 10 filas de muestra, entonces veo cuántas aciertan y cuántas fallan antes de guardar el perfil.

**CA-1102 Perfil de fábrica editable**
Dado el perfil precargado "Nixfarma", cuando la farmacia actualiza su versión de Nixfarma y cambian las columnas, entonces puedo editar el perfil existente sin crear uno nuevo desde cero.

**CA-1103 Exportación respeta el mapeo**
Dado un perfil de exportación que solo mapea nombre, apellidos y teléfono, cuando exporto, entonces el CSV resultante no contiene ningún otro campo del paciente.

## 7. Casos límite

- Fichero con codificación distinta de UTF-8 (habitual en programas de gestión antiguos, Windows-1252): el perfil registra la codificación y la aplicación la respeta al leer.
- Columna de unidades del nomenclátor con formato inconsistente entre filas: la regla regex puede no acertar en todas; las que fallan se marcan para completar a mano (Spec 003 FR-320), no detienen la importación completa.

## 8. Fuera de alcance de esta spec

- Qué se hace con cada fila importada de tratamiento (Spec 005 §4.8) o de nomenclátor (Spec 003 §4.3): esta spec solo define el mecanismo de mapeo y extracción reutilizable.
- Sincronización automática o programada con ningún programa externo (Constitución Artículo VI: sin conexiones de red salvo las dos excepciones tasadas; import/export de ficheros locales no es una conexión de red).

## 9. Preguntas abiertas

Ninguna bloqueante.
