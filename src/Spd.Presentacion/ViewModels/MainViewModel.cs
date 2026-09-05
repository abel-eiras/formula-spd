using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Views.Configuracion;

namespace Spd.Presentacion.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IServicioUsuarios _servicioUsuarios;

    [ObservableProperty] private string _greeting;
    [ObservableProperty] private Usuario _usuarioActual;

    /// <summary>Elaborador no accede a Configuración (spec 000, actor "Elaborador").</summary>
    public bool PuedeAccederAConfiguracion => UsuarioActual.Rol == Rol.Administrador;

    public MainViewModel(IServicioUsuarios servicioUsuarios, Usuario usuarioActual)
    {
        _servicioUsuarios = servicioUsuarios;
        _usuarioActual = usuarioActual;
        _greeting = $"Bienvenido/a, {usuarioActual.Nombre}";
    }

    [RelayCommand]
    private void AbrirUsuarios()
    {
        var ventana = new UsuariosWindow(_servicioUsuarios, UsuarioActual.Id);
        ventana.Show();
    }
}
