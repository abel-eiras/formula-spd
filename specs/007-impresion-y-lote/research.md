# Research: Impresión, generación en lote y documentación base

## Decisión 1 — PDF vía QuestPDF, no `.docx` (FR-701, corrección formal)

Ver cabecera de spec.md: el documento fuente (v0.1) es anterior a la Constitución 2.1.0, que fija
QuestPDF como motor de documentos (Art. VIII.2). Cambiar el motor exigiría una enmienda
constitucional, no una decisión de esta spec — se sigue la Constitución, igual que Spec 009 ya
hizo con una discrepancia similar en `FormacionPersonal`.

**Licencia QuestPDF (aviso, no bloqueante)**: desde 2023 QuestPDF exige declarar
`QuestPDF.Settings.License` al arrancar. Se declara `LicenseType.Community` en
`Spd.Presentacion/Program.cs` — gratuita para organizaciones con ingresos brutos anuales por
debajo del umbral que QuestPDF publica en su web, lo cual es el caso esperado de una farmacia
individual gallega. Es un hecho del negocio del propietario, no algo que esta sesión pueda
certificar: si la farmacia superase ese umbral, haría falta una licencia comercial de QuestPDF —
señalado aquí para que quede documentado, no resuelto unilateralmente.

## Decisión 2 — Un único punto de nombrado/carpeta/auditoría, reutilizado por cada generador (FR-701/710-713)

`GeneradorNombreFichero` (función pura, `Spd.Dominio`) decide entre nombre largo y corto según
FR-710/711, sin acceso a disco. `ServicioGeneracionDocumentos` (Infraestructura, como
`ServicioBackup` de Spec 010: hace E/S real) tiene un único método privado
`GuardarDocumento(...)` que aplica el nombre, crea la carpeta del día bajo
`Farmacia.RutaDocumentosGenerados`, escribe el PDF y audita — cada generador público (`GenerarFichaSpd`,
`GenerarEtiquetaAnverso`, …) solo construye el PDF con QuestPDF y delega el resto en ese único
método, satisfaciendo FR-701 ("una sola clase de servicio... sin duplicar lógica de maquetación
entre pantallas" se traduce aquí en "sin duplicar lógica de nombrado/carpeta/auditoría").

## Decisión 3 — "Apellidos completos" (FR-711) se interpreta sobre el dato real disponible

`Paciente.Apellidos` es un único campo de texto (Spec 001), no dos apellidos estructurados por
separado. "Si el paciente no tiene apellidos completos" se interpreta como "el campo está vacío",
la única condición verificable sobre el modelo de datos existente — no se inventa un parseo de
"dos palabras" sobre un campo que nunca se diseñó para eso.

## Decisión 4 — Maquetación funcional con los campos ya descritos, sin las plantillas reales del PNT

El usuario ha añadido `resources/` con PNT y plantillas reales, y ha pedido explícitamente esperar
a analizarlas con un modelo más potente. Esta spec no abre esa carpeta. Los cinco documentos de
esta iteración (`FICHA`, `ETQ-A`, `ETQ-R`, `INSTR`, `FICHA-PAC`) se maquetan con los campos que las
specs de origen (001, 006) ya nombran explícitamente en su propio texto (nombre, apellidos, CN,
posología, número de registro, validez, etc.), cumpliendo el mínimo del Artículo I.2 ("contienen
como mínimo los elementos que el PNT exige") sin pretender ser una réplica exacta de un Anexo que
no se ha revisado. Documentado como pendiente de una pasada de ajuste visual futura, no como
trabajo terminado a nivel de maquetación oficial.

## Decisión 5 — Alcance de esta iteración: los cinco documentos del ciclo pedido, no todo el catálogo

FR-720..733 (lote, documentación base) y el resto del catálogo de FR-700 (`CARTA-PRES`, `CARTA-INC`,
`RETIRADA`, `REG-*`, `IDONEIDAD`, `CONSENT`) se difieren explícitamente (spec.md §8). La prioridad
indicada por el propietario para esta sesión es completar el ciclo alta→tratamiento→preparación→
verificación→entrega→documentación de **un paciente**, no el catálogo completo de documentos de
toda la aplicación. `IServicioGeneracionDocumentos` queda diseñado para que añadir un generador
nuevo (p. ej. `CARTA-PRES`) sea una clase más que reutiliza `GuardarDocumento`, no un cambio de
arquitectura.
