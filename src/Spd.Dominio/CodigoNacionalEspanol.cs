using System.Text.RegularExpressions;

namespace Spd.Dominio;

/// <summary>Deduce el Código Nacional (CN) de un medicamento español a partir de su código de barras
/// (spec-012 FR-1209). En España el GTIN de un medicamento se construye con el prefijo GS1 `847000`
/// seguido de los seis dígitos del CN y un dígito de control GS1: el GTIN-14 del DataMatrix es
/// `0` + `847000` + CN + control, y el EAN-13 lineal de los medicamentos sin receta es el mismo sin el
/// cero inicial. Así casi todas las cajas españolas dan el CN aunque el fabricante no incluya el AI 712.
///
/// El dígito de control propio del CN (el séptimo dígito que a veces se escribe tras un punto,
/// «712729.3») NO se calcula ni se valida: el catálogo se identifica por los seis dígitos y una fórmula
/// sin contrastar rechazaría CN buenos.</summary>
public static partial class CodigoNacionalEspanol
{
    [GeneratedRegex(@"^0?847000(\d{6})\d$")]
    private static partial Regex PatronGtinEspanol();

    [GeneratedRegex(@"^\d{7}$")]
    private static partial Regex PatronCnConControl();

    /// <summary>CN de seis dígitos si el GTIN (14 dígitos) o EAN (13 dígitos) es de un medicamento
    /// español y su dígito de control GS1 es correcto; null en cualquier otro caso (un GTIN extranjero
    /// no lleva el CN dentro).</summary>
    public static string? DesdeGtin(string? gtin)
    {
        if (gtin is null) return null;
        var texto = gtin.Trim();
        var coincidencia = PatronGtinEspanol().Match(texto);
        if (!coincidencia.Success || !Gs1.DigitoControlCorrecto(texto)) return null;
        return coincidencia.Groups[1].Value;
    }

    /// <summary>El CN del AI 712 tal como se guarda en el catálogo: seis dígitos. Si llega con el dígito
    /// de control del CN (siete dígitos) se quita; cualquier otra forma se devuelve tal cual.</summary>
    public static string? SinDigitoDeControl(string? cnDelCodigo)
    {
        if (string.IsNullOrWhiteSpace(cnDelCodigo)) return null;
        var cn = cnDelCodigo.Trim();
        return PatronCnConControl().IsMatch(cn) ? cn[..6] : cn;
    }

    /// <summary>Formas de CN con las que buscar en el catálogo, por orden de preferencia: el AI 712 tal
    /// cual, sus seis primeros dígitos si viene con el dígito de control del CN (siete dígitos, como en
    /// los códigos reales escaneados), y el CN deducido del GTIN.</summary>
    public static IReadOnlyList<string> Candidatos(string? cnDelCodigo, string? gtin)
    {
        var candidatos = new List<string>();
        if (!string.IsNullOrWhiteSpace(cnDelCodigo)) candidatos.Add(cnDelCodigo.Trim());
        if (SinDigitoDeControl(cnDelCodigo) is { } seisDigitos) candidatos.Add(seisDigitos);
        if (DesdeGtin(gtin) is { } deducido) candidatos.Add(deducido);
        return candidatos.Distinct().ToList();
    }
}

/// <summary>Reglas generales de los códigos GS1 que no dependen del país.</summary>
public static class Gs1
{
    /// <summary>Comprueba el dígito de control GS1 (módulo 10, pesos 3 y 1 desde la derecha) de un
    /// GTIN-8/12/13/14. Una lectura con el dígito de control mal no es un GTIN: es un error de lectura.</summary>
    public static bool DigitoControlCorrecto(string codigo)
    {
        if (codigo.Length < 8 || !codigo.All(char.IsAsciiDigit)) return false;
        var suma = 0;
        for (var i = 0; i < codigo.Length - 1; i++)
        {
            var digito = codigo[codigo.Length - 2 - i] - '0';
            suma += digito * (i % 2 == 0 ? 3 : 1);
        }
        return (10 - suma % 10) % 10 == codigo[^1] - '0';
    }
}
