using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Pacientes;

public partial class FichaPacienteWindow : Window
{
    public FichaPacienteWindow(IServicioPacientes servicio, Paciente? pacienteExistente, int? usuarioActualId)
    {
        InitializeComponent();
        DataContext = new FichaPacienteViewModel(servicio, pacienteExistente, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public FichaPacienteWindow() => InitializeComponent();
}
