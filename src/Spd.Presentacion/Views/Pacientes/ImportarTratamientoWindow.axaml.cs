using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Pacientes;

public partial class ImportarTratamientoWindow : Window
{
    public ImportarTratamientoWindow(IServicioImportacionTratamientoEnvase servicio, int pacienteId, int? usuarioActualId)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        DataContext = new ImportarTratamientoViewModel(servicio, pacienteId, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public ImportarTratamientoWindow() => InitializeComponent();
}
