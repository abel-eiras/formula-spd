using System.Net;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioConsultaCimaTests
{
    private static (ServicioConsultaCima Servicio, FakeHttpMessageHandler Handler) Crear()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://cima.aemps.es/cima/rest/") };
        var servicio = new ServicioConsultaCima(httpClient, new RegistradorAuditoria(conexion));
        return (servicio, handler);
    }

    [Fact]
    public async Task ConsultarPorCn_con_respuesta_204_devuelve_no_encontrado_sin_error()
    {
        var (servicio, handler) = Crear();
        handler.CodigoEstado = HttpStatusCode.NoContent;

        var resultado = await servicio.ConsultarPorCnAsync("140004");

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Error);
    }

    [Fact]
    public async Task ConsultarPorCn_con_fallo_de_red_devuelve_el_motivo()
    {
        var (servicio, handler) = Crear();
        handler.LanzarFalloDeRed = true;

        var resultado = await servicio.ConsultarPorCnAsync("650004");

        Assert.False(resultado.Encontrado);
        Assert.NotNull(resultado.Error);
    }

    [Fact]
    public async Task ConsultarPorCn_mapea_forma_farmaceutica_simplificada_y_extrae_el_nombre_de_la_presentacion()
    {
        var (servicio, handler) = Crear();
        handler.RespuestaJson = """
            {
              "nombre": "DEPAKINE 500 mg COMPRIMIDOS GASTRORRESISTENTES",
              "pactivos": "VALPROATO SODIO",
              "labtitular": "Sanofi Aventis S.A.",
              "formaFarmaceuticaSimplificada": {"id":10,"nombre":"COMPRIMIDO"},
              "presentaciones": [{"cn":"650004","nombre":"DEPAKINE 500 mg COMPRIMIDOS GASTRORRESISTENTES, 20 comprimidos (Blister)"}]
            }
            """;

        var resultado = await servicio.ConsultarPorCnAsync("650004");

        Assert.True(resultado.Encontrado);
        Assert.Equal("DEPAKINE 500 mg COMPRIMIDOS GASTRORRESISTENTES, 20 comprimidos (Blister)", resultado.Nombre);
        Assert.Equal("VALPROATO SODIO", resultado.PrincipioActivo);
        Assert.Equal("Sanofi Aventis S.A.", resultado.Laboratorio);
        Assert.Equal(FormaFarmaceutica.Comprimido, resultado.FormaFarmaceutica);
        Assert.Equal("Blister", resultado.EnvaseIndicado);
    }

    [Fact]
    public async Task ConsultarPorCn_con_forma_simplificada_sin_equivalente_cerrado_deja_FormaFarmaceutica_a_null()
    {
        var (servicio, handler) = Crear();
        handler.RespuestaJson = """
            {
              "nombre": "AMBROXOL RATIOPHARM 3 mg/ ml JARABE EFG",
              "pactivos": "AMBROXOL HIDROCLORURO",
              "labtitular": "Ratiopharm Espana S.A.",
              "formaFarmaceuticaSimplificada": {"id":93,"nombre":"SOLUCIÓN/SUSPENSIÓN ORAL"},
              "presentaciones": [{"cn":"999999","nombre":"AMBROXOL RATIOPHARM 3 mg/ ml JARABE EFG , 1 frasco de 200 ml"}]
            }
            """;

        var resultado = await servicio.ConsultarPorCnAsync("999999");

        Assert.True(resultado.Encontrado);
        Assert.Null(resultado.FormaFarmaceutica);
        Assert.Null(resultado.EnvaseIndicado);
    }
}
