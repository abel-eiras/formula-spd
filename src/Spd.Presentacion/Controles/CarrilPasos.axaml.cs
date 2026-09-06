using Avalonia.Controls;

namespace Spd.Presentacion.Controles;

/// <summary>Carril de los cinco pasos de un blíster (Spec 015 FR-1540). La lógica —qué paso está
/// activo y por qué se bloquea otro— vive en `Preparacion.PasoPreparacion`, que es puro y se puede
/// probar sin abrir una ventana; aquí solo se pinta.</summary>
public partial class CarrilPasos : UserControl
{
    public CarrilPasos() => InitializeComponent();
}
