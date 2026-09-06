namespace Spd.Dominio;

public interface IRepositorioSpdLineaEnvases
{
    int Crear(SpdLineaEnvase fila);
    void Actualizar(SpdLineaEnvase fila);
    IReadOnlyList<SpdLineaEnvase> ListarPorLinea(int spdLineaId);
    IReadOnlyList<SpdLineaEnvase> ListarPorSpd(int spdId);
}
