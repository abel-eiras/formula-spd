# Constitución del proyecto SPD

**Versión 2.1.0 — 4 de septiembre de 2026**
**Ratificada por:** Abel (propietario del producto)
**Ámbito:** aplicación de escritorio para la gestión del servicio de Sistemas Personalizados de Dosificación en oficinas de farmacia de Galicia.

Esta constitución prevalece sobre cualquier spec, plan o tarea. Una spec que contradiga un artículo es inválida hasta que se corrija la spec o se enmiende la constitución (ver §Enmiendas). Todo agente o persona que escriba código, specs o planes debe leer este documento antes de empezar y volver a él cuando dude.

---

## Artículo I — Base normativa

1. El comportamiento del sistema deriva del *Procedimiento Normalizado de Trabajo de SPD del Colegio de Farmacéuticos de Pontevedra (junio 2022)* y del *Decreto 87/2022 de la Xunta de Galicia*. Cuando una spec y el PNT difieran, manda el PNT y se abre una incidencia sobre la spec.
2. Los documentos imprimibles pueden rediseñarse libremente, pero **contienen como mínimo** todos los elementos que el PNT exige para cada anexo. Una spec de documento enumera esos elementos y el test de aceptación comprueba su presencia.
3. Las siguientes reglas del PNT son invariantes del dominio y se implementan en la capa de Dominio con test unitario:
   - Un SPD tiene un periodo de validez ≤ 14 días y nunca posterior a la caducidad más próxima de los envases incluidos.
   - No se prepara un SPD para un paciente sin consentimiento vigente y evaluación de idoneidad APTO.
   - Solo entran en el blíster medicamentos marcados como aptos para SPD.
   - El verificador es distinto del elaborador; toda excepción exige motivo registrado. Esta regla se aplica por usuario, no por categoría profesional: la aplicación no distingue farmacéutico de técnico (Artículo VII).
   - Toda preparación registra temperatura y humedad de la zona en el momento de preparar.
   - La documentación de un paciente se conserva como mínimo un año después de su baja.

## Artículo II — El papel es la base legal

1. Todo documento con valor legal (consentimiento, ficha de paciente, ficha de preparación-control-entrega, etiquetas, instrucciones, cartas, registros de calidad) se **imprime y se firma a mano**. El sistema no almacena firmas digitales ni documentos escaneados.
2. El sistema registra **cuándo y quién** generó cada impresión. Esa traza es lo que permite reproducir en inspección lo que se entregó.
3. Lo impreso debe ser reproducible: ver Artículo IV.

## Artículo III — Nada se borra

1. Ninguna operación de usuario elimina físicamente datos de paciente, tratamiento, SPD, envase, comunicación o registro de calidad. Las entidades se dan de **baja** (`activo = 0`, `fecha_baja`, motivo).
2. La única eliminación física es la **purga manual** de pacientes con baja superior a cinco años, ejecutada por un administrador, paciente a paciente, con confirmación por contraseña y traza en auditoría. No existe purga automática ni programada.
3. Las tablas de auditoría son de solo inserción. No hay UPDATE ni DELETE sobre ellas en ningún código de la aplicación.
4. Un SPD **no entregado** puede reelaborarse (cambio de medicación solicitado por el paciente, un familiar o el médico antes de la recogida). La reelaboración nunca sobrescribe en silencio: conserva versionadas todas las líneas y el resultado de verificación anteriores, registra motivo y origen de la solicitud, y exige nueva verificación antes de poder entregarse. Un SPD **entregado** es inmutable; cualquier corrección posterior es un SPD nuevo.

## Artículo IV — Catálogo vivo, historia congelada

1. Médicos, medicamentos, material de acondicionamiento y usuarios son **catálogos**: se definen una vez y se referencian por clave desde todas partes. Está prohibido duplicar en texto libre un dato que exista en un catálogo.
2. Los catálogos se editan en cualquier momento. La descripción física de un medicamento puede cambiar hoy para todos los pacientes a partir de hoy.
3. Cada línea de un SPD guarda una **instantánea** (nombre, CN, posología, descripción física, momento de administración, unidades) tomada al crear la línea. Lo que se imprimió para un SPD se reconstruye desde su instantánea, nunca desde el estado actual del catálogo.
4. Un cambio de posología no modifica el tratamiento vigente: lo cierra con fecha y crea uno nuevo. El historial de tratamiento es consultable.

