using System.Text.RegularExpressions;

namespace Spd.Dominio;

/// <summary>Fila de muestra probada contra un patrón: el valor extraído si acertó, o null si el
/// patrón no coincidió o su grupo de captura no era numérico.</summary>
public sealed record FilaPrueba(string Texto, int? UnidadesExtraidas);

/// <summary>Resultado de FR-1110/CA-1101: cuántas filas de muestra aciertan y cuántas fallan,
/// antes de guardar el patrón.</summary>
public sealed record ResultadoPruebaExtraccion(IReadOnlyList<FilaPrueba> Filas)
{
    public int Aciertos => Filas.Count(f => f.UnidadesExtraidas is not null);
    public int Fallos => Filas.Count(f => f.UnidadesExtraidas is null);
}

/// <summary>Extrae el número de unidades por envase de una columna de texto libre del nomenclátor
/// vía una expresión regular con un grupo de captura (FR-1110/1111). Función pura: no accede al
/// lector real del nomenclátor (research.md Decisión 2 de Spec 011).</summary>
public static class ExtractorUnidadesEnvase
{
    public static ResultadoPruebaExtraccion Probar(IReadOnlyList<string> filasDeMuestra, string patron)
        => new(filasDeMuestra.Select(f => new FilaPrueba(f, Extraer(f, patron))).ToList());

    public static int? Extraer(string texto, string patron)
    {
        var coincidencia = Regex.Match(texto, patron);
        if (!coincidencia.Success || coincidencia.Groups.Count < 2) return null;
        return int.TryParse(coincidencia.Groups[1].Value, out var unidades) ? unidades : null;
    }
}
