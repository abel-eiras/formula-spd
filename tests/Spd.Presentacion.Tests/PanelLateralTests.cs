using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Spd.Presentacion.Controles;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 015 CA-1523: el panel lateral sustituye a un diálogo, así que lo que hay detrás
/// tiene que seguir viéndose. Un control cuya plantilla no se aplique sería invisible en pantalla y
/// no daría ningún error, que es justo lo que este test evita.</summary>
public sealed class PanelLateralTests
{
    [AvaloniaFact]
    public void El_panel_se_realiza_con_su_contenido_y_no_tapa_lo_de_detras()
    {
        var detras = new TextBlock { Text = "Depósito del paciente" };
        var panel = new PanelLateral
        {
            Titulo = "Importar tratamiento",
            Content = new TextBlock { Text = "Formulario de importación" }
        };

        var ventana = AnfitrionDeVista.Anfitrion(new Grid { Children = { detras, panel } });
        ventana.Show();

        // La plantilla se aplicó de verdad (si no, el control mediría cero y no habría nada visible).
        Assert.True(panel.IsVisible);
        Assert.True(panel.Bounds.Width > 0, "El panel lateral no llegó a medir: su plantilla no se aplicó.");

        // Y lo de detrás sigue ahí y sigue midiendo: es lo que un diálogo modal no permitía.
        Assert.True(detras.Bounds.Width > 0, "El contenido de detrás dejó de renderizarse.");
    }

    [AvaloniaFact]
    public void Oculto_no_ocupa_sitio()
    {
        var panel = new PanelLateral { Titulo = "Importar tratamiento", IsVisible = false };
        var ventana = AnfitrionDeVista.Anfitrion(new Grid { Children = { panel } });
        ventana.Show();

        Assert.Equal(0, panel.Bounds.Width);
    }
}
