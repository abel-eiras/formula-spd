using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Configuracion;

public partial class PerfilesImportacionWindow : Window
{
    public PerfilesImportacionWindow(IServicioPerfilesImportacion servicio, int? usuarioActualId)
    {
        InitializeComponent();
        DataContext = new PerfilesImportacionViewModel(servicio, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public PerfilesImportacionWindow() => InitializeComponent();
}
