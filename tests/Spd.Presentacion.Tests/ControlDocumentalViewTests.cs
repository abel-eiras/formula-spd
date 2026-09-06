using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.RegistrosCalidad;
using Spd.Presentacion.ViewModels;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión (mismo patrón que las demás vistas de esta spec): dos `ListBox` con
/// `ItemTemplate` sobre datos reales.</summary>
public sealed class ControlDocumentalViewTests
{
    [AvaloniaFact]
    public void ControlDocumentalWindow_se_construye_y_muestra_con_datos_reales_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var administradorId = repositorioUsuarios.Crear(new Usuario
        {
            Nombre = "Ana", Apellidos = "Administradora", Login = "ana.admin", HashPassword = "hash", Rol = Rol.Administrador
        });
        var auditoria = new RegistradorAuditoria(conexion);
        var servicio = new ServicioControlDocumental(new RepositorioControlDocumental(conexion), repositorioUsuarios, auditoria);
        servicio.RegistrarCambioPnt(
            new DatosCambioPnt("PNT-SPD", "1.1", "Actualización anual", DateOnly.FromDateTime(DateTime.Today), administradorId, administradorId, administradorId),
            administradorId);
        servicio.RegistrarCopia(new DatosCopia("PNT-SPD", 1, administradorId, DateOnly.FromDateTime(DateTime.Today)), administradorId);

        var ventana = AnfitrionDeVista.Anfitrion(new ControlDocumentalView { DataContext = new ControlDocumentalViewModel(servicio, administradorId) });

        // Show() fuerza la realización del ItemTemplate de ambos listados con datos reales.
        ventana.Show();
    }
}
