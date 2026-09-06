using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Pacientes;

public partial class IdoneidadConsentimientoWindow : Window
{
    public IdoneidadConsentimientoWindow(
        IServicioIdoneidadConsentimiento servicio, IServicioGeneracionDocumentos servicioDocumentos, int pacienteId, int? usuarioActualId)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        DataContext = new IdoneidadConsentimientoViewModel(servicio, servicioDocumentos, pacienteId, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public IdoneidadConsentimientoWindow() => InitializeComponent();
}
