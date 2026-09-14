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
        handler.RespuestaJson = """[{"tag_name": "2.0.0", "html_url": "https://github.com/abel-eiras/formula-spd/releases/tag/2.0.0", "draft": false}]""";

        var resultado = await servicio.ComprobarActualizacionesAsync("1.0.0", administradorQueEjecutaId: 1);

        Assert.True(resultado.Exito);
        Assert.True(resultado.HayNueva);
        Assert.Equal("2.0.0", resultado.VersionDisponible);
    }

    /// <summary>Las primeras versiones son betas: `releases/latest` las omite, así que se mira la lista y se
    /// elige la más alta. Los borradores no cuentan.</summary>
    [Fact]
    public async Task ComprobarActualizaciones_ve_la_beta_siguiente_e_ignora_los_borradores()
    {
        var (servicio, _, handler) = Crear();
        handler.RespuestaJson = """
            [{"tag_name": "v0.1.0-beta", "html_url": "u1", "draft": false},
             {"tag_name": "v0.3.0", "html_url": "u3", "draft": true},
             {"tag_name": "v0.1.0-beta.2", "html_url": "u2", "draft": false}]
            """;

        var resultado = await servicio.ComprobarActualizacionesAsync("0.1.0-beta", administradorQueEjecutaId: 1);

        Assert.True(resultado.Exito);
        Assert.True(resultado.HayNueva);
        Assert.Equal("v0.1.0-beta.2", resultado.VersionDisponible);
        Assert.Equal("u2", resultado.UrlDescarga);
    }

    [Fact]
    public async Task ComprobarActualizaciones_sin_releases_publicadas_no_hay_novedad_ni_error()
    {
        var (servicio, _, handler) = Crear();
        handler.RespuestaJson = "[]";

        var resultado = await servicio.ComprobarActualizacionesAsync("0.1.0-beta", administradorQueEjecutaId: 1);

        Assert.True(resultado.Exito);
        Assert.False(resultado.HayNueva);
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
