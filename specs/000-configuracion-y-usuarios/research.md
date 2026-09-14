# Research — Spec 000: Configuración inicial, farmacia y usuarios

Fase 0 de `/speckit-plan`. No hay dudas de "qué stack usar" (fijado por la Constitución Art. VIII);
las decisiones de aquí son puntos concretos que la spec y `docs/data-model.md` dejan sin fijar del
todo y que hacen falta para escribir `data-model.md` (Fase 1) y `tasks.md`.

## Decisión 1 — Dónde viven los campos de configuración que `docs/data-model.md` no enumera

**Contexto**: la spec exige `ruta_documentos_generados` (FR-031), `url_nomenclator` (FR-051),
umbral de reutilización de lectura ambiental (FR-022) y un contador de intentos fallidos de login
(FR-045). `docs/data-model.md` v0.4 no los lista en las tablas Farmacia/Usuario.

**Decisión**: se añaden como columnas nuevas de `Farmacia` (`ruta_documentos_generados`,
`url_nomenclator`, `umbral_reutilizacion_lectura_ambiental_horas`, con valor por defecto 2 horas
per FR-022) y de `Usuario` (`intentos_fallidos_consecutivos INTEGER DEFAULT 0`, `bloqueado INTEGER
DEFAULT 0`). Se actualiza `docs/data-model.md` a v0.5 con estas columnas y una entrada en su tabla
de cambios, en vez de mantener una definición paralela solo en esta spec — Farmacia/Usuario son
catálogos de configuración únicos en todo el proyecto (Art. IV.1) y otras specs futuras (003, 005,
009, 010, 011) los referencian por nombre.

**Alternativas consideradas**: tabla `Configuracion` clave-valor genérica — rechazada por Art. X.2
(más piezas móviles que columnas tipadas en una fila que ya existe) y porque estos valores no son
dinámicos por usuario, son de la única fila Farmacia.

## Decisión 2 — Librería de hash Argon2id para .NET

**Contexto**: Art. VII.1 exige Argon2id, pero no fija paquete concreto (no está en la lista cerrada
de Art. VIII.2 porque esa lista es de piezas de arquitectura, no de cada micro-dependencia).

**Decisión**: `Konscious.Security.Cryptography.Argon2` (MIT, sin dependencias de red en runtime,
puro .NET). Se invoca desde `Spd.Infraestructura`; `Spd.Dominio` solo conoce la interfaz
`IHasheadorPassword` (Art. VIII.1: Dominio no depende de nada externo).

**Alternativas consideradas**: `Isopoh.Cryptography.Argon2` — descartada sin motivo técnico fuerte,
ambas cumplen; se elige la de uso más extendido para minimizar riesgo de mantenimiento (Art. X.2).

## Decisión 3 — Comprobación de actualizaciones (FR-050, resuelto a GitHub Releases en `/speckit-clarify`)

**Decisión**: `GET https://api.github.com/repos/abel-eiras/spd/releases/latest` sin autenticación *(revisado el 2026-09-14: repositorio renombrado a `abel-eiras/formula-spd` y lista de releases en vez de `latest`, que omite las betas; ver PROGRESO.md)*
(API pública, límite de tasa suficiente para una comprobación manual ocasional). Se compara
`tag_name` con la versión instalada; si es mayor, se muestra el enlace de descarga del asset
correspondiente. Nunca se llama al arrancar (Art. VI.3): solo al pulsar "Comprobar
actualizaciones".

**Alternativas consideradas**: feed propio versionado — rechazado por Decisión Q1 de la spec
(GitHub Releases, sin servidor propio que mantener).

## Decisión 4 — Validación de rutas de backup/documentos (FR-030/031/032)

**Decisión**: comprobación estándar .NET — `Directory.Exists`, e intento de crear y borrar un
fichero temporal (`Path.Combine(ruta, ".spd_write_test")`) para confirmar permiso de escritura real
(un `Directory.Exists` no garantiza escritura en unidades de red). Se compara la ruta resuelta
(`Path.GetFullPath`) contra la carpeta de instalación para el aviso de FR-032, sin bloquear el
guardado.

**Alternativas consideradas**: librerías de terceros para permisos de fichero — rechazadas, la API
de `System.IO` ya cubre el caso sin dependencia añadida (Art. X.2).

## Decisión 5 — Descarga del nomenclátor (FR-051/052)

**Decisión**: `HttpClient` con timeout corto (10 s) para descargar el fichero desde
`url_nomenclator` a una ruta temporal; se informa éxito/fecha o motivo de fallo (CA-005). El
parseo del contenido (columnas, mapeo) es de Spec 003/011 y queda fuera de esta spec — aquí solo se
resuelve la descarga y el resultado visible.

**Alternativas consideradas**: ninguna — es la única forma que exige Art. VI.2 (URL configurable,
sin más infraestructura).

---

**Output**: todas las incógnitas técnicas de esta feature quedan resueltas; ninguna arrastra
`NEEDS CLARIFICATION` a `tasks.md`.
