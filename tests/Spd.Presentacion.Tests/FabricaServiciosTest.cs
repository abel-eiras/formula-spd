using System.Net.Http;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;

namespace Spd.Presentacion.Tests;

/// <summary>Construcción de servicios sobre una conexión de test, para que cada test de vista no
/// repita la lista completa de dependencias.</summary>
internal static class FabricaServiciosTest
{
    public static ServicioGeneracionDocumentos GeneracionDocumentos(SqliteConnection conexion)
        => new(
            new RepositorioSpd(conexion), new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion),
            new RepositorioSpdVerificaciones(conexion), new RepositorioPacientes(conexion), new RepositorioContactos(conexion),
            new RepositorioMedicos(conexion), new RepositorioTratamientos(conexion), new RepositorioMedicamentos(conexion),
            new RepositorioUsuarios(conexion), new RepositorioMaterialAcondicionamiento(conexion),
            new RepositorioRegistrosAmbientales(conexion), new RepositorioEvaluacionesIdoneidad(conexion),
            new RepositorioConsentimientos(conexion), new RepositorioComunicacionesMedico(conexion),
            new RepositorioFarmacia(conexion), new RegistradorAuditoria(conexion));

    /// <summary>Todos los servicios de la aplicación, tal y como los reúne `App` al arrancar
    /// (Spec 015 H1.4). Lo usa el test del marco para recorrer todas las secciones.</summary>
    public static ServiciosAplicacion Todos(SqliteConnection conexion)
    {
        var auditoria = new RegistradorAuditoria(conexion);
        var hasheador = new HasheadorArgon2id();
        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var repositorioEnvases = new RepositorioEnvases(conexion);
        var repositorioSpd = new RepositorioSpd(conexion);
        var repositorioEvaluaciones = new RepositorioEvaluacionesIdoneidad(conexion);
        var repositorioConsentimientos = new RepositorioConsentimientos(conexion);

        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var servicioTratamientos = new ServicioTratamientos(new RepositorioTratamientos(conexion), repositorioPacientes, auditoria);
        var servicioEnvases = new ServicioEnvases(repositorioEnvases, new RepositorioTratamientos(conexion), repositorioPacientes, auditoria);
        var servicioListadoRetirada = new ServicioListadoRetirada(
            repositorioPacientes, new RepositorioContactos(conexion), new RepositorioTratamientos(conexion),
            repositorioMedicamentos, repositorioEnvases, repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);
        var servicioPreparacion = new ServicioPreparacion(
            repositorioSpd, new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion),
            new RepositorioSpdVerificaciones(conexion), new RepositorioSpdModificaciones(conexion),
            new RepositorioRegistrosAmbientales(conexion), new RepositorioMaterialAcondicionamiento(conexion),
            repositorioPacientes, new RepositorioTratamientos(conexion), repositorioMedicamentos, repositorioEnvases,
            repositorioFarmacia, new ServicioAsignacionEnvases(repositorioEnvases, new RepositorioTratamientos(conexion), auditoria),
            servicioEnvases, servicioListadoRetirada,
            new ComprobadorIdoneidadYConsentimientoReal(repositorioEvaluaciones, repositorioConsentimientos), auditoria);
        var documentos = GeneracionDocumentos(conexion);

        return new ServiciosAplicacion(
            new ServicioUsuarios(repositorioUsuarios, hasheador, auditoria),
            new ServicioConfiguracionFarmacia(repositorioFarmacia, auditoria),
            new GestorLogoFarmacia(),
            new ServicioActualizaciones(new HttpClient(), auditoria),
            new ServicioNomenclator(new HttpClient(), auditoria),
            servicioPacientes,
            servicioTratamientos,
            new ServicioMedicamentos(repositorioMedicamentos, auditoria),
            new ServicioImportacionNomenclator(new LectorNomenclatorCsv(), repositorioMedicamentos, auditoria),
            new ServicioConsultaCima(new HttpClient(), auditoria),
            new ServicioRegistrosCalidad(new RepositorioRegistrosCalidad(conexion), auditoria),
            new ServicioControlDocumental(new RepositorioControlDocumental(conexion), repositorioUsuarios, auditoria),
            new ServicioBackup(conexion, repositorioFarmacia, auditoria),
            new ServicioCifrado(conexion, ":memory:", Path.GetTempPath(), auditoria, new GeneradorFraseRecuperacion()),
            servicioEnvases,
            servicioListadoRetirada,
            new ServicioImportacionTratamientoEnvase(
                repositorioMedicamentos, new RepositorioTratamientos(conexion), repositorioEnvases,
                new RepositorioPerfilesImportacionTratamiento(conexion), servicioTratamientos, auditoria),
            new ServicioComunicacionesMedico(
                new RepositorioComunicacionesMedico(conexion), repositorioPacientes, new RepositorioTratamientos(conexion), auditoria),
            new ServicioPerfilesImportacion(new RepositorioPerfilesImportacion(conexion), auditoria),
            new ServicioExportacionPacientes(),
            servicioPreparacion,
            documentos,
            new ServicioIdoneidadConsentimiento(
                repositorioEvaluaciones, repositorioConsentimientos, new RepositorioContactos(conexion),
                repositorioPacientes, servicioPacientes, auditoria),
            new ServicioAvisosInicio(servicioListadoRetirada, repositorioSpd, repositorioPacientes, new RepositorioRegistrosAmbientales(conexion)),
            new ServicioGeneracionLote(servicioPreparacion, documentos, repositorioSpd, repositorioPacientes),
            new ServicioPurga(
                repositorioPacientes, repositorioEnvases, repositorioUsuarios, hasheador, repositorioFarmacia,
                new RepositorioPurga(conexion), auditoria));
    }
}
