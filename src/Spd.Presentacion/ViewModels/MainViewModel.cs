using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Presentacion.Views.Configuracion;

namespace Spd.Presentacion.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IServicioUsuarios _servicioUsuarios;
    private readonly IServicioConfiguracionFarmacia _servicioFarmacia;
    private readonly GestorLogoFarmacia _gestorLogo;
    private readonly IServicioActualizaciones _servicioActualizaciones;
    private readonly IServicioNomenclator _servicioNomenclator;

    [ObservableProperty] private string _greeting;
    [ObservableProperty] private Usuario _usuarioActual;

    /// <summary>Elaborador no accede a Configuración (spec 000, actor "Elaborador").</summary>
    public bool PuedeAccederAConfiguracion => UsuarioActual.Rol == Rol.Administrador;

    public MainViewModel(
        IServicioUsuarios servicioUsuarios,
        IServicioConfiguracionFarmacia servicioFarmacia,
        GestorLogoFarmacia gestorLogo,
        IServicioActualizaciones servicioActualizaciones,
        IServicioNomenclator servicioNomenclator,
        Usuario usuarioActual)
    {
        _servicioUsuarios = servicioUsuarios;
        _servicioFarmacia = servicioFarmacia;
        _gestorLogo = gestorLogo;
        _servicioActualizaciones = servicioActualizaciones;
        _servicioNomenclator = servicioNomenclator;
        _usuarioActual = usuarioActual;
        _greeting = $"Bienvenido/a, {usuarioActual.Nombre}";
    }

    [RelayCommand]
    private void AbrirUsuarios()
    {
        var ventana = new UsuariosWindow(_servicioUsuarios, UsuarioActual.Id);
        ventana.Show();
    }

    [RelayCommand]
    private void AbrirFarmacia()
    {
        var ventana = new FarmaciaWindow(_servicioFarmacia, _gestorLogo, UsuarioActual.Id);
        ventana.Show();
    }

    [RelayCommand]
    private void AbrirActualizaciones()
    {
        var ventana = new ActualizacionesWindow(_servicioActualizaciones, UsuarioActual.Id);
        ventana.Show();
    }

    [RelayCommand]
    private void AbrirNomenclator()
    {
        var ventana = new NomenclatorWindow(_servicioFarmacia, _servicioNomenclator, UsuarioActual.Id);
        ventana.Show();
    }
}
