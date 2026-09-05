using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Smoke tests de la Fase 1 (Foundational) de Spec 009: migración 0002 (de esta rama) y
/// repositorios de registros de calidad funcionan de extremo a extremo antes de construir las
/// user stories encima (Art. IX.1). T013/T014 de tasks.md.</summary>
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
    public void AplicadorMigraciones_aplica_0002_de_forma_idempotente()
    {
        var (conexion, _) = AbrirBaseDeDatosDePrueba();

        new AplicadorMigraciones(conexion).Aplicar();

        var version = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");
        Assert.Equal(2, version);
    }

    [Fact]
    public void Farmacia_tiene_la_columna_umbral_dias_aviso_calidad_con_valor_por_defecto_7()
    {
        var (conexion, _) = AbrirBaseDeDatosDePrueba();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000"
        });

        var umbral = conexion.ExecuteScalar<int>("SELECT umbral_dias_aviso_calidad FROM Farmacia");

        Assert.Equal(7, umbral);
    }

    [Fact]
    public void RepositorioRegistrosCalidad_crea_y_lista_ambiental_y_limpieza()
    {
        var (conexion, usuarioId) = AbrirBaseDeDatosDePrueba();
        var repositorio = new RepositorioRegistrosCalidad(conexion);

        repositorio.Crear(new RegistroAmbiental
        {
            FechaHora = DateTime.UtcNow, Temperatura = 20, Humedad = 50, UsuarioId = usuarioId, FueraDeRango = false
        });
        repositorio.Crear(new RegistroLimpieza { Fecha = DateTime.UtcNow, UsuarioId = usuarioId, Tipo = TipoLimpieza.Rutinaria });

        Assert.Single(repositorio.ListarAmbiental());
        Assert.Single(repositorio.ListarLimpieza());
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
