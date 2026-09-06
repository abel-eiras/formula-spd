using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Configuracion;

public partial class SeguridadWindow : Window
{
    public SeguridadWindow(IServicioCifrado servicio, IServicioPurga servicioPurga, int administradorActualId)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        DataContext = new SeguridadViewModel(servicio, servicioPurga, administradorActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public SeguridadWindow() => InitializeComponent();
}
