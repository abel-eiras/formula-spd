using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioActualizacionesTests
{
    private static (ServicioActualizaciones Servicio, SqliteConnection Conexion, FakeHttpMessageHandler Handler) Crear()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") };
        var servicio = new ServicioActualizaciones(httpClient, new RegistradorAuditoria(conexion));
        return (servicio, conexion, handler);
    }

    [Fact]
    public async Task ComprobarActualizaciones_con_fallo_de_red_no_lanza_excepcion_no_controlada()
    {
        var (servicio, _, handler) = Crear();
        handler.LanzarFalloDeRed = true;

        var resultado = await servicio.ComprobarActualizacionesAsync("1.0.0", administradorQueEjecutaId: 1);

        Assert.False(resultado.Exito);
        Assert.NotNull(resultado.Motivo);
    }

    [Fact]
    public async Task ComprobarActualizaciones_detecta_una_version_nueva()
    {
        var (servicio, _, handler) = Crear();
        handler.RespuestaJson = """{"tag_name": "2.0.0", "html_url": "https://github.com/abel-eiras/spd/releases/tag/2.0.0"}""";

        var resultado = await servicio.ComprobarActualizacionesAsync("1.0.0", administradorQueEjecutaId: 1);

        Assert.True(resultado.Exito);
        Assert.True(resultado.HayNueva);
        Assert.Equal("2.0.0", resultado.VersionDisponible);
    }

    [Fact]
    public async Task ComprobarActualizaciones_registra_en_auditoria_tanto_el_exito_como_el_fallo()
    {
        var (servicio, conexion, handler) = Crear();

        await servicio.ComprobarActualizacionesAsync("1.0.0", administradorQueEjecutaId: 1);
        handler.LanzarFalloDeRed = true;
        await servicio.ComprobarActualizacionesAsync("1.0.0", administradorQueEjecutaId: 1);

        var total = conexion.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Auditoria WHERE accion = 'COMPROBAR_ACTUALIZACIONES'");
        Assert.Equal(2, total);
    }
}
