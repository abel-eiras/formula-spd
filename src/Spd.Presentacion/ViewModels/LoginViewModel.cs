using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Inicio de sesión con mensaje de bloqueo (FR-045).</summary>
public sealed partial class LoginViewModel(IServicioUsuarios servicio) : ViewModelBase
{
    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string? _mensajeError;

    /// <summary>Se dispara cuando el usuario introduce credenciales correctas y no está bloqueado.</summary>
    public event Action<Usuario>? SesionIniciada;

    [RelayCommand]
    private void Entrar()
    {
        var resultado = servicio.IntentarLogin(Login, Password);
        if (resultado.Exito)
        {
            MensajeError = null;
            SesionIniciada?.Invoke(resultado.Usuario!);
            return;
        }
        MensajeError = resultado.Mensaje;
    }
}
