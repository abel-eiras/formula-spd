using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Configuración / Seguridad: activar/desactivar el cifrado y cambiar la contraseña
/// maestra (FR-1010..1013). Solo Administrador (Art. VII.4).</summary>
public sealed partial class SeguridadViewModel : ViewModelBase
{
    private enum OperacionPendiente
    {
        Ninguna,
        Activar,
        CambiarContrasena
    }

    private readonly IServicioCifrado _servicio;
    private readonly int _administradorActualId;

    private byte[]? _mekPendiente;
    private string[]? _fraseGeneradaInterna;
    private OperacionPendiente _operacionPendiente = OperacionPendiente.Ninguna;

    [ObservableProperty] private bool _cifradoActivo;
    [ObservableProperty] private string? _contrasenaActual;
    [ObservableProperty] private string? _contrasenaNueva;
    [ObservableProperty] private string? _contrasenaNuevaConfirmacion;
    [ObservableProperty] private string? _fraseGeneradaTexto;
    [ObservableProperty] private bool _hayFraseParaConfirmar;
    [ObservableProperty] private string? _mensaje;

    public SeguridadViewModel(IServicioCifrado servicio, int administradorActualId)
    {
        _servicio = servicio;
        _administradorActualId = administradorActualId;
        _cifradoActivo = servicio.EstaActivo();
    }

    // FR-1010/CA-1003: genera y muestra la frase, pero no toca la base de datos todavía — eso
    // solo ocurre cuando el administrador confirma con ConfirmarFraseGuardadaCommand.
    [RelayCommand]
    private void IniciarActivacion()
    {
        if (string.IsNullOrWhiteSpace(ContrasenaNueva) || ContrasenaNueva != ContrasenaNuevaConfirmacion)
        {
            Mensaje = "Escriba la misma contraseña nueva en los dos campos.";
            return;
        }

        PrepararOperacion(OperacionPendiente.Activar);
    }

    // FR-1012 exige la misma confirmación de impresión que FR-1010.
    [RelayCommand]
    private void IniciarCambioContrasena()
    {
        if (string.IsNullOrWhiteSpace(ContrasenaActual))
        {
            Mensaje = "Escriba la contraseña maestra actual.";
            return;
        }
        if (string.IsNullOrWhiteSpace(ContrasenaNueva) || ContrasenaNueva != ContrasenaNuevaConfirmacion)
        {
            Mensaje = "Escriba la misma contraseña nueva en los dos campos.";
            return;
        }

        PrepararOperacion(OperacionPendiente.CambiarContrasena);
    }

    private void PrepararOperacion(OperacionPendiente operacion)
    {
        var (mek, frase) = _servicio.GenerarClaveYFraseNuevas();
        _mekPendiente = mek;
        _fraseGeneradaInterna = frase;
        _operacionPendiente = operacion;
        FraseGeneradaTexto = string.Join(' ', frase);
        HayFraseParaConfirmar = true;
        Mensaje = "Imprima o guarde la clave de recuperación antes de continuar.";
    }

    [RelayCommand]
    private void ConfirmarFraseGuardada()
    {
        if (_mekPendiente is null || _fraseGeneradaInterna is null)
        {
            return;
        }

        switch (_operacionPendiente)
        {
            case OperacionPendiente.Activar:
                var resultadoActivar = _servicio.ActivarCifrado(
                    _mekPendiente, _fraseGeneradaInterna, ContrasenaNueva!, _administradorActualId);
                Mensaje = resultadoActivar.Exito ? "Cifrado activado." : $"No se pudo activar el cifrado: {resultadoActivar.Motivo}";
                CifradoActivo = _servicio.EstaActivo();
                break;

            case OperacionPendiente.CambiarContrasena:
                var resultadoCambio = _servicio.CambiarContrasenaMaestra(
                    ContrasenaActual!, ContrasenaNueva!, _mekPendiente, _fraseGeneradaInterna, _administradorActualId);
                Mensaje = resultadoCambio.Exito
                    ? "Contraseña maestra cambiada."
                    : $"No se pudo cambiar la contraseña: {resultadoCambio.Motivo}";
                break;
        }

        LimpiarEstadoPendiente();
    }

    [RelayCommand]
    private void CancelarConfirmacion()
    {
        Mensaje = null;
        LimpiarEstadoPendiente();
    }

    // FR-1011: reconfirmación de la contraseña actual, sin frase de recuperación nueva (se borra
    // la existente junto con el resto del sobre — ya no hace falta ninguna).
    [RelayCommand]
    private void Desactivar()
    {
        if (string.IsNullOrWhiteSpace(ContrasenaActual))
        {
            Mensaje = "Escriba la contraseña maestra actual.";
            return;
        }

        var resultado = _servicio.DesactivarCifrado(ContrasenaActual, _administradorActualId);
        Mensaje = resultado.Exito ? "Cifrado desactivado." : $"No se pudo desactivar el cifrado: {resultado.Motivo}";
        CifradoActivo = _servicio.EstaActivo();
        ContrasenaActual = null;
    }

    private void LimpiarEstadoPendiente()
    {
        _mekPendiente = null;
        _fraseGeneradaInterna = null;
        _operacionPendiente = OperacionPendiente.Ninguna;
        FraseGeneradaTexto = null;
        HayFraseParaConfirmar = false;
        ContrasenaActual = null;
        ContrasenaNueva = null;
        ContrasenaNuevaConfirmacion = null;
    }
}
