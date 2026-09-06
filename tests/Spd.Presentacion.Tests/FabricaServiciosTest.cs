using Microsoft.Data.Sqlite;
using Spd.Infraestructura;

namespace Spd.Presentacion.Tests;

/// <summary>Construcción del motor de documentos con todos sus repositorios sobre una conexión de
/// test, para que cada test de ventana no repita la lista completa de dependencias.</summary>
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
}
