using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class InfraestructuraIdoneidadFundamentosTests
{
    [Fact]
    public void Migracion_0011_crea_las_tablas_y_los_repositorios_hacen_ida_y_vuelta()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var tablas = conexion.Query<string>("SELECT name FROM sqlite_master WHERE type = 'table'").ToList();
        Assert.Contains("EvaluacionIdoneidad", tablas);
        Assert.Contains("Consentimiento", tablas);

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0", Direccion = "D", Cp = "36000",
            Poblacion = "P", Telefono = "9"
        });
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var paciente = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, new RegistradorAuditoria(conexion)).Crear(
            new DatosAltaPaciente("Ana", "Pérez", null, "12345678Z", null, null, null, null, null, null, null, null, null, null, null, null, null, false, null, null, null), null);

        var evaluaciones = new RepositorioEvaluacionesIdoneidad(conexion);
        var id = evaluaciones.Crear(new EvaluacionIdoneidad
        {
            PacienteId = paciente.Id, Fecha = new DateTime(2026, 9, 6, 10, 0, 0, DateTimeKind.Utc), Criterio3 = true, Criterio7 = true,
            CondicionMotivacion = true, CondicionDestreza = false, Observaciones = "obs", Resultado = ResultadoIdoneidad.NoApto
        });
        var leida = evaluaciones.ObtenerPorId(id)!;
        Assert.True(leida.Criterio3); Assert.True(leida.Criterio7); Assert.False(leida.Criterio1);
        Assert.True(leida.CondicionMotivacion); Assert.False(leida.CondicionDestreza);
        Assert.Equal(ResultadoIdoneidad.NoApto, leida.Resultado);
        Assert.Equal("obs", leida.Observaciones);
        Assert.Equal(new DateTime(2026, 9, 6, 10, 0, 0, DateTimeKind.Utc), leida.Fecha);

        var consentimientos = new RepositorioConsentimientos(conexion);
        var cid = consentimientos.Crear(new Consentimiento { PacienteId = paciente.Id, Tipo = TipoConsentimiento.Paciente, FechaCreacion = DateTime.UtcNow });
        var c = consentimientos.ObtenerPorId(cid)!;
        Assert.Null(c.FechaFirma);
        c.FechaFirma = new DateOnly(2026, 9, 1);
        c.ImpresoEn = DateTime.UtcNow;
        consentimientos.Actualizar(c);
        var releido = consentimientos.ObtenerPorId(cid)!;
        Assert.Equal(new DateOnly(2026, 9, 1), releido.FechaFirma);
        Assert.NotNull(releido.ImpresoEn);
        Assert.True(releido.Vigente);
        Assert.Equal(cid, consentimientos.ObtenerVigente(paciente.Id)!.Id);
    }
}
