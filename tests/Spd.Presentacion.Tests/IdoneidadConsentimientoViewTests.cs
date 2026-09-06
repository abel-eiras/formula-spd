using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Pacientes;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión F5 (Spec 001): `ListBox`+`ItemTemplate` con comandos de ancestro; se fuerza
/// la realización con un consentimiento y una evaluación reales.</summary>
public sealed class IdoneidadConsentimientoViewTests
{
    [AvaloniaFact]
    public void IdoneidadConsentimientoWindow_se_construye_y_muestra_con_datos_reales_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular", Cif = "B00000000",
            Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra", Telefono = "986000000"
        });
        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, null),
            usuarioQueEjecutaId: null);
        var repositorioContactos = new RepositorioContactos(conexion);
        repositorioContactos.Crear(new Contacto { PacienteId = paciente.Id, Tipo = TipoContacto.RepresentanteLegal, Nombre = "Ana", Apellidos = "Vidal", Dni = "87654321X" });

        var evaluaciones = new RepositorioEvaluacionesIdoneidad(conexion);
        var consentimientos = new RepositorioConsentimientos(conexion);
        var servicioIdoneidad = new ServicioIdoneidadConsentimiento(evaluaciones, consentimientos, repositorioContactos, repositorioPacientes, servicioPacientes, auditoria);
        servicioIdoneidad.RegistrarEvaluacion(paciente.Id,
            new DatosEvaluacionIdoneidad(true, false, false, false, false, false, false, true, true, null, ResultadoIdoneidad.Apto, null), null);
        var representante = servicioIdoneidad.Consultar(paciente.Id).RepresentantesElegibles.Single();
        servicioIdoneidad.CrearConsentimiento(paciente.Id, TipoConsentimiento.Representante, representante.Id, null);

        var servicioDocumentos = new ServicioGeneracionDocumentos(
            new RepositorioSpd(conexion), new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion),
            new RepositorioSpdVerificaciones(conexion), repositorioPacientes, repositorioContactos, new RepositorioMedicos(conexion),
            new RepositorioTratamientos(conexion), new RepositorioMedicamentos(conexion), new RepositorioUsuarios(conexion),
            new RepositorioMaterialAcondicionamiento(conexion), new RepositorioRegistrosAmbientales(conexion),
            evaluaciones, consentimientos, new RepositorioComunicacionesMedico(conexion), repositorioFarmacia, auditoria);

        var ventana = new IdoneidadConsentimientoWindow(servicioIdoneidad, servicioDocumentos, paciente.Id, usuarioActualId: null);

        ventana.Show();
    }
}
