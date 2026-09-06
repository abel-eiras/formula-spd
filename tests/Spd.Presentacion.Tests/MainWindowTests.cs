using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views;
using Xunit;

namespace Spd.Presentacion.Tests;

public sealed class MainWindowTests
{
    [AvaloniaFact]
    public void MainWindow_se_construye_y_muestra_para_un_Administrador_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var auditoria = new RegistradorAuditoria(conexion);
        var hasheador = new HasheadorArgon2id();
        var servicioUsuarios = new ServicioUsuarios(repositorioUsuarios, hasheador, auditoria);
        var administrador = servicioUsuarios.CrearUsuario(
            new DatosAltaUsuario("Ana", "Administradora", "ana.admin", "contraseña-inicial", Rol.Administrador, null, null),
            administradorQueEjecutaId: null).Usuario;

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000"
        });
        var servicioFarmacia = new ServicioConfiguracionFarmacia(repositorioFarmacia, auditoria);
        var gestorLogo = new GestorLogoFarmacia();
        var servicioActualizaciones = new ServicioActualizaciones(new HttpClient(), auditoria);
        var servicioNomenclator = new ServicioNomenclator(new HttpClient(), auditoria);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var servicioTratamientos = new ServicioTratamientos(new RepositorioTratamientos(conexion), repositorioPacientes, auditoria);
        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var servicioMedicamentos = new ServicioMedicamentos(repositorioMedicamentos, auditoria);
        var servicioImportacionNomenclator = new ServicioImportacionNomenclator(
            new LectorNomenclatorCsv(), repositorioMedicamentos, auditoria);
        var servicioConsultaCima = new ServicioConsultaCima(new HttpClient(), auditoria);
        var servicioRegistrosCalidad = new ServicioRegistrosCalidad(new RepositorioRegistrosCalidad(conexion), auditoria);
        var servicioControlDocumental = new ServicioControlDocumental(
            new RepositorioControlDocumental(conexion), repositorioUsuarios, auditoria);
        var servicioBackup = new ServicioBackup(conexion, repositorioFarmacia, auditoria);
        var servicioCifrado = new ServicioCifrado(
            conexion, ":memory:", Path.GetTempPath(), auditoria, new GeneradorFraseRecuperacion());
        var repositorioEnvases = new RepositorioEnvases(conexion);
        var servicioEnvases = new ServicioEnvases(repositorioEnvases, new RepositorioTratamientos(conexion), repositorioPacientes, auditoria);
        var servicioListadoRetirada = new ServicioListadoRetirada(
            repositorioPacientes, new RepositorioContactos(conexion), new RepositorioTratamientos(conexion),
            repositorioMedicamentos, repositorioEnvases, repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);
        var servicioImportacion = new ServicioImportacionTratamientoEnvase(
            repositorioMedicamentos, new RepositorioTratamientos(conexion), repositorioEnvases,
            new RepositorioPerfilesImportacionTratamiento(conexion), servicioTratamientos, auditoria);
        var servicioComunicaciones = new ServicioComunicacionesMedico(
            new RepositorioComunicacionesMedico(conexion), repositorioPacientes, new RepositorioTratamientos(conexion), auditoria);
        var servicioPerfilesImportacion = new ServicioPerfilesImportacion(new RepositorioPerfilesImportacion(conexion), auditoria);
        var servicioExportacionPacientes = new ServicioExportacionPacientes();
        var servicioPreparacion = new ServicioPreparacion(
            new RepositorioSpd(conexion), new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion),
            new RepositorioSpdVerificaciones(conexion), new RepositorioSpdModificaciones(conexion),
            new RepositorioRegistrosAmbientales(conexion), new RepositorioMaterialAcondicionamiento(conexion),
            repositorioPacientes, new RepositorioTratamientos(conexion), repositorioMedicamentos, repositorioEnvases,
            repositorioFarmacia, new ServicioAsignacionEnvases(repositorioEnvases, new RepositorioTratamientos(conexion), auditoria),
            servicioEnvases, servicioListadoRetirada, new ComprobadorIdoneidadYConsentimientoNulo(), auditoria);
        var servicioGeneracionDocumentos = new ServicioGeneracionDocumentos(
            new RepositorioSpd(conexion), new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion),
            repositorioPacientes, repositorioFarmacia, auditoria);

        var viewModel = new MainViewModel(
            servicioUsuarios, servicioFarmacia, gestorLogo, servicioActualizaciones, servicioNomenclator,
            servicioPacientes, servicioTratamientos, servicioMedicamentos, servicioImportacionNomenclator, servicioConsultaCima,
            servicioRegistrosCalidad, servicioControlDocumental, servicioBackup, servicioCifrado,
            servicioEnvases, servicioListadoRetirada, servicioImportacion, servicioComunicaciones,
            servicioPerfilesImportacion, servicioExportacionPacientes, servicioPreparacion,
            servicioGeneracionDocumentos, administrador);
        var ventana = new MainWindow { DataContext = viewModel };

        ventana.Show();
    }
}
