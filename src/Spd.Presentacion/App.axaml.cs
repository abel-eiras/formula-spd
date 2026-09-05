using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Data.Sqlite;
using Serilog;
using Spd.Aplicacion;
using Spd.Dominio;
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
    private IServicioMedicamentos? _servicioMedicamentos;
    private IServicioImportacionNomenclator? _servicioImportacionNomenclator;
    private IServicioConsultaCima? _servicioConsultaCima;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Por defecto, Avalonia cierra toda la app cuando se cierra la ventana que en su
            // momento se asignó a MainWindow, aunque ya se haya sustituido por otra (el asistente
            // cerrándose "de golpe" en vez de pasar al login era este bug). Con apagado explícito,
            // solo la ventana principal real (tras iniciar sesión) cierra la aplicación al cerrarse.
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
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
        var repositorioMedicamentos = new RepositorioMedicamentos(_conexion);
        _servicioMedicamentos = new ServicioMedicamentos(repositorioMedicamentos, auditoria);
        _servicioImportacionNomenclator = new ServicioImportacionNomenclator(
            new LectorNomenclatorCsv(), repositorioMedicamentos, auditoria);
        // Consulta puntual por CN al alta, alternativa al nomenclátor CSV para forma farmacéutica
        // (research.md Decisión 5, corrección 2026-09-05): CIMA REST API pública de la AEMPS.
        _servicioConsultaCima = new ServicioConsultaCima(
            new HttpClient { BaseAddress = new Uri("https://cima.aemps.es/cima/rest/"), Timeout = TimeSpan.FromSeconds(10) },
            auditoria);
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
            ventanaAsistente.Show();
            RegistrarTiempoDeArranque("asistente");
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
        ventanaLogin.SesionIniciada += usuario => AbrirVentanaPrincipal(desktop, ventanaLogin, usuario);
        desktop.MainWindow = ventanaLogin;
        // Avalonia solo muestra la ventana inicial automáticamente al arrancar; al sustituir
        // MainWindow más tarde (p. ej. al terminar el asistente o al cerrar sesión) hay que
        // mostrarla explícitamente, si no la app se queda sin ninguna ventana visible.
        ventanaLogin.Show();
        RegistrarTiempoDeArranque("login");
    }

    private void AbrirVentanaPrincipal(
        IClassicDesktopStyleApplicationLifetime desktop, Window ventanaAnterior, Usuario usuario)
    {
        var mainViewModel = new MainViewModel(
            _servicioUsuarios!, _servicioFarmacia!, _gestorLogo!,
            _servicioActualizaciones!, _servicioNomenclator!, _servicioMedicamentos!,
            _servicioImportacionNomenclator!, _servicioConsultaCima!, usuario);
        var ventanaPrincipal = new MainWindow { DataContext = mainViewModel };

        // "Cerrar sesión" cierra esta ventana para volver al login, sin salir de la aplicación;
        // cerrarla de cualquier otra forma (X, Alt+F4) sí debe salir (única ventana de la sesión).
        var sesionCerradaPorElUsuario = false;
        mainViewModel.CerrarSesionSolicitado += () =>
        {
            sesionCerradaPorElUsuario = true;
            MostrarLogin(desktop);
            ventanaPrincipal.Close();
        };
        ventanaPrincipal.Closed += (_, _) =>
        {
            if (!sesionCerradaPorElUsuario)
            {
                desktop.Shutdown();
            }
        };

        desktop.MainWindow = ventanaPrincipal;
        ventanaPrincipal.Show();
        ventanaAnterior.Close();
    }

    // Art. IX.4: arranque completo hasta pantalla de login/asistente < 2 s en PC de gama media.
    private static void RegistrarTiempoDeArranque(string pantalla)
        => Log.Information(
            "Arranque hasta pantalla de {Pantalla} en {Milisegundos} ms",
            pantalla, Program.CronometroArranque.ElapsedMilliseconds);
}
