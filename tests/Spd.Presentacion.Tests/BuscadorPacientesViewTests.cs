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

/// <summary>Regresión F5 de `/speckit-analyze` (Spec 001): `BuscadorPacientesView` usa un
/// `ListBox` con `ItemTemplate` y un binding de comando a un ancestro, exactamente el patrón que
/// causó el cierre inesperado (SIGABRT) de `UsuariosWindow` en Spec 000. Este test fuerza la
/// realización de la plantilla con un paciente real.</summary>
public sealed class BuscadorPacientesViewTests
{
    [AvaloniaFact]
    public void BuscadorPacientesWindow_se_construye_y_muestra_con_un_paciente_real_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", PrefijoNumFicha = "F-"
        });
        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicio = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        servicio.Crear(
            new DatosAltaPaciente(
                "José", "Núñez", Sexo: null, Dni: "12345678Z", FechaNacimiento: null, NumSs: null, Cip: null,
                Direccion: null, Cp: null, Poblacion: null, Telefono1: null, Telefono2: null, Email: null,
                MedicoId: null, EnfermedadesCronicas: null, Alergias: null, Observaciones: null,
                PictogramaComidas: false, IdentificadorVisual: null, DiaRetirada: null, NBlisteres: null),
            usuarioQueEjecutaId: null);

        var servicioTratamientos = new ServicioTratamientos(new RepositorioTratamientos(conexion), repositorioPacientes, auditoria);
        var servicioMedicamentos = new ServicioMedicamentos(new RepositorioMedicamentos(conexion), auditoria);
        var repositorioEnvases = new RepositorioEnvases(conexion);
        var servicioEnvases = new ServicioEnvases(repositorioEnvases, new RepositorioTratamientos(conexion), repositorioPacientes, auditoria);
        var servicioImportacion = new ServicioImportacionTratamientoEnvase(
            new RepositorioMedicamentos(conexion), new RepositorioTratamientos(conexion), repositorioEnvases,
            new RepositorioPerfilesImportacionTratamiento(conexion), servicioTratamientos, auditoria);
        var servicioComunicaciones = new ServicioComunicacionesMedico(
            new RepositorioComunicacionesMedico(conexion), repositorioPacientes, new RepositorioTratamientos(conexion), auditoria);
        var servicioListadoRetirada = new ServicioListadoRetirada(
            repositorioPacientes, new RepositorioContactos(conexion), new RepositorioTratamientos(conexion),
            new RepositorioMedicamentos(conexion), repositorioEnvases, repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);
        var servicioPreparacion = new ServicioPreparacion(
            new RepositorioSpd(conexion), new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion),
            new RepositorioSpdVerificaciones(conexion), new RepositorioSpdModificaciones(conexion),
            new RepositorioRegistrosAmbientales(conexion), new RepositorioMaterialAcondicionamiento(conexion),
            repositorioPacientes, new RepositorioTratamientos(conexion), new RepositorioMedicamentos(conexion), repositorioEnvases,
            repositorioFarmacia, new ServicioAsignacionEnvases(repositorioEnvases, new RepositorioTratamientos(conexion), auditoria),
            servicioEnvases, servicioListadoRetirada, new ComprobadorIdoneidadYConsentimientoNulo(), auditoria);
        var servicioGeneracionDocumentos = new ServicioGeneracionDocumentos(
            new RepositorioSpd(conexion), new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion),
            new RepositorioSpdVerificaciones(conexion), repositorioPacientes, new RepositorioContactos(conexion),
            new RepositorioMedicos(conexion), new RepositorioTratamientos(conexion), new RepositorioMedicamentos(conexion),
            new RepositorioUsuarios(conexion), new RepositorioMaterialAcondicionamiento(conexion),
            new RepositorioRegistrosAmbientales(conexion), new RepositorioEvaluacionesIdoneidad(conexion),
            new RepositorioConsentimientos(conexion), new RepositorioComunicacionesMedico(conexion), repositorioFarmacia, auditoria);
        var servicioIdoneidad = new ServicioIdoneidadConsentimiento(
            new RepositorioEvaluacionesIdoneidad(conexion), new RepositorioConsentimientos(conexion),
            new RepositorioContactos(conexion), repositorioPacientes, servicio, auditoria);
        var ventana = AnfitrionDeVista.Anfitrion(new BuscadorPacientesView { DataContext = new BuscadorPacientesViewModel(
            servicio, servicioTratamientos, servicioMedicamentos, servicioEnvases, servicioImportacion,
            servicioComunicaciones, servicioPreparacion, servicioGeneracionDocumentos, servicioIdoneidad, usuarioActualId: null) });

        // Show() fuerza la realización del ItemTemplate del ListBox para el paciente recién creado.
        ventana.Show();
    }
}
