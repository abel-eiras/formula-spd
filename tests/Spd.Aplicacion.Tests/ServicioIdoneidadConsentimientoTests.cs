using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioIdoneidadConsentimientoTests
{
    private sealed record Contexto(
        SqliteConnection Conexion, ServicioIdoneidadConsentimiento Servicio, RepositorioPacientes Pacientes,
        ComprobadorIdoneidadYConsentimientoReal Comprobador, int PacienteId, int FarmaceuticoId);

    private static Contexto Crear(bool conRepresentante = false)
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular", Cif = "B00000000",
            Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra", Telefono = "986000000"
        });
        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("María", "López Vidal", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, null),
            usuarioQueEjecutaId: null);

        var repositorioContactos = new RepositorioContactos(conexion);
        if (conRepresentante)
            repositorioContactos.Crear(new Contacto
            {
                PacienteId = paciente.Id, Tipo = TipoContacto.RepresentanteLegal, Nombre = "Ana", Apellidos = "Vidal", Dni = "87654321X"
            });

        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var farmaceutico = new Usuario { Nombre = "Elena", Apellidos = "Ruiz", Login = "elena", HashPassword = "x", Rol = Rol.Elaborador };
        farmaceutico.Id = repositorioUsuarios.Crear(farmaceutico);

        var evaluaciones = new RepositorioEvaluacionesIdoneidad(conexion);
        var consentimientos = new RepositorioConsentimientos(conexion);
        var servicio = new ServicioIdoneidadConsentimiento(evaluaciones, consentimientos, repositorioContactos, repositorioPacientes, servicioPacientes, auditoria);
        return new Contexto(conexion, servicio, repositorioPacientes, new ComprobadorIdoneidadYConsentimientoReal(evaluaciones, consentimientos), paciente.Id, farmaceutico.Id);
    }

    private static DatosEvaluacionIdoneidad Apto(string? observaciones = null)
        => new(true, false, false, true, false, false, false, true, true, observaciones, ResultadoIdoneidad.Apto, null);

    private static DatosEvaluacionIdoneidad NoApto(string? observaciones)
        => new(false, false, false, false, false, false, false, false, false, observaciones, ResultadoIdoneidad.NoApto, null);

    private static EstadoPaciente Estado(Contexto ctx) => ctx.Pacientes.ObtenerPorId(ctx.PacienteId)!.Estado;

    [Fact]
    public void Evaluacion_APTO_y_consentimiento_firmado_activan_al_paciente_CA_200()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var evaluacion = ctx.Servicio.RegistrarEvaluacion(ctx.PacienteId, Apto(), ctx.FarmaceuticoId);
        Assert.False(evaluacion.PacienteActivado);                       // todavía falta el consentimiento
        Assert.Equal(EstadoPaciente.Evaluacion, Estado(ctx));

        var consentimiento = ctx.Servicio.CrearConsentimiento(ctx.PacienteId, TipoConsentimiento.Paciente, null, ctx.FarmaceuticoId);
        Assert.False(ctx.Comprobador.Aprobado(ctx.PacienteId));           // creado pero sin firmar

        var firma = ctx.Servicio.RegistrarFirma(consentimiento.Id, new DateOnly(2026, 9, 1), ctx.FarmaceuticoId);
        Assert.True(firma.PacienteActivado);
        Assert.Equal(EstadoPaciente.Activo, Estado(ctx));
        Assert.True(ctx.Comprobador.Aprobado(ctx.PacienteId));

        var acciones = ctx.Conexion.Query<string>("SELECT accion FROM Auditoria WHERE entidad = 'Paciente'").ToList();
        Assert.Contains("EVALUAR_IDONEIDAD", acciones);
        Assert.Contains("CREAR_CONSENTIMIENTO", acciones);
        Assert.Contains("FIRMAR_CONSENTIMIENTO", acciones);
    }

    [Fact]
    public void Ultima_evaluacion_NO_APTO_bloquea_la_preparacion_CA_201()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var consentimiento = ctx.Servicio.CrearConsentimiento(ctx.PacienteId, TipoConsentimiento.Paciente, null, null);
        ctx.Servicio.RegistrarFirma(consentimiento.Id, new DateOnly(2026, 9, 1), null);
        ctx.Servicio.RegistrarEvaluacion(ctx.PacienteId, Apto(), null);
        Assert.True(ctx.Comprobador.Aprobado(ctx.PacienteId));

        var resultado = ctx.Servicio.RegistrarEvaluacion(ctx.PacienteId, NoApto("Ya no puede manejar el blíster"), null);

        Assert.False(ctx.Comprobador.Aprobado(ctx.PacienteId));           // Spec 006 CrearSesion consulta esto (FR-602)
        Assert.True(resultado.SugerirSuspension);                         // caso límite: NO_APTO sobre un ACTIVO
        Assert.Equal(EstadoPaciente.Activo, Estado(ctx));                 // nunca se suspende solo
    }

    [Fact]
    public void Consentimiento_por_representante_sin_contacto_elegible_pide_crearlo_CA_202()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var ex = Assert.Throws<ErrorValidacionException>(() =>
            ctx.Servicio.CrearConsentimiento(ctx.PacienteId, TipoConsentimiento.Representante, null, null));
        Assert.Contains("cree el contacto", ex.Message);

        var contacto = ctx.Servicio.CrearRepresentante(
            ctx.PacienteId, new DatosContactoRepresentante(TipoContacto.PersonaAutorizada, "Ana", "Vidal", "87654321x", null, null), null);
        Assert.Equal("87654321X", contacto.Dni);
        var consentimiento = ctx.Servicio.CrearConsentimiento(ctx.PacienteId, TipoConsentimiento.Representante, contacto.Id, null);
        Assert.Equal(contacto.Id, consentimiento.ContactoId);
    }

    [Fact]
    public void CrearRepresentante_exige_dni_y_tipo_elegible_FR_211()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.CrearRepresentante(
            ctx.PacienteId, new DatosContactoRepresentante(TipoContacto.RepresentanteLegal, "Ana", "Vidal", "", null, null), null));
        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.CrearRepresentante(
            ctx.PacienteId, new DatosContactoRepresentante(TipoContacto.Cuidador, "Ana", "Vidal", "87654321X", null, null), null));
    }

    [Fact]
    public void Revocacion_conserva_ambas_fechas_y_sugiere_suspension_CA_203()
    {
        var ctx = Crear(conRepresentante: true);
        using var c = ctx.Conexion;
        ctx.Servicio.RegistrarEvaluacion(ctx.PacienteId, Apto(), null);
        var representante = ctx.Servicio.Consultar(ctx.PacienteId).RepresentantesElegibles.Single();
        var consentimiento = ctx.Servicio.CrearConsentimiento(ctx.PacienteId, TipoConsentimiento.Representante, representante.Id, null);
        ctx.Servicio.RegistrarFirma(consentimiento.Id, new DateOnly(2026, 1, 1), null);
        Assert.Equal(EstadoPaciente.Activo, Estado(ctx));

        var revocacion = ctx.Servicio.Revocar(consentimiento.Id, new DateOnly(2026, 6, 1), "Cambio de tutela", null);

        Assert.Equal(new DateOnly(2026, 1, 1), revocacion.Consentimiento.FechaFirma);
        Assert.Equal(new DateOnly(2026, 6, 1), revocacion.Consentimiento.FechaRevocacion);
        Assert.True(revocacion.SugerirSuspension);
        Assert.False(ctx.Comprobador.Aprobado(ctx.PacienteId));
        Assert.Null(ctx.Servicio.Consultar(ctx.PacienteId).ConsentimientoVigente);
        Assert.Single(ctx.Servicio.Consultar(ctx.PacienteId).Consentimientos);    // sigue existiendo (Art. III)

        ctx.Servicio.Suspender(ctx.PacienteId, null);
        Assert.Equal(EstadoPaciente.Suspendido, Estado(ctx));
    }

    [Fact]
    public void Revocar_exige_motivo_y_consentimiento_firmado()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var sinFirmar = ctx.Servicio.CrearConsentimiento(ctx.PacienteId, TipoConsentimiento.Paciente, null, null);
        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.Revocar(sinFirmar.Id, new DateOnly(2026, 6, 1), "x", null));

        ctx.Servicio.RegistrarFirma(sinFirmar.Id, new DateOnly(2026, 1, 1), null);
        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.Revocar(sinFirmar.Id, new DateOnly(2026, 6, 1), "  ", null));
    }

    [Fact]
    public void Varias_evaluaciones_la_ultima_manda_y_todas_siguen_consultables_CA_204()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        ctx.Servicio.RegistrarEvaluacion(ctx.PacienteId, NoApto("Sin cuidador por ahora"), null);
        ctx.Servicio.RegistrarEvaluacion(ctx.PacienteId, Apto(), null);

        var estado = ctx.Servicio.Consultar(ctx.PacienteId);
        Assert.Equal(ResultadoIdoneidad.Apto, estado.EvaluacionVigente!.Resultado);
        Assert.Equal(2, estado.Evaluaciones.Count);
        Assert.Equal(ResultadoIdoneidad.NoApto, estado.Evaluaciones[1].Resultado);
    }

    [Fact]
    public void NO_APTO_sin_observaciones_se_rechaza_CA_205()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var ex = Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.RegistrarEvaluacion(ctx.PacienteId, NoApto(null), null));
        Assert.Contains("FR-204", ex.Message);
        Assert.Empty(ctx.Servicio.Consultar(ctx.PacienteId).Evaluaciones);
    }

    [Fact]
    public void APTO_que_contradice_la_propuesta_exige_observaciones_FR_204()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var contradictorio = new DatosEvaluacionIdoneidad(true, false, false, false, false, false, false, false, true, null, ResultadoIdoneidad.Apto, null);

        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.RegistrarEvaluacion(ctx.PacienteId, contradictorio, null));
        ctx.Servicio.RegistrarEvaluacion(ctx.PacienteId, contradictorio with { Observaciones = "El cuidador se encarga de todo" }, null);
        Assert.Single(ctx.Servicio.Consultar(ctx.PacienteId).Evaluaciones);
    }

    [Fact]
    public void Nuevo_consentimiento_firmado_sustituye_al_anterior_como_vigente_FR_215()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var primero = ctx.Servicio.CrearConsentimiento(ctx.PacienteId, TipoConsentimiento.Paciente, null, null);
        ctx.Servicio.RegistrarFirma(primero.Id, new DateOnly(2025, 9, 1), null);
        var segundo = ctx.Servicio.CrearConsentimiento(ctx.PacienteId, TipoConsentimiento.Paciente, null, null);
        ctx.Servicio.RegistrarFirma(segundo.Id, new DateOnly(2026, 9, 1), null);

        var estado = ctx.Servicio.Consultar(ctx.PacienteId);
        Assert.Equal(segundo.Id, estado.ConsentimientoVigente!.Id);
        Assert.Equal(2, estado.Consentimientos.Count);
        Assert.Null(estado.Consentimientos.Single(x => x.Id == primero.Id).FechaRevocacion);   // no se revoca, solo deja de ser el vigente
    }

    [Fact]
    public void Firma_futura_o_doble_se_rechaza_FR_212()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var consentimiento = ctx.Servicio.CrearConsentimiento(ctx.PacienteId, TipoConsentimiento.Paciente, null, null);

        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.RegistrarFirma(consentimiento.Id, DateOnly.FromDateTime(DateTime.Today).AddDays(1), null));
        ctx.Servicio.RegistrarFirma(consentimiento.Id, DateOnly.FromDateTime(DateTime.Today), null);
        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.RegistrarFirma(consentimiento.Id, DateOnly.FromDateTime(DateTime.Today), null));
    }
}
