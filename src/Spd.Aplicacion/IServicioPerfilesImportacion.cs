using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Perfiles genéricos de importación/exportación (FR-1100).</summary>
public interface IServicioPerfilesImportacion
{
    PerfilImportacion Crear(DatosAltaPerfilImportacion datos, int? usuarioQueEjecutaId);

    void Actualizar(int perfilId, DatosAltaPerfilImportacion datos, int? usuarioQueEjecutaId);

    PerfilImportacion? ObtenerPorNombre(string nombre);

    IReadOnlyList<PerfilImportacion> ListarPorTipo(TipoPerfilImportacion tipo);
}
