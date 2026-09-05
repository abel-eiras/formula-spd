using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Smoke tests de la Fase 2 (Foundational): migraciones, repositorios y hasheador
/// funcionan de extremo a extremo antes de construir las user stories encima (Art. IX.1).</summary>
public sealed class InfraestructuraFundamentosTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    [Fact]
    public void AplicadorMigraciones_es_idempotente()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var versionTrasLaPrimeraAplicacion = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");

        new AplicadorMigraciones(conexion).Aplicar();

        // No debe reaplicar ni cambiar la versión ya alcanzada (Art. VIII.3); el número exacto de
        // migraciones embebidas lo cubre InfraestructuraMedicamentosFundamentosTests (Spec 003).
        var version = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");
        Assert.Equal(versionTrasLaPrimeraAplicacion, version);
    }

    [Fact]
    public void RepositorioFarmacia_crea_y_obtiene_la_fila_unica()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var repositorio = new RepositorioFarmacia(conexion);
        var farmacia = new Farmacia
        {
            CodigoSanitario = "PO-001",
            Nombre = "Farmacia de Prueba",
            TitularOComunidadBienes = "Titular de Prueba",
            Cif = "B00000000",
            Direccion = "Calle Falsa 1",
            Cp = "36000",
            Poblacion = "Pontevedra",
            Telefono = "986000000"
        };

        repositorio.Crear(farmacia);
        var obtenida = repositorio.Obtener();

        Assert.NotNull(obtenida);
        Assert.Equal("Farmacia de Prueba", obtenida!.Nombre);
    }

    [Fact]
    public void RepositorioUsuarios_persiste_el_rol_correctamente()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var repositorio = new RepositorioUsuarios(conexion);
        var usuario = new Usuario
        {
            Nombre = "Ana",
            Apellidos = "Administradora",
            Login = "ana.admin",
            HashPassword = "hash-de-prueba",
            Rol = Rol.Administrador
        };

        var id = repositorio.Crear(usuario);
        var obtenido = repositorio.ObtenerPorId(id);

        Assert.NotNull(obtenido);
        Assert.Equal(Rol.Administrador, obtenido!.Rol);
        Assert.Equal(1, repositorio.ContarAdministradoresActivos());
    }

    [Fact]
    public void HasheadorArgon2id_verifica_solo_la_contrasena_correcta()
    {
        var hasheador = new HasheadorArgon2id();
        var hash = hasheador.Hashear("contraseña-correcta");

        Assert.True(hasheador.Verificar("contraseña-correcta", hash));
        Assert.False(hasheador.Verificar("otra-contraseña", hash));
    }

    [Fact]
    public void RegistradorAuditoria_inserta_una_fila()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var registrador = new RegistradorAuditoria(conexion);

        registrador.Registrar(usuarioId: null, accion: "PRUEBA", entidad: "Farmacia", entidadId: 1, detalle: null);

        var total = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Auditoria");
        Assert.Equal(1, total);
    }
}
