using Avalonia.Headless.XUnit;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Preparacion;
using Spd.Presentacion.Navegacion;
using Spd.Presentacion.ViewModels;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión F5: plantilla de fila con comando de ancestro y casilla de selección; se
/// fuerza la realización con SPD reales (Spec 006 FR-690, Spec 007 FR-720). Desde Spec 015 la lista
/// es un `DataGrid`, así que este test es además el que comprueba que la tabla se realiza.</summary>
public sealed class PreparacionesViewTests
{
    /// <summary>Deja dos pacientes con sesión abierta, que es el mínimo para poder hablar de
    /// ordenar y de selección múltiple.</summary>
    private sealed record Montaje(
        PreparacionesViewModel Vm, ServicioPreparacion Preparacion, ServicioMedicamentos Medicamentos,
        ServicioGeneracionDocumentos Documentos, ServicioComunicacionesMedico Comunicaciones, Navegador Navegador,
        int PacienteId, int PacienteSinSesionId, int MedicamentoId, int ElaboradorId);

    private static Montaje Montar(SqliteConnection conexion)
    {
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular", Cif = "B00000000",
            Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra", Telefono = "986000000", PrefijoNumSpd = "F-", DiasAntelacionListado = 0
        });
        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var diaLejano = DiasSemana.Codigos[((int)DateTime.Today.DayOfWeek + 6 + 3) % 7];
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, diaLejano, 1), null);
        servicioPacientes.CambiarEstado(paciente.Id, EstadoPaciente.Activo, null, null);
        var segundo = servicioPacientes.Crear(
            new DatosAltaPaciente("Ana", "Álvarez", null, "87654321X", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, diaLejano, 2), null);
        servicioPacientes.CambiarEstado(segundo.Id, EstadoPaciente.Activo, null, null);
        // Sin sesión abierta: el candidato para «Nueva preparación».
        var tercero = servicioPacientes.Crear(
            new DatosAltaPaciente("Luis", "Pérez", null, "11111111H", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, diaLejano, 1), null);
        servicioPacientes.CambiarEstado(tercero.Id, EstadoPaciente.Activo, null, null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g", UnidadesEnvase = 28 };
        medicamento.Id = repositorioMedicamentos.Crear(medicamento);
        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        repositorioTratamientos.Crear(new Tratamiento { PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno, FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1) });
        repositorioTratamientos.Crear(new Tratamiento { PacienteId = tercero.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno, FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1) });
        repositorioTratamientos.Crear(new Tratamiento { PacienteId = segundo.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno, FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1) });
        var repositorioEnvases = new RepositorioEnvases(conexion);
        repositorioEnvases.Crear(new Envase { PacienteId = paciente.Id, MedicamentoId = medicamento.Id, Serie = "S1", Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28 });
        repositorioEnvases.Crear(new Envase { PacienteId = tercero.Id, MedicamentoId = medicamento.Id, Serie = "S3", Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28 });
        repositorioEnvases.Crear(new Envase { PacienteId = segundo.Id, MedicamentoId = medicamento.Id, Serie = "S2", Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28 });

        var servicioEnvases = new ServicioEnvases(repositorioEnvases, repositorioTratamientos, repositorioPacientes, auditoria);
        var servicioListadoRetirada = new ServicioListadoRetirada(repositorioPacientes, new RepositorioContactos(conexion), repositorioTratamientos,
            repositorioMedicamentos, repositorioEnvases, repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);
        var repositorioSpd = new RepositorioSpd(conexion);
        var servicioPreparacion = new ServicioPreparacion(
            repositorioSpd, new RepositorioSpdLineas(conexion), new RepositorioSpdLineaEnvases(conexion), new RepositorioSpdVerificaciones(conexion),
            new RepositorioSpdModificaciones(conexion), new RepositorioRegistrosAmbientales(conexion), new RepositorioMaterialAcondicionamiento(conexion),
            repositorioPacientes, repositorioTratamientos, repositorioMedicamentos, repositorioEnvases, repositorioFarmacia,
            new ServicioAsignacionEnvases(repositorioEnvases, repositorioTratamientos, auditoria), servicioEnvases, servicioListadoRetirada,
            new ComprobadorIdoneidadYConsentimientoNulo(), auditoria);
        var repositorioUsuarios = new RepositorioUsuarios(conexion);
        var hasheador = new HasheadorArgon2id();
        var servicioUsuarios = new ServicioUsuarios(repositorioUsuarios, hasheador, auditoria);
        var elaborador = servicioUsuarios.CrearUsuario(new DatosAltaUsuario("Elena", "Ruiz", "elena", "contraseña-inicial", Rol.Elaborador, null, null), null).Usuario;
        servicioPreparacion.CrearSesion(paciente.Id, elaborador.Id);
        servicioPreparacion.CrearSesion(segundo.Id, elaborador.Id);

        var documentos = FabricaServiciosTest.GeneracionDocumentos(conexion);
        var comunicaciones = new ServicioComunicacionesMedico(new RepositorioComunicacionesMedico(conexion), repositorioPacientes, repositorioTratamientos, auditoria);
        var lote = new ServicioGeneracionLote(servicioPreparacion, documentos, repositorioSpd, repositorioPacientes);

        var servicioMedicamentos = new ServicioMedicamentos(repositorioMedicamentos, auditoria);
        var navegador = new Navegador(esAdministrador: true);
        var vm = new PreparacionesViewModel(
            servicioPreparacion, servicioPacientes, servicioUsuarios, servicioMedicamentos,
            documentos, comunicaciones, lote, navegador, usuarioActualId: elaborador.Id);
        return new Montaje(vm, servicioPreparacion, servicioMedicamentos, documentos, comunicaciones, navegador,
            paciente.Id, tercero.Id, medicamento.Id, elaborador.Id);
    }

    [AvaloniaFact]
    public void La_tabla_de_preparaciones_se_construye_y_muestra_con_spd_reales_sin_lanzar()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var ventana = AnfitrionDeVista.Anfitrion(new PreparacionesView { DataContext = Montar(conexion).Vm });

        ventana.Show();
    }

    /// <summary>Spec 015 CA-1512. Al pasar de lista a tabla ordenable aparece un riesgo nuevo: que
    /// la selección para el lote fuese por posición y ordenar por otra columna marcase filas
    /// distintas de las que el usuario eligió. Vive en cada fila, así que reordenar no la toca.</summary>
    [AvaloniaFact]
    public void Reordenar_la_tabla_conserva_la_seleccion_para_el_lote_CA_1512()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var vm = Montar(conexion).Vm;
        vm.SoloPendientes = false;
        vm.CargarCommand.Execute(null);

        Assert.True(vm.Filas.Count >= 2, "El montaje debe dejar al menos dos preparaciones.");
        var elegidas = vm.Filas.Take(2).ToList();
        foreach (var fila in elegidas) fila.Seleccionado = true;

        // Lo que hace la rejilla al ordenar por una cabecera: cambiar el orden de las filas.
        var invertidas = vm.Filas.Reverse().ToList();
        vm.Filas = new System.Collections.ObjectModel.ObservableCollection<PreparacionesViewModel.FilaPreparacion>(invertidas);

        var seleccionadasDespues = vm.Filas.Where(f => f.Seleccionado).ToList();
        Assert.Equal(elegidas.Count, seleccionadasDespues.Count);
        foreach (var fila in elegidas) Assert.Contains(fila, seleccionadasDespues);
    }

    /// <summary>Petición del propietario (2026-09-14): abrir una preparación desde la lista, buscando al
    /// paciente, sin pasar antes por su ficha.</summary>
    [AvaloniaFact]
    public void Nueva_preparacion_busca_al_paciente_abre_su_sesion_y_lleva_a_su_preparacion()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var m = Montar(conexion);
        var ventana = AnfitrionDeVista.Anfitrion(new PreparacionesView { DataContext = m.Vm });
        ventana.Show();

        m.Vm.AbrirNuevaPreparacionCommand.Execute(null);
        Assert.True(m.Vm.PanelNuevaAbierto);

        m.Vm.BusquedaPaciente = "p";
        Assert.Empty(m.Vm.Candidatos);             // una letra no busca
        m.Vm.BusquedaPaciente = "perez";           // sin tilde, como se teclea
        ventana.UpdateLayout();
        var candidato = Assert.Single(m.Vm.Candidatos);
        Assert.Equal("Pérez, Luis", candidato.Etiqueta);
        Assert.Null(candidato.Ultima);
        Assert.False(candidato.PuedePrepararSiguiente);

        m.Vm.CrearSesionParaCommand.Execute(candidato);

        Assert.Null(m.Vm.MensajeNueva);
        Assert.False(m.Vm.PanelNuevaAbierto);
        Assert.NotEmpty(m.Preparacion.ListarPorFiltro(new FiltrosPreparaciones(PacienteId: m.PacienteSinSesionId)));
        var destino = m.Navegador.Actual!;
        Assert.Equal(Seccion.Paciente, destino.Seccion);
        Assert.Equal(m.PacienteSinSesionId, destino.PacienteId);
        Assert.Equal("Preparacion", destino.Detalle);
    }

    [AvaloniaFact]
    public void Si_la_sesion_no_se_puede_abrir_lo_explica_en_el_panel_y_no_navega()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var m = Montar(conexion);

        m.Vm.AbrirNuevaPreparacionCommand.Execute(null);
        m.Vm.BusquedaPaciente = "nunez";
        var candidato = Assert.Single(m.Vm.Candidatos);
        Assert.NotNull(candidato.Ultima);           // ya tiene una sesión en borrador

        m.Vm.PrepararSiguienteParaCommand.Execute(candidato);   // no está entregada: no hay continuidad

        Assert.NotNull(m.Vm.MensajeNueva);
        Assert.True(m.Vm.PanelNuevaAbierto);
        Assert.Null(m.Navegador.Actual);
    }

    /// <summary>Art. I.3, enmienda 3.0.0: al preparar se avisa de los medicamentos sin aptitud confirmada y
    /// se confirman todos con un botón. Nunca bloquea, y nunca vuelca un «no apto».</summary>
    [AvaloniaFact]
    public void La_preparacion_avisa_de_la_aptitud_sin_confirmar_y_se_confirma_de_una_vez()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var m = Montar(conexion);

        var preparacion = new PreparacionViewModel(
            m.Preparacion, m.Medicamentos, m.Documentos, m.Comunicaciones, m.PacienteId, m.ElaboradorId);
        var ventana = AnfitrionDeVista.Anfitrion(new PreparacionView { DataContext = preparacion });
        ventana.Show();

        Assert.True(preparacion.HayAptitudSinConfirmar);
        Assert.Contains("Paracetamol 1g", preparacion.MedicamentosSinConfirmar);
        Assert.False(preparacion.HayNoAptos);

        preparacion.ConfirmarAptitudSpdCommand.Execute(null);

        Assert.False(preparacion.HayAptitudSinConfirmar);
        Assert.True(m.Medicamentos.ObtenerPorId(m.MedicamentoId)!.AptoSpd);
        Assert.Contains("1 medicamento", preparacion.Mensaje);

        // Un «no apto» explícito se enseña, pero no se ofrece confirmarlo.
        conexion.Execute("UPDATE Medicamento SET apto_spd = 0 WHERE id = @id", new { id = m.MedicamentoId });
        var otraVez = new PreparacionViewModel(
            m.Preparacion, m.Medicamentos, m.Documentos, m.Comunicaciones, m.PacienteId, m.ElaboradorId);
        Assert.False(otraVez.HayAptitudSinConfirmar);
        Assert.True(otraVez.HayNoAptos);
    }
}
