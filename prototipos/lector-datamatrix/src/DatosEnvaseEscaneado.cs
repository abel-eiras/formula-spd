namespace LectorDataMatrix;

/// <summary>
/// Fecha de caducidad tal como viene codificada en el AI (17) de GS1: AAMMDD.
/// Dia puede venir a 0 cuando el fabricante no especifica dia del mes (convencion habitual de GS1);
/// esta clase no decide que hacer con ese caso, solo lo expone tal cual para que lo decida quien integre esto.
/// </summary>
public sealed record FechaCaducidadGs1(int Anio, int Mes, int Dia)
{
    public bool DiaSinEspecificar => Dia == 0;
}

/// <summary>
/// Datos extraidos de un DataMatrix GS1 de un envase de medicamento: GTIN, lote, caducidad,
/// numero de serie y, si el fabricante lo incluye (AI 712), el Codigo Nacional espanol directamente.
/// </summary>
public sealed record DatosEnvaseEscaneado(
    string Gtin,
    string Lote,
    FechaCaducidadGs1 Caducidad,
    string NumeroSerie,
    string? CodigoNacional);
