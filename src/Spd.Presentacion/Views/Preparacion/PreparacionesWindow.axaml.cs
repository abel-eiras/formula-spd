using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Preparacion;

public partial class PreparacionesWindow : Window
{
    public PreparacionesWindow(
        IServicioPreparacion servicioPreparacion, IServicioPacientes servicioPacientes, IServicioUsuarios servicioUsuarios,
        IServicioMedicamentos servicioMedicamentos, IServicioGeneracionDocumentos servicioDocumentos,
        IServicioComunicacionesMedico servicioComunicaciones, IServicioGeneracionLote servicioLote, int? usuarioActualId)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        DataContext = new PreparacionesViewModel(
            servicioPreparacion, servicioPacientes, servicioUsuarios, servicioMedicamentos, servicioDocumentos,
            servicioComunicaciones, servicioLote, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public PreparacionesWindow() => InitializeComponent();
}
