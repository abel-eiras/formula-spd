using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Smoke tests de la Fase 1 (Foundational) de Spec 005: migración 0006 y repositorio de
/// Envase funcionan de extremo a extremo antes de construir las user stories (Art. IX.1).</summary>
public sealed class InfraestructuraEnvasesFundamentosTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    [Fact]
    public void AplicadorMigraciones_aplica_0006_de_forma_idempotente()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var versionTrasLaPrimeraAplicacion = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");

        new AplicadorMigraciones(conexion).Aplicar();

        var version = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");
        Assert.Equal(versionTrasLaPrimeraAplicacion, version);
        Assert.True(version >= 6);
    }

    [Fact]
    public void RepositorioEnvases_crea_y_obtiene_por_id_y_por_serie()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, medicamentoId) = InfraestructuraTratamientosFundamentosTests.CrearPacienteYMedicamento(conexion);
        var repositorio = new RepositorioEnvases(conexion);

        var envase = new Envase
        {
            PacienteId = pacienteId,
            MedicamentoId = medicamentoId,
            Serie = "S-0001",
            Lote = "L1",
            Caducidad = new DateOnly(2027, 6, 30),
            UnidadesIniciales = 28,
            UnidadesRestantes = 28
        };
        envase.Id = repositorio.Crear(envase);

        var porId = repositorio.ObtenerPorId(envase.Id);
        var porSerie = repositorio.ObtenerPorSerie("S-0001");

        Assert.NotNull(porId);
        Assert.NotNull(porSerie);
        Assert.Equal(EstadoEnvase.EnCustodia, porId!.Estado);
        Assert.Equal(28, porId.UnidadesRestantes);
        Assert.Equal(envase.Id, porSerie!.Id);
    }
}
