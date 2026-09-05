using System;
using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views;

public partial class LoginWindow : Window
{
    /// <summary>Se dispara al iniciar sesión correctamente, para que App.axaml.cs abra la ventana principal.</summary>
    public event Action<Usuario>? SesionIniciada;

    public LoginWindow(IServicioUsuarios servicio)
    {
        InitializeComponent();

        var viewModel = new LoginViewModel(servicio);
        viewModel.SesionIniciada += usuario => SesionIniciada?.Invoke(usuario);
        DataContext = viewModel;
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public LoginWindow() => InitializeComponent();
}
