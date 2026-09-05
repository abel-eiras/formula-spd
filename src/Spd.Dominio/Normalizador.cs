using System.Globalization;
using System.Text;

namespace Spd.Dominio;

/// <summary>Normaliza texto para búsquedas sin distinguir mayúsculas ni tildes (FR-305). Sin
/// extensión SQLite: la comparación siempre pasa por aquí, en ambos lados (mismo patrón que
/// research.md Decisión 1 de Spec 001).
///
/// Nota de coordinación entre ramas: este fichero existe idéntico en la rama
/// `001-pacientes-y-medicos` (todavía sin mergear en `main`); al integrar ambas ramas quedará un
/// solo fichero.</summary>
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
