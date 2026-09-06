using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ServicioPreparacionTests
{
    private sealed class ComprobadorSiempreRechaza : IComprobadorIdoneidadYConsentimiento
    {
        public bool Aprobado(int pacienteId) => false;
    }

    private sealed record Contexto(
        SqliteConnection Conexion, ServicioPreparacion Servicio, int PacienteId, int MedicamentoId,
        int ElaboradorId, int VerificadorId, RepositorioSpd RepositorioSpd, RepositorioSpdLineas RepositorioLineas,
        RepositorioSpdLineaEnvases RepositorioLineaEnvases, RepositorioEnvases RepositorioEnvases,
        RepositorioTratamientos RepositorioTratamientos, RepositorioMaterialAcondicionamiento RepositorioMaterial,
        RepositorioRegistrosAmbientales RepositorioAmbiental);

    /// <summary>Día de la semana garantizado distinto de "hoy", para que el paciente de prueba
    /// nunca aparezca por sorpresa en la ventana del listado de retirada (Spec 005) a menos que un
    /// test lo pida explícitamente — evita una dependencia oculta del día real de ejecución.</summary>
    private static string DiaRetiradaFueraDeVentana()
        => DiasSemana.Codigos[((int)DateTime.Today.DayOfWeek + 6 + 3) % 7];

    private static Contexto Crear(
        IComprobadorIdoneidadYConsentimiento? comprobador = null, int nBlisteres = 1, string? diaRetirada = null)
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", PrefijoNumSpd = "F-", DiasAntelacionListado = 0
        });

        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("Ana", "Pérez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, diaRetirada ?? DiaRetiradaFueraDeVentana(), nBlisteres), null);
        servicioPacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g", UnidadesEnvase = 28 };
        medicamento.Id = repositorioMedicamentos.Crear(medicamento);

        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        repositorioTratamientos.Crear(new Tratamiento
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno,
            FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });

        var repositorioEnvases = new RepositorioEnvases(conexion);
        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var elaborador = new Usuario { Nombre = "Elena", Apellidos = "Ruiz", Login = "elena", HashPassword = "x", Rol = Rol.Elaborador };
        elaborador.Id = repositorioUsuarios.Crear(elaborador);
        var verificador = new Usuario { Nombre = "Marcos", Apellidos = "Vila", Login = "marcos", HashPassword = "x", Rol = Rol.Elaborador };
        verificador.Id = repositorioUsuarios.Crear(verificador);

        var repositorioMaterial = new RepositorioMaterialAcondicionamiento(conexion);
        var repositorioAmbiental = new RepositorioRegistrosAmbientales(conexion);
        var repositorioSpd = new RepositorioSpd(conexion);
        var repositorioLineas = new RepositorioSpdLineas(conexion);
        var repositorioLineaEnvases = new RepositorioSpdLineaEnvases(conexion);
        var repositorioVerificaciones = new RepositorioSpdVerificaciones(conexion);
        var repositorioModificaciones = new RepositorioSpdModificaciones(conexion);

        var servicioAsignacion = new ServicioAsignacionEnvases(repositorioEnvases, repositorioTratamientos, auditoria);
        var servicioEnvases = new ServicioEnvases(repositorioEnvases, repositorioTratamientos, repositorioPacientes, auditoria);
        var servicioListadoRetirada = new ServicioListadoRetirada(
            repositorioPacientes, new RepositorioContactos(conexion), repositorioTratamientos, repositorioMedicamentos,
            repositorioEnvases, repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);

        var servicio = new ServicioPreparacion(
            repositorioSpd, repositorioLineas, repositorioLineaEnvases, repositorioVerificaciones, repositorioModificaciones,
            repositorioAmbiental, repositorioMaterial, repositorioPacientes, repositorioTratamientos, repositorioMedicamentos,
            repositorioEnvases, repositorioFarmacia, servicioAsignacion, servicioEnvases, servicioListadoRetirada,
            comprobador ?? new ComprobadorIdoneidadYConsentimientoNulo(), auditoria);

        return new Contexto(
            conexion, servicio, paciente.Id, medicamento.Id, elaborador.Id, verificador.Id, repositorioSpd, repositorioLineas,
            repositorioLineaEnvases, repositorioEnvases, repositorioTratamientos, repositorioMaterial, repositorioAmbiental);
    }

    private static void CrearEnvase(Contexto ctx, string serie, int unidades, DateOnly caducidad)
        => ctx.RepositorioEnvases.Crear(new Envase
        {
            PacienteId = ctx.PacienteId, MedicamentoId = ctx.MedicamentoId, Serie = serie,
            Caducidad = caducidad, UnidadesIniciales = unidades, UnidadesRestantes = unidades
        });

    private static int CrearMaterial(Contexto ctx)
    {
        var material = new MaterialAcondicionamiento { Descripcion = "Blíster 7x4", Lote = "L1", FechaEntrada = new DateOnly(2026, 1, 1) };
        return ctx.RepositorioMaterial.Crear(material);
    }

    private static int CrearLecturaAmbiental(Contexto ctx)
        => ctx.RepositorioAmbiental.Crear(new RegistroAmbiental { Fecha = DateTime.UtcNow, Temperatura = 20, Humedad = 50, FueraRango = false });

    [Fact]
    public void CrearSesion_con_dos_blisteres_crea_dos_spd_con_validez_consecutiva_CA_600()
    {
        var ctx = Crear(nBlisteres: 2);
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 56, new DateOnly(2030, 1, 1));

        var spds = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId);

        Assert.Equal(2, spds.Count);
        Assert.NotEqual(spds[0].NumRegistro, spds[1].NumRegistro);
        Assert.Equal(spds[0].ValidezHasta.AddDays(1), spds[1].ValidezDesde);
        Assert.Equal(spds[0].ValidezDesde.AddDays(6), spds[0].ValidezHasta);
    }

    [Fact]
    public void CrearSesion_bloquea_si_hay_faltantes_en_el_listado_de_retirada_CA_601()
    {
        // Día de retirada = hoy, para que el paciente sí entre en la ventana del listado de
        // retirada (a diferencia del resto de tests, que usan DiaRetiradaFueraDeVentana()).
        var diaRetiradaHoy = DiasSemana.Codigos[((int)DateTime.Today.DayOfWeek + 6) % 7];
        var ctx = Crear(diaRetirada: diaRetiradaHoy);
        using var c = ctx.Conexion;
        // Sin envase alguno: el listado de retirada mostrará faltantes para este paciente.

        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId));
        Assert.Empty(ctx.RepositorioSpd.Listar(null, ctx.PacienteId, null));
    }

    [Fact]
    public void CrearSesion_respeta_el_comprobador_de_idoneidad_y_consentimiento()
    {
        var ctx = Crear(comprobador: new ComprobadorSiempreRechaza());
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 28, new DateOnly(2030, 1, 1));

        Assert.Throws<ErrorValidacionException>(() => ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId));
    }

    [Fact]
    public void PasarAPreparado_reparte_un_envase_insuficiente_en_dos_filas_CA_602()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "A", 3, new DateOnly(2030, 1, 1));
        CrearEnvase(ctx, "B", 28, new DateOnly(2030, 1, 1));
        var spd = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId).Single();
        ctx.Servicio.AsignarMaterial(spd.Id, CrearMaterial(ctx));
        var lecturaId = CrearLecturaAmbiental(ctx);

        ctx.Servicio.PasarAPreparado(spd.Id, lecturaId, ctx.ElaboradorId);

        var linea = ctx.RepositorioLineas.ListarPorSpd(spd.Id).Single();
        var filas = ctx.RepositorioLineaEnvases.ListarPorLinea(linea.Id);
        Assert.Equal(2, filas.Count);
        Assert.Equal(EstadoSpd.Preparado, ctx.RepositorioSpd.ObtenerPorId(spd.Id)!.Estado);
    }

    [Fact]
    public void PasarAPreparado_exige_lectura_ambiental_Art_I_3()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 28, new DateOnly(2030, 1, 1));
        var spd = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId).Single();
        ctx.Servicio.AsignarMaterial(spd.Id, CrearMaterial(ctx));

        ctx.Servicio.PasarAPreparado(spd.Id, CrearLecturaAmbiental(ctx), ctx.ElaboradorId);

        Assert.NotNull(ctx.RepositorioSpd.ObtenerPorId(spd.Id)!.RegistroAmbientalId);
    }

    [Fact]
    public void CrearSesion_y_PasarAPreparado_registran_en_auditoria_Art_VII_6()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 28, new DateOnly(2030, 1, 1));

        var spd = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId).Single();
        ctx.Servicio.AsignarMaterial(spd.Id, CrearMaterial(ctx));
        ctx.Servicio.PasarAPreparado(spd.Id, CrearLecturaAmbiental(ctx), ctx.ElaboradorId);

        var registros = ctx.Conexion.Query<string>("SELECT accion FROM Auditoria WHERE entidad = 'SPD'");
        Assert.Contains("CREAR_SESION_PREPARACION", registros);
        Assert.Contains("PASAR_A_PREPARADO", registros);
    }

    [Fact]
    public void PasarAPreparado_sin_saldo_indica_medicamento_y_unidades_que_faltan_CA_603()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 3, new DateOnly(2030, 1, 1));
        var spd = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId).Single();
        ctx.Servicio.AsignarMaterial(spd.Id, CrearMaterial(ctx));

        var excepcion = Assert.Throws<ErrorValidacionException>(
            () => ctx.Servicio.PasarAPreparado(spd.Id, CrearLecturaAmbiental(ctx), ctx.ElaboradorId));
        Assert.Contains("Paracetamol", excepcion.Message);
        Assert.Contains("4", excepcion.Message);
    }

    [Fact]
    public void RegistrarEnvaseDesdeLinea_permite_pasar_a_preparado_tras_el_alta_CA_604()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 3, new DateOnly(2030, 1, 1));
        var spd = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId).Single();
        ctx.Servicio.AsignarMaterial(spd.Id, CrearMaterial(ctx));
        var linea = ctx.RepositorioLineas.ListarPorSpd(spd.Id).Single();

        ctx.Servicio.RegistrarEnvaseDesdeLinea(
            linea.Id, new DatosAltaEnvase(ctx.PacienteId, ctx.MedicamentoId, "S2", "L1", new DateOnly(2030, 1, 1), 28, OrigenEnvase.Manual), ctx.ElaboradorId);

        ctx.Servicio.PasarAPreparado(spd.Id, CrearLecturaAmbiental(ctx), ctx.ElaboradorId);
        Assert.Equal(EstadoSpd.Preparado, ctx.RepositorioSpd.ObtenerPorId(spd.Id)!.Estado);
    }

    [Fact]
    public void Verificar_blister_1_no_cambia_el_estado_del_blister_2_CA_605()
    {
        var ctx = Crear(nBlisteres: 2);
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 56, new DateOnly(2030, 1, 1));
        var spds = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId);
        var materialId = CrearMaterial(ctx);
        foreach (var s in spds)
        {
            ctx.Servicio.AsignarMaterial(s.Id, materialId);
            ctx.Servicio.PasarAPreparado(s.Id, CrearLecturaAmbiental(ctx), ctx.ElaboradorId);
        }

        ctx.Servicio.Verificar(spds[0].Id, ctx.VerificadorId, new ChecklistVerificacion(true, true, true, true, true), null, ctx.VerificadorId);

        Assert.Equal(EstadoSpd.Verificado, ctx.RepositorioSpd.ObtenerPorId(spds[0].Id)!.Estado);
        Assert.Equal(EstadoSpd.Preparado, ctx.RepositorioSpd.ObtenerPorId(spds[1].Id)!.Estado);
    }

    [Fact]
    public void Verificar_exige_motivo_si_verificador_es_el_elaborador()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 28, new DateOnly(2030, 1, 1));
        var spd = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId).Single();
        ctx.Servicio.AsignarMaterial(spd.Id, CrearMaterial(ctx));
        ctx.Servicio.PasarAPreparado(spd.Id, CrearLecturaAmbiental(ctx), ctx.ElaboradorId);

        Assert.Throws<ErrorValidacionException>(() =>
            ctx.Servicio.Verificar(spd.Id, ctx.ElaboradorId, new ChecklistVerificacion(true, true, true, true, true), null, ctx.ElaboradorId));

        ctx.Servicio.Verificar(spd.Id, ctx.ElaboradorId, new ChecklistVerificacion(true, true, true, true, true), "Solo hay un usuario disponible hoy", ctx.ElaboradorId);
        Assert.Equal(EstadoSpd.Verificado, ctx.RepositorioSpd.ObtenerPorId(spd.Id)!.Estado);
    }

    [Fact]
    public void Verificar_sobre_un_spd_no_preparado_lanza()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 28, new DateOnly(2030, 1, 1));
        var spd = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId).Single();

        Assert.Throws<ErrorValidacionException>(() =>
            ctx.Servicio.Verificar(spd.Id, ctx.VerificadorId, new ChecklistVerificacion(true, true, true, true, true), null, ctx.VerificadorId));
    }

    private static SPD PrepararYVerificar(Contexto ctx, int spdId)
    {
        ctx.Servicio.AsignarMaterial(spdId, CrearMaterial(ctx));
        ctx.Servicio.PasarAPreparado(spdId, CrearLecturaAmbiental(ctx), ctx.ElaboradorId);
        return ctx.Servicio.Verificar(spdId, ctx.VerificadorId, new ChecklistVerificacion(true, true, true, true, true), null, ctx.VerificadorId);
    }

    [Fact]
    public void RegistrarEntrega_conjunta_pasa_ambos_a_entregado_con_los_mismos_datos_CA_606()
    {
        var ctx = Crear(nBlisteres: 2);
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 56, new DateOnly(2030, 1, 1));
        var spds = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId);
        foreach (var s in spds) PrepararYVerificar(ctx, s.Id);

        var datos = new DatosEntregaSpd(new DateOnly(2026, 9, 10), "El propio paciente", true, null, null, null, false, null);
        var entregados = ctx.Servicio.RegistrarEntrega(datos, spds.Select(s => s.Id).ToList(), ctx.ElaboradorId);

        Assert.All(entregados, s => Assert.Equal(EstadoSpd.Entregado, s.Estado));
        Assert.All(entregados, s => Assert.Equal("El propio paciente", s.EntregadoA));
    }

    [Fact]
    public void RegistrarEntrega_parcial_deja_el_otro_en_verificado_CA_607()
    {
        var ctx = Crear(nBlisteres: 2);
        using var c = ctx.Conexion;
        CrearEnvase(ctx, "S1", 56, new DateOnly(2030, 1, 1));
        var spds = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId);
        foreach (var s in spds) PrepararYVerificar(ctx, s.Id);

        var datos = new DatosEntregaSpd(new DateOnly(2026, 9, 10), "El propio paciente", true, null, null, null, false, null);
        ctx.Servicio.RegistrarEntrega(datos, [spds[0].Id], ctx.ElaboradorId);

        Assert.Equal(EstadoSpd.Entregado, ctx.RepositorioSpd.ObtenerPorId(spds[0].Id)!.Estado);
        Assert.Equal(EstadoSpd.Verificado, ctx.RepositorioSpd.ObtenerPorId(spds[1].Id)!.Estado);
    }

    /// <summary>Crea a mano un SPD ENTREGADO "de la semana pasada" (sin ejecutar una sesión real
    /// de principio a fin) para poder controlar exactamente el estado de custodia con el que
    /// arranca cada test de continuidad (US5).</summary>
    private static void CrearSesionAnteriorEntregadaManual(Contexto ctx)
    {
        var tratamiento = ctx.RepositorioTratamientos.ListarVigentesDePaciente(ctx.PacienteId).Single();
        var spd = new SPD
        {
            NumRegistro = "F-000001",
            CorrelativoNumRegistro = ctx.RepositorioSpd.ObtenerSiguienteCorrelativo(),
            PacienteId = ctx.PacienteId,
            SesionId = Guid.NewGuid(),
            ValidezDesde = DateOnly.FromDateTime(DateTime.Today).AddDays(-7),
            ValidezHasta = DateOnly.FromDateTime(DateTime.Today).AddDays(-1),
            ElaboradorId = ctx.ElaboradorId,
            Estado = EstadoSpd.Entregado,
            MaterialId = CrearMaterial(ctx)
        };
        spd.Id = ctx.RepositorioSpd.Crear(spd);
        ctx.RepositorioLineas.Crear(new SpdLinea
        {
            SpdId = spd.Id, TratamientoId = tratamiento.Id, MedicamentoId = ctx.MedicamentoId,
            SnapNombre = "Paracetamol 1g", SnapCn = "654321", SnapPautaD = tratamiento.PautaD,
            SnapDiasSemana = tratamiento.DiasSemana, UnidadesDosis = 7, UnidadesEnvase = 7
        });
    }

    private static Tratamiento CambiarPauta(Contexto ctx, FraccionDosis nuevaPauta)
    {
        var anterior = ctx.RepositorioTratamientos.ListarVigentesDePaciente(ctx.PacienteId).Single();
        anterior.Estado = EstadoTratamiento.Finalizado;
        anterior.FechaFin = DateOnly.FromDateTime(DateTime.Today);
        ctx.RepositorioTratamientos.Actualizar(anterior);

        var nuevo = new Tratamiento
        {
            PacienteId = ctx.PacienteId, MedicamentoId = ctx.MedicamentoId, EnSpd = true, PautaD = nuevaPauta,
            FechaInicio = DateOnly.FromDateTime(DateTime.Today), FechaPrescripcionInicial = anterior.FechaPrescripcionInicial
        };
        nuevo.Id = ctx.RepositorioTratamientos.Crear(nuevo);
        return nuevo;
    }

    [Fact]
    public void PrepararSiguiente_sin_cambios_solo_exige_lectura_ambiental_CA_608()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearSesionAnteriorEntregadaManual(ctx);
        CrearEnvase(ctx, "S1", 28, new DateOnly(2030, 1, 1));

        var nuevo = ctx.Servicio.PrepararSiguiente(ctx.PacienteId, ctx.ElaboradorId, out var resultado).Single();

        Assert.Empty(resultado.LineasModificadas);
        Assert.Empty(resultado.TratamientosNuevos);
        Assert.Empty(resultado.LineasEliminadas);
        Assert.Empty(resultado.LineasEnvasePendiente);

        ctx.Servicio.PasarAPreparado(nuevo.Id, CrearLecturaAmbiental(ctx), ctx.ElaboradorId);
        Assert.Equal(EstadoSpd.Preparado, ctx.RepositorioSpd.ObtenerPorId(nuevo.Id)!.Estado);
    }

    [Fact]
    public void PrepararSiguiente_con_linea_cubierta_por_dos_envases_resuelve_ambos_automaticamente_CA_608b()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearSesionAnteriorEntregadaManual(ctx);
        CrearEnvase(ctx, "A", 3, new DateOnly(2030, 1, 1));
        CrearEnvase(ctx, "B", 28, new DateOnly(2030, 1, 1));

        var nuevo = ctx.Servicio.PrepararSiguiente(ctx.PacienteId, ctx.ElaboradorId, out var resultado).Single();
        Assert.Empty(resultado.LineasEnvasePendiente);

        ctx.Servicio.PasarAPreparado(nuevo.Id, CrearLecturaAmbiental(ctx), ctx.ElaboradorId);

        var linea = ctx.RepositorioLineas.ListarPorSpd(nuevo.Id).Single();
        Assert.Equal(2, ctx.RepositorioLineaEnvases.ListarPorLinea(linea.Id).Count);
    }

    [Fact]
    public void PrepararSiguiente_con_posologia_modificada_marca_solo_esa_linea_CA_609()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearSesionAnteriorEntregadaManual(ctx);
        CrearEnvase(ctx, "S1", 28, new DateOnly(2030, 1, 1));
        CambiarPauta(ctx, FraccionDosis.Media);

        ctx.Servicio.PrepararSiguiente(ctx.PacienteId, ctx.ElaboradorId, out var resultado);

        Assert.Single(resultado.LineasModificadas);
        Assert.Empty(resultado.TratamientosNuevos);
        Assert.Empty(resultado.LineasEliminadas);
    }

    [Fact]
    public void PrepararSiguiente_con_envase_agotado_sin_sustituto_marca_envase_pendiente_solo_en_esa_linea_CA_610()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        CrearSesionAnteriorEntregadaManual(ctx);
        // Sin envase nuevo: el que cubría la línea se agotó sin sustituto en custodia.

        ctx.Servicio.PrepararSiguiente(ctx.PacienteId, ctx.ElaboradorId, out var resultado);

        Assert.Single(resultado.LineasEnvasePendiente);
        Assert.Equal(EstadoLinea.EnvasePendiente, resultado.LineasEnvasePendiente[0].EstadoLinea);
    }

    private static SPD CrearPrepararYVerificar(Contexto ctx, int unidadesEnvase = 28)
    {
        CrearEnvase(ctx, "S1", unidadesEnvase, new DateOnly(2030, 1, 1));
        var spd = ctx.Servicio.CrearSesion(ctx.PacienteId, ctx.ElaboradorId).Single();
        return PrepararYVerificar(ctx, spd.Id);
    }

    [Fact]
    public void Reelaborar_conserva_num_registro_y_sube_version_CA_6120()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var spd = CrearPrepararYVerificar(ctx);

        var reelaborado = ctx.Servicio.Reelaborar(spd.Id, OrigenSolicitudReelaboracion.Paciente, "Cambio pedido por el paciente", ctx.ElaboradorId);

        Assert.Equal(spd.NumRegistro, reelaborado.NumRegistro);
        Assert.Equal(2, reelaborado.Version);
        Assert.Equal(EstadoSpd.Preparado, reelaborado.Estado);
    }

    [Fact]
    public void Reelaborar_sin_cambios_no_mueve_el_envase_CA_6121()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var spd = CrearPrepararYVerificar(ctx); // consume 7 de 28 -> quedan 21

        ctx.Servicio.Reelaborar(spd.Id, OrigenSolicitudReelaboracion.Paciente, "Revisión sin cambios reales", ctx.ElaboradorId);

        var envase = ctx.RepositorioEnvases.ObtenerPorSerie("S1")!;
        Assert.Equal(21, envase.UnidadesRestantes);
    }

    [Fact]
    public void Reelaborar_con_linea_aumentada_consume_solo_la_diferencia_CA_6122()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var spd = CrearPrepararYVerificar(ctx); // Uno/día x7 = 7 uds consumidas; quedan 21
        CambiarPauta(ctx, FraccionDosis.UnoYMedio); // 1,5x7=10,5 -> floor+1 = 11

        ctx.Servicio.Reelaborar(spd.Id, OrigenSolicitudReelaboracion.Medico, "Aumento de dosis prescrito", ctx.ElaboradorId);

        var envase = ctx.RepositorioEnvases.ObtenerPorSerie("S1")!;
        Assert.Equal(17, envase.UnidadesRestantes); // 21 - (11-7) = 17, no 21-11=10
    }

    [Fact]
    public void Reelaborar_con_linea_eliminada_devuelve_las_unidades_CA_6123()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var repositorioMedicamentos = new RepositorioMedicamentos(ctx.Conexion);
        var segundoMedicamento = new Medicamento { Cn = "999999", Nombre = "Ibuprofeno 600", UnidadesEnvase = 28 };
        segundoMedicamento.Id = repositorioMedicamentos.Crear(segundoMedicamento);
        var segundoTratamiento = new Tratamiento
        {
            PacienteId = ctx.PacienteId, MedicamentoId = segundoMedicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno,
            FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        };
        segundoTratamiento.Id = ctx.RepositorioTratamientos.Crear(segundoTratamiento);
        ctx.RepositorioEnvases.Crear(new Envase
        {
            PacienteId = ctx.PacienteId, MedicamentoId = segundoMedicamento.Id, Serie = "S2",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28
        });

        var spd = CrearPrepararYVerificar(ctx); // consume 7 del primer medicamento y 7 del segundo

        segundoTratamiento.Estado = EstadoTratamiento.Finalizado;
        segundoTratamiento.FechaFin = DateOnly.FromDateTime(DateTime.Today);
        ctx.RepositorioTratamientos.Actualizar(segundoTratamiento);

        ctx.Servicio.Reelaborar(spd.Id, OrigenSolicitudReelaboracion.Medico, "Medicamento retirado por el médico", ctx.ElaboradorId);

        var envaseSegundo = ctx.RepositorioEnvases.ObtenerPorSerie("S2")!;
        Assert.Equal(28, envaseSegundo.UnidadesRestantes);
        var lineaEliminada = ctx.RepositorioLineas.ListarPorSpd(spd.Id).Single(l => l.MedicamentoId == segundoMedicamento.Id);
        Assert.Equal(EstadoLinea.Excluida, lineaEliminada.EstadoLinea);
    }

    [Fact]
    public void Reelaborar_deja_historial_integro_en_SPD_Modificacion_CA_6124()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var spd = CrearPrepararYVerificar(ctx);

        ctx.Servicio.Reelaborar(spd.Id, OrigenSolicitudReelaboracion.Familiar, "Un familiar pidió el cambio", ctx.ElaboradorId);

        var modificacion = new RepositorioSpdModificaciones(ctx.Conexion).ListarPorSpd(spd.Id).Single();
        Assert.Equal(1, modificacion.VersionAnterior);
        Assert.Equal(2, modificacion.VersionNueva);
        Assert.Equal(OrigenSolicitudReelaboracion.Familiar, modificacion.OrigenSolicitud);
        Assert.Contains("Paracetamol", modificacion.LineasSnapshotAnterior);
    }

    [Fact]
    public void Reelaborar_un_spd_entregado_no_esta_disponible_CA_6125()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var spd = CrearPrepararYVerificar(ctx);
        var datos = new DatosEntregaSpd(DateOnly.FromDateTime(DateTime.Today), "El propio paciente", true, null, null, null, false, null);
        ctx.Servicio.RegistrarEntrega(datos, [spd.Id], ctx.ElaboradorId);

        Assert.Throws<ErrorValidacionException>(() =>
            ctx.Servicio.Reelaborar(spd.Id, OrigenSolicitudReelaboracion.Paciente, "Intento tras la entrega", ctx.ElaboradorId));
    }

    [Fact]
    public void Reelaborar_exige_nueva_verificacion_CA_6126()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var spd = CrearPrepararYVerificar(ctx);
        Assert.Equal(EstadoSpd.Verificado, spd.Estado);

        var reelaborado = ctx.Servicio.Reelaborar(spd.Id, OrigenSolicitudReelaboracion.Paciente, "Cambio antes de recoger", ctx.ElaboradorId);

        Assert.Equal(EstadoSpd.Preparado, reelaborado.Estado);
        Assert.Null(reelaborado.ResultadoVerificacion);
        Assert.Null(reelaborado.VerificadorId);
    }

    [Fact]
    public void ListarPorFiltro_filtra_por_estado_y_paciente_FR_690()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var spd = CrearPrepararYVerificar(ctx);

        var verificados = ctx.Servicio.ListarPorFiltro(new FiltrosPreparaciones(Estado: EstadoSpd.Verificado, PacienteId: ctx.PacienteId));
        var entregados = ctx.Servicio.ListarPorFiltro(new FiltrosPreparaciones(Estado: EstadoSpd.Entregado, PacienteId: ctx.PacienteId));

        Assert.Single(verificados, s => s.Id == spd.Id);
        Assert.Empty(entregados);
    }

    [Fact]
    public void RegistrarImpresion_actualiza_el_timestamp_y_audita_sin_generar_fichero_FR_680_681()
    {
        var ctx = Crear();
        using var c = ctx.Conexion;
        var spd = CrearPrepararYVerificar(ctx);

        ctx.Servicio.RegistrarImpresion(spd.Id, TipoDocumentoSpd.Ficha, ctx.ElaboradorId);

        var actualizado = ctx.RepositorioSpd.ObtenerPorId(spd.Id)!;
        Assert.NotNull(actualizado.ImpresoFichaEn);
        Assert.Null(actualizado.ImpresoEtiquetasEn);

        var registros = ctx.Conexion.Query<string>("SELECT accion FROM Auditoria WHERE entidad = 'SPD' AND accion = 'IMPRIMIR'");
        Assert.Single(registros);
    }
}
