using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Pacientes;
using Xunit;

namespace Spd.Presentacion.Tests;

public sealed class ImportarTratamientoViewTests
{
    [AvaloniaFact]
    public void ImportarTratamientoWindow_se_construye_y_muestra_sin_lanzar()
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
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, null),
            usuarioQueEjecutaId: null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        var repositorioEnvases = new RepositorioEnvases(conexion);
        var servicioTratamientos = new ServicioTratamientos(repositorioTratamientos, repositorioPacientes, auditoria);
        var servicioImportacion = new ServicioImportacionTratamientoEnvase(
            repositorioMedicamentos, repositorioTratamientos, repositorioEnvases,
            new RepositorioPerfilesImportacionTratamiento(conexion), servicioTratamientos, auditoria);

        var ventana = new ImportarTratamientoWindow(servicioImportacion, paciente.Id, usuarioActualId: null);

        ventana.Show();
    }
}
