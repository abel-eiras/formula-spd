using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Smoke tests de la Fase 1 (Foundational) de Spec 004: migración 0005 y repositorio de
/// Tratamiento funcionan de extremo a extremo antes de construir las user stories (Art. IX.1).</summary>
public sealed class InfraestructuraTratamientosFundamentosTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    [Fact]
    public void AplicadorMigraciones_aplica_0005_de_forma_idempotente()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var versionTrasLaPrimeraAplicacion = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");

        new AplicadorMigraciones(conexion).Aplicar();

        var version = conexion.ExecuteScalar<int>("SELECT MAX(version) FROM schema_version");
        Assert.Equal(versionTrasLaPrimeraAplicacion, version);
        Assert.True(version >= 5);
    }

    [Fact]
    public void RepositorioTratamientos_crea_y_obtiene_por_id()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, medicamentoId) = CrearPacienteYMedicamento(conexion);
        var repositorio = new RepositorioTratamientos(conexion);

        var tratamiento = new Tratamiento
        {
            PacienteId = pacienteId,
            MedicamentoId = medicamentoId,
            PautaD = FraccionDosis.Uno,
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        };
        var id = repositorio.Crear(tratamiento);
        var obtenido = repositorio.ObtenerPorId(id);

        Assert.NotNull(obtenido);
        Assert.Equal(FraccionDosis.Uno, obtenido!.PautaD);
        Assert.Equal(EstadoTratamiento.Activo, obtenido.Estado);
    }

    internal static (int PacienteId, int MedicamentoId) CrearPacienteYMedicamento(SqliteConnection conexion)
    {
        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000"
        });
        var servicioPacientes = new ServicioPacientes(new RepositorioPacientes(conexion), repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente(
                "Ana", "Pérez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, null),
            usuarioQueEjecutaId: null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g" };
        medicamento.Id = repositorioMedicamentos.Crear(medicamento);

        return (paciente.Id, medicamento.Id);
    }
}
