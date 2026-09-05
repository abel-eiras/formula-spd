using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Art. VI.3: ninguna de las dos excepciones de red es automática al arrancar.
/// Cobertura real: construir <see cref="ServicioActualizaciones"/>/<see cref="ServicioNomenclator"/>
/// no hace ninguna petición por sí solo — solo sus métodos explícitos la hacen. Esto NO sustituye a
/// una revisión de que <c>App.axaml.cs</c> nunca llama a esos métodos en el arranque; esa parte es
/// de revisión de código, no de test automatizado (remediación E1, con esta limitación documentada
/// en PROGRESO.md).</summary>
public sealed class ArranqueSinRedTests
{
    [Fact]
    public void Construir_ServicioActualizaciones_no_hace_ninguna_peticion_de_red()
    {
        var conexion = AbrirBaseDeDatos();
        var handler = new FakeHttpMessageHandler { LanzarFalloDeRed = true }; // cualquier llamada fallaría
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") };

        var servicio = new ServicioActualizaciones(httpClient, new RegistradorAuditoria(conexion));

        Assert.NotNull(servicio); // construir no lanza, porque no llama a nada todavía
        var totalAuditoria = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Auditoria");
        Assert.Equal(0, totalAuditoria);
    }

    [Fact]
    public void Construir_ServicioNomenclator_no_hace_ninguna_peticion_de_red()
    {
        var conexion = AbrirBaseDeDatos();
        var handler = new FakeHttpMessageHandler { LanzarFalloDeRed = true };

        var servicio = new ServicioNomenclator(new HttpClient(handler), new RegistradorAuditoria(conexion));

        Assert.NotNull(servicio);
        var totalAuditoria = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Auditoria");
        Assert.Equal(0, totalAuditoria);
    }

    private static SqliteConnection AbrirBaseDeDatos()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }
}
