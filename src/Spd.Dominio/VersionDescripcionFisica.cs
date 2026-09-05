namespace Spd.Dominio;

/// <summary>Una versión histórica de la descripción física de un medicamento (FR-304), con su
/// periodo de vigencia ya cerrado. Solo lectura: `Medicamento_Hist` es de solo inserción (Art. III.3).</summary>
public sealed record VersionDescripcionFisica(
    string? DescForma, string? DescColor, string? DescRanura, string? DescSerigrafia,
    string? DescTamano, string? DescTexto, DateTime VigenteDesde, DateTime VigenteHasta);
