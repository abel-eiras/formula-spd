using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.RegistrosCalidad;

public partial class ControlDocumentalWindow : Window
{
    public ControlDocumentalWindow(IServicioControlDocumental servicio, int administradorActualId)
    {
        InitializeComponent();
        DataContext = new ControlDocumentalViewModel(servicio, administradorActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public ControlDocumentalWindow() => InitializeComponent();
}
