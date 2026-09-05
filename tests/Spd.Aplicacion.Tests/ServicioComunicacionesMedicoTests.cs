using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioComunicacionesMedicoTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    private static (ServicioComunicacionesMedico Servicio, int PacienteId, int MedicoId, SqliteConnection Conexion) Crear()
    {
        var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, medicoId) = InfraestructuraComunicacionesMedicoFundamentosTests.CrearPacienteYMedico(conexion);
        var repositorio = new RepositorioComunicacionesMedico(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        var auditoria = new RegistradorAuditoria(conexion);
        var servicio = new ServicioComunicacionesMedico(repositorio, repositorioPacientes, repositorioTratamientos, auditoria);
        return (servicio, pacienteId, medicoId, conexion);
    }

    [Fact]
    public void Crear_presentacion_prerrellena_el_medico_de_cabecera_CA_800()
    {
        var (servicio, pacienteId, medicoId, conexion) = Crear();
        using var c = conexion;

        var comunicacion = servicio.Crear(
            new DatosAltaComunicacionMedico(pacienteId, MedicoId: null, TipoComunicacionMedico.Presentacion, null, null), null);

        Assert.Equal(medicoId, comunicacion.MedicoId);
    }

    [Fact]
    public void Crear_registra_en_auditoria_Art_VII_6()
    {
        var (servicio, pacienteId, medicoId, conexion) = Crear();
        using var c = conexion;
        var administrador = new ServicioUsuarios(new RepositorioUsuarios(conexion), new HasheadorArgon2id(), new RegistradorAuditoria(conexion))
            .CrearUsuario(new DatosAltaUsuario("Ana", "Admin", "ana.admin", "contraseña-inicial", Rol.Administrador, null, null), administradorQueEjecutaId: null)
            .Usuario;

        servicio.Crear(new DatosAltaComunicacionMedico(pacienteId, medicoId, TipoComunicacionMedico.Presentacion, null, null), administrador.Id);

        var registros = conexion.Query<(string Accion, int? UsuarioId)>("SELECT accion, usuario_id FROM Auditoria WHERE entidad = 'ComunicacionMedico'");
        Assert.Contains(registros, r => r.Accion == "ALTA_COMUNICACION_MEDICO" && r.UsuarioId == administrador.Id);
    }

    [Fact]
    public void Crear_incidencia_sin_propuesta_se_impide_CA_801()
    {
        var (servicio, pacienteId, medicoId, conexion) = Crear();
        using var c = conexion;

        Assert.Throws<ErrorValidacionException>(() =>
            servicio.Crear(new DatosAltaComunicacionMedico(pacienteId, medicoId, TipoComunicacionMedico.Incidencia, "Detectado X", null), null));
    }

    [Fact]
    public void RegistrarRespuesta_anade_respuesta_sin_cambiar_la_fecha_original_CA_802()
    {
        var (servicio, pacienteId, medicoId, conexion) = Crear();
        using var c = conexion;
        var creada = servicio.Crear(
            new DatosAltaComunicacionMedico(pacienteId, medicoId, TipoComunicacionMedico.Incidencia, "Detectado X", "Propuesta Y"), null);
        var fechaOriginal = creada.Fecha;

        servicio.RegistrarRespuesta(creada.Id, "El médico confirma el cambio", new DateOnly(2026, 9, 20), null);

        var actualizada = servicio.ListarDePaciente(pacienteId).Single(x => x.Id == creada.Id);
        Assert.Equal(fechaOriginal, actualizada.Fecha);
        Assert.Equal("El médico confirma el cambio", actualizada.Respuesta);
        Assert.Equal(new DateOnly(2026, 9, 20), actualizada.FechaRespuesta);
    }

    [Fact]
    public void Telefono_no_es_imprimible_presentacion_e_incidencia_si_CA_804()
    {
        var (servicio, pacienteId, medicoId, conexion) = Crear();
        using var c = conexion;

        var telefono = servicio.Crear(new DatosAltaComunicacionMedico(pacienteId, medicoId, TipoComunicacionMedico.Telefono, null, null), null);
        var presentacion = servicio.Crear(new DatosAltaComunicacionMedico(pacienteId, medicoId, TipoComunicacionMedico.Presentacion, null, null), null);
        var incidencia = servicio.Crear(new DatosAltaComunicacionMedico(pacienteId, medicoId, TipoComunicacionMedico.Incidencia, "D", "P"), null);

        Assert.False(telefono.EsImprimible);
        Assert.True(presentacion.EsImprimible);
        Assert.True(incidencia.EsImprimible);
    }

    [Fact]
    public void PrepararDesdeTratamiento_fija_paciente_y_medico_prescriptor_sin_guardar_nada_FR_804()
    {
        var (servicio, pacienteId, medicoId, conexion) = Crear();
        using var c = conexion;
        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g" };
        var medicamentoId = repositorioMedicamentos.Crear(medicamento);
        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        var tratamientoId = repositorioTratamientos.Crear(new Tratamiento
        {
            PacienteId = pacienteId, MedicamentoId = medicamentoId, EnSpd = true, MedicoId = medicoId,
            PautaD = FraccionDosis.Uno, FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });

        var preparado = servicio.PrepararDesdeTratamiento(tratamientoId);

        Assert.Equal(pacienteId, preparado.PacienteId);
        Assert.Equal(medicoId, preparado.MedicoId);
        Assert.Null(preparado.IncidenciasDetectadas);
        Assert.Empty(servicio.ListarDePaciente(pacienteId));
    }

    [Fact]
    public void PrepararDesdeAvisoCambioReferido_fija_paciente_y_medico_con_incidencias_vacio_CA_803()
    {
        var (servicio, pacienteId, medicoId, conexion) = Crear();
        using var c = conexion;

        var preparado = servicio.PrepararDesdeAvisoCambioReferido(pacienteId, medicoId);

        Assert.Equal(pacienteId, preparado.PacienteId);
        Assert.Equal(medicoId, preparado.MedicoId);
        Assert.Null(preparado.IncidenciasDetectadas);
        Assert.Empty(servicio.ListarDePaciente(pacienteId));
    }
}
