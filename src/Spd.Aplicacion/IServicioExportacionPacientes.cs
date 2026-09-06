using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Exportación de pacientes a CSV con el mapeo explícito de un perfil (FR-1130/1131).</summary>
public interface IServicioExportacionPacientes
{
    string ExportarACsv(PerfilImportacion perfil, IReadOnlyList<Paciente> pacientes);
}
