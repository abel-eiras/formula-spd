namespace Spd.Dominio;

/// <summary>Catálogo de medicamentos por CN (Art. IV). La descripción física se edita en
/// cualquier momento para todo uso futuro, pero cada cambio se versiona en Medicamento_Hist sin
/// afectar a instantáneas ya congeladas de otras specs (Art. IV.2/IV.3).</summary>
public sealed class Medicamento
{
    public int Id { get; set; }
    public required string Cn { get; set; }
    public required string Nombre { get; set; }
    public string NombreNormalizado { get; set; } = string.Empty;
    public string? PrincipioActivo { get; set; }
    public string? Laboratorio { get; set; }
    public FormaFarmaceutica? FormaFarmaceutica { get; set; }
    public bool AptoSpd { get; set; } = true;
    public string? MotivoNoApto { get; set; }
    public bool Fraccionable { get; set; }
    public int? UnidadesEnvase { get; set; }
    public OrigenUnidadesEnvase UnidadesEnvaseOrigen { get; set; } = OrigenUnidadesEnvase.Manual;
    public string? DescForma { get; set; }
    public string? DescColor { get; set; }
    public string? DescRanura { get; set; }
    public string? DescSerigrafia { get; set; }
    public string? DescTamano { get; set; }
    public string? DescTexto { get; set; }
    public DateTime DescVigenteDesde { get; set; }
    public string? Gtin { get; set; }
    public bool Activo { get; set; } = true;
}
