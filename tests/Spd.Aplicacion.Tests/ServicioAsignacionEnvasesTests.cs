using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioAsignacionEnvasesTests
{
    private static SqliteConnection AbrirBaseDeDatosDePrueba()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        return conexion;
    }

    private static (int TratamientoId, RepositorioEnvases RepositorioEnvases, ServicioAsignacionEnvases Servicio, SqliteConnection Conexion)
        Crear()
    {
        var conexion = AbrirBaseDeDatosDePrueba();
        var (pacienteId, medicamentoId) = InfraestructuraTratamientosFundamentosTests.CrearPacienteYMedicamento(conexion);
        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        var tratamientoId = repositorioTratamientos.Crear(new Tratamiento
        {
            PacienteId = pacienteId,
            MedicamentoId = medicamentoId,
            EnSpd = true,
            PautaD = FraccionDosis.Uno,
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });

        var repositorioEnvases = new RepositorioEnvases(conexion);
        var servicio = new ServicioAsignacionEnvases(repositorioEnvases, repositorioTratamientos, new RegistradorAuditoria(conexion));
        return (tratamientoId, repositorioEnvases, servicio, conexion);
    }

    [Fact]
    public void Descontar_agota_primero_el_envase_con_menos_restantes_CA_505()
    {
        var (tratamientoId, repositorioEnvases, servicio, conexion) = Crear();
        using var c = conexion;
        var tratamiento = new RepositorioTratamientos(conexion).ObtenerPorId(tratamientoId)!;
        var e1 = new Envase { PacienteId = tratamiento.PacienteId, MedicamentoId = tratamiento.MedicamentoId, UnidadesIniciales = 3, UnidadesRestantes = 3, Caducidad = new DateOnly(2027, 6, 30), Serie = "E1" };
        e1.Id = repositorioEnvases.Crear(e1);
        var e2 = new Envase { PacienteId = tratamiento.PacienteId, MedicamentoId = tratamiento.MedicamentoId, UnidadesIniciales = 28, UnidadesRestantes = 28, Caducidad = new DateOnly(2027, 1, 31), Serie = "E2" };
        e2.Id = repositorioEnvases.Crear(e2);

        var resultado = servicio.Descontar(tratamientoId, unidadesOverride: 7, caducidadMinima: new DateOnly(2026, 1, 1), null);

        Assert.Equal(2, resultado.Asignaciones.Count);
        Assert.Equal(3, resultado.Asignaciones[0].UnidadesTomadas);
        Assert.Equal(4, resultado.Asignaciones[1].UnidadesTomadas);
        Assert.Equal(EstadoEnvase.Agotado, repositorioEnvases.ObtenerPorId(e1.Id)!.Estado);
        Assert.Equal(0, repositorioEnvases.ObtenerPorId(e1.Id)!.UnidadesRestantes);
        Assert.Equal(EstadoEnvase.EnCustodia, repositorioEnvases.ObtenerPorId(e2.Id)!.Estado);
        Assert.Equal(24, repositorioEnvases.ObtenerPorId(e2.Id)!.UnidadesRestantes);
    }

    [Fact]
    public void Sobrante_permanece_en_custodia_y_nunca_se_descarta_CA_506()
    {
        var (tratamientoId, repositorioEnvases, servicio, conexion) = Crear();
        using var c = conexion;
        var tratamiento = new RepositorioTratamientos(conexion).ObtenerPorId(tratamientoId)!;
        var envase = new Envase { PacienteId = tratamiento.PacienteId, MedicamentoId = tratamiento.MedicamentoId, UnidadesIniciales = 28, UnidadesRestantes = 28, Caducidad = new DateOnly(2027, 6, 30), Serie = "E1" };
        envase.Id = repositorioEnvases.Crear(envase);

        servicio.Descontar(tratamientoId, unidadesOverride: 7, caducidadMinima: new DateOnly(2026, 1, 1), null);

        var actualizado = repositorioEnvases.ObtenerPorId(envase.Id)!;
        Assert.Equal(EstadoEnvase.EnCustodia, actualizado.Estado);
        Assert.Equal(21, actualizado.UnidadesRestantes);
    }

    [Fact]
    public void Descontar_respeta_caducidadMinima_y_registra_en_auditoria()
    {
        var (tratamientoId, repositorioEnvases, servicio, conexion) = Crear();
        using var c = conexion;
        var tratamiento = new RepositorioTratamientos(conexion).ObtenerPorId(tratamientoId)!;
        var caducado = new Envase { PacienteId = tratamiento.PacienteId, MedicamentoId = tratamiento.MedicamentoId, UnidadesIniciales = 28, UnidadesRestantes = 28, Caducidad = new DateOnly(2026, 1, 1), Serie = "E1" };
        caducado.Id = repositorioEnvases.Crear(caducado);
        var valido = new Envase { PacienteId = tratamiento.PacienteId, MedicamentoId = tratamiento.MedicamentoId, UnidadesIniciales = 10, UnidadesRestantes = 10, Caducidad = new DateOnly(2027, 1, 1), Serie = "E2" };
        valido.Id = repositorioEnvases.Crear(valido);

        var resultado = servicio.Descontar(tratamientoId, unidadesOverride: 5, caducidadMinima: new DateOnly(2026, 6, 1), 3);

        Assert.Single(resultado.Asignaciones);
        Assert.Equal(valido.Id, resultado.Asignaciones[0].Envase.Id);
        Assert.Equal(28, repositorioEnvases.ObtenerPorId(caducado.Id)!.UnidadesRestantes);

        var registros = conexion.Query<(string Accion, int? UsuarioId)>("SELECT accion, usuario_id FROM Auditoria WHERE entidad = 'Tratamiento'");
        Assert.Contains(registros, r => r.Accion == "DESCUENTO_ENVASE" && r.UsuarioId == 3);
    }
}
