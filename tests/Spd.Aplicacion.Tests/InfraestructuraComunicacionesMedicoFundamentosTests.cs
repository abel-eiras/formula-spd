using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Smoke tests de la Fase 1 (Foundational) de Spec 008: migración 0007 y repositorio de
/// ComunicacionMedico funcionan de extremo a extremo antes de construir las user stories
/// (Art. IX.1).</summary>
public sealed class InfraestructuraComunicacionesMedicoFundamentosTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    [Fact]
    public void AplicadorMigraciones_aplica_0007_de_forma_idempotente()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var versionTrasLaPrimeraAplicacion = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");

        new AplicadorMigraciones(conexion).Aplicar();

        var version = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");
        Assert.Equal(versionTrasLaPrimeraAplicacion, version);
        Assert.True(version >= 7);
    }

    [Fact]
    public void RepositorioComunicacionesMedico_crea_y_obtiene_por_id()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, medicoId) = CrearPacienteYMedico(conexion);
        var repositorio = new RepositorioComunicacionesMedico(conexion);

        var comunicacion = new ComunicacionMedico
        {
            PacienteId = pacienteId, MedicoId = medicoId, Tipo = TipoComunicacionMedico.Presentacion,
            Fecha = new DateOnly(2026, 9, 6)
        };
        var id = repositorio.Crear(comunicacion);
        var obtenida = repositorio.ObtenerPorId(id);

        Assert.NotNull(obtenida);
        Assert.Equal(TipoComunicacionMedico.Presentacion, obtenida!.Tipo);
        Assert.Null(obtenida.Respuesta);
    }

    internal static (int PacienteId, int MedicoId) CrearPacienteYMedico(SqliteConnection conexion)
    {
        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000"
        });
        var repositorioMedicos = new RepositorioMedicos(conexion);
        var medico = new Medico { Nombre = "Carmen", Apellidos = "López", Colegiado = "36-1234" };
        medico.Id = repositorioMedicos.Crear(medico);

        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente(
                "Ana", "Pérez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, medico.Id, null, null, null, false, null, null, null),
            usuarioQueEjecutaId: null);

        return (paciente.Id, medico.Id);
    }
}
