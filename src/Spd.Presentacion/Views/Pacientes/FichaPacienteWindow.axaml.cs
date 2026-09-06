using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Pacientes;

public partial class FichaPacienteWindow : Window
{
    public FichaPacienteWindow(
        IServicioPacientes servicio, IServicioTratamientos servicioTratamientos, IServicioMedicamentos servicioMedicamentos,
        IServicioEnvases servicioEnvases, IServicioImportacionTratamientoEnvase servicioImportacion,
        IServicioComunicacionesMedico servicioComunicaciones, IServicioPreparacion servicioPreparacion,
        IServicioGeneracionDocumentos servicioGeneracionDocumentos, IServicioIdoneidadConsentimiento servicioIdoneidad,
        Paciente? pacienteExistente, int? usuarioActualId)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        DataContext = new FichaPacienteViewModel(
            servicio, servicioTratamientos, servicioMedicamentos, servicioEnvases, servicioImportacion,
            servicioComunicaciones, servicioPreparacion, servicioGeneracionDocumentos, servicioIdoneidad, pacienteExistente, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public FichaPacienteWindow() => InitializeComponent();
}