## Artículo V — Un dato, una entrada

1. El usuario introduce cada dato una sola vez. Si un dato ya existe en el sistema, la interfaz lo **propone** (autocompletado, prerrelleno, "preparar semana siguiente") y el usuario confirma o corrige.
2. Los datos de la farmacia se configuran una vez y aparecen en todos los documentos sin intervención.
3. Los cálculos derivables (unidades semanales, envases necesarios, periodo de validez por defecto, texto de descripción física) se calculan; nunca se piden al usuario, aunque puede sobrescribirlos donde la spec lo permita.

## Artículo VI — Aislamiento y portabilidad

1. La aplicación **no abre conexiones de red**, salvo las dos excepciones tasadas del punto 2. No hay telemetría, comprobación de licencias en línea, ni ninguna otra llamada de red. Cualquier dependencia que requiera red para funcionar en lo demás queda excluida.
2. Las únicas conexiones de red permitidas, en toda la aplicación, son:
   - **Actualizaciones del propio software**: comprobar si existe una versión nueva y descargarla.
   - **Descarga del nomenclátor**: obtener el fichero Excel/CSV del nomenclátor desde una URL configurable en Opciones.
   Ninguna otra funcionalidad abre red bajo ningún concepto. Un tercer caso de uso que necesite red es una enmienda a este artículo, no una excepción de implementación.
3. Ninguna de las dos conexiones es automática en segundo plano sin que el usuario la vea: ambas se inician o se confirman explícitamente desde la pantalla correspondiente (Configuración / Actualizaciones, Configuración / Nomenclátor). El fallo de cualquiera de las dos —sin conexión disponible— no impide el funcionamiento normal de la aplicación con los datos que ya tiene.
4. La instalación completa es **una carpeta** con un único ejecutable, la base de datos, la configuración, los logs y las copias de seguridad. Copiar esa carpeta a otro PC es una restauración válida y completa.
5. Sin instalador, sin escritura en el registro de Windows, sin dependencias de runtime externas. Objetivo: Windows 11 x64.
6. Al cerrar la aplicación se genera una copia de seguridad íntegra y consistente en la ruta configurada.

## Artículo VII — Seguridad y acceso

1. Acceso mediante usuario y contraseña individual. Las contraseñas se almacenan con hash Argon2id; nunca en claro ni reversibles.
2. La base de datos admite cifrado completo con contraseña maestra (SQLCipher). El cifrado es opcional y activable/desactivable por un administrador. La contraseña maestra no se almacena en ningún lugar del sistema.
3. Al activar el cifrado el sistema genera una clave de recuperación que se imprime. El sistema no activa el cifrado sin que el administrador confirme haberla impreso.
4. La aplicación reconoce dos roles: **Administrador** y **Elaborador**. No modela categoría profesional (titular, farmacéutico, técnico): quien firma qué documento en papel es una decisión profesional ajena a la aplicación. El rol Administrador gobierna configuración global y aspectos sensibles (cifrado, copias de seguridad, usuarios, valores por defecto); el rol Elaborador cubre toda acción sobre pacientes, tratamientos, depósito y preparaciones. Ninguna pantalla asume permisos: cada acción los comprueba en la capa de Aplicación.
5. La regla "verificador distinto del elaborador" (Artículo I.3) se comprueba comparando el usuario que verificó con el que preparó, no su rol: dos usuarios Elaborador cualesquiera la satisfacen.
6. Toda acción de escritura y todo inicio/cierre de sesión, impresión, exportación, importación, copia y restauración deja traza en auditoría con usuario, fecha-hora, entidad y detalle.

## Artículo VIII — Arquitectura

1. Cuatro capas: Presentación (Avalonia, MVVM) → Aplicación (servicios de caso de uso) → Dominio (entidades y reglas) → Infraestructura (SQLite/SQLCipher, PDF, ficheros). Las dependencias apuntan hacia dentro; Dominio no depende de nada.
2. Stack fijado: .NET 8 LTS, Avalonia UI 11, Microsoft.Data.Sqlite + SQLitePCLRaw.bundle_e_sqlcipher, Dapper, QuestPDF, Serilog, xUnit. Cambiar cualquiera de ellos es una enmienda.
3. El esquema de base de datos se gestiona con scripts SQL numerados embebidos y una tabla `schema_version`. Toda migración es idempotente y se aplica al arrancar. Una migración nunca destruye datos; si necesita transformar, copia y conserva.
4. SQL explícito. No se usan ORMs que generen esquema o consultas implícitas.

