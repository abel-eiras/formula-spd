using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Configuracion;
using Spd.Presentacion.ViewModels;
using Xunit;

namespace Spd.Presentacion.Tests;

public sealed class FarmaciaWindowTests
{
    [AvaloniaFact]
    public void FarmaciaWindow_se_construye_y_muestra_con_el_boton_de_backup_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        var auditoria = new RegistradorAuditoria(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000"
        });
        var servicioFarmacia = new ServicioConfiguracionFarmacia(repositorioFarmacia, auditoria);
        var servicioBackup = new ServicioBackup(conexion, repositorioFarmacia, auditoria);

        var ventana = AnfitrionDeVista.Anfitrion(new FarmaciaView { DataContext = new FarmaciaViewModel(servicioFarmacia, new GestorLogoFarmacia(), servicioBackup, administradorActualId: 1) });

        ventana.Show();
    }
}
