# Quickstart — validación de Spec 011

## Escenario 1 — Perfil reutilizable (US1, CA-1100)

1. `ServicioPerfilesImportacion.Crear(new DatosAltaPerfilImportacion("Mi perfil",
   TipoPerfilImportacion.Pacientes, ",", "UTF-8", true, mapeo, null), usuarioId)`.
2. `ObtenerPorNombre("Mi perfil")` → devuelve el perfil con el mismo mapeo.

## Escenario 2 — Regex probada antes de guardar (US2, CA-1101)

1. `ExtractorUnidadesEnvase.Probar(["COMP 30 mg 28 UDS", "JBE 150 ml"], @"(\d+)\s*UDS")`.
2. Verificar: 1 acierto (28), 1 fallo, nada persistido.

## Escenario 3 — Exportación respeta el mapeo (US3, CA-1103)

1. Perfil con mapeo `{Nombre→"Nombre", Apellidos→"Apellidos", Telefono1→"Teléfono"}`.
2. `ServicioExportacionPacientes.ExportarACsv(perfil, pacientes)`.
3. Verificar que el CSV tiene exactamente esas 3 columnas, ningún otro campo del paciente.

## Validación de UI (manual, para el informe de mañana)

- Pantalla de administración de perfiles de importación (crear, editar, listar por tipo).
- Pantalla de exportación de pacientes con selección de perfil.
