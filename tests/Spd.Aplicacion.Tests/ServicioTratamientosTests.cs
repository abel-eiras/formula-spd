using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioTratamientosTests
{
    private static (ServicioTratamientos Servicio, SqliteConnection Conexion, int PacienteId, int MedicamentoId)
        Crear(int? medicoDeCabeceraId = null)
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var (pacienteId, medicamentoId) = InfraestructuraTratamientosFundamentosTests.CrearPacienteYMedicamento(conexion);

        if (medicoDeCabeceraId is not null)
        {
            var repositorioPacientes = new RepositorioPacientes(conexion);
            var paciente = repositorioPacientes.ObtenerPorId(pacienteId)!;
            paciente.MedicoId = medicoDeCabeceraId;
            repositorioPacientes.Actualizar(paciente);
        }

        var auditoria = new RegistradorAuditoria(conexion);
        var servicio = new ServicioTratamientos(
            new RepositorioTratamientos(conexion), new RepositorioPacientes(conexion), auditoria);
        return (servicio, conexion, pacienteId, medicamentoId);
    }

    private static DatosAltaTratamiento DatosDePrueba(int medicamentoId, int? medicoId = null, DateOnly? fechaInicio = null)
        => new(
            medicamentoId, EnSpd: true, ProblemaSalud: "Hipertensión", MedicoId: medicoId,
            PautaD: FraccionDosis.Uno, PautaA: FraccionDosis.Cero, PautaC: FraccionDosis.Cero, PautaN: FraccionDosis.Cero,
            PautaTexto: null, DiasSemana: "1111111", Via: "Oral", Momento: "Con comida",
            FechaInicio: fechaInicio ?? new DateOnly(2026, 1, 1), Tipo: TipoTratamiento.Cronico);

    [Fact]
    public void Crear_prerrellena_el_medico_de_cabecera_si_no_se_indica_otro_CA_400()
    {
        var (servicio, conexion, pacienteId, medicamentoId) = Crear(medicoDeCabeceraId: null);
        var repositorioMedicos = new RepositorioMedicos(conexion);
        var medico = new Medico { Nombre = "Juan", Apellidos = "Fernández Souto", BusquedaNormalizada = "juan fernandez souto" };
        medico.Id = repositorioMedicos.Crear(medico);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var paciente = repositorioPacientes.ObtenerPorId(pacienteId)!;
        paciente.MedicoId = medico.Id;
        repositorioPacientes.Actualizar(paciente);

        var tratamiento = servicio.Crear(pacienteId, DatosDePrueba(medicamentoId, medicoId: null), usuarioQueEjecutaId: 1);

        Assert.Equal(medico.Id, tratamiento.MedicoId);
    }

    [Fact]
    public void Crear_respeta_un_medico_prescriptor_distinto_del_de_cabecera()
    {
        var (servicio, conexion, pacienteId, medicamentoId) = Crear(medicoDeCabeceraId: null);
        var repositorioMedicos = new RepositorioMedicos(conexion);
        var otroMedico = new Medico { Nombre = "Marta", Apellidos = "Lago", BusquedaNormalizada = "marta lago" };
        otroMedico.Id = repositorioMedicos.Crear(otroMedico);

        var tratamiento = servicio.Crear(pacienteId, DatosDePrueba(medicamentoId, medicoId: otroMedico.Id), usuarioQueEjecutaId: 1);

        Assert.Equal(otroMedico.Id, tratamiento.MedicoId);
    }

    [Fact]
    public void Crear_con_en_spd_false_acepta_pauta_texto_libre_FR_401()
    {
        var (servicio, _, pacienteId, medicamentoId) = Crear();
        var datos = new DatosAltaTratamiento(
            medicamentoId, EnSpd: false, ProblemaSalud: null, MedicoId: null,
            PautaD: null, PautaA: null, PautaC: null, PautaN: null,
            PautaTexto: "Cada 12 horas si dolor", DiasSemana: "1111111", Via: null, Momento: null,
            FechaInicio: new DateOnly(2026, 1, 1), Tipo: TipoTratamiento.Esporadico);

        var tratamiento = servicio.Crear(pacienteId, datos, usuarioQueEjecutaId: 1);

        Assert.False(tratamiento.EnSpd);
        Assert.Equal("Cada 12 horas si dolor", tratamiento.PautaTexto);
        Assert.Null(tratamiento.PautaD);
    }

    [Fact]
    public void ListarVigentesDePaciente_solo_devuelve_sin_fecha_fin_y_no_finalizados()
    {
        var (servicio, _, pacienteId, medicamentoId) = Crear();
        var vigente = servicio.Crear(pacienteId, DatosDePrueba(medicamentoId), usuarioQueEjecutaId: 1);
        var otro = servicio.Crear(pacienteId, DatosDePrueba(medicamentoId, fechaInicio: new DateOnly(2025, 1, 1)), usuarioQueEjecutaId: 1);
        servicio.CambiarEstado(otro.Id, EstadoTratamiento.Finalizado, usuarioQueEjecutaId: 1);

        var vigentes = servicio.ListarVigentesDePaciente(pacienteId);

        Assert.Single(vigentes);
        Assert.Equal(vigente.Id, vigentes[0].Id);
    }

    [Fact]
    public void Crear_registra_en_auditoria()
    {
        var (servicio, conexion, pacienteId, medicamentoId) = Crear();

        servicio.Crear(pacienteId, DatosDePrueba(medicamentoId), usuarioQueEjecutaId: 1);

        var total = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Auditoria WHERE accion = 'ALTA_TRATAMIENTO'");
        Assert.Equal(1, total);
    }

    // --- User Story 2 ---

    [Fact]
    public void CambiarPauta_cierra_la_fila_original_y_abre_una_nueva_CA_401()
    {
        var (servicio, conexion, pacienteId, medicamentoId) = Crear();
        var original = servicio.Crear(pacienteId, DatosDePrueba(medicamentoId), usuarioQueEjecutaId: 1);
        var datosNuevos = DatosDePrueba(medicamentoId, fechaInicio: new DateOnly(2026, 2, 1)) with { PautaA = FraccionDosis.Uno };

        var nuevo = servicio.CambiarPauta(original.Id, datosNuevos, usuarioQueEjecutaId: 1);

        Assert.NotEqual(original.Id, nuevo.Id);
        Assert.Equal(FraccionDosis.Uno, nuevo.PautaA);
        Assert.Equal(EstadoTratamiento.Activo, nuevo.Estado);
        Assert.Equal(original.FechaPrescripcionInicial, nuevo.FechaPrescripcionInicial);

        var anteriorActualizado = new RepositorioTratamientos(conexion).ObtenerPorId(original.Id)!;
        Assert.Equal(EstadoTratamiento.Finalizado, anteriorActualizado.Estado);
        Assert.Equal(new DateOnly(2026, 2, 1), anteriorActualizado.FechaFin);
    }

    [Fact]
    public void ListarHistorialDeMedicamento_devuelve_todas_las_versiones_CA_402()
    {
        var (servicio, _, pacienteId, medicamentoId) = Crear();
        var v1 = servicio.Crear(pacienteId, DatosDePrueba(medicamentoId, fechaInicio: new DateOnly(2026, 1, 1)), usuarioQueEjecutaId: 1);
        var v2 = servicio.CambiarPauta(v1.Id, DatosDePrueba(medicamentoId, fechaInicio: new DateOnly(2026, 2, 1)), usuarioQueEjecutaId: 1);
        var v3 = servicio.CambiarPauta(v2.Id, DatosDePrueba(medicamentoId, fechaInicio: new DateOnly(2026, 3, 1)), usuarioQueEjecutaId: 1);

        var historial = servicio.ListarHistorialDeMedicamento(pacienteId, medicamentoId);

        Assert.Equal(3, historial.Count);
        Assert.Contains(historial, t => t.Id == v1.Id);
        Assert.Contains(historial, t => t.Id == v2.Id);
        Assert.Contains(historial, t => t.Id == v3.Id);
    }

    [Fact]
    public void ActualizarCamposNoClinicos_no_cierra_la_fila_FR_411()
    {
        var (servicio, _, pacienteId, medicamentoId) = Crear();
        var tratamiento = servicio.Crear(pacienteId, DatosDePrueba(medicamentoId), usuarioQueEjecutaId: 1);

        servicio.ActualizarCamposNoClinicos(
            tratamiento.Id, new DatosNoClinicos("Buen cumplimiento", null, null, AjusteUnidadesManual: 5), usuarioQueEjecutaId: 1);

        var vigentes = servicio.ListarVigentesDePaciente(pacienteId);
        Assert.Single(vigentes);
        Assert.Equal(tratamiento.Id, vigentes[0].Id);
        Assert.Equal(5, vigentes[0].AjusteUnidadesManual);
        Assert.Equal("Buen cumplimiento", vigentes[0].ConocimientoCumplimiento);
    }

    [Fact]
    public void ActualizarCamposNoClinicos_puede_limpiar_el_ajuste_manual_CA_405()
    {
        var (servicio, _, pacienteId, medicamentoId) = Crear();
        var tratamiento = servicio.Crear(pacienteId, DatosDePrueba(medicamentoId), usuarioQueEjecutaId: 1);
        servicio.ActualizarCamposNoClinicos(tratamiento.Id, new DatosNoClinicos(null, null, null, 5), usuarioQueEjecutaId: 1);

        servicio.ActualizarCamposNoClinicos(tratamiento.Id, new DatosNoClinicos(null, null, null, null), usuarioQueEjecutaId: 1);

        var vigentes = servicio.ListarVigentesDePaciente(pacienteId);
        Assert.Null(vigentes[0].AjusteUnidadesManual);
    }

    [Fact]
    public void CambiarEstado_de_suspendido_a_activo_no_cierra_ni_abre_fila()
    {
        var (servicio, _, pacienteId, medicamentoId) = Crear();
        var tratamiento = servicio.Crear(pacienteId, DatosDePrueba(medicamentoId), usuarioQueEjecutaId: 1);
        servicio.CambiarEstado(tratamiento.Id, EstadoTratamiento.Suspendido, usuarioQueEjecutaId: 1);

        servicio.CambiarEstado(tratamiento.Id, EstadoTratamiento.Activo, usuarioQueEjecutaId: 1);

        var vigentes = servicio.ListarVigentesDePaciente(pacienteId);
        Assert.Single(vigentes);
        Assert.Equal(tratamiento.Id, vigentes[0].Id);
    }

    [Fact]
    public void CambiarPauta_y_ActualizarCamposNoClinicos_y_CambiarEstado_registran_en_auditoria()
    {
        var (servicio, conexion, pacienteId, medicamentoId) = Crear();
        var tratamiento = servicio.Crear(pacienteId, DatosDePrueba(medicamentoId), usuarioQueEjecutaId: 1);

        servicio.CambiarPauta(tratamiento.Id, DatosDePrueba(medicamentoId, fechaInicio: new DateOnly(2026, 2, 1)), usuarioQueEjecutaId: 1);
        servicio.ActualizarCamposNoClinicos(tratamiento.Id, new DatosNoClinicos("x", null, null, null), usuarioQueEjecutaId: 1);
        servicio.CambiarEstado(tratamiento.Id, EstadoTratamiento.Suspendido, usuarioQueEjecutaId: 1);

        var acciones = conexion.Query<string>("SELECT accion FROM Auditoria ORDER BY id").ToList();
        Assert.Contains("CAMBIAR_PAUTA_TRATAMIENTO", acciones);
        Assert.Contains("ACTUALIZAR_TRATAMIENTO", acciones);
        Assert.Contains("CAMBIAR_ESTADO_TRATAMIENTO", acciones);
    }
}
