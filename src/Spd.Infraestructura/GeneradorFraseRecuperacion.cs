using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Spd.Infraestructura;

/// <summary>Genera la clave de recuperación de 24 palabras (FR-1010/FR-1012, research.md
/// Decisión 4 de Spec 010) a partir de la lista oficial BIP39 en español (2048 palabras, dominio
/// público) — no implementa el resto del estándar BIP39 (checksum, mapeo de entropía exacto):
/// aquí es solo una codificación humana-amigable de palabras al azar, no una cartera de
/// criptomonedas.</summary>
public sealed partial class GeneradorFraseRecuperacion
{
    private const int NumeroPalabras = 24;

    [GeneratedRegex(@"wordlist-es\.txt$")]
    private static partial Regex NombreRecursoWordlist();

    private static readonly string[] Palabras = CargarWordlist();

    public string[] GenerarFrase()
    {
        var frase = new string[NumeroPalabras];
        for (var i = 0; i < NumeroPalabras; i++)
        {
            frase[i] = Palabras[RandomNumberGenerator.GetInt32(Palabras.Length)];
        }
        return frase;
    }

    private static string[] CargarWordlist()
    {
        var ensamblado = typeof(GeneradorFraseRecuperacion).Assembly;
        var nombreRecurso = ensamblado.GetManifestResourceNames().Single(n => NombreRecursoWordlist().IsMatch(n));
        using var flujo = ensamblado.GetManifestResourceStream(nombreRecurso)!;
        using var lector = new StreamReader(flujo);
        return lector.ReadToEnd()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
