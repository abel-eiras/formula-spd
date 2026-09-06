using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Medicamentos;

public partial class CatalogoMedicamentosWindow : Window
{
    public CatalogoMedicamentosWindow(
        IServicioMedicamentos servicio, IServicioImportacionNomenclator servicioImportacion,
        IServicioConsultaCima servicioConsultaCima, int? usuarioActualId)
    {
        InitializeComponent();
        AyudaContextual.Registrar(this);
        DataContext = new CatalogoMedicamentosViewModel(servicio, servicioImportacion, servicioConsultaCima, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public CatalogoMedicamentosWindow() => InitializeComponent();
}
