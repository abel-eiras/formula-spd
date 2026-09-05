namespace Spd.Dominio;

/// <summary>Recogida de residuos no SIGRE: material de acondicionamiento sobrante y similares
/// (FR-930). Los residuos de medicamentos van a SIGRE y se gestionan en Spec 005, no aquí.</summary>
public sealed class RecogidaResiduos
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public required string EmpresaGestora { get; set; }
    public int UsuarioId { get; set; }
    public string? Observaciones { get; set; }
}
