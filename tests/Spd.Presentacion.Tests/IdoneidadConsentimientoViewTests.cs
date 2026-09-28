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

/// <summary>Regresión F5 (Spec 001): `ListBox`+`ItemTemplate` con comandos de ancestro; se fuerza
/// la realización con un consentimiento y una evaluación reales.</summary>
public sealed class IdoneidadConsentimientoViewTests
{
    [AvaloniaFact]
    public void IdoneidadConsentimientoView_se_construye_y_muestra_con_datos_reales_sin_lanzar()
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

        var ventana = AnfitrionDeVista.Anfitrion(new IdoneidadConsentimientoView
        {
            DataContext = new IdoneidadConsentimientoViewModel(servicioIdoneidad, servicioDocumentos, paciente.Id, usuarioActualId: null)
        });

        ventana.Show();
    }

    /// <summary>Atajo para pacientes ya evaluados y consentidos antes de usar la aplicación: debe
    /// activar al paciente de verdad, con evaluación APTO y consentimiento vigentes (Art. I.3,
    /// FR-213) — no basta con el estado ACTIVO por sí solo (Spec 006 FR-602).</summary>
    [AvaloniaFact]
    public void MarcarAptoYaEvaluado_activa_al_paciente_con_evaluacion_y_consentimiento_reales()
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
        var evaluaciones = new RepositorioEvaluacionesIdoneidad(conexion);
        var consentimientos = new RepositorioConsentimientos(conexion);
        var servicioIdoneidad = new ServicioIdoneidadConsentimiento(
            evaluaciones, consentimientos, repositorioContactos, repositorioPacientes, servicioPacientes, auditoria);
        var servicioDocumentos = new ServicioGeneracionDocumentos(
            new RepositorioSpd(conexion), new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion),
            new RepositorioSpdVerificaciones(conexion), repositorioPacientes, repositorioContactos, new RepositorioMedicos(conexion),
            new RepositorioTratamientos(conexion), new RepositorioMedicamentos(conexion), new RepositorioUsuarios(conexion),
            new RepositorioMaterialAcondicionamiento(conexion), new RepositorioRegistrosAmbientales(conexion),
            evaluaciones, consentimientos, new RepositorioComunicacionesMedico(conexion), repositorioFarmacia, auditoria);

        var vm = new IdoneidadConsentimientoViewModel(servicioIdoneidad, servicioDocumentos, paciente.Id, usuarioActualId: null)
        {
            FechaFirma = new DateTimeOffset(new DateTime(2023, 3, 15))
        };

        vm.MarcarAptoYaEvaluadoCommand.Execute(null);

        var estado = servicioIdoneidad.Consultar(paciente.Id);
        Assert.Equal(EstadoPaciente.Activo, estado.Paciente.Estado);
        Assert.Equal(ResultadoIdoneidad.Apto, estado.EvaluacionVigente?.Resultado);
        Assert.NotNull(estado.ConsentimientoVigente);
        Assert.Equal(new DateOnly(2023, 3, 15), estado.ConsentimientoVigente!.FechaFirma);
        // Comprobador real de Spec 006 FR-602: no basta con Estado == Activo, exige los registros.
        var comprobador = new ComprobadorIdoneidadYConsentimientoReal(evaluaciones, consentimientos);
        Assert.True(comprobador.Aprobado(paciente.Id));
    }
}
