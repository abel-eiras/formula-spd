using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Pacientes;

public partial class TratamientoWindow : Window
{
    public TratamientoWindow(
        IServicioTratamientos servicioTratamientos, IServicioMedicamentos servicioMedicamentos,
        IServicioComunicacionesMedico servicioComunicaciones, IServicioGeneracionDocumentos servicioDocumentos,
        int pacienteId, int? usuarioActualId)
    {
        InitializeComponent();
        DataContext = new TratamientoViewModel(
            servicioTratamientos, servicioMedicamentos, servicioComunicaciones, servicioDocumentos, pacienteId, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public TratamientoWindow() => InitializeComponent();
}
