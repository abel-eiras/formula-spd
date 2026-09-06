using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Spd.Presentacion.Controles;

/// <summary>Panel lateral que sustituye a un diálogo (Spec 015 FR-1533).
///
/// Lo que resuelve: registrar un envase, dar de alta a un representante o contestar al médico eran
/// ventanas que tapaban justo el dato que hacía falta consultar para rellenarlas. Como panel, el
/// formulario aparece al lado y lo de detrás se sigue viendo y se sigue pudiendo leer.
///
/// Es un <see cref="ContentControl"/>: el contenido es el formulario de cada caso, y el control solo
/// aporta el marco, el título y el cierre. Su plantilla vive en `Estilos/Controles.axaml`.</summary>
public class PanelLateral : ContentControl
{
    public static readonly StyledProperty<string?> TituloProperty =
        AvaloniaProperty.Register<PanelLateral, string?>(nameof(Titulo));

    /// <summary>El botón de cerrar del marco. Se enlaza al comando que apaga el panel en el
    /// ViewModel, para que cerrar sea siempre lo mismo lo abra quien lo abra.</summary>
    public static readonly StyledProperty<System.Windows.Input.ICommand?> CerrarCommandProperty =
        AvaloniaProperty.Register<PanelLateral, System.Windows.Input.ICommand?>(nameof(CerrarCommand));

    public string? Titulo
    {
        get => GetValue(TituloProperty);
        set => SetValue(TituloProperty, value);
    }

    public System.Windows.Input.ICommand? CerrarCommand
    {
        get => GetValue(CerrarCommandProperty);
        set => SetValue(CerrarCommandProperty, value);
    }
}
