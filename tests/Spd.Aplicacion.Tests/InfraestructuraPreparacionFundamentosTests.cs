using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Smoke tests de la Fase 1 (Foundational) de Spec 006: migración 0009 y repositorio de
/// SPD funcionan de extremo a extremo antes de construir las user stories (Art. IX.1).</summary>
public sealed class InfraestructuraPreparacionFundamentosTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    [Fact]
    public void AplicadorMigraciones_aplica_0009_de_forma_idempotente()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var versionTrasLaPrimeraAplicacion = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");

        new AplicadorMigraciones(conexion).Aplicar();

        var version = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");
        Assert.Equal(versionTrasLaPrimeraAplicacion, version);
        Assert.True(version >= 9);
    }

    [Fact]
    public void RepositorioSpd_crea_y_obtiene_por_id()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, _) = InfraestructuraTratamientosFundamentosTests.CrearPacienteYMedicamento(conexion);
        var elaboradorId = CrearUsuario(conexion);
        var repositorio = new RepositorioSpd(conexion);

        var spd = new SPD
        {
            NumRegistro = "F-000001",
            PacienteId = pacienteId,
            SesionId = Guid.NewGuid(),
            ValidezDesde = new DateOnly(2026, 9, 7),
            ValidezHasta = new DateOnly(2026, 9, 13),
            ElaboradorId = elaboradorId
        };
        spd.Id = repositorio.Crear(spd);
        var obtenido = repositorio.ObtenerPorId(spd.Id);

        Assert.NotNull(obtenido);
        Assert.Equal(EstadoSpd.Borrador, obtenido!.Estado);
        Assert.Equal(spd.SesionId, obtenido.SesionId);
    }

    internal static int CrearUsuario(SqliteConnection conexion)
    {
        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var usuario = new Usuario
        {
            Nombre = "Elena", Apellidos = "Ruiz", Login = "elena.ruiz", HashPassword = "x", Rol = Rol.Elaborador
        };
        return repositorioUsuarios.Crear(usuario);
    }
}
