namespace Spd.Dominio;

public interface IRepositorioPerfilesImportacionTratamiento
{
    int Crear(PerfilImportacionTratamiento perfil);
    IReadOnlyList<PerfilImportacionTratamiento> Listar();
}
