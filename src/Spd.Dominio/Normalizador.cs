using System.Globalization;
using System.Text;

namespace Spd.Dominio;

/// <summary>Normaliza texto para búsquedas sin distinguir mayúsculas ni tildes (FR-010, FR-032,
/// CA-011 de Spec 001; FR-305 de Spec 003 — mismo helper compartido, sin duplicarlo por catálogo).
/// Sin extensión SQLite: la comparación siempre pasa por aquí, en ambos lados (research.md
/// Decisión 1 de Spec 001).</summary>
public static class Normalizador
{
    public static string QuitarTildesYMayusculas(string texto)
    {
        var descompuesto = texto.Normalize(NormalizationForm.FormD);
        var sinDiacriticos = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sinDiacriticos.Append(c);
            }
        }
        return sinDiacriticos.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
