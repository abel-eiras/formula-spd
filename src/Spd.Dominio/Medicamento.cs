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
    /// <summary>Aptitud para SPD (FR-301). <c>null</c> = **sin confirmar**: ni apto ni no apto, sino que
    /// nadie lo ha decidido todavía. Es el estado de todo medicamento nuevo, porque el nomenclátor no
    /// trae ese dato; lo confirma el farmacéutico al elaborar (Spec 006, decisión del propietario del
    /// 2026-09-14). Un «no apto» explícito es una decisión clínica y ninguna confirmación en bloque lo
    /// cambia.</summary>
    public bool? AptoSpd { get; set; }
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
