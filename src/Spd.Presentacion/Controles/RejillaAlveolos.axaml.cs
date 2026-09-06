using Avalonia.Controls;

namespace Spd.Presentacion.Controles;

/// <summary>Rejilla de alvéolos (Spec 015 FR-1541). El DataContext es la lista que devuelve
/// `Preparacion.MapaAlveolos.Construir`, ordenada por día y toma; el reparto de cada línea a su
/// alvéolo es lógica pura y se prueba sin interfaz.</summary>
public partial class RejillaAlveolos : UserControl
{
    public RejillaAlveolos() => InitializeComponent();
}
