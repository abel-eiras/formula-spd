namespace Spd.Dominio;

public interface IRepositorioSpdVerificaciones
{
    int Crear(SpdVerificacion verificacion);
    IReadOnlyList<SpdVerificacion> ListarPorSpd(int spdId);
}
