using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views;

public partial class RetiradaEnvasesWindow : Window
{
    public RetiradaEnvasesWindow(IServicioListadoRetirada servicioListadoRetirada, IServicioEnvases servicioEnvases, int? usuarioActualId)
    {
        InitializeComponent();
        DataContext = new RetiradaEnvasesViewModel(servicioListadoRetirada, servicioEnvases, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public RetiradaEnvasesWindow() => InitializeComponent();
}
