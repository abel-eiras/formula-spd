namespace Spd.Dominio;

public interface IRepositorioMaterialAcondicionamiento
{
    int Crear(MaterialAcondicionamiento material);
    MaterialAcondicionamiento? ObtenerPorId(int id);
    IReadOnlyList<MaterialAcondicionamiento> ListarActivos();
}
