using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Smoke tests de la Fase 1 (Foundational) de Spec 011: migración 0008 y repositorio de
/// PerfilImportacion funcionan de extremo a extremo antes de construir las user stories
/// (Art. IX.1).</summary>
public sealed class InfraestructuraPerfilesImportacionFundamentosTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    [Fact]
    public void AplicadorMigraciones_aplica_0008_de_forma_idempotente()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var versionTrasLaPrimeraAplicacion = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");

        new AplicadorMigraciones(conexion).Aplicar();

        var version = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");
        Assert.Equal(versionTrasLaPrimeraAplicacion, version);
        Assert.True(version >= 8);
    }

    [Fact]
    public void RepositorioPerfilesImportacion_crea_y_obtiene_por_nombre()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var repositorio = new RepositorioPerfilesImportacion(conexion);

        var perfil = new PerfilImportacion
        {
            Nombre = "Mi perfil",
            Tipo = TipoPerfilImportacion.Pacientes,
            Mapeo = [new ParCampoColumna("Nombre", "0"), new ParCampoColumna("Apellidos", "1")]
        };
        repositorio.Crear(perfil);

        var obtenido = repositorio.ObtenerPorNombre("Mi perfil");

        Assert.NotNull(obtenido);
        Assert.Equal(2, obtenido!.Mapeo.Count);
        Assert.Equal("Nombre", obtenido.Mapeo[0].Campo);
    }
}
