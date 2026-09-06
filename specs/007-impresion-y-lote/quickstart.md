# Quickstart — validación de Spec 007

## Escenario 1 — Nombre por defecto y acortado (US3, CA-700/701)

1. `GeneradorNombreFichero.Generar("Ficha de preparación", "FICHA", "María", "López Vidal",
   "F-000123", new DateOnly(2026,9,4))` → `"Ficha de preparación. María López Vidal.04092026.pdf"`.
2. Con apellidos/nombre que superen 120 caracteres → `"FICHA_F-000123_04092026.pdf"`.

## Escenario 2 — Generar la ficha de un SPD real (US1, CA-709/710)

1. SPD con una línea de pauta D=1/2.
2. `ServicioGeneracionDocumentos.GenerarFichaSpd(spdId, usuarioId)`.
3. Verificar: el fichero existe en `<RutaDocumentosGenerados>/<hoy>/`, y una entrada de auditoría
   `GENERAR_DOCUMENTO` referencia el SPD.
4. (Manual) Abrir el PDF y comprobar que la posología muestra `1/2`, nunca `0,5`.

## Escenario 3 — Ficha del paciente (US2)

1. `ServicioGeneracionDocumentos.GenerarFichaPaciente(pacienteId, usuarioId)`.
2. Verificar que el fichero existe con los datos básicos del paciente.

## Validación de UI (manual, para el informe de mañana)

- Botones "Imprimir ficha/etiquetas/instrucciones" en la pantalla de preparación (Spec 006).
- Botón "Imprimir ficha" en la ficha de paciente (Spec 001).
- Abrir alguno de los PDF generados y comprobar visualmente que es legible y contiene los datos
  esperados (esta iteración no replica el Anexo oficial, ver research.md Decisión 4).
