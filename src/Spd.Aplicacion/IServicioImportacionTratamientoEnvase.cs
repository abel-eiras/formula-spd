using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Alta masiva de tratamiento+envase por copiar/pegar o fichero (FR-570..577).</summary>
public interface IServicioImportacionTratamientoEnvase
{
    ResultadoImportacion ImportarDesdePegado(int pacienteId, PerfilImportacionTratamiento perfil, string textoTabulado, int? usuarioQueEjecutaId);

    ResultadoImportacion ImportarDesdeFichero(int pacienteId, PerfilImportacionTratamiento perfil, string contenidoCsv, int? usuarioQueEjecutaId);

    PerfilImportacionTratamiento GuardarPerfil(string nombre, OrigenImportacionTratamiento origen, string? separador, bool? tieneCabecera, MapeoColumnasImportacion mapeo);

    IReadOnlyList<PerfilImportacionTratamiento> ListarPerfiles();
}
