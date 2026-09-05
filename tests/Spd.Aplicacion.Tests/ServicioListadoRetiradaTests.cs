using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioListadoRetiradaTests
{
    private sealed class ComprobadorSiempreVerdadero : IComprobadorCoberturaSpd
    {
        public bool YaCubierta(int pacienteId, DateOnly proximaRetirada) => true;
    }

    private sealed record Contexto(
        SqliteConnection Conexion, ServicioListadoRetirada Servicio, RepositorioPacientes RepositorioPacientes,
        RepositorioMedicamentos RepositorioMedicamentos, RepositorioTratamientos RepositorioTratamientos,
        RepositorioEnvases RepositorioEnvases, RepositorioContactos RepositorioContactos, RegistradorAuditoria Auditoria);

    private static Contexto Crear(int diasAntelacionListado = 2, IComprobadorCoberturaSpd? comprobador = null)
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", DiasAntelacionListado = diasAntelacionListado
        });

        var repositorioPacientes = new RepositorioPacientes(conexion);
        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        var repositorioEnvases = new RepositorioEnvases(conexion);
        var repositorioContactos = new RepositorioContactos(conexion);
        var auditoria = new RegistradorAuditoria(conexion);

        var servicio = new ServicioListadoRetirada(
            repositorioPacientes, repositorioContactos, repositorioTratamientos, repositorioMedicamentos,
            repositorioEnvases, repositorioFarmacia, comprobador ?? new ComprobadorCoberturaSpdNulo(), auditoria);

        return new Contexto(conexion, servicio, repositorioPacientes, repositorioMedicamentos, repositorioTratamientos, repositorioEnvases, repositorioContactos, auditoria);
    }

    private static Paciente CrearPacienteActivo(Contexto ctx, string diaRetirada, int nBlisteres, string dni)
    {
        var servicioPacientes = new ServicioPacientes(ctx.RepositorioPacientes, new RepositorioFarmacia(ctx.Conexion), ctx.Auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("Ana", "Pérez", null, dni, null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, diaRetirada, nBlisteres), null);
        servicioPacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);
        return ctx.RepositorioPacientes.ObtenerPorId(paciente.Id)!;
    }

    private static int CrearMedicamento(Contexto ctx, string cn, int? unidadesEnvase)
    {
        var medicamento = new Medicamento { Cn = cn, Nombre = "Paracetamol", UnidadesEnvase = unidadesEnvase };
        medicamento.Id = ctx.RepositorioMedicamentos.Crear(medicamento);
        return medicamento.Id;
    }

    private static int CrearTratamientoActivo(Contexto ctx, int pacienteId, int medicamentoId, FraccionDosis pautaD, string diasSemana = "1111111")
        => ctx.RepositorioTratamientos.Crear(new Tratamiento
        {
            PacienteId = pacienteId,
            MedicamentoId = medicamentoId,
            EnSpd = true,
            PautaD = pautaD,
            DiasSemana = diasSemana,
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });

    [Fact]
    public void Calcula_necesarias_disponibles_faltan_y_envases_a_retirar_CA_500()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var hoy = new DateOnly(2026, 9, 7); // lunes
        var paciente = CrearPacienteActivo(ctx, "LU", nBlisteres: 2, dni: "12345678Z");
        var medicamentoId = CrearMedicamento(ctx, "654321", unidadesEnvase: 28);
        CrearTratamientoActivo(ctx, paciente.Id, medicamentoId, FraccionDosis.Uno);
        ctx.RepositorioEnvases.Crear(new Envase
        {
            PacienteId = paciente.Id, MedicamentoId = medicamentoId, Serie = "S1",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 4, UnidadesRestantes = 4
        });

        var listado = ctx.Servicio.ObtenerListado(hoy, new FiltrosListadoRetirada());

        var fila = Assert.Single(listado);
        Assert.Equal(14, fila.Necesarias);
        Assert.Equal(4, fila.Disponibles);
        Assert.Equal(10, fila.Faltan);
        Assert.Equal(1, fila.EnvasesARetirar);
    }

    [Fact]
    public void Paciente_fuera_de_ventana_de_antelacion_no_aparece_CA_501()
    {
        var ctx = Crear(diasAntelacionListado: 2);
        using var c = ctx.Conexion;
        var lunes = new DateOnly(2026, 9, 7);
        var paciente = CrearPacienteActivo(ctx, "JU", nBlisteres: 1, dni: "12345678Z"); // jueves = lunes+3
        var medicamentoId = CrearMedicamento(ctx, "654321", 28);
        CrearTratamientoActivo(ctx, paciente.Id, medicamentoId, FraccionDosis.Uno);

        Assert.Empty(ctx.Servicio.ObtenerListado(lunes, new FiltrosListadoRetirada()));

        var martes = lunes.AddDays(1);
        Assert.Single(ctx.Servicio.ObtenerListado(martes, new FiltrosListadoRetirada()));
    }

    [Fact]
    public void Unidades_envase_desconocido_muestra_null_sin_romper_el_resto_CA_512()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var hoy = new DateOnly(2026, 9, 7);
        var paciente = CrearPacienteActivo(ctx, "LU", 1, "12345678Z");
        var medicamentoId = CrearMedicamento(ctx, "654321", unidadesEnvase: null);
        CrearTratamientoActivo(ctx, paciente.Id, medicamentoId, FraccionDosis.Uno);

        var fila = Assert.Single(ctx.Servicio.ObtenerListado(hoy, new FiltrosListadoRetirada()));

        Assert.Null(fila.EnvasesARetirar);
        Assert.True(fila.Faltan > 0);
    }

    [Fact]
    public void Dni_de_retirada_usa_el_contacto_marcado_si_existe_CA_514()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var hoy = new DateOnly(2026, 9, 7);
        var paciente = CrearPacienteActivo(ctx, "LU", 1, "22222222B");
        var medicamentoId = CrearMedicamento(ctx, "654321", 28);
        CrearTratamientoActivo(ctx, paciente.Id, medicamentoId, FraccionDosis.Uno);
        ctx.RepositorioContactos.Crear(new Contacto
        {
            PacienteId = paciente.Id, Tipo = TipoContacto.Familiar, Nombre = "Luis", Apellidos = "Pérez",
            Dni = "11111111A", RetiraMedicacion = true
        });

        var fila = Assert.Single(ctx.Servicio.ObtenerListado(hoy, new FiltrosListadoRetirada()));

        Assert.Equal("11111111A", fila.DniRetirada);
        Assert.False(fila.AvisoSinDni);
    }

    [Fact]
    public void Dni_de_retirada_sin_contacto_usa_el_del_paciente_CA_515()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var hoy = new DateOnly(2026, 9, 7);
        var paciente = CrearPacienteActivo(ctx, "LU", 1, "22222222B");
        var medicamentoId = CrearMedicamento(ctx, "654321", 28);
        CrearTratamientoActivo(ctx, paciente.Id, medicamentoId, FraccionDosis.Uno);

        var fila = Assert.Single(ctx.Servicio.ObtenerListado(hoy, new FiltrosListadoRetirada()));

        Assert.Equal("22222222B", fila.DniRetirada);
    }

    [Fact]
    public void Filtro_solo_con_faltantes_por_defecto_oculta_pacientes_con_stock_suficiente_FR_534()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var hoy = new DateOnly(2026, 9, 7);
        var paciente = CrearPacienteActivo(ctx, "LU", 1, "12345678Z");
        var medicamentoId = CrearMedicamento(ctx, "654321", 28);
        CrearTratamientoActivo(ctx, paciente.Id, medicamentoId, FraccionDosis.Uno);
        ctx.RepositorioEnvases.Crear(new Envase
        {
            PacienteId = paciente.Id, MedicamentoId = medicamentoId, Serie = "S1",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28
        });

        Assert.Empty(ctx.Servicio.ObtenerListado(hoy, new FiltrosListadoRetirada(SoloConFaltantes: true)));
        Assert.Single(ctx.Servicio.ObtenerListado(hoy, new FiltrosListadoRetirada(SoloConFaltantes: false)));
    }

    [Fact]
    public void Comprobador_nulo_nunca_excluye_documenta_CA_502_diferido()
    {
        var ctx = Crear(comprobador: new ComprobadorCoberturaSpdNulo());
        using var c = ctx.Conexion;
        var hoy = new DateOnly(2026, 9, 7);
        var paciente = CrearPacienteActivo(ctx, "LU", 1, "12345678Z");
        var medicamentoId = CrearMedicamento(ctx, "654321", 28);
        CrearTratamientoActivo(ctx, paciente.Id, medicamentoId, FraccionDosis.Uno);

        Assert.Single(ctx.Servicio.ObtenerListado(hoy, new FiltrosListadoRetirada()));
    }

    [Fact]
    public void Comprobador_real_futuro_excluiria_al_paciente()
    {
        var ctx = Crear(comprobador: new ComprobadorSiempreVerdadero());
        using var c = ctx.Conexion;
        var hoy = new DateOnly(2026, 9, 7);
        var paciente = CrearPacienteActivo(ctx, "LU", 1, "12345678Z");
        var medicamentoId = CrearMedicamento(ctx, "654321", 28);
        CrearTratamientoActivo(ctx, paciente.Id, medicamentoId, FraccionDosis.Uno);

        Assert.Empty(ctx.Servicio.ObtenerListado(hoy, new FiltrosListadoRetirada()));
    }

    [Fact]
    public void RegistrarImpresion_audita_IMPRIMIR_FR_535()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;

        ctx.Servicio.RegistrarImpresion(9);

        var registros = ctx.Conexion.Query<(string Accion, int? UsuarioId)>("SELECT accion, usuario_id FROM Auditoria WHERE entidad = 'ListadoRetirada'");
        Assert.Contains(registros, r => r.Accion == "IMPRIMIR" && r.UsuarioId == 9);
    }
}
