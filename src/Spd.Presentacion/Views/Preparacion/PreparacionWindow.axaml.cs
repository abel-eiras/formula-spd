using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Preparacion;

public partial class PreparacionWindow : Window
{
    public PreparacionWindow(
        IServicioPreparacion servicio, IServicioMedicamentos servicioMedicamentos,
        IServicioGeneracionDocumentos servicioGeneracionDocumentos, int pacienteId, int? usuarioActualId)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        DataContext = new PreparacionViewModel(servicio, servicioMedicamentos, servicioGeneracionDocumentos, pacienteId, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public PreparacionWindow() => InitializeComponent();
}
