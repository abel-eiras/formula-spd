using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Pacientes;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Mismo patrón de regresión que `BuscadorPacientesViewTests`/`CatalogoMedicamentosViewTests`:
/// `TratamientoView` usa un `ListBox` con `ItemTemplate` y un binding de comando a un ancestro.</summary>
public sealed class TratamientoViewTests
{
    [AvaloniaFact]
    public void TratamientoWindow_se_construye_y_muestra_con_un_tratamiento_real_sin_lanzar()
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
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente(
                "Ana", "Pérez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, null),
            usuarioQueEjecutaId: null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var servicioMedicamentos = new ServicioMedicamentos(repositorioMedicamentos, auditoria);
        var medicamento = servicioMedicamentos.Crear(new DatosAltaMedicamento("654321", "Paracetamol 1g"), null);

        var servicioTratamientos = new ServicioTratamientos(new RepositorioTratamientos(conexion), repositorioPacientes, auditoria);
        servicioTratamientos.Crear(
            paciente.Id,
            new DatosAltaTratamiento(
                medicamento.Id, true, null, null, FraccionDosis.Uno, null, null, null, null,
                "1111111", null, null, DateOnly.FromDateTime(DateTime.Now), TipoTratamiento.Cronico),
            usuarioQueEjecutaId: null);

        var servicioComunicaciones = new ServicioComunicacionesMedico(
            new RepositorioComunicacionesMedico(conexion), repositorioPacientes, new RepositorioTratamientos(conexion), auditoria);
        var ventana = new TratamientoWindow(
            servicioTratamientos, servicioMedicamentos, servicioComunicaciones, FabricaServiciosTest.GeneracionDocumentos(conexion),
            paciente.Id, usuarioActualId: null);

        ventana.Show();
    }
}
