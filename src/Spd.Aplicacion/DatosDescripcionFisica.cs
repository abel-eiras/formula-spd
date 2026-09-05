namespace Spd.Aplicacion;

/// <summary>Campos de descripción física de un medicamento (FR-303). `DescTexto` es la propuesta
/// autogenerada o la corrección manual del profesional.</summary>
public sealed record DatosDescripcionFisica(
    string? DescForma, string? DescColor, string? DescRanura, string? DescSerigrafia,
    string? DescTamano, string? DescTexto);
