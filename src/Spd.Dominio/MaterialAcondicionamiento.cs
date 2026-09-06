namespace Spd.Dominio;

/// <summary>Catálogo simple de material de acondicionamiento (FR-632). Baja lógica, nunca
/// eliminación (Art. III.1).</summary>
public sealed class MaterialAcondicionamiento
{
    public int Id { get; set; }
    public required string Descripcion { get; set; }
    public required string Lote { get; set; }
    public DateOnly FechaEntrada { get; set; }
    public bool Activo { get; set; } = true;
}
