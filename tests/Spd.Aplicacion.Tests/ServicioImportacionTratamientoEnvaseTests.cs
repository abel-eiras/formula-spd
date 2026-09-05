using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioImportacionTratamientoEnvaseTests
{
    private static readonly MapeoColumnasImportacion Mapeo = new(Cn: "0", NumSerie: "1", Lote: "2", Caducidad: "3");
    private static readonly PerfilImportacionTratamiento PerfilPegado = new()
    { Nombre = "Pegado", Origen = OrigenImportacionTratamiento.Portapapeles, Mapeo = Mapeo };

    private sealed record Contexto(
        SqliteConnection Conexion, ServicioImportacionTratamientoEnvase Servicio, int PacienteId, int MedicamentoId,
        RepositorioTratamientos RepositorioTratamientos, RepositorioEnvases RepositorioEnvases);

    private static Contexto Crear()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var (pacienteId, medicamentoId) = InfraestructuraTratamientosFundamentosTests.CrearPacienteYMedicamento(conexion);
        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = repositorioMedicamentos.ObtenerPorId(medicamentoId)!;
        medicamento.UnidadesEnvase = 28;
        repositorioMedicamentos.Actualizar(medicamento);

        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        var repositorioEnvases = new RepositorioEnvases(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var auditoria = new RegistradorAuditoria(conexion);
        var servicioTratamientos = new ServicioTratamientos(repositorioTratamientos, repositorioPacientes, auditoria);
        var repositorioPerfiles = new RepositorioPerfilesImportacionTratamiento(conexion);

        var servicio = new ServicioImportacionTratamientoEnvase(
            repositorioMedicamentos, repositorioTratamientos, repositorioEnvases, repositorioPerfiles, servicioTratamientos, auditoria);

        return new Contexto(conexion, servicio, pacienteId, medicamentoId, repositorioTratamientos, repositorioEnvases);
    }

    [Fact]
    public void Cn_con_tratamiento_activo_reutiliza_y_solo_crea_envase_CA_516()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        ctx.RepositorioTratamientos.Crear(new Tratamiento
        {
            PacienteId = ctx.PacienteId, MedicamentoId = ctx.MedicamentoId, EnSpd = true, PautaD = FraccionDosis.Uno,
            FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });

        var resultado = ctx.Servicio.ImportarDesdePegado(ctx.PacienteId, PerfilPegado, "654321\tS123\tL1\t2027-01", null);

        Assert.Single(resultado.EnvasesCreados);
        Assert.Empty(resultado.TratamientosPendientesCreados);
        Assert.Equal(1, ctx.RepositorioTratamientos.ListarVigentesDePaciente(ctx.PacienteId).Count);
        Assert.NotNull(ctx.RepositorioEnvases.ObtenerPorSerie("S123"));
    }

    [Fact]
    public void Cn_sin_tratamiento_crea_uno_pendiente_de_posologia_CA_517()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var resultado = ctx.Servicio.ImportarDesdePegado(ctx.PacienteId, PerfilPegado, "654321\tS999\tL1\t2027-01", null);

        var tratamientoId = Assert.Single(resultado.TratamientosPendientesCreados);
        var tratamiento = ctx.RepositorioTratamientos.ObtenerPorId(tratamientoId);
        Assert.Equal(EstadoTratamiento.PendienteRevision, tratamiento!.Estado);
        Assert.True(resultado.EnvasesCreados.Single().TratamientoPendienteDePosologia);
    }

    [Fact]
    public void Fichero_con_serie_duplicada_la_separa_sin_detener_el_resto_CA_518()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var repositorioPacientes = new RepositorioPacientes(ctx.Conexion);
        var otroPaciente = new ServicioPacientes(repositorioPacientes, new RepositorioFarmacia(ctx.Conexion), new RegistradorAuditoria(ctx.Conexion))
            .Crear(new DatosAltaPaciente("Luis", "Gómez", null, "87654321X", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, null), null);
        var otroEnvase = new Envase { PacienteId = otroPaciente.Id, MedicamentoId = ctx.MedicamentoId, Serie = "DUP", UnidadesIniciales = 1, UnidadesRestantes = 1 };
        otroEnvase.Id = ctx.RepositorioEnvases.Crear(otroEnvase);

        var csv = string.Join('\n',
            "654321,S1,L1,2027-01", "654321,S2,L1,2027-01", "654321,DUP,L1,2027-01",
            "654321,S4,L1,2027-01", "654321,S5,L1,2027-01");
        var perfilFichero = new PerfilImportacionTratamiento
        { Nombre = "Fichero", Origen = OrigenImportacionTratamiento.Fichero, Separador = ",", TieneCabecera = false, Mapeo = Mapeo };

        var resultado = ctx.Servicio.ImportarDesdeFichero(ctx.PacienteId, perfilFichero, csv, null);

        Assert.Equal(4, resultado.EnvasesCreados.Count);
        var conflictiva = Assert.Single(resultado.FilasConSerieDuplicada);
        Assert.Equal("DUP", conflictiva.Fila.NumSerie);
        Assert.Equal(otroPaciente.Id, conflictiva.PacienteIdExistente);
    }

    [Fact]
    public void Cn_no_encontrado_se_separa_sin_bloquear_las_demas_filas_FR_575()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        var texto = "999999\tS1\tL1\t2027-01\n654321\tS2\tL1\t2027-01";
        var resultado = ctx.Servicio.ImportarDesdePegado(ctx.PacienteId, PerfilPegado, texto, null);

        Assert.Single(resultado.FilasConCnNoEncontrado);
        Assert.Single(resultado.EnvasesCreados);
    }

    [Fact]
    public void GuardarPerfil_y_ListarPerfiles_persisten_el_mapeo_FR_572()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        ctx.Servicio.GuardarPerfil("Farmatic — pegado", OrigenImportacionTratamiento.Portapapeles, null, false, Mapeo);

        var perfiles = ctx.Servicio.ListarPerfiles();
        var guardado = Assert.Single(perfiles);
        Assert.Equal("Farmatic — pegado", guardado.Nombre);
        Assert.Equal(Mapeo, guardado.Mapeo);
    }
}
