# Plan y progreso — Spec 011: Import/export con programas de gestión

Este documento se actualiza al completar cada fase, cada test y cada prueba. No se reordena
retroactivamente lo ya marcado como hecho; solo se añade.

**Nota de contexto**: esta rama se bifurca de `main` tras fusionar Specs 001, 003, 009, 010, 004,
005 y 008 (2026-09-06). Migración nueva `0008_perfiles_importacion.sql`, sin colisión, siguiendo
directamente a la 0007 de Spec 008.

- **2026-09-06** — Ciclo completo `/speckit-specify` → `/speckit-plan` → `/speckit-tasks` →
  `/speckit-implement`, ejecutado de forma autónoma. Dos partes del documento fuente se difirieron
  explícitamente por falta de datos reales, no por dificultad técnica: FR-1101 (perfiles de
  fábrica para Farmatic/Nixfarma/Unycop) y la integración de FR-1110 con el lector real del
  nomenclátor de Spec 003 — ambas exigirían fabricar el formato de columnas de un sistema externo,
  el mismo riesgo que ya se evitó con el Anexo 9 de Spec 002.

- **Foundational**: `PerfilImportacion` genérico (4 tipos: Pacientes/Dispensaciones/Medicamentos/
  Nomenclator), con `Mapeo` como lista de pares campo↔columna reutilizable en ambos sentidos.

- **User Story 1 (P1, MVP) — Perfil genérico reutilizable**: `Crear`/`ObtenerPorNombre`/`Actualizar`/
  `ListarPorTipo` (CA-1100, CA-1102 adaptado). Pantalla de administración de perfiles con el mapeo
  editado como texto `campo=columna` línea a línea, sin una grilla dedicada — mismo nivel de
  sencillez que `PerfilImportacionTratamiento` de Spec 005.

- **User Story 2 (P2) — Extracción de unidades probada**: `ExtractorUnidadesEnvase.Probar` cuenta
  aciertos/fallos sobre una muestra sin persistir nada (CA-1101). Función pura en `Spd.Dominio`,
  deliberadamente sin conectar a `LectorNomenclatorCsv` (research.md Decisión 2): ese lector ya
  está afinado contra el fichero real de la AEMPS y no hay una muestra real de una columna de
  "descripción con unidades" para conectarlo sin riesgo de romperlo especulativamente.

- **User Story 3 (P2) — Exportación de pacientes**: `ServicioExportacionPacientes.ExportarACsv`
  lee el campo de `Paciente` indicado en cada par del mapeo por reflexión, nunca serializa el
  objeto completo (CA-1103). Reutiliza `PerfilImportacion` en sentido inverso en vez de una
  entidad `PerfilExportacion` separada (research.md Decisión 3, Art. V). Pantalla de exportación
  con el CSV resultante mostrado para copiar — sin diálogo de guardado de fichero, por simplicidad
  (no hay todavía un caso de uso real que exija escribirlo directamente a disco).

  `dotnet build` sin errores; **54 (Dominio) + 16 (Presentación) + 159 (Aplicación) = 229 tests en
  verde**. Las 23 tareas de [tasks.md](./tasks.md) están completas.

## Pendiente (documentado, no fabricado)

- FR-1101: perfiles de fábrica para Farmatic/Nixfarma/Unycop — necesita una fila de ejemplo real
  de cada programa.
- FR-1110 conectado de verdad: necesita una muestra real de la columna de descripción con
  unidades del nomenclátor de la AEMPS para no arriesgar el parser ya verificado de Spec 003.
- FR-1102: detección automática de cabeceras candidatas — se difiere; el mapeo por índice de
  columna ya es suficiente y es el mismo patrón aceptado en Spec 005.
- Prueba manual real (`dotnet run`) por el usuario.
