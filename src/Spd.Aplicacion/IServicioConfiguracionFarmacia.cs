using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de la farmacia, prefijos de numeración y rutas (FR-010..FR-013, FR-030..FR-032).</summary>
public interface IServicioConfiguracionFarmacia
{
    Farmacia ObtenerConfiguracion();
    void ActualizarDatosFarmacia(Farmacia datos, int? administradorQueEjecutaId);
    void ActualizarPrefijos(string prefijoNumFicha, string prefijoNumSpd, int? administradorQueEjecutaId);
    void ActualizarValoresDefecto(DatosValoresDefecto valores, int? administradorQueEjecutaId);
    ResultadoValidacionRuta ValidarRuta(string ruta);
}
