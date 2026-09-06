namespace Spd.Dominio;

public interface IRepositorioSpdModificaciones
{
    int Crear(SpdModificacion modificacion);
    IReadOnlyList<SpdModificacion> ListarPorSpd(int spdId);
}
