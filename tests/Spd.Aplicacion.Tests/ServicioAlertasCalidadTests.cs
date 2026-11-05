using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Spec 018 CA-1801..1805: trazabilidad inversa lote → caja → blíster → paciente.</summary>
public sealed class ServicioAlertasCalidadTests
{
    private sealed record Contexto(ServicioAlertasCalidad Servicio, ServicioAvisosInicio Avisos, int EnvaseLoteId, int SpdEntregadoId, int SpdPendienteId);

    private static Contexto Montar(SqliteConnection c)
    {
        c.Open();
        new AplicadorMigraciones(c).Aplicar();
        var auditoria = new RegistradorAuditoria(c);
        var repositorioFarmacia = new RepositorioFarmacia(c);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9", PrefijoNumSpd = "F-"
        });
        var repositorioPacientes = new RepositorioPacientes(c);
        var pacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var maria = pacientes.Crear(new DatosAltaPaciente("María", "López", null, "12345678Z", null, null, null, null, null, null,
            null, null, null, null, null, null, null, false, null, null, null), null);
        var jose = pacientes.Crear(new DatosAltaPaciente("José", "Núñez", null, "87654321X", null, null, null, null, null, null,
            null, null, null, null, null, null, null, false, null, null, null), null);

        var repositorioMedicamentos = new RepositorioMedicamentos(c);
        var med = new Medicamento { Cn = "654321", Nombre = "Enalapril 20 mg" };
        med.Id = repositorioMedicamentos.Crear(med);
        var usuario = new Usuario { Nombre = "E", Apellidos = "R", Login = "e", HashPassword = "x", Rol = Rol.Elaborador };
        usuario.Id = new RepositorioUsuarios(c).Crear(usuario);
        var tratamientos = new RepositorioTratamientos(c);
        var envases = new RepositorioEnvases(c);
        var spds = new RepositorioSpd(c);
        var lineas = new RepositorioSpdLineas(c);
        var lineaEnvases = new RepositorioSpdLineaEnvases(c);

        int EnvaseCon(int pacienteId, string serie, string lote)
            => envases.Crear(new Envase
            {
                PacienteId = pacienteId, MedicamentoId = med.Id, Serie = serie, Lote = lote,
                Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 20
            });

        int BlisterCon(int pacienteId, int envaseId, string lote, EstadoSpd estado, int n)
        {
            var tratamientoId = tratamientos.Crear(new Tratamiento
            {
                PacienteId = pacienteId, MedicamentoId = med.Id, EnSpd = true, PautaD = FraccionDosis.Uno,
                FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
            });
            var spdId = spds.Crear(new SPD
            {
                NumRegistro = $"F-00000{n}", CorrelativoNumRegistro = n, PacienteId = pacienteId, SesionId = Guid.NewGuid(),
                ElaboradorId = usuario.Id, ValidezDesde = new DateOnly(2026, 11, 2), ValidezHasta = new DateOnly(2026, 11, 8), Estado = estado
            });
            var lineaId = lineas.Crear(new SpdLinea
            {
                SpdId = spdId, TratamientoId = tratamientoId, MedicamentoId = med.Id, SnapNombre = med.Nombre, SnapCn = med.Cn,
                SnapPautaD = FraccionDosis.Uno, SnapDiasSemana = "1111111"
            });
            lineaEnvases.Crear(new SpdLineaEnvase
            {
                SpdLineaId = lineaId, EnvaseId = envaseId, UnidadesTomadas = 7, SnapSerie = "x", SnapLote = lote,
                SnapCaducidad = new DateOnly(2030, 1, 1), SnapCn = med.Cn
            });
            return spdId;
        }

        var envaseLote = EnvaseCon(maria.Id, "S1", "AB-123");
        EnvaseCon(maria.Id, "S2", "OTRO");
        var envaseJose = EnvaseCon(jose.Id, "S3", "ab123");
        var entregado = BlisterCon(maria.Id, envaseLote, "AB-123", EstadoSpd.Entregado, 1);
        var pendiente = BlisterCon(jose.Id, envaseJose, "ab123", EstadoSpd.Verificado, 2);

        var servicio = new ServicioAlertasCalidad(new RepositorioAlertasCalidad(c), repositorioMedicamentos, auditoria);
        var listado = new ServicioListadoRetirada(repositorioPacientes, new RepositorioContactos(c), tratamientos, repositorioMedicamentos,
            envases, repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);
        var avisos = new ServicioAvisosInicio(listado, spds, repositorioPacientes, new RepositorioRegistrosAmbientales(c), servicio);
        return new Contexto(servicio, avisos, envaseLote, entregado, pendiente);
    }

    [Fact]
    public void Una_alerta_por_lote_CA_1801()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        var creadas = ctx.Servicio.Registrar(new DatosAltaAlertaCalidad("R 7/2026", new DateOnly(2026, 11, 5), "654321.7", "AB-123, CD9; ab123", null), null);
        Assert.Equal(2, creadas.Count);   // «ab123» es el mismo lote que «AB-123»
        Assert.All(creadas, a => Assert.Equal("654321", a.Cn));
    }

    [Fact]
    public void Traza_cajas_y_blisteres_de_todos_los_pacientes_CA_1802()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        var alerta = ctx.Servicio.Registrar(new DatosAltaAlertaCalidad("R 7/2026", new DateOnly(2026, 11, 5), "654321", "AB 123", null), null)[0];

        var traza = ctx.Servicio.Trazar(alerta.Id);

        Assert.Equal(2, traza.EnCustodia.Count);
        Assert.Contains(traza.Envases, e => e.EnvaseId == ctx.EnvaseLoteId);
        Assert.DoesNotContain(traza.Envases, e => e.Serie == "S2");
        Assert.Equal(ctx.SpdEntregadoId, Assert.Single(traza.BlisteresEntregados).SpdId);
        Assert.Equal(ctx.SpdPendienteId, Assert.Single(traza.BlisteresPendientes).SpdId);
    }

    [Fact]
    public void Alerta_activa_aparece_en_inicio_y_al_cerrarla_desaparece_CA_1803()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        var alerta = ctx.Servicio.Registrar(new DatosAltaAlertaCalidad("R 7/2026", new DateOnly(2026, 11, 5), "654321", "AB123", null), null)[0];

        var aviso = Assert.Single(ctx.Avisos.Obtener(new DateOnly(2026, 11, 5)), a => a.Tipo == "Alerta de calidad");
        Assert.Contains("2 caja(s) en custodia", aviso.Texto);
        Assert.Equal(AreaAviso.AlertasCalidad, aviso.Area);

        ctx.Servicio.Cerrar(alerta.Id, "Cajas retiradas y pacientes avisados", new DateOnly(2026, 11, 6), null);

        Assert.DoesNotContain(ctx.Avisos.Obtener(new DateOnly(2026, 11, 6)), a => a.Tipo == "Alerta de calidad");
        var cerrada = Assert.Single(ctx.Servicio.Listar(soloActivas: false));
        Assert.False(cerrada.Alerta.Activa);
        Assert.Equal("Enalapril 20 mg", cerrada.NombreMedicamento);
    }

    [Theory]
    [InlineData("", "654321", "AB1")]
    [InlineData("R1", "1234", "AB1")]
    [InlineData("R1", "654321", " , ;")]
    public void Datos_incompletos_se_rechazan_CA_1804(string referencia, string cn, string lotes)
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        Assert.Throws<ErrorValidacionException>(() =>
            ctx.Servicio.Registrar(new DatosAltaAlertaCalidad(referencia, new DateOnly(2026, 11, 5), cn, lotes, null), null));
    }

    [Fact]
    public void Cerrar_exige_motivo_CA_1805()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        var alerta = ctx.Servicio.Registrar(new DatosAltaAlertaCalidad("R", new DateOnly(2026, 11, 5), "654321", "AB123", null), null)[0];
        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.Cerrar(alerta.Id, " ", new DateOnly(2026, 11, 6), null));
    }
}
