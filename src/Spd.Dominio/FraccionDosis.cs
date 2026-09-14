using System.Text.RegularExpressions;

namespace Spd.Dominio;

/// <summary>Vocabulario cerrado de fracciones para la posología D/A/C/N (FR-402, research.md
/// Decisión 2 de Spec 004) — nunca un decimal libre, para que la impresión en fracción (Spec 007
/// FR-741) sea siempre exacta, no una aproximación.
///
/// **Se guarda por nombre, no por posición**: las columnas `pauta_*` y `snap_pauta_*` son TEXT y se
/// leen con `Enum.Parse`. Por eso se pueden añadir valores sin migración, pero **nunca renombrar uno
/// existente**: cada instantánea de un SPD ya entregado (Art. IV.3) dejaría de poder leerse.</summary>
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
    UnoYMedio,
    // Ampliación del 2026-09-14, decidida por el propietario: dosis de 2 y 3 unidades, y los tercios
    // y tres cuartos por encima de la unidad. Van al final por claridad; al guardarse por nombre, el
    // orden no afecta a lo ya almacenado.
    UnoYTercio,
    UnoYDosTercios,
    UnoYTresCuartos,
    Dos,
    Tres
}

public static class FraccionDosisExtensiones
{
    /// <summary>Los valores que se pueden teclear, en el orden en que el propietario los enumeró.
    /// Es también el texto del aviso cuando se teclea otra cosa.</summary>
    public static readonly IReadOnlyList<FraccionDosis> Admitidas =
    [
        FraccionDosis.Cero, FraccionDosis.Uno, FraccionDosis.Dos, FraccionDosis.Tres,
        FraccionDosis.UnCuarto, FraccionDosis.Media, FraccionDosis.UnTercio, FraccionDosis.TresCuartos,
        FraccionDosis.DosTercios, FraccionDosis.UnoYMedio, FraccionDosis.UnoYCuarto,
        FraccionDosis.UnoYTercio, FraccionDosis.UnoYDosTercios, FraccionDosis.UnoYTresCuartos
    ];

    /// <summary>La dosis en **doceavos** de unidad: un entero exacto.
    ///
    /// Todas las fracciones admitidas son múltiplos de 1/12 (1/4 = 3/12, 1/3 = 4/12), así que en
    /// doceavos la suma de una semana es aritmética entera, sin redondeo. En `decimal` no lo es:
    /// `1m/3m` vale 0,333…3, y tres tercios suman 0,999…9 — no 1. Esa diferencia invisible cambiaba
    /// el resultado de la regla «entero + 1» y el techo del listado de retirada.</summary>
    public static int Doceavos(this FraccionDosis fraccion) => fraccion switch
    {
        FraccionDosis.Cero => 0,
        FraccionDosis.UnCuarto => 3,
        FraccionDosis.UnTercio => 4,
        FraccionDosis.Media => 6,
        FraccionDosis.DosTercios => 8,
        FraccionDosis.TresCuartos => 9,
        FraccionDosis.Uno => 12,
        FraccionDosis.UnoYCuarto => 15,
        FraccionDosis.UnoYTercio => 16,
        FraccionDosis.UnoYMedio => 18,
        FraccionDosis.UnoYDosTercios => 20,
        FraccionDosis.UnoYTresCuartos => 21,
        FraccionDosis.Dos => 24,
        FraccionDosis.Tres => 36,
        _ => throw new ArgumentOutOfRangeException(nameof(fraccion))
    };

    public static decimal Valor(this FraccionDosis fraccion) => fraccion.Doceavos() / 12m;

    /// <summary>La notación **impresa** en los documentos (FR-741). No se ha cambiado para los valores
    /// que ya existían: el Art. IV.3 exige que lo impreso para un SPD se reconstruya desde su
    /// instantánea, y reimprimir con otra notación un SPD ya entregado daría un papel distinto del
    /// original.</summary>
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
        FraccionDosis.UnoYTercio => "1 1/3",
        FraccionDosis.UnoYMedio => "1 1/2",
        FraccionDosis.UnoYDosTercios => "1 2/3",
        FraccionDosis.UnoYTresCuartos => "1 3/4",
        FraccionDosis.Dos => "2",
        FraccionDosis.Tres => "3",
        _ => throw new ArgumentOutOfRangeException(nameof(fraccion))
    };

    /// <summary>La notación con la que se **teclea** y se muestra en pantalla: «1+1/2». Sin
    /// ambigüedad: «1 1/2» con un espacio estrecho o mal alineado se lee «11/2».</summary>
    public static string TextoEntrada(this FraccionDosis fraccion) => fraccion.Texto().Replace(' ', '+');

    public static string TextoAdmitidas => string.Join(", ", Admitidas.Select(f => f.TextoEntrada()));

    /// <summary>Interpreta lo tecleado en una casilla de pauta.
    ///
    /// Es estricto con el **valor** —solo el vocabulario cerrado; «0,5» o «0.33» se rechazan (FR-402,
    /// CA-710)— y tolerante solo con la **forma**: espacios alrededor del «+», y la notación con
    /// espacio de los documentos («1 1/2»), que es la que alguien copiará de un papel.
    ///
    /// Vacío significa «sin dosis en esa toma» y devuelve <c>null</c>, que no es lo mismo que teclear
    /// «0».</summary>
    public static bool TryParsear(string? texto, out FraccionDosis? fraccion)
    {
        fraccion = null;
        if (string.IsNullOrWhiteSpace(texto)) return true;

        var normalizado = Regex.Replace(texto.Trim(), @"\s*\+\s*", "+");
        normalizado = Regex.Replace(normalizado, @"^(\d)\s+(\d/\d)$", "$1+$2");

        foreach (var candidata in Admitidas)
        {
            if (candidata.TextoEntrada() == normalizado)
            {
                fraccion = candidata;
                return true;
            }
        }

        return false;
    }
}
