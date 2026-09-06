using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Configuracion;

public partial class UsuariosWindow : Window
{
    public UsuariosWindow(IServicioUsuarios servicio, int administradorActualId)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        DataContext = new UsuariosViewModel(servicio, administradorActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public UsuariosWindow() => InitializeComponent();
}
