using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Spd.Presentacion.Views.Pacientes;

public partial class IdoneidadConsentimientoView : UserControl
{
    public IdoneidadConsentimientoView() => InitializeComponent();

    // Spec 014 FR-1402: observaciones obligatorias, representante con DNI, firma futura… tienen
    // razón normativa; el botón abre su explicación.
    private void Porque_Click(object? sender, RoutedEventArgs e)
        => AyudaContextual.Abrir("procedimiento", "porque-de-los-bloqueos");
}
