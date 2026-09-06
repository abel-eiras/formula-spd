using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Spd.Presentacion.Estilos;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 015 CA-1502. El fallo clásico de un tema a medias es una clave definida solo en una
/// variante: la aplicación acaba pintando texto de un tema sobre el fondo del otro. Aquí se recorren
/// las claves declaradas y se exige que resuelvan en clara y en oscura.</summary>
public sealed class TemaTests
{
    [AvaloniaFact]
    public void Toda_clave_de_color_resuelve_en_las_dos_variantes()
    {
        foreach (var clave in ClavesTema.Colores)
        {
            foreach (var variante in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                Assert.True(
                    Application.Current!.TryGetResource(clave, variante, out var valor) && valor is IBrush,
                    $"La clave de color «{clave}» no resuelve a un pincel en la variante {variante}.");
            }
        }
    }

    [AvaloniaFact]
    public void Las_fuentes_embebidas_y_los_tamanos_resuelven()
    {
        foreach (var clave in ClavesTema.Fuentes)
        {
            Assert.True(Application.Current!.TryGetResource(clave, ThemeVariant.Light, out var valor), clave);
            var familia = Assert.IsType<FontFamily>(valor);
            // Si la fuente embebida no se encontrara, Avalonia caería a la familia por defecto.
            Assert.Contains("IBM Plex", familia.Name);
        }

        foreach (var clave in ClavesTema.Tamanos)
        {
            Assert.True(Application.Current!.TryGetResource(clave, ThemeVariant.Light, out var valor), clave);
            Assert.IsType<double>(valor);
        }
    }

    [AvaloniaFact]
    public void El_acento_de_Fluent_es_el_de_la_paleta_en_ambas_variantes()
    {
        foreach (var variante in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            Assert.True(Application.Current!.TryGetResource("SystemAccentColor", variante, out var valor), $"SystemAccentColor en {variante}");
            Assert.IsType<Color>(valor);
        }
    }
}
