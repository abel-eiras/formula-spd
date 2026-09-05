using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views;
using Spd.Presentacion.Views.Asistente;
using System;
using System.IO;
using System.Net.Http;

namespace Spd.Presentacion;

public partial class App : Application
{
    // Toda la instalación es una carpeta (Art. VI.4): la base de datos vive junto al ejecutable.
    private static string RutaBaseDeDatos => Path.Combine(AppContext.BaseDirectory, "spd.db");

    private SqliteConnection? _conexion;
    private IServicioAsistentePrimerArranque? _servicioAsistente;
    private IServicioUsuarios? _servicioUsuarios;
    private IServicioConfiguracionFarmacia? _servicioFarmacia;
    private GestorLogoFarmacia? _gestorLogo;
    private IServicioActualizaciones? _servicioActualizaciones;
    private IServicioNomenclator? _servicioNomenclator;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            InicializarInfraestructura();
            MostrarAsistenteOLogin(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InicializarInfraestructura()
    {
        _conexion = new SqliteConnection($"Data Source={RutaBaseDeDatos}");
        _conexion.Open();
        new AplicadorMigraciones(_conexion).Aplicar();

        var repositorioUsuarios = new RepositorioUsuarios(_conexion);
        var repositorioFarmacia = new RepositorioFarmacia(_conexion);
        var hasheador = new HasheadorArgon2id();
        var auditoria = new RegistradorAuditoria(_conexion);

        _servicioAsistente = new ServicioAsistentePrimerArranque(
            repositorioFarmacia, repositorioUsuarios, hasheador, auditoria);
        _servicioUsuarios = new ServicioUsuarios(repositorioUsuarios, hasheador, auditoria);
        _servicioFarmacia = new ServicioConfiguracionFarmacia(repositorioFarmacia, auditoria);
        _gestorLogo = new GestorLogoFarmacia();

        // research.md Decisión 3/5: BaseAddress fija (GitHub) para actualizaciones; timeout corto
        // para el nomenclátor, cuya URL es la que configure el Administrador.
        _servicioActualizaciones = new ServicioActualizaciones(
            new HttpClient { BaseAddress = new Uri("https://api.github.com/") }, auditoria);
        _servicioNomenclator = new ServicioNomenclator(
            new HttpClient { Timeout = TimeSpan.FromSeconds(10) }, auditoria);
    }

    private void MostrarAsistenteOLogin(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (!_servicioAsistente!.HayConfiguracionInicial())
        {
            var ventanaAsistente = new AsistentePrimerArranqueWindow(_servicioAsistente);
            ventanaAsistente.AsistenteFinalizado += () =>
            {
                MostrarLogin(desktop);
                ventanaAsistente.Close();
            };
            desktop.MainWindow = ventanaAsistente;
        }
        else
        {
            MostrarLogin(desktop);
        }
    }

    // CA-000/FR-045: hasta iniciar sesión, la única ventana visible es el asistente o el login.
    private void MostrarLogin(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var ventanaLogin = new LoginWindow(_servicioUsuarios!);
        ventanaLogin.SesionIniciada += usuario =>
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(
                    _servicioUsuarios!, _servicioFarmacia!, _gestorLogo!,
                    _servicioActualizaciones!, _servicioNomenclator!, usuario)
            };
            desktop.MainWindow.Show();
            ventanaLogin.Close();
        };
        desktop.MainWindow = ventanaLogin;
    }
}
