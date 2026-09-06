using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Spd.Presentacion.Views.Preparacion;

public partial class PreparacionView : UserControl
{
    public PreparacionView() => InitializeComponent();

    // Spec 014 FR-1402: los bloqueos de esta pantalla (idoneidad, faltantes, verificador =
    // elaborador, estado del SPD…) tienen razón normativa; el botón abre su explicación.
    private void Porque_Click(object? sender, RoutedEventArgs e) => AyudaWindow.Abrir("procedimiento", "porque-de-los-bloqueos");
}
