namespace Spd.Dominio;

/// <summary>Trazabilidad de a quién se entregó cada copia controlada del PNT en papel (FR-941).
/// Exclusivo de Administrador (FR-942).</summary>
public sealed class ControlCopias
{
    public int Id { get; set; }
    public required string Documento { get; set; }
    public int NumCopia { get; set; }
    public int UsuarioId { get; set; }
    public DateOnly Fecha { get; set; }
}
