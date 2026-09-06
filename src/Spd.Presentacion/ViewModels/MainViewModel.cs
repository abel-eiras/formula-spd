using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Presentacion.Views;
using Spd.Presentacion.Views.Configuracion;
using Spd.Presentacion.Views.Pacientes;
using Spd.Presentacion.Views.Medicamentos;
using Spd.Presentacion.Views.Preparacion;
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
    private readonly IServicioEnvases _servicioEnvases;
    private readonly IServicioListadoRetirada _servicioListadoRetirada;
    private readonly IServicioImportacionTratamientoEnvase _servicioImportacion;
    private readonly IServicioComunicacionesMedico _servicioComunicaciones;
    private readonly IServicioPerfilesImportacion _servicioPerfilesImportacion;
    private readonly IServicioExportacionPacientes _servicioExportacionPacientes;
    private readonly IServicioPreparacion _servicioPreparacion;
    private readonly IServicioGeneracionDocumentos _servicioGeneracionDocumentos;
    private readonly IServicioIdoneidadConsentimiento _servicioIdoneidad;
    private readonly IServicioAvisosInicio _servicioAvisos;
    private readonly IServicioGeneracionLote _servicioLote;
    private readonly IServicioPurga _servicioPurga;

    /// <summary>Spec 006 FR-691 / Spec 009 FR-950: avisos informativos del panel de inicio.</summary>
    [ObservableProperty] private ObservableCollection<AvisoInicio> _avisos = [];

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
        IServicioEnvases servicioEnvases,
        IServicioListadoRetirada servicioListadoRetirada,
        IServicioImportacionTratamientoEnvase servicioImportacion,
        IServicioComunicacionesMedico servicioComunicaciones,
        IServicioPerfilesImportacion servicioPerfilesImportacion,
        IServicioExportacionPacientes servicioExportacionPacientes,
        IServicioPreparacion servicioPreparacion,
        IServicioGeneracionDocumentos servicioGeneracionDocumentos,
        IServicioIdoneidadConsentimiento servicioIdoneidad,
        IServicioAvisosInicio servicioAvisos,
        IServicioGeneracionLote servicioLote,
        IServicioPurga servicioPurga,
        Usuario usuarioActual)
    {
        _servicioIdoneidad = servicioIdoneidad;
        _servicioAvisos = servicioAvisos;
        _servicioLote = servicioLote;
        _servicioPurga = servicioPurga;
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
        _servicioEnvases = servicioEnvases;
        _servicioListadoRetirada = servicioListadoRetirada;
        _servicioImportacion = servicioImportacion;
        _servicioComunicaciones = servicioComunicaciones;
        _servicioPerfilesImportacion = servicioPerfilesImportacion;
        _servicioExportacionPacientes = servicioExportacionPacientes;
        _servicioPreparacion = servicioPreparacion;
        _servicioGeneracionDocumentos = servicioGeneracionDocumentos;
        _usuarioActual = usuarioActual;
        _greeting = $"Bienvenido/a, {usuarioActual.Nombre}";
        RecargarAvisos();
    }

    [RelayCommand]
    private void RecargarAvisos()
        => Avisos = new ObservableCollection<AvisoInicio>(_servicioAvisos.Obtener(DateOnly.FromDateTime(DateTime.Today)));

    /// <summary>Spec 006 FR-690 / Spec 007 FR-720: listado global y generación en lote.</summary>
    [RelayCommand]
    private void AbrirPreparaciones()
        => new PreparacionesWindow(
            _servicioPreparacion, _servicioPacientes, _servicioUsuarios, _servicioMedicamentos, _servicioGeneracionDocumentos,
            _servicioComunicaciones, _servicioLote, UsuarioActual.Id).Show();

    /// <summary>Spec 014 FR-1400: ayuda accesible desde cualquier pantalla (aquí, el índice).</summary>
    [RelayCommand]
    private void AbrirAyuda() => AyudaWindow.Abrir(null, null);

    /// <summary>Cualquier Elaborador o Administrador accede a Pacientes, sin restricción (FR-040).</summary>
    [RelayCommand]
    private void AbrirPacientes()
    {
        var ventana = new BuscadorPacientesWindow(
            _servicioPacientes, _servicioTratamientos, _servicioMedicamentos, _servicioEnvases, _servicioImportacion,
            _servicioComunicaciones, _servicioPreparacion, _servicioGeneracionDocumentos, _servicioIdoneidad, UsuarioActual.Id);
        ventana.Show();
    }

    /// <summary>Cualquier Elaborador o Administrador accede al listado de retirada, sin
    /// restricción (mismo criterio que Pacientes, FR-040).</summary>
    [RelayCommand]
    private void AbrirRetiradaEnvases()
    {
        var ventana = new RetiradaEnvasesWindow(_servicioListadoRetirada, _servicioEnvases, UsuarioActual.Id);
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
        var ventana = new SeguridadWindow(_servicioCifrado, _servicioPurga, UsuarioActual.Id);
        ventana.Show();
    }

    /// <summary>Solo Administrador, igual que el resto de Configuración (FR-1100 lo describe como
    /// tarea de administración de perfiles).</summary>
    [RelayCommand]
    private void AbrirPerfilesImportacion()
    {
        var ventana = new PerfilesImportacionWindow(_servicioPerfilesImportacion, UsuarioActual.Id);
        ventana.Show();
    }

    /// <summary>Cualquier Elaborador o Administrador puede exportar, igual que Pacientes (FR-040).</summary>
    [RelayCommand]
    private void AbrirExportarPacientes()
    {
        var ventana = new ExportarPacientesWindow(_servicioPerfilesImportacion, _servicioExportacionPacientes, _servicioPacientes);
        ventana.Show();
    }

    [RelayCommand]
    private void CerrarSesion() => CerrarSesionSolicitado?.Invoke();
}
