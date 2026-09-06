# Plan y progreso — Spec 007: Impresión, generación en lote y documentación base

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: esta rama se bifurca de `main` tras fusionar la Spec 006 (2026-09-06), que
dejó el ciclo alta→tratamiento→preparación→verificación→entrega funcionando de verdad pero sin
generar ningún documento real (`RegistrarImpresion` solo marcaba el timestamp). El usuario pidió
explícitamente completar hoy el ciclo completo "hasta generar toda la documentación a imprimir de
este proceso", excluyendo DataMatrix e importación de ficheros de fábrica por no disponer de esos
recursos todavía.

- **2026-09-06** — Ciclo `/speckit-specify` → `/speckit-plan` → `/speckit-tasks` →
  `/speckit-implement`, ejecutado de forma autónoma. Contenido de `resources/` (PNTs, plantillas)
  deliberadamente NO analizado en esta iteración por instrucción expresa del usuario, que revisará
  ese material en una sesión posterior con un modelo más potente; se documenta como corrección
  formal en la cabecera de spec.md, no como un olvido.

- **Corrección formal (cabecera de spec.md)**: el documento fuente de la Spec 007 especificaba
  `.docx` como formato de los documentos generados (FR-701). La constitución (Art. VIII.2) fija de
  antemano el motor de documentos del proyecto en **QuestPDF** (biblioteca solo-PDF), decisión
  anterior y de rango superior a la del documento de la spec. Se resuelve como "PDF vía QuestPDF,
  no `.docx`" — mismo patrón de "la constitución prevalece sobre texto de spec desactualizado" ya
  aplicado una vez en Spec 009.

- **User Story 3 (P1) — Motor de nombrado y generación común**: `GeneradorNombreFichero` (FR-710/
  711) construye el nombre largo (`Ficha de preparación. Nombre Apellidos.ddMMyyyy.pdf`) y cae al
  nombre corto (`CODIGO_id_ddMMyyyy.pdf`) si el paciente no tiene apellidos, si supera 120
  caracteres o si colisiona con un nombre ya usado en el mismo lote (CA-700/701). El método privado
  común `GuardarDocumento` en `ServicioGeneracionDocumentos` centraliza carpeta
  (`<RutaDocumentosGenerados>/<yyyy-MM-dd>/`, con fallback a `AppContext.BaseDirectory` si la
  Farmacia no tiene esa ruta configurada), `GeneratePdf` y auditoría `GENERAR_DOCUMENTO` (FR-712/
  713) — no repetido por cada tipo de documento (research.md Decisión 2).

- **User Story 1 (P1, MVP) — Ficha, etiquetas e instrucciones de un SPD**: `GenerarFichaSpd`,
  `GenerarEtiquetaAnverso`, `GenerarEtiquetaReverso`, `GenerarInstrucciones` construyen el PDF con
  QuestPDF a partir del SPD, sus líneas y filas de envase reales; la posología se imprime siempre
  como fracción (`FraccionDosis.Texto()`, nunca `.Valor()` — FR-740/743). Las etiquetas de reverso
  incluyen una fila por cada fila de envase de cada línea (serie/lote/caducidad), cubriendo el caso
  multi-envase de una línea (Spec 006 CA-602/CA-611).

- **User Story 2 (P1) — Ficha del paciente**: `GenerarFichaPaciente` reutiliza el mismo mecanismo
  común para el documento `FICHA-PAC` (Spec 001), con los datos de identificación y observaciones
  del paciente.

- **Wiring end-to-end (T018/T021)**: `PreparacionView` tiene ahora botones "Imprimir ficha /
  etiquetas / instrucciones" por cada blíster, que llaman primero a
  `IServicioGeneracionDocumentos` para generar el PDF real y, solo si eso tiene éxito, a
  `ServicioPreparacion.RegistrarImpresion` para marcar `impreso_*_en` — sustituyendo por fin el
  punto de extensión que Spec 006 dejó documentado (research.md Decisión 8 de esa spec).
  `FichaPacienteView` tiene un botón "Imprimir ficha" equivalente para `FICHA-PAC`. Cascada de
  dependencia completa: `App.axaml.cs` → `MainViewModel` → `BuscadorPacientesWindow`/ViewModel →
  `FichaPacienteWindow`/ViewModel → `PreparacionWindow`/ViewModel, igual que cada servicio anterior.

  `dotnet build` sin errores; **59 (Dominio) + 17 (Presentación) + 194 (Aplicación) = 270 tests en
  verde**, sin regresiones sobre los 262 de Spec 006.

## Pendiente (documentado, no fabricado)

- **FR-720..733 (generación en lote, varios pacientes a la vez)**: no implementado. Cada documento
  se genera hoy uno a uno desde su pantalla; `GeneradorNombreFichero` ya soporta la detección de
  colisión de nombres dentro de un lote (`nombresYaUsadosEnLote`) para cuando se construya.
- **Catálogo completo de FR-700** (IDONEIDAD, CONSENT, CARTA-PRES, CARTA-INC, RETIRADA, REG-*, y
  las plantillas exactas de Anexo que puedan venir en `resources/`): diferido. Bloqueado en parte
  por Spec 002 (idoneidad/consentimiento no existe todavía) y en parte por la revisión pendiente
  del contenido de `resources/` que el usuario pidió posponer explícitamente.
- DataMatrix (Spec 012) e importación de ficheros de programas de fábrica (perfiles de Spec 011):
  confirmados por el usuario como fuera de alcance por ahora, no algo a retomar sin que lo pida.
- Prueba manual real (`dotnet run`) del ciclo completo con impresión real por el usuario — hasta
  ahora solo verificado con tests headless (Avalonia) y de generación de PDF (existencia de
  fichero, prefijo/sufijo del nombre, entradas de auditoría), sin inspección visual del PDF
  resultante.
