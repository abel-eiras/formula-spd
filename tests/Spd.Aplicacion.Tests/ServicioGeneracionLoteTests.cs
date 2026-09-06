using Microsoft.Data.Sqlite;
using QuestPDF.Infrastructure;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>Spec 007 FR-720..725: lote sobre pacientes "con envases al día"; los que no lo están
/// quedan excluidos con motivo sin detener al resto (CA-703/704/705).</summary>
public sealed class ServicioGeneracionLoteTests : IDisposable
{
    private readonly string _carpetaSalida = Path.Combine(Path.GetTempPath(), "spd-tests-lote-" + Guid.NewGuid());

    public ServicioGeneracionLoteTests() => QuestPDF.Settings.License = LicenseType.Community;

    public void Dispose()
    {
        if (Directory.Exists(_carpetaSalida)) Directory.Delete(_carpetaSalida, recursive: true);
    }

    [Fact]
    public void Generar_prepara_la_sesion_y_los_documentos_del_listo_y_excluye_al_que_no_tiene_envases()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "T", Cif = "B0", Direccion = "D", Cp = "36000",
            Poblacion = "Pontevedra", Telefono = "9", PrefijoNumSpd = "F-", RutaDocumentosGenerados = _carpetaSalida, DiasAntelacionListado = 0
        });
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var diaLejano = DiasSemana.Codigos[((int)DateTime.Today.DayOfWeek + 6 + 3) % 7];
        var listo = servicioPacientes.Crear(new DatosAltaPaciente("Ana", "Lista", null, "12345678Z", null, null, null, null, null, null,
            null, null, null, null, null, null, null, false, null, diaLejano, 1), null);
        var sinEnvases = servicioPacientes.Crear(new DatosAltaPaciente("Luis", "Sin Envases", null, "87654321X", null, null, null, null, null, null,
            null, null, null, null, null, null, null, false, null, diaLejano, 1), null);
        servicioPacientes.CambiarEstado(listo.Id, EstadoPaciente.Activo, null, null);
        servicioPacientes.CambiarEstado(sinEnvases.Id, EstadoPaciente.Activo, null, null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var med = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g", UnidadesEnvase = 28 };
        med.Id = repositorioMedicamentos.Crear(med);
        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        foreach (var p in new[] { listo, sinEnvases })
            repositorioTratamientos.Crear(new Tratamiento { PacienteId = p.Id, MedicamentoId = med.Id, EnSpd = true, PautaD = FraccionDosis.Uno, FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1) });
        var repositorioEnvases = new RepositorioEnvases(conexion);
        repositorioEnvases.Crear(new Envase { PacienteId = listo.Id, MedicamentoId = med.Id, Serie = "S1", Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28 });

        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var elaborador = new Usuario { Nombre = "Elena", Apellidos = "Ruiz", Login = "elena", HashPassword = "x", Rol = Rol.Elaborador };
        elaborador.Id = repositorioUsuarios.Crear(elaborador);

        var servicioEnvases = new ServicioEnvases(repositorioEnvases, repositorioTratamientos, repositorioPacientes, auditoria);
        var listado = new ServicioListadoRetirada(repositorioPacientes, new RepositorioContactos(conexion), repositorioTratamientos, repositorioMedicamentos,
            repositorioEnvases, repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);
        var repositorioSpd = new RepositorioSpd(conexion);
        var preparacion = new ServicioPreparacion(
            repositorioSpd, new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion), new RepositorioSpdVerificaciones(conexion),
            new RepositorioSpdModificaciones(conexion), new RepositorioRegistrosAmbientales(conexion), new RepositorioMaterialAcondicionamiento(conexion),
            repositorioPacientes, repositorioTratamientos, repositorioMedicamentos, repositorioEnvases, repositorioFarmacia,
            new ServicioAsignacionEnvases(repositorioEnvases, repositorioTratamientos, auditoria), servicioEnvases, listado,
            new ComprobadorIdoneidadYConsentimientoNulo(), auditoria);
        var documentos = new ServicioGeneracionDocumentos(
            repositorioSpd, new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion), new RepositorioSpdVerificaciones(conexion),
            repositorioPacientes, new RepositorioContactos(conexion), new RepositorioMedicos(conexion), repositorioTratamientos, repositorioMedicamentos,
            repositorioUsuarios, new RepositorioMaterialAcondicionamiento(conexion), new RepositorioRegistrosAmbientales(conexion),
            new RepositorioEvaluacionesIdoneidad(conexion), new RepositorioConsentimientos(conexion), new RepositorioComunicacionesMedico(conexion),
            repositorioFarmacia, auditoria);

        preparacion.CrearMaterial("Blíster semanal", "LOTE-1", DateOnly.FromDateTime(DateTime.Today));
        preparacion.ObtenerOCrearLecturaAmbiental(21, 50, elaborador.Id);   // lectura reciente que el lote reutiliza

        var lote = new ServicioGeneracionLote(preparacion, documentos, repositorioSpd, repositorioPacientes);
        var resultado = lote.Generar([listo.Id, sinEnvases.Id], new TiposDocumentoLote(true, true, true), elaborador.Id);

        Assert.True(resultado.Generados.Count == 1,
            $"Generados: {resultado.Generados.Count}; excluidos: {string.Join(" | ", resultado.Excluidos.Select(e => e.Paciente + ": " + e.Motivo))}; fallidos: {string.Join(" | ", resultado.Fallidos.Select(e => e.Paciente + ": " + e.Motivo))}");
        Assert.Equal(4, resultado.Generados[0].Ficheros.Count);   // ficha, etiqueta anverso, etiqueta reverso, instrucciones
        Assert.Contains(resultado.Generados[0].Avisos, a => a.Contains("preparado automáticamente"));
        Assert.Single(resultado.Excluidos);
        Assert.Contains("Faltan", resultado.Excluidos[0].Motivo);
        Assert.Empty(resultado.Fallidos);

        var spd = repositorioSpd.ListarUltimaSesionDePaciente(listo.Id).Single();
        Assert.Equal(EstadoSpd.Preparado, spd.Estado);
        Assert.NotNull(spd.ImpresoFichaEn);
        Assert.NotNull(spd.ImpresoEtiquetasEn);
        Assert.NotNull(spd.ImpresoInstruccionesEn);

        // Segunda pasada: la sesión ya existe en PREPARADO y se reutiliza sin crear otra (FR-722).
        var segunda = lote.Generar([listo.Id], new TiposDocumentoLote(true, false, false), elaborador.Id);
        Assert.Single(segunda.Generados);
        Assert.Empty(segunda.Generados[0].Avisos);
        Assert.Single(repositorioSpd.Listar(null, listo.Id, null));
    }

    [Fact]
    public void Generar_sin_tipos_lanza_FR_723()
    {
        var lote = new ServicioGeneracionLote(null!, null!, null!, null!);
        Assert.Throws<ErrorValidacionException>(() => lote.Generar([1], new TiposDocumentoLote(false, false, false), 1));
    }
}
