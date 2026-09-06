namespace Spd.Dominio;

public interface IRepositorioPerfilesImportacion
{
    int Crear(PerfilImportacion perfil);
    void Actualizar(PerfilImportacion perfil);
    PerfilImportacion? ObtenerPorId(int id);
    PerfilImportacion? ObtenerPorNombre(string nombre);
    IReadOnlyList<PerfilImportacion> ListarPorTipo(TipoPerfilImportacion tipo);
}
