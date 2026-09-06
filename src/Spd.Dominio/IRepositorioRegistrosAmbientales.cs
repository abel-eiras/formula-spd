namespace Spd.Dominio;

public interface IRepositorioRegistrosAmbientales
{
    int Crear(RegistroAmbiental registro);
    RegistroAmbiental? ObtenerPorId(int id);
    RegistroAmbiental? ObtenerUltimo();
}
