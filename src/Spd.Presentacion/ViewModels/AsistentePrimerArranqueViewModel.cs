using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Los 5 pasos del asistente, en el orden fijado por FR-000.</summary>
public enum PasoAsistente { Cifrado, Farmacia, PrimerUsuario, ValoresDefecto, Rutas }

/// <summary>ViewModel del asistente obligatorio de primer arranque (FR-000/FR-001). Navega entre
/// los 5 pasos y solo llama a <see cref="IServicioAsistentePrimerArranque.FinalizarAsistente"/> al
/// completar el último; nada se guarda antes (CA-000).</summary>
public sealed partial class AsistentePrimerArranqueViewModel : ViewModelBase
{
    private readonly IServicioAsistentePrimerArranque _servicio;

    [ObservableProperty] private PasoAsistente _pasoActual = PasoAsistente.Cifrado;
    [ObservableProperty] private string? _mensajeError;

    // Paso 1 — Cifrado. Preferencia solo de UI: Spec 010 todavía no existe (remediación U1).
    [ObservableProperty] private bool _cifradoDeseado;

    // Paso 2 — Datos de la farmacia (FR-010)
    [ObservableProperty] private string _codigoSanitario = string.Empty;
    [ObservableProperty] private string _nombreFarmacia = string.Empty;
    [ObservableProperty] private string _titularOComunidadBienes = string.Empty;
    [ObservableProperty] private string _cif = string.Empty;
    [ObservableProperty] private string _direccion = string.Empty;
    [ObservableProperty] private string _cp = string.Empty;
    [ObservableProperty] private string _poblacion = string.Empty;
    [ObservableProperty] private string _telefono = string.Empty;
    [ObservableProperty] private string? _logo;
    [ObservableProperty] private string? _fax;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _whatsapp;

    // Paso 3 — Primer usuario, rol Administrador forzado
    [ObservableProperty] private string _nombreUsuario = string.Empty;
    [ObservableProperty] private string _apellidosUsuario = string.Empty;
    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmarPassword = string.Empty;
    [ObservableProperty] private bool _mostrarPassword;

    // Paso 4 — Valores por defecto (FR-020)
    [ObservableProperty] private string _diaRetiradaDefecto = "LU";
    [ObservableProperty] private int _nBlisteresDefecto = 1;
    [ObservableProperty] private int _diasAntelacionListado = 2;

    // Paso 5 — Rutas (FR-030/031)
    [ObservableProperty] private string? _rutaBackup;
    [ObservableProperty] private string? _rutaDocumentosGenerados;

    /// <summary>Se dispara al completar con éxito el último paso, para que la vista contenedora navegue fuera del asistente.</summary>
    public event Action? AsistenteFinalizado;

    public AsistentePrimerArranqueViewModel(IServicioAsistentePrimerArranque servicio)
    {
        _servicio = servicio;
    }

    [RelayCommand]
    private void Siguiente()
    {
        MensajeError = null;
        try
        {
            EjecutarPasoActualYAvanzar();
        }
        catch (ErrorValidacionException ex)
        {
            MensajeError = ex.Message;
        }
    }

    private void EjecutarPasoActualYAvanzar()
    {
        switch (PasoActual)
        {
            case PasoAsistente.Cifrado:
                PasoActual = PasoAsistente.Farmacia;
                break;
            case PasoAsistente.Farmacia:
                _servicio.EjecutarPasoFarmacia(ConstruirFarmacia());
                PasoActual = PasoAsistente.PrimerUsuario;
                break;
            case PasoAsistente.PrimerUsuario:
                if (Password != ConfirmarPassword)
                {
                    throw new ErrorValidacionException("Las dos contraseñas no coinciden.");
                }
                _servicio.EjecutarPasoPrimerUsuario(NombreUsuario, ApellidosUsuario, Login, Password);
                PasoActual = PasoAsistente.ValoresDefecto;
                break;
            case PasoAsistente.ValoresDefecto:
                _servicio.EjecutarPasoValoresDefecto(DiaRetiradaDefecto, NBlisteresDefecto, DiasAntelacionListado);
                PasoActual = PasoAsistente.Rutas;
                break;
            case PasoAsistente.Rutas:
                _servicio.EjecutarPasoRutas(RutaBackup, RutaDocumentosGenerados);
                _servicio.FinalizarAsistente();
                AsistenteFinalizado?.Invoke();
                break;
        }
    }

    [RelayCommand]
    private void Atras()
    {
        if (PasoActual > PasoAsistente.Cifrado)
        {
            PasoActual = (PasoAsistente)((int)PasoActual - 1);
        }
    }

    private Farmacia ConstruirFarmacia() => new()
    {
        CodigoSanitario = CodigoSanitario,
        Nombre = NombreFarmacia,
        TitularOComunidadBienes = TitularOComunidadBienes,
        Cif = Cif,
        Direccion = Direccion,
        Cp = Cp,
        Poblacion = Poblacion,
        Telefono = Telefono,
        Logo = Logo,
        Fax = Fax,
        Email = Email,
        Whatsapp = Whatsapp
    };
}
