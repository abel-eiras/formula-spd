using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Presentacion.Views.Configuracion;
using Spd.Presentacion.Views.Pacientes;
using Spd.Presentacion.Views.Medicamentos;
using Spd.Presentacion.Views.RegistrosCalidad;

namespace Spd.Presentacion.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IServicioUsuarios _servicioUsuarios;
    private readonly IServicioConfiguracionFarmacia _servicioFarmacia;
    private readonly GestorLogoFarmacia _gestorLogo;
    private readonly IServicioActualizaciones _servicioActualizaciones;
    private readonly IServicioNomenclator _servicioNomenclator;
    private readonly IServicioPacientes _servicioPacientes;
    private readonly IServicioTratamientos _servicioTratamientos;
    private readonly IServicioMedicamentos _servicioMedicamentos;
    private readonly IServicioImportacionNomenclator _servicioImportacionNomenclator;
    private readonly IServicioConsultaCima _servicioConsultaCima;
    private readonly IServicioRegistrosCalidad _servicioRegistrosCalidad;
    private readonly IServicioControlDocumental _servicioControlDocumental;
    private readonly IServicioBackup _servicioBackup;
    private readonly IServicioCifrado _servicioCifrado;

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
        IServicioPacientes servicioPacientes,
        IServicioTratamientos servicioTratamientos,
        IServicioMedicamentos servicioMedicamentos,
        IServicioImportacionNomenclator servicioImportacionNomenclator,
        IServicioConsultaCima servicioConsultaCima,
        IServicioRegistrosCalidad servicioRegistrosCalidad,
        IServicioControlDocumental servicioControlDocumental,
        IServicioBackup servicioBackup,
        IServicioCifrado servicioCifrado,
        Usuario usuarioActual)
    {
        _servicioUsuarios = servicioUsuarios;
        _servicioFarmacia = servicioFarmacia;
        _gestorLogo = gestorLogo;
        _servicioActualizaciones = servicioActualizaciones;
        _servicioNomenclator = servicioNomenclator;
        _servicioPacientes = servicioPacientes;
        _servicioTratamientos = servicioTratamientos;
        _servicioMedicamentos = servicioMedicamentos;
        _servicioImportacionNomenclator = servicioImportacionNomenclator;
        _servicioConsultaCima = servicioConsultaCima;
        _servicioRegistrosCalidad = servicioRegistrosCalidad;
        _servicioControlDocumental = servicioControlDocumental;
        _servicioBackup = servicioBackup;
        _servicioCifrado = servicioCifrado;
        _usuarioActual = usuarioActual;
        _greeting = $"Bienvenido/a, {usuarioActual.Nombre}";
    }

    /// <summary>Cualquier Elaborador o Administrador accede a Pacientes, sin restricción (FR-040).</summary>
    [RelayCommand]
    private void AbrirPacientes()
    {
        var ventana = new BuscadorPacientesWindow(_servicioPacientes, _servicioTratamientos, _servicioMedicamentos, UsuarioActual.Id);
        ventana.Show();
    }

    /// <summary>Cualquier Elaborador o Administrador accede al catálogo, sin restricción.</summary>
    [RelayCommand]
    private void AbrirCatalogoMedicamentos()
    {
        var ventana = new CatalogoMedicamentosWindow(
            _servicioMedicamentos, _servicioImportacionNomenclator, _servicioConsultaCima, UsuarioActual.Id);
        ventana.Show();
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
        var ventana = new FarmaciaWindow(_servicioFarmacia, _gestorLogo, _servicioBackup, UsuarioActual.Id);
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
    private void AbrirSeguridad()
    {
        var ventana = new SeguridadWindow(_servicioCifrado, UsuarioActual.Id);
        ventana.Show();
    }

    [RelayCommand]
    private void CerrarSesion() => CerrarSesionSolicitado?.Invoke();
}
