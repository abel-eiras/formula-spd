using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Spd.Dominio;

/// <summary>Propuesta de descripción física sacada del texto de la sección 3 («Forma farmacéutica») de
/// la ficha técnica (Spec 003 FR-312). Es solo una **propuesta**: rellena los campos para que la
/// persona los revise y corrija antes de guardar (Art. I.2: lo que se imprime lo decide el
/// farmacéutico). Nada se inventa: lo que el texto no dice queda vacío.</summary>
public sealed record PropuestaDescripcionFisica(
    string? Forma, string? Color, string? Ranura, string? Serigrafia, string? Tamano, IReadOnlyList<string> Avisos)
{
    /// <summary>La ranura de la ficha técnica puede servir solo para facilitar la deglución, no para
    /// dividir la dosis: si aparece, se avisa siempre.</summary>
    public const string AvisoRanura = "La ranura puede no servir para dividir la dosis: compruébalo en la ficha técnica.";
    public const string AvisoSinDatos = "El texto no describe el aspecto: completa la descripción mirando el comprimido.";

    public bool Vacia => Forma is null && Color is null && Ranura is null && Serigrafia is null && Tamano is null;
}

/// <summary>Analiza el texto de la ficha técnica sin expresiones con retroceso catastrófico; cada
/// patrón recorre el texto una vez (Art. IX.4: la pantalla no debe congelarse con un texto largo).</summary>
public static partial class AnalizadorDescripcionFisica
{
    // Orden: lo más específico primero («cápsula dura» antes que «cápsula»).
    private static readonly (string Patron, string Forma)[] Tipos =
    [
        ("comprimido recubierto con pelicula", "comprimido recubierto"),
        ("comprimidos recubiertos con pelicula", "comprimido recubierto"),
        ("comprimido recubierto", "comprimido recubierto"),
        ("comprimidos recubiertos", "comprimido recubierto"),
        ("comprimido gastrorresistente", "comprimido gastrorresistente"),
        ("comprimidos gastrorresistentes", "comprimido gastrorresistente"),
        ("comprimido de liberacion prolongada", "comprimido de liberación prolongada"),
        ("comprimidos de liberacion prolongada", "comprimido de liberación prolongada"),
        ("capsula dura", "cápsula dura"),
        ("capsulas duras", "cápsula dura"),
        ("capsula blanda", "cápsula blanda"),
        ("capsulas blandas", "cápsula blanda"),
        ("capsula", "cápsula"),
        ("capsulas", "cápsula"),
        ("gragea", "gragea"),
        ("grageas", "gragea"),
        ("comprimido", "comprimido"),
        ("comprimidos", "comprimido"),
    ];

    private static readonly (string Patron, string Forma)[] FormasGeometricas =
    [
        ("redond", "redondo"), ("circular", "redondo"), ("oblong", "oblongo"), ("alargad", "oblongo"),
        ("oval", "ovalado"), ("capsular", "oblongo"), ("triangular", "triangular"), ("cuadrad", "cuadrado"),
        ("hexagonal", "hexagonal"), ("octogonal", "octogonal"), ("romboidal", "romboidal"), ("pentagonal", "pentagonal"),
    ];

    private static readonly string[] Colores =
    [
        "blanco", "blanquecino", "amarillo", "naranja", "anaranjado", "rosa", "rosado", "rojo", "granate", "marron",
        "beige", "crema", "verde", "azul", "celeste", "violeta", "lila", "morado", "gris", "negro", "ocre", "salmon", "purpura"
    ];

    private static readonly Dictionary<string, string> ConTilde = new()
    {
        ["marron"] = "marrón", ["salmon"] = "salmón", ["purpura"] = "púrpura"
    };

    [GeneratedRegex(@"[""“«']([^""”»']{1,15})[""”»']")]
    private static partial Regex Comillas();

    [GeneratedRegex(@"(\d+(?:[.,]\d+)?)\s*(?:x|×)\s*(\d+(?:[.,]\d+)?)\s*mm")]
    private static partial Regex Dimensiones();

    [GeneratedRegex(@"(?:diametro|longitud|tamano)[^0-9]{0,30}(\d+(?:[.,]\d+)?)\s*mm|(\d+(?:[.,]\d+)?)\s*mm\s*de\s*diametro")]
    private static partial Regex Diametro();

    public static PropuestaDescripcionFisica Analizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return new PropuestaDescripcionFisica(null, null, null, null, null, [PropuestaDescripcionFisica.AvisoSinDatos]);

        var normal = Normalizar(texto);
        var avisos = new List<string>();

        var tipo = Tipos.FirstOrDefault(t => ContienePalabra(normal, t.Patron)).Forma;
        var geometrica = FormasGeometricas.FirstOrDefault(f => normal.Contains(f.Patron, StringComparison.Ordinal)).Forma;
        var forma = string.Join(" ", new[] { tipo, geometrica }.Where(s => s is not null));

