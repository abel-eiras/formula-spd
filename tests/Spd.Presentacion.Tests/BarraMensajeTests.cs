using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Spd.Presentacion.Controles;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 015 FR-1503 y Spec 014 FR-1402: la barra donde la aplicación contesta, con el
/// «¿Por qué?» que abre la explicación normativa de un bloqueo (Art. XI).</summary>
public sealed class BarraMensajeTests
{
    [AvaloniaFact]
    public void Sin_texto_no_ocupa_sitio_y_con_texto_se_realiza()
    {
        var barra = new BarraMensaje();
        var ventana = AnfitrionDeVista.Anfitrion(new StackPanel { Children = { barra } });
        ventana.Show();

        // Una barra vacía ocupando sitio es ruido: se oculta sola.
        Assert.False(barra.IsVisible);

        barra.Texto = "El representante legal necesita DNI (FR-022).";
        ventana.UpdateLayout();

        Assert.True(barra.IsVisible);
        Assert.True(barra.Bounds.Height > 0, "La plantilla de la barra no se aplicó.");

        // Y vuelve a esconderse al limpiarse el mensaje.
        barra.Texto = null;
        Assert.False(barra.IsVisible);
    }

    [AvaloniaFact]
    public void El_boton_porque_solo_aparece_cuando_hay_apartado_que_abrir()
    {
        var barra = new BarraMensaje { Texto = "No se puede verificar todavía." };
        var ventana = AnfitrionDeVista.Anfitrion(new StackPanel { Children = { barra } });
        ventana.Show();

        Assert.False(barra.HayApartado);

        barra.Apartado = "procedimiento:porque-de-los-bloqueos";
        ventana.UpdateLayout();

        Assert.True(barra.HayApartado);
    }

    /// <summary>Las dos pantallas que ya tenían el «¿Por qué?» copiado a mano ahora lo heredan del
    /// control; este test impide que vuelva a duplicarse el patrón.</summary>
    [Fact]
    public void Ninguna_vista_reimplementa_el_boton_porque_a_mano()
    {
        var raiz = SinIdentificadoresEnLaInterfazTests.RaizDePresentacion();
        var infracciones = System.IO.Directory
            .EnumerateFiles(raiz, "*.axaml", System.IO.SearchOption.AllDirectories)
            .Where(f => System.IO.File.ReadAllText(f).Contains("Porque_Click"))
            .Select(System.IO.Path.GetFileName)
            .ToList();

        Assert.True(infracciones.Count == 0,
            "Estas vistas reimplementan el botón «¿Por qué?» en vez de usar BarraMensaje: "
            + string.Join(", ", infracciones));
    }
}
