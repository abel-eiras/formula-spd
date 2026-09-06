namespace Spd.Dominio;

/// <summary>Una línea de SPD: un medicamento incluido en ese blíster, con instantánea completa
/// (Art. IV.3) — nunca se reconstruye desde el catálogo actual.</summary>
public sealed class SpdLinea
{
    public int Id { get; set; }
    public required int SpdId { get; set; }
    public required int TratamientoId { get; set; }
    public required int MedicamentoId { get; set; }
    public required string SnapNombre { get; set; }
    public required string SnapCn { get; set; }
    public FraccionDosis? SnapPautaD { get; set; }
    public FraccionDosis? SnapPautaA { get; set; }
    public FraccionDosis? SnapPautaC { get; set; }
    public FraccionDosis? SnapPautaN { get; set; }
    public required string SnapDiasSemana { get; set; }
    public string? SnapDescTexto { get; set; }
    public string? SnapMomento { get; set; }
    public decimal UnidadesDosis { get; set; }
    public int UnidadesEnvase { get; set; }
    public string? Incidencias { get; set; }
    public EstadoLinea EstadoLinea { get; set; } = EstadoLinea.Normal;
    public string? MotivoExclusion { get; set; }
}
