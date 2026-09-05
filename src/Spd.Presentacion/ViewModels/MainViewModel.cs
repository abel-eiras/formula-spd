using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Presentacion.Views.Configuracion;
using Spd.Presentacion.Views.Medicamentos;

namespace Spd.Presentacion.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IServicioUsuarios _servicioUsuarios;
    private readonly IServicioConfiguracionFarmacia _servicioFarmacia;
    private readonly GestorLogoFarmacia _gestorLogo;
    private readonly IServicioActualizaciones _servicioActualizaciones;
    private readonly IServicioNomenclator _servicioNomenclator;
    private readonly IServicioMedicamentos _servicioMedicamentos;

    [ObservableProperty] private string _greeting;
    [ObservableProperty] private Usuario _usuarioActual;

    /// <summary>Elaborador no accede a Configuración (spec 000, actor "Elaborador").</summary>
    public bool PuedeAccederAConfiguracion => UsuarioActual.Rol == Rol.Administrador;

    /// <summary>Se dispara al pulsar "Cerrar sesión", para volver al login sin cerrar la aplicación.</summary>
    public event Action? CerrarSesionSolicitado;

    public MainViewModel(
        IServicioUsuarios servicioUsuarios,
        IServicioConfiguracionFarmacia servicioFarmacia,
        GestorLogoFarmacia gestorLogo,
        IServicioActualizaciones servicioActualizaciones,
        IServicioNomenclator servicioNomenclator,
        IServicioMedicamentos servicioMedicamentos,
        Usuario usuarioActual)
    {
        _servicioUsuarios = servicioUsuarios;
        _servicioFarmacia = servicioFarmacia;
        _gestorLogo = gestorLogo;
        _servicioActualizaciones = servicioActualizaciones;
        _servicioNomenclator = servicioNomenclator;
        _servicioMedicamentos = servicioMedicamentos;
        _usuarioActual = usuarioActual;
        _greeting = $"Bienvenido/a, {usuarioActual.Nombre}";
    }

    /// <summary>Cualquier Elaborador o Administrador accede al catálogo, sin restricción.</summary>
    [RelayCommand]
    private void AbrirCatalogoMedicamentos()
    {
        var ventana = new CatalogoMedicamentosWindow(_servicioMedicamentos, UsuarioActual.Id);
        ventana.Show();
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

    [RelayCommand]
    private void CerrarSesion() => CerrarSesionSolicitado?.Invoke();
}
