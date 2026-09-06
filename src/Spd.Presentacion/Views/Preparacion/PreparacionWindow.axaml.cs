using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Preparacion;

public partial class PreparacionWindow : Window
{
    public PreparacionWindow(IServicioPreparacion servicio, IServicioMedicamentos servicioMedicamentos, int pacienteId, int? usuarioActualId)
    {
        InitializeComponent();
        DataContext = new PreparacionViewModel(servicio, servicioMedicamentos, pacienteId, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public PreparacionWindow() => InitializeComponent();
}
