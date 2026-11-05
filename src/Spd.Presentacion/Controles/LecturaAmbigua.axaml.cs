using Avalonia.Controls;

namespace Spd.Presentacion.Controles;

/// <summary>Elección entre las lecturas posibles de un escaneo ambiguo (spec-012 FR-1210). El
/// DataContext es un <see cref="ViewModels.LecturaAmbiguaViewModel"/>.</summary>
public partial class LecturaAmbigua : UserControl
{
    public LecturaAmbigua() => InitializeComponent();
}
