namespace Spd.Infraestructura;

/// <summary>Guarda el logo dentro de la carpeta de instalación (Art. VI.4). Si se sustituye, el
/// anterior se conserva en un histórico con fecha, sin tabla de negocio (FR-011).</summary>
public sealed class GestorLogoFarmacia(string? carpetaInstalacion = null)
{
    private readonly string _carpetaInstalacion = carpetaInstalacion ?? AppContext.BaseDirectory;

    public string GuardarNuevoLogo(string rutaLogoOrigen, string? rutaLogoActual)
    {
        if (!string.IsNullOrWhiteSpace(rutaLogoActual) && File.Exists(rutaLogoActual))
        {
            ArchivarLogoAnterior(rutaLogoActual);
        }

        var carpetaLogos = Path.Combine(_carpetaInstalacion, "logo");
        Directory.CreateDirectory(carpetaLogos);
        var rutaDestino = Path.Combine(carpetaLogos, $"logo{Path.GetExtension(rutaLogoOrigen)}");
        File.Copy(rutaLogoOrigen, rutaDestino, overwrite: true);
        return rutaDestino;
    }

    private void ArchivarLogoAnterior(string rutaLogoActual)
    {
        var carpetaHistorico = Path.Combine(_carpetaInstalacion, "logo", "historico");
        Directory.CreateDirectory(carpetaHistorico);
        var nombreArchivado = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}{Path.GetExtension(rutaLogoActual)}";
        File.Copy(rutaLogoActual, Path.Combine(carpetaHistorico, nombreArchivado), overwrite: false);
    }
}
