using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Preparacion;
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
    private static PreparacionesViewModel Montar(SqliteConnection conexion)
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

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Paracetamol 1g", UnidadesEnvase = 28 };
        medicamento.Id = repositorioMedicamentos.Crear(medicamento);
        var repositorioTratamientos = new RepositorioTratamientos(conexion);
        repositorioTratamientos.Crear(new Tratamiento { PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno, FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1) });
        repositorioTratamientos.Crear(new Tratamiento { PacienteId = segundo.Id, MedicamentoId = medicamento.Id, EnSpd = true, PautaD = FraccionDosis.Uno, FechaInicio = new DateOnly(2026, 1, 1), FechaPrescripcionInicial = new DateOnly(2026, 1, 1) });
        var repositorioEnvases = new RepositorioEnvases(conexion);
        repositorioEnvases.Crear(new Envase { PacienteId = paciente.Id, MedicamentoId = medicamento.Id, Serie = "S1", Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28 });
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

        return new PreparacionesViewModel(
            servicioPreparacion, servicioPacientes, servicioUsuarios, new ServicioMedicamentos(repositorioMedicamentos, auditoria),
            documentos, comunicaciones, lote, new Spd.Presentacion.Navegacion.Navegador(esAdministrador: true),
            usuarioActualId: elaborador.Id);
    }

    [AvaloniaFact]
    public void La_tabla_de_preparaciones_se_construye_y_muestra_con_spd_reales_sin_lanzar()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var ventana = AnfitrionDeVista.Anfitrion(new PreparacionesView { DataContext = Montar(conexion) });

        ventana.Show();
    }

    /// <summary>Spec 015 CA-1512. Al pasar de lista a tabla ordenable aparece un riesgo nuevo: que
    /// la selección para el lote fuese por posición y ordenar por otra columna marcase filas
    /// distintas de las que el usuario eligió. Vive en cada fila, así que reordenar no la toca.</summary>
    [AvaloniaFact]
    public void Reordenar_la_tabla_conserva_la_seleccion_para_el_lote_CA_1512()
    {
        using var conexion = new SqliteConnection("Data Source=:memory:");
        var vm = Montar(conexion);
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
}
