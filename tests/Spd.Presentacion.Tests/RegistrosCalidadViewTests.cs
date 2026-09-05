using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.RegistrosCalidad;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión (mismo patrón que las demás vistas de esta spec): las pestañas de esta
/// ventana usan `ListBox` con `ItemTemplate` sobre datos reales.</summary>
public sealed class RegistrosCalidadViewTests
{
    [AvaloniaFact]
    public void RegistrosCalidadWindow_se_construye_y_muestra_con_datos_reales_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var usuarioId = new RepositorioUsuarios(conexion).Crear(new Dominio.Usuario
        {
            Nombre = "Eva", Apellidos = "Elaboradora", Login = "eva.elab", HashPassword = "hash", Rol = Dominio.Rol.Elaborador
        });
        var servicio = new ServicioRegistrosCalidad(new RepositorioRegistrosCalidad(conexion), auditoria);
        servicio.RegistrarFormacion(
            new DatosFormacion(usuarioId, "Curso de manipulación", "Colegio", DateOnly.FromDateTime(DateTime.Today), true), usuarioId);
        servicio.RegistrarRecogidaResiduos(
            new DatosRecogidaResiduos(DateOnly.FromDateTime(DateTime.Today), "Gestora S.L.", null), usuarioId);

        var ventana = new RegistrosCalidadWindow(servicio, usuarioId);

        // Show() fuerza la realización del ItemTemplate de ambas pestañas con datos reales.
        ventana.Show();
    }
}
