using Microsoft.Data.Sqlite;
using QuestPDF.Infrastructure;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using UglyToad.PdfPig;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Spec 019 CA-1901..1906: centros, albarán de entrega y lista de reposición.</summary>
public sealed class ServicioCentrosYReposicionTests : IDisposable
{
    private readonly CarpetaTemporalDePrueba _carpeta = new();

    public ServicioCentrosYReposicionTests() => QuestPDF.Settings.License = LicenseType.Community;

    public void Dispose() => _carpeta.Dispose();

    private sealed record Contexto(
        ServicioCentros Centros, ServicioReposicion Reposicion, ServicioDocumentosCentros Documentos,
        Paciente Maria, Paciente Jose, int SpdVerificadoId, RepositorioEnvases Envases, int MedicamentoId);

    private Contexto Montar(SqliteConnection c)
    {
        c.Open();
        new AplicadorMigraciones(c).Aplicar();
        var auditoria = new RegistradorAuditoria(c);
        var repositorioFarmacia = new RepositorioFarmacia(c);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia Proba", TitularOComunidadBienes = "T", Cif = "B0",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9", PrefijoNumSpd = "F-",
            RutaDocumentosGenerados = _carpeta.Ruta
        });
        var repositorioPacientes = new RepositorioPacientes(c);
        var pacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        Paciente Alta(string nombre, string apellidos, string dni)
        {
            var p = pacientes.Crear(new DatosAltaPaciente(nombre, apellidos, null, dni, null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, null), null);
            pacientes.CambiarEstado(p.Id, EstadoPaciente.Activo, null, null);
            return repositorioPacientes.ObtenerPorId(p.Id)!;
        }
        var maria = Alta("María", "López", "12345678Z");
        var jose = Alta("José", "Núñez", "87654321X");

        var repositorioMedicamentos = new RepositorioMedicamentos(c);
        var med = new Medicamento { Cn = "654321", Nombre = "Enalapril 20 mg", UnidadesEnvase = 28 };
        med.Id = repositorioMedicamentos.Crear(med);
        var tratamientos = new RepositorioTratamientos(c);
        foreach (var p in new[] { maria, jose })
            tratamientos.Crear(new Tratamiento
            {
                PacienteId = p.Id, MedicamentoId = med.Id, EnSpd = true, PautaD = FraccionDosis.Uno, PautaN = FraccionDosis.Uno,
                FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
            });
        var envases = new RepositorioEnvases(c);
        // María: 10 unidades a 2 al día → 5 días. José: 56 → 28 días.
        envases.Crear(new Envase { PacienteId = maria.Id, MedicamentoId = med.Id, Serie = "S1", Lote = "L1",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 10 });
        envases.Crear(new Envase { PacienteId = jose.Id, MedicamentoId = med.Id, Serie = "S2", Lote = "L1",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 56, UnidadesRestantes = 56 });

        var usuario = new Usuario { Nombre = "E", Apellidos = "R", Login = "e", HashPassword = "x", Rol = Rol.Elaborador };
        usuario.Id = new RepositorioUsuarios(c).Crear(usuario);
        var spds = new RepositorioSpd(c);
        var verificado = spds.Crear(new SPD
        {
            NumRegistro = "F-000007", CorrelativoNumRegistro = 7, PacienteId = maria.Id, SesionId = Guid.NewGuid(),
            ElaboradorId = usuario.Id, ValidezDesde = new DateOnly(2026, 11, 9), ValidezHasta = new DateOnly(2026, 11, 15),
            Estado = EstadoSpd.Verificado
        });
        spds.Crear(new SPD
        {
            NumRegistro = "F-000006", CorrelativoNumRegistro = 6, PacienteId = maria.Id, SesionId = Guid.NewGuid(),
            ElaboradorId = usuario.Id, ValidezDesde = new DateOnly(2026, 11, 2), ValidezHasta = new DateOnly(2026, 11, 8),
            Estado = EstadoSpd.Entregado
        });

        var repositorioCentros = new RepositorioCentros(c);
        var centros = new ServicioCentros(repositorioCentros, repositorioPacientes, spds, auditoria);
        var reposicion = new ServicioReposicion(repositorioPacientes, tratamientos, repositorioMedicamentos, envases,
            new RepositorioMedicos(c), repositorioCentros);
        var documentos = new ServicioDocumentosCentros(repositorioCentros, centros, repositorioFarmacia, auditoria);
        return new Contexto(centros, reposicion, documentos, maria, jose, verificado, envases, med.Id);
    }

    [Fact]
    public void Albaran_lista_solo_los_verificados_de_los_pacientes_del_centro_CA_1901()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        var centro = ctx.Centros.Crear(new DatosCentro("Residencia As Camelias", "Rúa do Mar 1", null, "Lucía"), null);
        ctx.Centros.AsignarPaciente(ctx.Maria.Id, centro.Id, null);

        var blisteres = ctx.Centros.BlisteresParaAlbaran(centro.Id);

        Assert.Equal(ctx.SpdVerificadoId, Assert.Single(blisteres).SpdId);
        Assert.Equal(ctx.Maria.Id, Assert.Single(ctx.Centros.PacientesDe(centro.Id)).Id);
    }

    [Fact]
    public void El_PDF_del_albaran_lleva_centro_blisteres_y_firmas_CA_1902()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        var centro = ctx.Centros.Crear(new DatosCentro("Residencia As Camelias", null, null, null), null);
        ctx.Centros.AsignarPaciente(ctx.Maria.Id, centro.Id, null);

        var r = ctx.Documentos.GenerarAlbaran(centro.Id, [ctx.SpdVerificadoId], null);

        using var pdf = PdfDocument.Open(r.RutaCompleta);
        var texto = string.Join(" ", pdf.GetPages().SelectMany(p => p.GetWords()).Select(w => w.Text));
        foreach (var e in new[] { "Farmacia", "Proba", "ALBARÁN", "Camelias", "F-000007", "López", "09/11/2026", "Recibido", "Firma", "DNI" })
            Assert.Contains(e, texto);
    }

    [Fact]
    public void Centro_con_pacientes_no_se_da_de_baja_CA_1903()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        var centro = ctx.Centros.Crear(new DatosCentro("Centro de día", null, null, null), null);
        ctx.Centros.AsignarPaciente(ctx.Jose.Id, centro.Id, null);
        Assert.Throws<ErrorValidacionException>(() => ctx.Centros.DarDeBaja(centro.Id, "cierra", null));

        ctx.Centros.AsignarPaciente(ctx.Jose.Id, null, null);
        ctx.Centros.DarDeBaja(centro.Id, "cierra", null);
        Assert.Empty(ctx.Centros.Listar(incluirBaja: false));
        Assert.Single(ctx.Centros.Listar(incluirBaja: true));
    }

    [Fact]
    public void Reposicion_lista_lo_que_se_agota_dentro_del_horizonte_CA_1904()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        var hoy = new DateOnly(2026, 11, 5);

        var fila = Assert.Single(ctx.Reposicion.Obtener(hoy, 14));
        Assert.Equal(ctx.Maria.Id, fila.PacienteId);
        Assert.Equal(5, fila.DiasCobertura);
        Assert.Equal(hoy.AddDays(5), fila.FechaAgotamiento);

        Assert.Equal(2, ctx.Reposicion.Obtener(hoy, 30).Count);
    }

    [Fact]
    public void Reposicion_filtra_por_centro_y_no_cuenta_cajas_caducadas_CA_1905()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        var centro = ctx.Centros.Crear(new DatosCentro("Residencia", null, null, null), null);
        ctx.Centros.AsignarPaciente(ctx.Jose.Id, centro.Id, null);
        ctx.Envases.Crear(new Envase { PacienteId = ctx.Maria.Id, MedicamentoId = ctx.MedicamentoId, Serie = "S9", Lote = "L9",
            Caducidad = new DateOnly(2026, 11, 1), UnidadesIniciales = 28, UnidadesRestantes = 28 });

        var hoy = new DateOnly(2026, 11, 5);
        Assert.Equal("Residencia", Assert.Single(ctx.Reposicion.Obtener(hoy, 30, centro.Id)).Centro);
        Assert.Equal(5, ctx.Reposicion.Obtener(hoy, 14).Single(f => f.PacienteId == ctx.Maria.Id).DiasCobertura);
    }

    [Fact]
    public void Lista_de_reposicion_imprime_los_datos_CA_1906()
    {
        using var c = new SqliteConnection("Data Source=:memory:");
        var ctx = Montar(c);
        var hoy = new DateOnly(2026, 11, 5);
        var r = ctx.Documentos.GenerarListaReposicion(ctx.Reposicion.Obtener(hoy, 14), 14, hoy, null);
        using var pdf = PdfDocument.Open(r.RutaCompleta);
        var texto = string.Join(" ", pdf.GetPages().SelectMany(p => p.GetWords()).Select(w => w.Text));
        foreach (var e in new[] { "REPOSICIÓN", "López", "Enalapril", "654321", "10/11/2026" })
            Assert.Contains(e, texto);
    }

    [Fact]
    public void Cobertura_sin_consumo_no_tiene_fecha()
    {
        Assert.Null(CoberturaEnvases.DiasDeCobertura(10, 0));
        Assert.Equal(5, CoberturaEnvases.DiasDeCobertura(10, 14));
        Assert.Equal(0, CoberturaEnvases.DiasDeCobertura(-3, 14));
    }
}