        var color = DetectarColor(normal, avisos);
        var ranura = DetectarRanura(normal);
        if (ranura is not null) avisos.Add(PropuestaDescripcionFisica.AvisoRanura);

        var serigrafia = DetectarSerigrafia(texto, normal);
        var tamano = DetectarTamano(normal);

        var propuesta = new PropuestaDescripcionFisica(
            forma.Length > 0 ? forma : null, color, ranura, serigrafia, tamano, avisos);
        return propuesta.Vacia ? propuesta with { Avisos = [PropuestaDescripcionFisica.AvisoSinDatos] } : propuesta;
    }

    // Los colores se buscan como palabra (y sus femeninos/plurales: «blanca», «blancos»). «Blanco o casi
    // blanco» se conserva entero: simplificarlo a «blanco» sería decir más de lo que dice la ficha.
    private static string? DetectarColor(string normal, List<string> avisos)
    {
        if (normal.Contains("blanco o casi blanco", StringComparison.Ordinal) || normal.Contains("blanca o casi blanca", StringComparison.Ordinal))
            return "blanco o casi blanco";

        var palabras = Palabras(normal);
        var encontrados = new List<string>();
        foreach (var palabra in palabras)
        {
            var raiz = Colores.FirstOrDefault(c => EsFormaDe(palabra, c));
            if (raiz is null) continue;
            var nombre = ConTilde.GetValueOrDefault(raiz, raiz);
            if (!encontrados.Contains(nombre)) encontrados.Add(nombre);
        }
        if (encontrados.Count == 0) return null;
        if (encontrados.Count > 1)
            avisos.Add("Aparece más de un color (cápsula bicolor, cubierta y núcleo, o varias dosis en la misma ficha): revisa cuál corresponde a esta presentación.");
        return string.Join(" y ", encontrados);
    }

    private static bool EsFormaDe(string palabra, string color)
    {
        if (palabra == color) return true;
        var raiz = color.EndsWith('o') ? color[..^1] : color;
        return palabra is var p && (p == raiz + "a" || p == raiz + "os" || p == raiz + "as" || p == color + "s" || p == color + "es");
    }

    private static string? DetectarRanura(string normal)
    {
        if (!normal.Contains("ranura", StringComparison.Ordinal) && !normal.Contains("ranurad", StringComparison.Ordinal)) return null;
        if (normal.Contains("sin ranura", StringComparison.Ordinal)) return null;
        if (normal.Contains("ambas caras", StringComparison.Ordinal) || normal.Contains("las dos caras", StringComparison.Ordinal)) return "en ambas caras";
        if (normal.Contains("en una cara", StringComparison.Ordinal) || normal.Contains("en un lado", StringComparison.Ordinal)
            || normal.Contains("en la otra", StringComparison.Ordinal)) return "en una cara";
        return "sí";
    }

    // Las marcas grabadas suelen ir entre comillas («M 20», "A1"). Se toman del texto original para
    // conservar mayúsculas y signos; solo si el texto habla de grabado, inscripción o marca.
    private static string? DetectarSerigrafia(string original, string normal)
    {
        var hablaDeMarca = new[] { "grabad", "inscri", "marcad", "impres", "serigraf", "relieve", "logotipo" }
            .Any(p => normal.Contains(p, StringComparison.Ordinal));
        if (!hablaDeMarca) return null;
        var marcas = Comillas().Matches(original).Select(m => m.Groups[1].Value.Trim()).Where(m => m.Length > 0).Distinct().ToList();
        return marcas.Count == 0 ? null : string.Join(" / ", marcas);
    }

    private static string? DetectarTamano(string normal)
    {
        var dimensiones = Dimensiones().Match(normal);
        if (dimensiones.Success) return $"{dimensiones.Groups[1].Value} × {dimensiones.Groups[2].Value} mm";
        var diametro = Diametro().Match(normal);
        if (!diametro.Success) return null;
        var valor = diametro.Groups[1].Success ? diametro.Groups[1].Value : diametro.Groups[2].Value;
        return $"{valor} mm";
    }

    private static bool ContienePalabra(string normal, string frase)
    {
        var indice = 0;
        while ((indice = normal.IndexOf(frase, indice, StringComparison.Ordinal)) >= 0)
        {
            var inicioOk = indice == 0 || !char.IsLetter(normal[indice - 1]);
            var fin = indice + frase.Length;
            var finOk = fin == normal.Length || !char.IsLetter(normal[fin]);
            if (inicioOk && finOk) return true;
            indice = fin;
        }
        return false;
    }

    private static IEnumerable<string> Palabras(string normal)
        => normal.Split((char[])[' ', ',', '.', ';', ':', '(', ')', '/', '-', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);

    /// <summary>Minúsculas y sin tildes, para comparar sin depender de cómo esté escrita la ficha.</summary>
    private static string Normalizar(string texto)
    {
        var descompuesto = texto.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) resultado.Append(c);
        }
        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }
}
