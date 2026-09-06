using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Spec 010 §4.3: purga manual, paciente a paciente, con contraseña (CA-1005), traza que
/// sobrevive (CA-1006) y bloqueo por envases en custodia (CA-1007).</summary>
public sealed class ServicioPurgaTests
{
    private const string PasswordAdmin = "contraseña-inicial";

    private sealed record Contexto(SqliteConnection Conexion, ServicioPurga Servicio, ServicioPacientes Pacientes, RepositorioEnvases Envases, int AdministradorId, int MedicamentoId);

    private static Contexto Crear()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0", Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9"
        });
        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var hasheador = new HasheadorArgon2id();
        var admin = new ServicioUsuarios(repositorioUsuarios, hasheador, auditoria).CrearUsuario(
            new DatosAltaUsuario("Ana", "Admin", "ana", PasswordAdmin, Rol.Administrador, null, null), null).Usuario;

        var repositorioPacientes = new RepositorioPacientes(conexion);
        var pacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var medicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "111111", Nombre = "Med" };
        medicamento.Id = medicamentos.Crear(medicamento);

        var envases = new RepositorioEnvases(conexion);
        var servicio = new ServicioPurga(repositorioPacientes, envases, repositorioUsuarios, hasheador, repositorioFarmacia, new RepositorioPurga(conexion), auditoria);
        return new Contexto(conexion, servicio, pacientes, envases, admin.Id, medicamento.Id);
    }

    private static Paciente PacienteEnBaja(Contexto ctx, string nombre, string dni, int aniosDesdeBaja)
    {
        var p = ctx.Pacientes.Crear(new DatosAltaPaciente(nombre, "Purgable", null, dni, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null, null, null), null);
        ctx.Pacientes.CambiarEstado(p.Id, EstadoPaciente.Baja, new DatosBaja(DateTime.Today.AddYears(-aniosDesdeBaja).AddDays(-1), MotivoBaja.Fallecimiento, null), null);
        return p;
    }

    [Fact]
    public void Lista_solo_bajas_con_mas_de_cinco_anios_FR_1020()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var antigua = PacienteEnBaja(ctx, "Antigua", "12345678Z", 6);
        PacienteEnBaja(ctx, "Reciente", "87654321X", 2);

        var purgables = ctx.Servicio.ListarPurgables(DateOnly.FromDateTime(DateTime.Today));

        Assert.Single(purgables);
        Assert.Equal(antigua.Id, purgables[0].Id);
    }

    [Fact]
    public void Purga_exige_la_contrasena_del_administrador_CA_1005()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var p = PacienteEnBaja(ctx, "Antigua", "12345678Z", 6);

        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.Purgar(p.Id, ctx.AdministradorId, "incorrecta", DateOnly.FromDateTime(DateTime.Today)));
        Assert.NotNull(ctx.Pacientes.ObtenerPorId(p.Id));
    }

    [Fact]
    public void Purga_borra_en_cascada_y_la_traza_de_auditoria_sobrevive_CA_1006()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var p = PacienteEnBaja(ctx, "Antigua", "12345678Z", 6);
        new RepositorioContactos(ctx.Conexion).Crear(new Contacto { PacienteId = p.Id, Tipo = TipoContacto.Familiar, Nombre = "X", Apellidos = "Y" });
        var envaseId = ctx.Envases.Crear(new Envase { PacienteId = p.Id, MedicamentoId = ctx.MedicamentoId, Serie = "S", Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 10, UnidadesRestantes = 0, Estado = EstadoEnvase.Agotado });

        var resultado = ctx.Servicio.Purgar(p.Id, ctx.AdministradorId, PasswordAdmin, DateOnly.FromDateTime(DateTime.Today));

        Assert.Equal(p.NumFicha, resultado.NumFicha);
        Assert.True(resultado.FilasEliminadas >= 3);   // paciente + contacto + envase
        Assert.Null(ctx.Pacientes.ObtenerPorId(p.Id));
        Assert.Equal(0, ctx.Conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Contacto WHERE paciente_id = @p", new { p = p.Id }));
        Assert.Equal(0, ctx.Conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Envase WHERE id = @e", new { e = envaseId }));
        var traza = ctx.Conexion.QuerySingle<string>("SELECT detalle FROM Auditoria WHERE accion = 'PURGAR_PACIENTE'");
        Assert.Contains($"num_ficha={p.NumFicha}", traza);
        Assert.Contains("Antigua Purgable", traza);
    }

    [Fact]
    public void Purga_bloqueada_si_quedan_envases_en_custodia_CA_1007()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var p = PacienteEnBaja(ctx, "Antigua", "12345678Z", 6);
        ctx.Envases.Crear(new Envase { PacienteId = p.Id, MedicamentoId = ctx.MedicamentoId, Serie = "S", Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 10, UnidadesRestantes = 10 });

        var ex = Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.Purgar(p.Id, ctx.AdministradorId, PasswordAdmin, DateOnly.FromDateTime(DateTime.Today)));

        Assert.Contains("custodia", ex.Message);
        Assert.NotNull(ctx.Pacientes.ObtenerPorId(p.Id));
    }

    [Fact]
    public void No_se_purga_una_baja_reciente_ni_un_paciente_activo()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var reciente = PacienteEnBaja(ctx, "Reciente", "12345678Z", 1);
        var activo = ctx.Pacientes.Crear(new DatosAltaPaciente("Viva", "Activa", null, "87654321X", null, null, null, null, null, null, null, null, null, null, null, null, null, false, null, null, null), null);

        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.Purgar(reciente.Id, ctx.AdministradorId, PasswordAdmin, DateOnly.FromDateTime(DateTime.Today)));
        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.Purgar(activo.Id, ctx.AdministradorId, PasswordAdmin, DateOnly.FromDateTime(DateTime.Today)));
    }
}
