using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Aloja una vista en una ventana anfitriona (Spec 015 H1.6, research.md Decisión 7).
///
/// Con el marco único las pantallas ya no son ventanas, pero la cobertura que aportaban estos tests
/// sigue siendo necesaria: la regresión F5 de Spec 001 (un `ListBox` con `ItemTemplate` y un comando
/// enlazado al ancestro cerraba la aplicación con SIGABRT) solo aparece cuando la plantilla se
/// **realiza**, y para eso hace falta mostrar una ventana de verdad. Cada test sigue llamando a
/// `Show()` sobre lo que devuelve este método, igual que antes.</summary>
internal static class AnfitrionDeVista
{
    public static Window Anfitrion(Control vista) => new() { Content = vista, Width = 1100, Height = 760 };

    public static void ComprobarScrollHastaElUltimo(Window ventana, ListBox lista)
    {
        ventana.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        var scroll = lista.GetVisualDescendants().OfType<ScrollViewer>().First();
        Assert.True(scroll.Extent.Height > scroll.Viewport.Height);
        lista.ScrollIntoView(lista.Items[^1]!);
        ventana.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        ventana.UpdateLayout();
        Assert.True(scroll.Offset.Y > 0);
        var ultimo = lista.ContainerFromIndex(lista.ItemCount - 1);
        Assert.NotNull(ultimo);
        Assert.True(ultimo!.IsEffectivelyVisible);
        var posicion = ultimo.TranslatePoint(default, lista);
        Assert.NotNull(posicion);
        Assert.True(posicion!.Value.Y < lista.Bounds.Height && posicion.Value.Y + ultimo.Bounds.Height > 0);
    }
}
