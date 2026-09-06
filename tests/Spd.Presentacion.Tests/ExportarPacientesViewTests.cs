using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Pacientes;
using Xunit;

namespace Spd.Presentacion.Tests;

public sealed class ExportarPacientesViewTests
{
    [AvaloniaFact]
    public void ExportarPacientesWindow_se_construye_y_muestra_con_un_perfil_real_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000"
        });
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);

        var servicioPerfiles = new ServicioPerfilesImportacion(new RepositorioPerfilesImportacion(conexion), auditoria);
        servicioPerfiles.Crear(
            new DatosAltaPerfilImportacion(
                "Exportación básica", TipoPerfilImportacion.Pacientes, ",", "UTF-8", true,
                [new ParCampoColumna("Nombre", "Nombre")], null),
            null);

        var ventana = new ExportarPacientesWindow(servicioPerfiles, new ServicioExportacionPacientes(), servicioPacientes);

        ventana.Show();
    }
}
