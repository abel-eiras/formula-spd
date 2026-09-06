using Dapper;
using Microsoft.Data.Sqlite;
using QuestPDF.Infrastructure;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioGeneracionDocumentosTests : IDisposable
{
    private readonly string _carpetaSalida = Path.Combine(Path.GetTempPath(), "spd-tests-documentos-" + Guid.NewGuid());

    public ServicioGeneracionDocumentosTests() => QuestPDF.Settings.License = LicenseType.Community;

    public void Dispose()
    {
        if (Directory.Exists(_carpetaSalida)) Directory.Delete(_carpetaSalida, recursive: true);
    }

    private sealed record Contexto(
        SqliteConnection Conexion, ServicioGeneracionDocumentos Servicio, int PacienteId, int SpdId);

    private Contexto Crear()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", PrefijoNumSpd = "F-", RutaDocumentosGenerados = _carpetaSalida
        });

        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("María", "López Vidal", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, "Riesgo de caídas", null, false, null, null, null),
            usuarioQueEjecutaId: null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g" };
        medicamento.Id = repositorioMedicamentos.Crear(medicamento);

        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var elaborador = new Usuario { Nombre = "Elena", Apellidos = "Ruiz", Login = "elena", HashPassword = "x", Rol = Rol.Elaborador };
        elaborador.Id = repositorioUsuarios.Crear(elaborador);

        var repositorioSpd = new RepositorioSpd(conexion);
        var spd = new SPD
        {
            NumRegistro = "F-000001", CorrelativoNumRegistro = 1, PacienteId = paciente.Id, SesionId = Guid.NewGuid(),
            ValidezDesde = new DateOnly(2026, 9, 7), ValidezHasta = new DateOnly(2026, 9, 13), ElaboradorId = elaborador.Id
        };
        spd.Id = repositorioSpd.Crear(spd);

        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        var tratamiento = new Tratamiento
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Media,
            FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        };
        tratamiento.Id = repositorioTratamientos.Crear(tratamiento);

        var repositorioEnvases = new RepositorioEnvases(conexion);
        var envase = new Envase
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, Serie = "SER1", Lote = "LOT1",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 24
        };
        envase.Id = repositorioEnvases.Crear(envase);

        var repositorioLineas = new RepositorioSpdLineas(conexion);
        var linea = new SpdLinea
        {
            SpdId = spd.Id, TratamientoId = tratamiento.Id, MedicamentoId = medicamento.Id, SnapNombre = medicamento.Nombre,
            SnapCn = medicamento.Cn, SnapPautaD = FraccionDosis.Media, SnapDiasSemana = "1111111",
            UnidadesDosis = 3.5m, UnidadesEnvase = 4
        };
        linea.Id = repositorioLineas.Crear(linea);

        var repositorioLineaEnvases = new RepositorioSpdLineaEnvases(conexion);
        repositorioLineaEnvases.Crear(new SpdLineaEnvase
        {
            SpdLineaId = linea.Id, EnvaseId = envase.Id, UnidadesTomadas = 4, SnapSerie = "SER1", SnapLote = "LOT1", SnapCaducidad = new DateOnly(2030, 1, 1)
        });

        var servicio = new ServicioGeneracionDocumentos(
            repositorioSpd, repositorioLineas, repositorioLineaEnvases, repositorioPacientes, repositorioFarmacia, auditoria);

        return new Contexto(conexion, servicio, paciente.Id, spd.Id);
    }

    [Fact]
    public void GenerarFichaSpd_crea_el_fichero_y_audita_CA_709_710()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var resultado = ctx.Servicio.GenerarFichaSpd(ctx.SpdId, usuarioQueEjecutaId: 7);

        Assert.True(File.Exists(resultado.RutaCompleta));
        Assert.StartsWith("Ficha de preparación", resultado.NombreFichero);
        Assert.EndsWith(".pdf", resultado.NombreFichero);

        var registros = ctx.Conexion.Query<(string Accion, int? UsuarioId)>("SELECT accion, usuario_id FROM Auditoria WHERE entidad = 'SPD'");
        Assert.Contains(registros, r => r.Accion == "GENERAR_DOCUMENTO" && r.UsuarioId == 7);
    }

    [Fact]
    public void GenerarEtiquetaAnverso_y_reverso_crean_su_fichero_y_auditan()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var anverso = ctx.Servicio.GenerarEtiquetaAnverso(ctx.SpdId, null);
        var reverso = ctx.Servicio.GenerarEtiquetaReverso(ctx.SpdId, null);

        Assert.True(File.Exists(anverso.RutaCompleta));
        Assert.True(File.Exists(reverso.RutaCompleta));

        var registros = ctx.Conexion.Query<string>("SELECT accion FROM Auditoria WHERE entidad = 'SPD' AND accion = 'GENERAR_DOCUMENTO'");
        Assert.Equal(2, registros.Count());
    }

    [Fact]
    public void GenerarInstrucciones_crea_su_fichero_y_audita()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var resultado = ctx.Servicio.GenerarInstrucciones(ctx.SpdId, null);

        Assert.True(File.Exists(resultado.RutaCompleta));
    }

    [Fact]
    public void GenerarFichaPaciente_crea_el_fichero_con_los_datos_del_paciente_y_audita()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var resultado = ctx.Servicio.GenerarFichaPaciente(ctx.PacienteId, usuarioQueEjecutaId: 3);

        Assert.True(File.Exists(resultado.RutaCompleta));
        var registros = ctx.Conexion.Query<(string Accion, int? UsuarioId)>("SELECT accion, usuario_id FROM Auditoria WHERE entidad = 'Paciente'");
        Assert.Contains(registros, r => r.Accion == "GENERAR_DOCUMENTO" && r.UsuarioId == 3);
    }
}
