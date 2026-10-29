using Avalonia.Controls;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Preparacion;

public partial class PreparacionView : UserControl
{
    private PreparacionViewModel? _suscrito;

    public PreparacionView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Suscribir(DataContext as PreparacionViewModel);
    }

    // Tras escanear solo queda revisar las unidades: el foco va allí y un Intro guarda el envase.
    private void Suscribir(PreparacionViewModel? vm)
    {
        if (_suscrito is not null) _suscrito.EnvaseLecturaAplicada -= LlevarFocoAUnidades;
        _suscrito = vm;
        if (vm is not null) vm.EnvaseLecturaAplicada += LlevarFocoAUnidades;
    }

    private void LlevarFocoAUnidades() => Controles.Foco.LlevarA(this, "CampoUnidades");
}
