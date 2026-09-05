using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioNomenclatorTests
{
    private static (ServicioNomenclator Servicio, SqliteConnection Conexion, FakeHttpMessageHandler Handler) Crear()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var handler = new FakeHttpMessageHandler();
        var servicio = new ServicioNomenclator(new HttpClient(handler), new RegistradorAuditoria(conexion));
        return (servicio, conexion, handler);
    }

    [Fact]
    public async Task DescargarNomenclator_con_url_que_no_responde_devuelve_motivo_de_fallo()
    {
        var (servicio, _, handler) = Crear();
        handler.LanzarFalloDeRed = true;
        var rutaDestino = Path.Combine(Path.GetTempPath(), $"nomenclator-{Guid.NewGuid():N}.csv");

        var resultado = await servicio.DescargarNomenclatorAsync(
            "http://url-que-no-responde.invalid/nomenclator.csv", rutaDestino, administradorQueEjecutaId: 1);

        Assert.False(resultado.Exito);
        Assert.NotNull(resultado.Motivo);
        Assert.False(File.Exists(rutaDestino));
    }

    [Fact]
    public async Task DescargarNomenclator_con_exito_guarda_el_fichero_y_registra_auditoria()
    {
        var (servicio, conexion, handler) = Crear();
        handler.RespuestaJson = "cn,nombre\n123456,Ejemplo";
        var rutaDestino = Path.Combine(Path.GetTempPath(), $"nomenclator-{Guid.NewGuid():N}.csv");

        try
        {
            var resultado = await servicio.DescargarNomenclatorAsync(
                "http://nomenclator.local/fichero.csv", rutaDestino, administradorQueEjecutaId: 1);

            Assert.True(resultado.Exito);
            Assert.True(File.Exists(rutaDestino));
            var total = conexion.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Auditoria WHERE accion = 'DESCARGA_NOMENCLATOR'");
            Assert.Equal(1, total);
        }
        finally
        {
            if (File.Exists(rutaDestino))
            {
                File.Delete(rutaDestino);
            }
        }
    }
}
