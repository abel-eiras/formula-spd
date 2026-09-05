using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Configuracion;

public partial class NomenclatorWindow : Window
{
    public NomenclatorWindow(
        IServicioConfiguracionFarmacia servicioFarmacia, IServicioNomenclator servicioNomenclator, int? administradorActualId)
    {
        InitializeComponent();
        DataContext = new NomenclatorViewModel(servicioFarmacia, servicioNomenclator, administradorActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public NomenclatorWindow() => InitializeComponent();
}
