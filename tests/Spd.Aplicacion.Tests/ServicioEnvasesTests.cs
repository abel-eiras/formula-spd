using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioEnvasesTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    private static (ServicioEnvases Servicio, RegistradorAuditoria Auditoria, int PacienteId, int MedicamentoId, SqliteConnection Conexion)
        Crear(bool enSpd = true)
    {
        var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, medicamentoId) = InfraestructuraTratamientosFundamentosTests.CrearPacienteYMedicamento(conexion);
        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        repositorioTratamientos.Crear(new Tratamiento
        {
            PacienteId = pacienteId,
            MedicamentoId = medicamentoId,
            EnSpd = enSpd,
            PautaTexto = enSpd ? null : "A demanda",
            PautaD = enSpd ? FraccionDosis.Uno : null,
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });

        var repositorioPacientes = new RepositorioPacientes(conexion);
        var auditoria = new RegistradorAuditoria(conexion);
        var servicio = new ServicioEnvases(new RepositorioEnvases(conexion), repositorioTratamientos, repositorioPacientes, auditoria);
        return (servicio, auditoria, pacienteId, medicamentoId, conexion);
    }

    [Fact]
    public void RegistrarEnvase_exige_tratamiento_activo_en_spd_FR_515()
    {
        using var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, medicamentoId) = InfraestructuraTratamientosFundamentosTests.CrearPacienteYMedicamento(conexion);
        var servicio = new ServicioEnvases(
            new RepositorioEnvases(conexion), new RepositorioTratamientos(conexion), new RepositorioPacientes(conexion), new RegistradorAuditoria(conexion));

        var datos = new DatosAltaEnvase(pacienteId, medicamentoId, "S1", "L1", new DateOnly(2027, 1, 1), 28, OrigenEnvase.Manual);

        Assert.Throws<ErrorValidacionException>(() => servicio.RegistrarEnvase(datos, null));
    }

    [Fact]
    public void RegistrarEnvase_bloquea_serie_duplicada_en_otro_paciente_CA_504()
    {
        var (servicio, _, pacienteA, medicamentoId, conexion) = Crear();
        using var c = conexion;
        servicio.RegistrarEnvase(new DatosAltaEnvase(pacienteA, medicamentoId, "SERIE-X", "L1", new DateOnly(2027, 1, 1), 28, OrigenEnvase.Manual), null);

        var repositorioPacientes = new RepositorioPacientes(conexion);
        var pacienteB = new ServicioPacientes(repositorioPacientes, new RepositorioFarmacia(conexion), new RegistradorAuditoria(conexion))
            .Crear(new DatosAltaPaciente("Luis", "Gómez", null, "87654321X", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, null), null);
        new RepositorioTratamientos(conexion).Crear(new Tratamiento
        {
            PacienteId = pacienteB.Id,
            MedicamentoId = medicamentoId,
            EnSpd = true,
            PautaD = FraccionDosis.Uno,
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });

        var excepcion = Assert.Throws<ErrorValidacionException>(() =>
            servicio.RegistrarEnvase(new DatosAltaEnvase(pacienteB.Id, medicamentoId, "SERIE-X", "L2", new DateOnly(2027, 1, 1), 28, OrigenEnvase.Manual), null));
        Assert.Contains("SERIE-X", excepcion.Message);
    }

    [Fact]
    public void RegistrarEnvase_con_caducidad_pasada_no_bloquea_FR_514()
    {
        var (servicio, _, pacienteId, medicamentoId, conexion) = Crear();
        using var c = conexion;

        var envase = servicio.RegistrarEnvase(
            new DatosAltaEnvase(pacienteId, medicamentoId, "S1", "L1", new DateOnly(2020, 1, 1), 28, OrigenEnvase.Manual), null);

        Assert.Equal(EstadoEnvase.EnCustodia, envase.Estado);
        Assert.True(envase.Caducidad < DateOnly.FromDateTime(DateTime.Today));
    }

    [Fact]
    public void RegistrarEnvase_registra_en_auditoria_Art_VII_6()
    {
        var (servicio, auditoria, pacienteId, medicamentoId, conexion) = Crear();
        using var c = conexion;

        servicio.RegistrarEnvase(new DatosAltaEnvase(pacienteId, medicamentoId, "S1", "L1", new DateOnly(2027, 1, 1), 28, OrigenEnvase.Manual), 7);

        var registros = conexion.Query<(string Accion, int? UsuarioId)>("SELECT accion, usuario_id FROM Auditoria WHERE entidad = 'Envase'");
        Assert.Contains(registros, r => r.Accion == "ALTA_ENVASE" && r.UsuarioId == 7);
    }

    [Fact]
    public void ListarEnCustodia_y_ListarHistorico_separan_por_estado_FR_517()
    {
        var (servicio, _, pacienteId, medicamentoId, conexion) = Crear();
        using var c = conexion;
        var enCustodia = servicio.RegistrarEnvase(new DatosAltaEnvase(pacienteId, medicamentoId, "S1", "L1", new DateOnly(2027, 1, 1), 28, OrigenEnvase.Manual), null);
        var enResiduo = servicio.RegistrarEnvase(new DatosAltaEnvase(pacienteId, medicamentoId, "S2", "L1", new DateOnly(2027, 1, 1), 10, OrigenEnvase.Manual), null);
        servicio.DarSalidaSigre(enResiduo.Id, MotivoSalidaEnvase.Caducado, null, null);

        var custodia = servicio.ListarEnCustodiaDePaciente(pacienteId);
        var historico = servicio.ListarHistoricoDePaciente(pacienteId);

        Assert.Single(custodia, e => e.Id == enCustodia.Id);
        Assert.Single(historico, e => e.Id == enResiduo.Id);
    }

    [Fact]
    public void ProponerSalidaSigrePorFinDeTratamiento_no_ejecuta_nada_FR_541()
    {
        var (servicio, _, pacienteId, medicamentoId, conexion) = Crear();
        using var c = conexion;
        var envase = servicio.RegistrarEnvase(new DatosAltaEnvase(pacienteId, medicamentoId, "S1", "L1", new DateOnly(2027, 1, 1), 12, OrigenEnvase.Manual), null);

        var propuestos = servicio.ProponerSalidaSigrePorFinDeTratamiento(pacienteId, medicamentoId);

        Assert.Single(propuestos, e => e.Id == envase.Id);
        Assert.Equal(EstadoEnvase.EnCustodia, servicio.ListarEnCustodiaDePaciente(pacienteId).Single().Estado);
    }

    [Fact]
    public void DarSalidaSigre_pasa_a_residuo_con_motivo_y_unidades_desechadas_CA_508()
    {
        var (servicio, _, pacienteId, medicamentoId, conexion) = Crear();
        using var c = conexion;
        var envase = servicio.RegistrarEnvase(new DatosAltaEnvase(pacienteId, medicamentoId, "S1", "L1", new DateOnly(2027, 1, 1), 12, OrigenEnvase.Manual), null);

        var resultado = servicio.DarSalidaSigre(envase.Id, MotivoSalidaEnvase.CeseTratamiento, null, null);

        Assert.Equal(EstadoEnvase.ResiduoSigre, resultado.Estado);
        Assert.Equal(MotivoSalidaEnvase.CeseTratamiento, resultado.MotivoSalida);
        Assert.Equal(12, resultado.UnidadesRestantes);
    }

    [Fact]
    public void DarSalidaSigreMasiva_cubre_todos_los_envases_en_custodia_del_paciente()
    {
        var (servicio, _, pacienteId, medicamentoId, conexion) = Crear();
        using var c = conexion;
        servicio.RegistrarEnvase(new DatosAltaEnvase(pacienteId, medicamentoId, "S1", "L1", new DateOnly(2027, 1, 1), 12, OrigenEnvase.Manual), null);
        servicio.RegistrarEnvase(new DatosAltaEnvase(pacienteId, medicamentoId, "S2", "L1", new DateOnly(2027, 1, 1), 5, OrigenEnvase.Manual), null);

        var resultado = servicio.DarSalidaSigreMasiva(pacienteId, MotivoSalidaEnvase.BajaPaciente, null);

        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, e => Assert.Equal(EstadoEnvase.ResiduoSigre, e.Estado));
        Assert.Empty(servicio.ListarEnCustodiaDePaciente(pacienteId));
    }

    [Fact]
    public void Ninguna_accion_devuelve_un_envase_al_stock_ni_lo_reasigna_CA_509()
    {
        var interfaz = typeof(IServicioEnvases);
        Assert.DoesNotContain(interfaz.GetMethods(), m => m.Name.Contains("Stock") || m.Name.Contains("Reasignar") || m.Name.Contains("Devolver"));
    }

    [Fact]
    public void RegistrarEntregaFueraBlister_exige_en_spd_falso_y_no_exige_serie_CA_510()
    {
        var (servicio, _, pacienteId, medicamentoId, conexion) = Crear(enSpd: false);
        using var c = conexion;

        var envase = servicio.RegistrarEntregaFueraBlister(new DatosEntregaFueraBlister(pacienteId, medicamentoId, null, null, "El propio paciente"), null);

        Assert.Equal(EstadoEnvase.EntregadoPaciente, envase.Estado);
        Assert.Null(envase.Serie);
        Assert.Equal("El propio paciente", envase.EntregadoA);
    }

    [Fact]
    public void RegistrarEntregaFueraBlister_rechaza_medicamento_en_spd_verdadero()
    {
        var (servicio, _, pacienteId, medicamentoId, conexion) = Crear(enSpd: true);
        using var c = conexion;

        Assert.Throws<ErrorValidacionException>(() =>
            servicio.RegistrarEntregaFueraBlister(new DatosEntregaFueraBlister(pacienteId, medicamentoId, null, null, "Alguien"), null));
    }

    [Fact]
    public void Entrega_fuera_de_blister_no_cuenta_en_custodia()
    {
        var (servicio, _, pacienteId, medicamentoId, conexion) = Crear(enSpd: false);
        using var c = conexion;

        servicio.RegistrarEntregaFueraBlister(new DatosEntregaFueraBlister(pacienteId, medicamentoId, null, null, "Paciente"), null);

        Assert.Empty(servicio.ListarEnCustodiaDePaciente(pacienteId));
    }
}
