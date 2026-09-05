using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Smoke tests de la Fase 1 (Foundational) de Spec 009: migración 0004 (renumerada de
/// 0002 al fusionar tras Specs 001 y 003, que se mergearon primero) y repositorios de registros de
/// calidad funcionan de extremo a extremo antes de construir las user stories encima (Art. IX.1).
/// T013/T014 de tasks.md.</summary>
public sealed class InfraestructuraRegistrosCalidadFundamentosTests
{
    private static (SqliteConnection Conexion, int UsuarioId) AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var usuarioId = repositorioUsuarios.Crear(new Usuario
        {
            Nombre = "Ana", Apellidos = "Administradora", Login = "ana.admin",
            HashPassword = "hash-de-prueba", Rol = Rol.Administrador
        });
        return (conexion, usuarioId);
    }

    [Fact]
    public void AplicadorMigraciones_aplica_0004_de_forma_idempotente()
    {
        var (conexion, _) = AbrirBaseDeDatosDePrueba();
        var versionTrasLaPrimeraAplicacion = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");

        new AplicadorMigraciones(conexion).Aplicar();

        var version = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");
        Assert.Equal(versionTrasLaPrimeraAplicacion, version);
        Assert.True(version >= 4);
    }

    [Fact]
    public void RepositorioRegistrosCalidad_crea_y_lista_formacion_y_residuos()
    {
        var (conexion, usuarioId) = AbrirBaseDeDatosDePrueba();
        var repositorio = new RepositorioRegistrosCalidad(conexion);

        repositorio.Crear(new FormacionPersonal
        {
            UsuarioId = usuarioId, NombreCurso = "Curso de manipulación", Fecha = DateOnly.FromDateTime(DateTime.Today)
        });
        repositorio.Crear(new RecogidaResiduos
        {
            Fecha = DateOnly.FromDateTime(DateTime.Today), EmpresaGestora = "Gestora S.L.", UsuarioId = usuarioId
        });

        Assert.Single(repositorio.ListarFormacion(usuarioId));
        Assert.Single(repositorio.ListarRecogidaResiduos());
    }

    [Fact]
    public void RepositorioControlDocumental_crea_y_lista_cambios_y_copias()
    {
        var (conexion, usuarioId) = AbrirBaseDeDatosDePrueba();
        var repositorio = new RepositorioControlDocumental(conexion);

        repositorio.Crear(new ControlCambiosPNT
        {
            Documento = "PNT-SPD", Version = "1.1", DescripcionCambio = "Actualización anual",
            Fecha = DateOnly.FromDateTime(DateTime.Today), RedactadoPor = usuarioId, RevisadoPor = usuarioId, AprobadoPor = usuarioId
        });
        repositorio.Crear(new ControlCopias
        {
            Documento = "PNT-SPD", NumCopia = 1, UsuarioId = usuarioId, Fecha = DateOnly.FromDateTime(DateTime.Today)
        });

        Assert.Single(repositorio.ListarCambiosPnt());
        Assert.Single(repositorio.ListarCopias());
    }
}
