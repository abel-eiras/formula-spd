using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Spd.Presentacion.Controles;

/// <summary>Mover el foco tras una lectura del escáner (spec-012): después de escanear solo queda
/// revisar las unidades, y con el foco allí un Intro guarda el envase sin tocar el ratón.</summary>
public static class Foco
{
    /// <summary>Diferido porque el panel de registro puede acabar de hacerse visible. Se enfoca el
    /// cuadro de texto interior del <see cref="NumericUpDown"/>, que es el que recibe las teclas.</summary>
    public static void LlevarA(Control raiz, string nombre) => Dispatcher.UIThread.Post(() =>
    {
        if (raiz.FindControl<Control>(nombre) is not { } campo) return;
        var interior = campo.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        if (interior is not null) interior.Focus();
        else campo.Focus();
    }, DispatcherPriority.Loaded);
}
