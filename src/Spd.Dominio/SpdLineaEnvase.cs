namespace Spd.Dominio;

/// <summary>Una fila de envase bajo una línea de SPD (FR-611: normal tener varias por línea).
/// Copia serie/lote/caducidad del envase en el momento del descuento, para que la instantánea no
/// dependa de que el envase original siga existiendo con esos datos.</summary>
public sealed class SpdLineaEnvase
{
    public int Id { get; set; }
    public required int SpdLineaId { get; set; }
    public required int EnvaseId { get; set; }
    public decimal UnidadesTomadas { get; set; }
    public string? SnapSerie { get; set; }
    public string? SnapLote { get; set; }
    public DateOnly? SnapCaducidad { get; set; }
}
