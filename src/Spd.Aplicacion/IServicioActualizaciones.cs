namespace Spd.Aplicacion;

/// <summary>Comprobación manual de actualizaciones del software (FR-050). Nunca automática al
/// arrancar (Art. VI.3) — solo se invoca desde el botón "Comprobar actualizaciones".</summary>
public interface IServicioActualizaciones
{
    Task<ResultadoComprobacionActualizacion> ComprobarActualizacionesAsync(
        string versionInstalada, int? administradorQueEjecutaId);
}
