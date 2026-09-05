using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Smoke tests de la Fase 1 (Foundational) de Spec 003: migración 0002 (de esta rama) y
/// repositorio de Medicamento funcionan de extremo a extremo antes de construir las user stories
/// encima (Art. IX.1). T008/T009 de tasks.md.</summary>
public sealed class InfraestructuraMedicamentosFundamentosTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    [Fact]
    public void AplicadorMigraciones_aplica_0002_de_forma_idempotente()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();

        new AplicadorMigraciones(conexion).Aplicar();

        var version = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");
        Assert.Equal(2, version);
    }

    [Fact]
    public void RepositorioMedicamentos_crea_y_obtiene_por_cn()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var repositorio = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento
        {
            Cn = "654321",
            Nombre = "Paracetamol 1g",
            NombreNormalizado = Normalizador.QuitarTildesYMayusculas("Paracetamol 1g")
        };

        repositorio.Crear(medicamento);
        var obtenido = repositorio.ObtenerPorCn("654321");

        Assert.NotNull(obtenido);
        Assert.Equal("Paracetamol 1g", obtenido!.Nombre);
        Assert.True(obtenido.AptoSpd);
        Assert.True(obtenido.Activo);
    }
}
