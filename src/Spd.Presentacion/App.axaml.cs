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

namespace Spd.Presentacion;

public partial class App : Application
{
    // Toda la instalación es una carpeta (Art. VI.4): la base de datos vive junto al ejecutable.
    private static string RutaBaseDeDatos => Path.Combine(AppContext.BaseDirectory, "spd.db");

    private SqliteConnection? _conexion;
    private IServicioAsistentePrimerArranque? _servicioAsistente;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            InicializarInfraestructura();
            MostrarAsistenteOVentanaPrincipal(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InicializarInfraestructura()
    {
        _conexion = new SqliteConnection($"Data Source={RutaBaseDeDatos}");
        _conexion.Open();
        new AplicadorMigraciones(_conexion).Aplicar();

        _servicioAsistente = new ServicioAsistentePrimerArranque(
            new RepositorioFarmacia(_conexion),
            new RepositorioUsuarios(_conexion),
            new HasheadorArgon2id(),
            new RegistradorAuditoria(_conexion));
    }

    private void MostrarAsistenteOVentanaPrincipal(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (!_servicioAsistente!.HayConfiguracionInicial())
        {
            var ventanaAsistente = new AsistentePrimerArranqueWindow(_servicioAsistente);
            ventanaAsistente.AsistenteFinalizado += () =>
            {
                desktop.MainWindow = CrearVentanaPrincipal();
                desktop.MainWindow.Show();
                ventanaAsistente.Close();
            };
            desktop.MainWindow = ventanaAsistente;
        }
        else
        {
            desktop.MainWindow = CrearVentanaPrincipal();
        }
    }

    // CA-000: hasta que el asistente termina, esta es la única ventana de la aplicación (FR-000).
    // A partir de aquí, la pantalla de login (Fase 4, User Story 2) sustituirá a este placeholder.
    private static MainWindow CrearVentanaPrincipal() => new() { DataContext = new MainViewModel() };
}