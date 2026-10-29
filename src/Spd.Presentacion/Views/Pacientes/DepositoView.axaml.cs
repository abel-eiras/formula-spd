using Avalonia.Controls;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Pacientes;

public partial class DepositoView : UserControl
{
    private DepositoViewModel? _suscrito;

    public DepositoView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Suscribir(DataContext as DepositoViewModel);
    }

    // Tras escanear solo queda revisar las unidades: el foco va allí y un Intro guarda el envase.
    private void Suscribir(DepositoViewModel? vm)
    {
        if (_suscrito is not null) _suscrito.LecturaAplicada -= LlevarFocoAUnidades;
        _suscrito = vm;
        if (vm is not null) vm.LecturaAplicada += LlevarFocoAUnidades;
    }

    private void LlevarFocoAUnidades() => Controles.Foco.LlevarA(this, "CampoUnidades");
}
