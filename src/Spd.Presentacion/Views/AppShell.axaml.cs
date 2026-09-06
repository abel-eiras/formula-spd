using Avalonia.Controls;
using Avalonia.Input;

namespace Spd.Presentacion.Views;

/// <summary>La única ventana de trabajo (Spec 015 FR-1500). F1 se resuelve contra la vista que haya
/// en la región de contenido, no contra la ventana: con el marco único, lo que identifica a una
/// pantalla es su vista.</summary>
public partial class AppShell : Window
{
    public AppShell()
    {
        InitializeComponent();
        KeyDown += (_, e) =>
        {
            if (e.Key != Key.F1) return;
            e.Handled = true;
            AyudaContextual.AbrirParaVista(AyudaContextual.VistaDe(this.FindControl<ContentControl>("RegionContenido")));
        };
    }
}
