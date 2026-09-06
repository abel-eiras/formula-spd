namespace Spd.Dominio;

public interface IRepositorioSpdLineas
{
    int Crear(SpdLinea linea);
    void Actualizar(SpdLinea linea);
    SpdLinea? ObtenerPorId(int id);
    IReadOnlyList<SpdLinea> ListarPorSpd(int spdId);
}
