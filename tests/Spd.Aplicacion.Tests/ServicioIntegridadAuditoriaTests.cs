using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Spec 017 CA-1701..1703 sobre una base real: las entradas se sellan al registrarse y una
/// manipulación hecha con SQL directo se detecta.</summary>
public sealed class ServicioIntegridadAuditoriaTests
{
    private static (RegistradorAuditoria Registrador, ServicioIntegridadAuditoria Servicio) Montar(SqliteConnection c)
    {
        c.Open();
        new AplicadorMigraciones(c).Aplicar();
        var registrador = new RegistradorAuditoria(c);
        return (registrador, new ServicioIntegridadAuditoria(registrador, registrador));
    }

    [Fact]
    public void Entradas_registradas_por_la_aplicacion_son_integras_CA_1701()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var (registrador, servicio) = Montar(c);
        registrador.Registrar(1, "ALTA", "Paciente", 1, "uno");
        registrador.Registrar(1, "MODIFICACION", "Paciente", 1, null);

        var r = servicio.Comprobar(1);

        Assert.True(r.Integra);
        Assert.Equal(2, r.Selladas);
        // La comprobación deja su propia traza, sellada también.
        Assert.True(servicio.Comprobar(1).Integra);
    }

    [Fact]
    public void Modificacion_con_SQL_directo_se_detecta_CA_1702()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var (registrador, servicio) = Montar(c);
        registrador.Registrar(1, "ALTA", "Paciente", 1, "uno");
        registrador.Registrar(1, "ALTA", "Paciente", 2, "dos");
        registrador.Registrar(1, "ALTA", "Paciente", 3, "tres");

        c.Execute("UPDATE Auditoria SET detalle = 'cambiado' WHERE entidad_id = 2");

        var r = servicio.Comprobar(1);
        Assert.False(r.Integra);
        Assert.Contains("modificado", r.Problema);
    }

    [Fact]
    public void Borrado_con_SQL_directo_se_detecta_CA_1703()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var (registrador, servicio) = Montar(c);
        registrador.Registrar(1, "ALTA", "Paciente", 1, "uno");
        registrador.Registrar(1, "ALTA", "Paciente", 2, "dos");
        registrador.Registrar(1, "ALTA", "Paciente", 3, "tres");

        c.Execute("DELETE FROM Auditoria WHERE entidad_id = 2");

        Assert.False(servicio.Comprobar(1).Integra);
    }
}
