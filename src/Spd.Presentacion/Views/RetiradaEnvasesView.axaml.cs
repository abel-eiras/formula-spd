using Avalonia.Controls;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views;

public partial class RetiradaEnvasesView : UserControl
{
    private RetiradaEnvasesViewModel? _suscrito;

    public RetiradaEnvasesView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Suscribir(DataContext as RetiradaEnvasesViewModel);
        // Llegando desde un escaneo en la búsqueda global, la lectura ya se aplicó antes de que la
        // vista existiera: en cuanto se muestra, el foco va a Unidades igualmente.
        AttachedToVisualTree += (_, _) =>
        {
            if (_suscrito?.FilaSeleccionadaParaRegistrar is not null && !string.IsNullOrEmpty(_suscrito.Serie))
                LlevarFocoAUnidades();
        };
    }

    // Tras escanear solo queda revisar las unidades: el foco va allí y un Intro guarda el envase.
    private void Suscribir(RetiradaEnvasesViewModel? vm)
    {
        if (_suscrito is not null) _suscrito.LecturaAplicada -= LlevarFocoAUnidades;
        _suscrito = vm;
        if (vm is not null) vm.LecturaAplicada += LlevarFocoAUnidades;
    }

    private void LlevarFocoAUnidades() => Controles.Foco.LlevarA(this, "CampoUnidades");
}