## Artículo IX — Calidad

1. Toda regla del Artículo I.3 y del Artículo IV tiene tests unitarios en Dominio antes de considerarse implementada.
2. Cada spec define criterios de aceptación en formato Dado/Cuando/Entonces; una funcionalidad está terminada cuando todos sus criterios pasan, no antes.
3. Generación de documentos: cada plantilla tiene un test que genera el PDF con datos de ejemplo y comprueba la presencia de los elementos obligatorios del Artículo I.2.
4. Rendimiento: arranque completo hasta pantalla de login < 2 s en un PC de gama media; ninguna acción de la pantalla de preparación tarda más de 200 ms en responder.
5. Idioma de la interfaz y de los documentos: castellano. Los textos de interfaz viven en un único fichero de recursos, no dispersos en el código.

## Artículo X — Simplicidad

1. Se construye lo que la spec pide. No se añaden campos, opciones de configuración, abstracciones ni "por si acaso" no cubiertos por una spec aprobada.
2. Ante dos soluciones equivalentes, gana la que tiene menos partes móviles.
3. Un `[NEEDS CLARIFICATION]` en una spec bloquea la implementación de ese punto. No se resuelve con una suposición del implementador.

## Artículo XI — Software libre y legibilidad humana

1. El código se publica bajo licencia **GPLv3**. Toda distribución de una versión modificada, incluida a farmacias clientes, debe ir acompañada del código fuente correspondiente bajo la misma licencia.
2. Prioridad explícita: legibilidad humana por encima de la abstracción prematura o la elegancia técnica. Un mantenedor nuevo en el proyecto debe poder entender un módulo leyéndolo, sin necesitar explicación oral. Se prefiere código explícito y algo repetitivo a una abstracción genérica que ahorra líneas pero exige seguir la pista a varias capas de indirección.
3. Nombres de variables, métodos y clases en castellano cuando nombran conceptos del dominio del PNT (`Paciente`, `SPD`, `EvaluacionIdoneidad`, `unidadesRestantes`), en inglés cuando son mecánica técnica genérica sin equivalente de dominio (`Repository`, `ILogger`). No se mezclan los dos idiomas dentro del mismo nombre.
4. Cada clase de Dominio y cada servicio de Aplicación lleva un comentario de cabecera que explica su propósito en una o dos frases. Toda regla de negocio no evidente a partir del código lleva un comentario que explica el **porqué**, citando el artículo del PNT o de esta constitución del que procede, no el qué (el código ya dice el qué).
5. Documentación técnica obligatoria, versionada junto al código, sin relleno de marketing ni repetición entre documentos: visión general de arquitectura (una página), diagrama de capas y de entidades, guía para levantar el entorno de desarrollo, guía para ejecutar los tests, guía para generar el ejecutable portable. Cada documento va al grano; si un párrafo no aporta información que el lector necesite para actuar, se elimina.
6. Métodos y clases cortos: un método hace una cosa y la nombra bien. Un método que supera ~40 líneas o una clase que supera ~300 es candidato a dividirse salvo justificación explícita en comentario.
7. Este artículo no releva al Artículo X: legibilidad y simplicidad son la misma prioridad vista desde dos ángulos — para el mantenedor humano y para el alcance de la funcionalidad.

---

## Enmiendas

- Una enmienda se propone como cambio a este fichero con justificación, se numera (MAJOR para cambios de principio, MINOR para adiciones, PATCH para redacción) y se registra abajo.
- Las specs y planes existentes se revisan contra cada enmienda MAJOR.

| Versión | Fecha | Cambio |
|---|---|---|
| 1.0.0 | 2026-09-04 | Ratificación inicial |
| 2.0.0 | 2026-09-04 | MAJOR. Artículo VI: se sustituye el aislamiento total de red por dos excepciones tasadas (actualizaciones del software, descarga del nomenclátor). Artículo VII: roles simplificados a Administrador/Elaborador (cambio de principio ya aplicado a las specs afectadas). Nuevo Artículo XI: software libre y legibilidad humana. |
| 2.1.0 | 2026-09-04 | MINOR. Artículo III.4: se permite reelaborar un SPD no entregado (reemblistado por cambio solicitado), siempre versionado, nunca entregado sin nueva verificación. Artículo XI.1: licencia fijada en GPLv3. |
