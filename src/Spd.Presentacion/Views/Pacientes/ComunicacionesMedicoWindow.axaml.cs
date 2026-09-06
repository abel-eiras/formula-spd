using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Pacientes;

public partial class ComunicacionesMedicoWindow : Window
{
    public ComunicacionesMedicoWindow(
        IServicioComunicacionesMedico servicio, IServicioGeneracionDocumentos servicioDocumentos, int pacienteId, int? usuarioActualId,
        DatosAltaComunicacionMedico? prerrelleno = null)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        DataContext = new ComunicacionesMedicoViewModel(servicio, servicioDocumentos, pacienteId, usuarioActualId, prerrelleno);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public ComunicacionesMedicoWindow() => InitializeComponent();
}
