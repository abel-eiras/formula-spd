namespace Spd.Dominio;

/// <summary>Una fila de envase bajo una línea de SPD (FR-611: normal tener varias por línea).
/// Copia marca, CN, descripción, serie, lote y caducidad en el momento del descuento; la
/// documentación entregada no depende del catálogo vivo ni del estado posterior de la caja.</summary>
public sealed class SpdLineaEnvase
{
    public int Id { get; set; }
    public required int SpdLineaId { get; set; }
    public required int EnvaseId { get; set; }
    public decimal UnidadesTomadas { get; set; }
    // La marca real puede diferir de la prescrita si comparten grupo homogéneo (Art. IV.3).
    public string? SnapCn { get; set; }
    public string? SnapNombre { get; set; }
    public string? SnapDescTexto { get; set; }
    public string? SnapSerie { get; set; }
    public string? SnapLote { get; set; }
    public DateOnly? SnapCaducidad { get; set; }
}
