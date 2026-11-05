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

    /// <summary>spec-012 FR-1211: separador que el lector manda en lugar del GS (null o vacío: ninguno).
    /// No puede contener letras ni dígitos, que aparecen dentro de lotes y series.</summary>
    void ActualizarSeparadorLector(string? separador, int? administradorQueEjecutaId);

    /// <summary>Spec 016 FR-1603: disposición del blíster físico de la farmacia.</summary>
    void ActualizarDisposicionBlister(DisposicionBlister disposicion, int? administradorQueEjecutaId);
}
