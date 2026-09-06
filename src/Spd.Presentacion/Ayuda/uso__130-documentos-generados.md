# Documentos generados: dónde están y cómo se llaman

Todos los documentos se generan en **PDF** y se guardan en la carpeta configurada en [[uso:configuracion]] ("Carpeta de documentos generados"), dentro de una subcarpeta con la **fecha del día** (`2026-09-06`). Cada generación queda en auditoría con tipo, documento de origen, ruta y usuario.

## Nombre de fichero

Por defecto `Nombre del documento. Nombre Apellidos.ddmmaaaa.pdf` (p. ej. `Ficha de preparación. María López Vidal.06092026.pdf`). Si el nombre sería demasiado largo, el paciente no tiene apellidos o habría dos iguales, se usa la forma corta `CODIGO_identificador_ddmmaaaa.pdf`.

## Catálogo

- `FICHA` — Ficha de preparación, control y entrega (Anexo I.G), desde [[uso:preparacion]].
- `ETQ-A` / `ETQ-R` — Etiquetas anverso y reverso (Anexo I.F), desde [[uso:preparacion]].
- `INSTR` — Hoja de instrucciones al paciente (Anexo I.H), desde [[uso:preparacion]].
- `FICHA-PAC` — Ficha del paciente (Anexo I.E), desde [[uso:ficha-paciente]]; incluye la evaluación de idoneidad vigente.
- `RGPD` — Información sobre protección de datos (Anexo I.D), desde [[uso:ficha-paciente]].
- `CONSENT` — Consentimiento informado (Anexo I.B), desde [[uso:idoneidad-consentimiento]].
- `CARTA-PRES` / `CARTA-INC` — Cartas al médico (Anexo I.C), desde [[uso:comunicaciones-medico]].

La posología se imprime **siempre en fracciones** (½, no 0,5). Los documentos se imprimen y **se firman en papel**: la aplicación registra quién generó qué y cuándo, no las firmas (constitución Art. II).

Procedimiento relacionado: [[procedimiento:documentacion-y-conservacion]].
