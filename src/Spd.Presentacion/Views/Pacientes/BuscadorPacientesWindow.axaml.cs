using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Pacientes;

public partial class BuscadorPacientesWindow : Window
{
    public BuscadorPacientesWindow(
        IServicioPacientes servicio, IServicioTratamientos servicioTratamientos, IServicioMedicamentos servicioMedicamentos,
        IServicioEnvases servicioEnvases, IServicioImportacionTratamientoEnvase servicioImportacion, int? usuarioActualId)
    {
        InitializeComponent();
        DataContext = new BuscadorPacientesViewModel(servicio, servicioTratamientos, servicioMedicamentos, servicioEnvases, servicioImportacion, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public BuscadorPacientesWindow() => InitializeComponent();
}
