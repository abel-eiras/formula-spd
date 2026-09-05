using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Pacientes;

public partial class DepositoWindow : Window
{
    public DepositoWindow(
        IServicioEnvases servicioEnvases, IServicioMedicamentos servicioMedicamentos,
        IServicioImportacionTratamientoEnvase servicioImportacion, int pacienteId, int? usuarioActualId)
    {
        InitializeComponent();
        DataContext = new DepositoViewModel(servicioEnvases, servicioMedicamentos, servicioImportacion, pacienteId, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public DepositoWindow() => InitializeComponent();
}
