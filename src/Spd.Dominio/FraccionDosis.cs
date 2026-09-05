namespace Spd.Dominio;

/// <summary>Vocabulario cerrado de fracciones para la posología D/A/C/N (FR-402, research.md
/// Decisión 2 de Spec 004) — nunca un decimal libre, para que la impresión en fracción (Spec 007
/// FR-741) sea siempre exacta, no una aproximación.</summary>
public enum FraccionDosis
{
    Cero,
    UnCuarto,
    UnTercio,
    Media,
    DosTercios,
    TresCuartos,
    Uno,
    UnoYCuarto,
    UnoYMedio
}

public static class FraccionDosisExtensiones
{
    public static decimal Valor(this FraccionDosis fraccion) => fraccion switch
    {
        FraccionDosis.Cero => 0m,
        FraccionDosis.UnCuarto => 0.25m,
        FraccionDosis.UnTercio => 1m / 3m,
        FraccionDosis.Media => 0.5m,
        FraccionDosis.DosTercios => 2m / 3m,
        FraccionDosis.TresCuartos => 0.75m,
        FraccionDosis.Uno => 1m,
        FraccionDosis.UnoYCuarto => 1.25m,
        FraccionDosis.UnoYMedio => 1.5m,
        _ => throw new ArgumentOutOfRangeException(nameof(fraccion))
    };

    public static string Texto(this FraccionDosis fraccion) => fraccion switch
    {
        FraccionDosis.Cero => "0",
        FraccionDosis.UnCuarto => "1/4",
        FraccionDosis.UnTercio => "1/3",
        FraccionDosis.Media => "1/2",
        FraccionDosis.DosTercios => "2/3",
        FraccionDosis.TresCuartos => "3/4",
        FraccionDosis.Uno => "1",
        FraccionDosis.UnoYCuarto => "1 1/4",
        FraccionDosis.UnoYMedio => "1 1/2",
        _ => throw new ArgumentOutOfRangeException(nameof(fraccion))
    };
}
