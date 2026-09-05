using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Configuración / Usuarios: listado, alta, baja y reseteo de contraseña (FR-040..FR-044).</summary>
public sealed partial class UsuariosViewModel : ViewModelBase
{
    private readonly IServicioUsuarios _servicio;
    private readonly int _administradorActualId;

    [ObservableProperty] private ObservableCollection<Usuario> _usuarios = [];
    [ObservableProperty] private string? _mensaje;

    [ObservableProperty] private string _nombreNuevo = string.Empty;
    [ObservableProperty] private string _apellidosNuevo = string.Empty;
    [ObservableProperty] private string _loginNuevo = string.Empty;
    [ObservableProperty] private bool _esAdministradorNuevo;
    [ObservableProperty] private string _passwordNuevo = string.Empty;
    [ObservableProperty] private string _confirmarPasswordNuevo = string.Empty;
    [ObservableProperty] private bool _mostrarPassword;

    public UsuariosViewModel(IServicioUsuarios servicio, int administradorActualId)
    {
        _servicio = servicio;
        _administradorActualId = administradorActualId;
        Recargar();
    }

    [RelayCommand]
    private void CrearUsuario()
    {
        if (PasswordNuevo != ConfirmarPasswordNuevo)
        {
            Mensaje = "Las dos contraseñas no coinciden.";
            return;
        }

        try
        {
            var rol = EsAdministradorNuevo ? Rol.Administrador : Rol.Elaborador;
            var passwordElegida = string.IsNullOrWhiteSpace(PasswordNuevo) ? null : PasswordNuevo;
            var resultado = _servicio.CrearUsuario(
                new DatosAltaUsuario(NombreNuevo, ApellidosNuevo, LoginNuevo, passwordElegida, rol, null, null),
                _administradorActualId);

            // La contraseña solo se puede leer en claro en este momento (Art. VII.1): si no la ha
            // elegido el propio Administrador, hay que mostrársela para que se la dé al usuario.
            Mensaje = passwordElegida is null
                ? $"Usuario '{resultado.Usuario.Login}' creado. Contraseña provisional: {resultado.PasswordProvisional} (debe cambiarla en su primer acceso)."
                : $"Usuario '{resultado.Usuario.Login}' creado.";
            LimpiarFormularioAlta();
            Recargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void DarDeBaja(Usuario usuario)
    {
        try
        {
            _servicio.DarDeBaja(usuario.Id, _administradorActualId);
            Mensaje = null;
            Recargar();
        }
        catch (UltimoAdministradorException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void ResetearPassword(Usuario usuario)
    {
        var passwordProvisional = _servicio.ResetearPassword(usuario.Id, _administradorActualId);
        Mensaje = $"Contraseña reseteada para {usuario.Login}: {passwordProvisional} (debe cambiarla en su próximo acceso).";
    }

    private void Recargar() => Usuarios = new ObservableCollection<Usuario>(_servicio.ListarActivos());

    private void LimpiarFormularioAlta()
    {
        NombreNuevo = string.Empty;
        ApellidosNuevo = string.Empty;
        LoginNuevo = string.Empty;
        EsAdministradorNuevo = false;
        PasswordNuevo = string.Empty;
        ConfirmarPasswordNuevo = string.Empty;
        MostrarPassword = false;
    }
}
