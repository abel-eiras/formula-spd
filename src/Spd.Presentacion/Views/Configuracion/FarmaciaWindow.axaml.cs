using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Infraestructura;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Configuracion;

public partial class FarmaciaWindow : Window
{
    public FarmaciaWindow(IServicioConfiguracionFarmacia servicio, GestorLogoFarmacia gestorLogo, int? administradorActualId)
    {
        InitializeComponent();
        DataContext = new FarmaciaViewModel(servicio, gestorLogo, administradorActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public FarmaciaWindow() => InitializeComponent();
}
