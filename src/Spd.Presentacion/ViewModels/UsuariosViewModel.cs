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

    public UsuariosViewModel(IServicioUsuarios servicio, int administradorActualId)
    {
        _servicio = servicio;
        _administradorActualId = administradorActualId;
        Recargar();
    }

    [RelayCommand]
    private void CrearUsuario()
    {
        try
        {
            var rol = EsAdministradorNuevo ? Rol.Administrador : Rol.Elaborador;
            _servicio.CrearUsuario(
                new DatosAltaUsuario(NombreNuevo, ApellidosNuevo, LoginNuevo, null, rol, null, null),
                _administradorActualId);
            Mensaje = null;
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
        _servicio.ResetearPassword(usuario.Id, _administradorActualId);
        Mensaje = $"Contraseña reseteada para {usuario.Login}. Debe cambiarla en su próximo acceso.";
    }

    private void Recargar() => Usuarios = new ObservableCollection<Usuario>(_servicio.ListarActivos());

    private void LimpiarFormularioAlta()
    {
        NombreNuevo = string.Empty;
        ApellidosNuevo = string.Empty;
        LoginNuevo = string.Empty;
        EsAdministradorNuevo = false;
    }
}
