using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.RegistrosCalidad;

public partial class RegistrosCalidadWindow : Window
{
    public RegistrosCalidadWindow(IServicioRegistrosCalidad servicio, int usuarioActualId)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        FormacionView.DataContext = new FormacionPersonalViewModel(servicio, usuarioActualId);
        ResiduosView.DataContext = new RecogidaResiduosViewModel(servicio, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public RegistrosCalidadWindow() => InitializeComponent();
}
