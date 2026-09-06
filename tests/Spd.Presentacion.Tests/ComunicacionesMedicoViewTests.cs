using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Pacientes;
using Spd.Presentacion.ViewModels;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión F5 de `/speckit-analyze` (Spec 001): mismo patrón de `ListBox`+`ItemTemplate`
/// con comando de ancestro; se fuerza la realización con una comunicación real.</summary>
public sealed class ComunicacionesMedicoViewTests
{
    [AvaloniaFact]
    public void ComunicacionesMedicoView_se_construye_y_muestra_con_una_comunicacion_real_sin_lanzar()
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
        var repositorioMedicos = new RepositorioMedicos(conexion);
        var medico = new Medico { Nombre = "Carmen", Apellidos = "López" };
        medico.Id = repositorioMedicos.Crear(medico);
        var medicoId = medico.Id;

        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, medicoId, null, null, null, false, null, null, null),
            usuarioQueEjecutaId: null);
        var pacienteId = paciente.Id;

        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        var servicioComunicaciones = new ServicioComunicacionesMedico(
            new RepositorioComunicacionesMedico(conexion), repositorioPacientes, repositorioTratamientos, auditoria);
        servicioComunicaciones.Crear(new DatosAltaComunicacionMedico(pacienteId, medicoId, TipoComunicacionMedico.Presentacion, null, null), null);

        var ventana = AnfitrionDeVista.Anfitrion(new ComunicacionesMedicoView
        {
            DataContext = new ComunicacionesMedicoViewModel(
                servicioComunicaciones, FabricaServiciosTest.GeneracionDocumentos(conexion), pacienteId, usuarioActualId: null,
                new ServicioMedicos(new RepositorioMedicos(conexion), repositorioPacientes, auditoria))
        });

        ventana.Show();
    }
}
