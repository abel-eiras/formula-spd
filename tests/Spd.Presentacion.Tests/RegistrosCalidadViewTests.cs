using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.RegistrosCalidad;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión (mismo patrón que Specs 000/001/003): las pestañas de esta ventana usan
/// `ListBox` con `ItemTemplate` sobre datos reales.</summary>
public sealed class RegistrosCalidadViewTests
{
    [AvaloniaFact]
    public void RegistrosCalidadWindow_se_construye_y_muestra_con_datos_reales_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000"
        });
        var auditoria = new RegistradorAuditoria(conexion);
        var usuarioId = new RepositorioUsuarios(conexion).Crear(new Usuario
        {
            Nombre = "Eva", Apellidos = "Elaboradora", Login = "eva.elab", HashPassword = "hash", Rol = Rol.Elaborador
        });
        var servicio = new ServicioRegistrosCalidad(new RepositorioRegistrosCalidad(conexion), repositorioFarmacia, auditoria);
        servicio.RegistrarAmbiental(new DatosRegistroAmbiental(20, 50, null), usuarioId);
        servicio.RegistrarLimpieza(TipoLimpieza.Rutinaria, null, usuarioId);

        var ventana = new RegistrosCalidadWindow(servicio, usuarioId);

        // Show() fuerza la realización del ItemTemplate de cada pestaña con datos reales.
        ventana.Show();
    }
}
