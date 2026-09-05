using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Presentacion.Views.Configuracion;
using Spd.Presentacion.Views.RegistrosCalidad;

namespace Spd.Presentacion.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IServicioUsuarios _servicioUsuarios;
    private readonly IServicioConfiguracionFarmacia _servicioFarmacia;
    private readonly GestorLogoFarmacia _gestorLogo;
    private readonly IServicioActualizaciones _servicioActualizaciones;
    private readonly IServicioNomenclator _servicioNomenclator;
    private readonly IServicioRegistrosCalidad _servicioRegistrosCalidad;
    private readonly IServicioControlDocumental _servicioControlDocumental;

    [ObservableProperty] private string _greeting;
    [ObservableProperty] private Usuario _usuarioActual;
    [ObservableProperty] private string? _avisoRegistrosCalidad;

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
        IServicioRegistrosCalidad servicioRegistrosCalidad,
        IServicioControlDocumental servicioControlDocumental,
        Usuario usuarioActual)
    {
        _servicioUsuarios = servicioUsuarios;
        _servicioFarmacia = servicioFarmacia;
        _gestorLogo = gestorLogo;
        _servicioActualizaciones = servicioActualizaciones;
        _servicioNomenclator = servicioNomenclator;
        _servicioRegistrosCalidad = servicioRegistrosCalidad;
        _servicioControlDocumental = servicioControlDocumental;
        _usuarioActual = usuarioActual;
        _greeting = $"Bienvenido/a, {usuarioActual.Nombre}";

        // FR-950: aviso informativo en el panel de inicio, nunca bloquea nada.
        var aviso = servicioRegistrosCalidad.ComprobarAvisos();
        var mensaje = string.Empty;
        if (aviso.AvisoAmbiental) mensaje += $"Sin registro ambiental hace {aviso.DiasSinAmbiental} días. ";
        if (aviso.AvisoLimpieza) mensaje += $"Sin limpieza rutinaria hace {aviso.DiasSinLimpiezaRutinaria} días.";
        _avisoRegistrosCalidad = string.IsNullOrEmpty(mensaje) ? null : mensaje.Trim();
    }

    /// <summary>Solo Administrador (FR-942); el propio servicio también comprueba el rol.</summary>
    [RelayCommand]
    private void AbrirControlDocumental()
    {
        var ventana = new ControlDocumentalWindow(_servicioControlDocumental, UsuarioActual.Id);
        ventana.Show();
    }

    /// <summary>Cualquier Elaborador o Administrador accede a los registros de calidad, sin
    /// restricción (FR-940 se restringe aparte en Control documental).</summary>
    [RelayCommand]
    private void AbrirRegistrosCalidad()
    {
        var ventana = new RegistrosCalidadWindow(_servicioRegistrosCalidad, UsuarioActual.Id);
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
