using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Pacientes;

public partial class ExportarPacientesWindow : Window
{
    public ExportarPacientesWindow(
        IServicioPerfilesImportacion servicioPerfiles, IServicioExportacionPacientes servicioExportacion, IServicioPacientes servicioPacientes)
    {
        InitializeComponent();
        DataContext = new ExportarPacientesViewModel(servicioPerfiles, servicioExportacion, servicioPacientes);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public ExportarPacientesWindow() => InitializeComponent();
}
