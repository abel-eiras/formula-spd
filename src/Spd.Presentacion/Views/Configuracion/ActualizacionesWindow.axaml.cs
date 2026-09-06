using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Configuracion;

public partial class ActualizacionesWindow : Window
{
    public ActualizacionesWindow(IServicioActualizaciones servicio, int? administradorActualId)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        DataContext = new ActualizacionesViewModel(servicio, administradorActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public ActualizacionesWindow() => InitializeComponent();
}
