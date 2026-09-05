namespace Spd.Dominio;

/// <summary>Control de cambios del procedimiento en papel de la farmacia (FR-940). Registro
/// administrativo del cambio del PNT, no del código de la aplicación. Exclusivo de Administrador
/// (FR-942, research.md Decisión 4 de Spec 009).</summary>
public sealed class ControlCambiosPNT
{
    public int Id { get; set; }
    public required string Documento { get; set; }
    public required string Version { get; set; }
    public required string DescripcionCambio { get; set; }
    public DateOnly Fecha { get; set; }
    public int RedactadoPor { get; set; }
    public int RevisadoPor { get; set; }
    public int AprobadoPor { get; set; }
}
