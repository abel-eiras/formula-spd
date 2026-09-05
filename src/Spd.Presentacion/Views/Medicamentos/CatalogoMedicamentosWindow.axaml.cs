using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Medicamentos;

public partial class CatalogoMedicamentosWindow : Window
{
    public CatalogoMedicamentosWindow(
        IServicioMedicamentos servicio, IServicioImportacionNomenclator servicioImportacion, int? usuarioActualId)
    {
        InitializeComponent();
        DataContext = new CatalogoMedicamentosViewModel(servicio, servicioImportacion, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public CatalogoMedicamentosWindow() => InitializeComponent();
}
