using Avalonia;
using Avalonia.Controls.Primitives;

namespace Spd.Presentacion.Controles;

public enum Severidad
{
    Ninguna,
    Apto,
    Aviso,
    Bloqueo
}

/// <summary>Franja de color a la izquierda de una fila con algo que atender: permite ver de un
/// vistazo qué filas necesitan acción sin leerlas (Spec 015 FR-1504).</summary>
public class FranjaSeveridad : TemplatedControl
{
    public static readonly StyledProperty<Severidad> SeveridadProperty =
        AvaloniaProperty.Register<FranjaSeveridad, Severidad>(nameof(Severidad));

    public Severidad Severidad
    {
        get => GetValue(SeveridadProperty);
        set => SetValue(SeveridadProperty, value);
    }
}
