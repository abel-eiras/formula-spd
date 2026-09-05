using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Medicamentos;

public partial class RevisionNomenclatorWindow : Window
{
    public RevisionNomenclatorWindow(IServicioImportacionNomenclator servicio, int? usuarioActualId)
    {
        InitializeComponent();
        DataContext = new RevisionNomenclatorViewModel(servicio, usuarioActualId);
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public RevisionNomenclatorWindow() => InitializeComponent();
}
