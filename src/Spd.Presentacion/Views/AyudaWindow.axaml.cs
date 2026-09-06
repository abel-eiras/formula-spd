using Avalonia.Controls;
using Spd.Presentacion.Ayuda;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views;

/// <summary>Ventana de ayuda (Spec 014). Sin servicios: el contenido es estático y embebido.</summary>
public partial class AyudaWindow : Window
{
    public AyudaWindow() : this(IndiceAyuda.Global, null, null) { }

    public AyudaWindow(IndiceAyuda indice, string? seccion, string? id)
    {
        InitializeComponent();
        var vm = new AyudaViewModel(indice);
        vm.Seleccionar(seccion, id);
        DataContext = vm;
    }

    /// <summary>Abre la ayuda en un apartado (F1, "¿Por qué?", botón Ayuda). Con id nulo, el índice.</summary>
    public static void Abrir(string? seccion, string? id) => new AyudaWindow(IndiceAyuda.Global, seccion, id).Show();
}
