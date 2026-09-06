using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Completa el punto de extensión que Spec 005 dejó documentado (research.md Decisión 3):
/// ahora que existe SPD (Spec 006), esta es la implementación real de FR-530/CA-502.</summary>
public sealed class ComprobadorCoberturaSpdRealTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    private static int CrearSpd(SqliteConnection conexion, int pacienteId, EstadoSpd estado, DateOnly desde, DateOnly hasta)
    {
        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var elaborador = new Usuario { Nombre = "Elena", Apellidos = "Ruiz", Login = "elena", HashPassword = "x", Rol = Rol.Elaborador };
        elaborador.Id = repositorioUsuarios.Crear(elaborador);

        var repositorio = new RepositorioSpd(conexion);
        var spd = new SPD
        {
            NumRegistro = $"F-{Guid.NewGuid():N}"[..12], PacienteId = pacienteId, SesionId = Guid.NewGuid(),
            ValidezDesde = desde, ValidezHasta = hasta, ElaboradorId = elaborador.Id, Estado = estado
        };
        return repositorio.Crear(spd);
    }

    [Fact]
    public void YaCubierta_es_true_si_hay_un_spd_preparado_que_cubre_la_fecha()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, _) = InfraestructuraTratamientosFundamentosTests.CrearPacienteYMedicamento(conexion);
        CrearSpd(conexion, pacienteId, EstadoSpd.Preparado, new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 13));
        var comprobador = new ComprobadorCoberturaSpdReal(new RepositorioSpd(conexion));

        Assert.True(comprobador.YaCubierta(pacienteId, new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void YaCubierta_es_false_si_el_spd_esta_en_borrador()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, _) = InfraestructuraTratamientosFundamentosTests.CrearPacienteYMedicamento(conexion);
        CrearSpd(conexion, pacienteId, EstadoSpd.Borrador, new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 13));
        var comprobador = new ComprobadorCoberturaSpdReal(new RepositorioSpd(conexion));

        Assert.False(comprobador.YaCubierta(pacienteId, new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void YaCubierta_es_false_si_la_fecha_no_esta_dentro_de_la_validez()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, _) = InfraestructuraTratamientosFundamentosTests.CrearPacienteYMedicamento(conexion);
        CrearSpd(conexion, pacienteId, EstadoSpd.Verificado, new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 13));
        var comprobador = new ComprobadorCoberturaSpdReal(new RepositorioSpd(conexion));

        Assert.False(comprobador.YaCubierta(pacienteId, new DateOnly(2026, 9, 20)));
    }
}
