using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views;
using Spd.Presentacion.ViewModels;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión F5 de `/speckit-analyze` (Spec 001): mismo patrón de `ListBox`+`ItemTemplate`
/// con comando de ancestro que causó el SIGABRT de Spec 000; se fuerza la realización con una
/// fila real del listado de retirada.</summary>
public sealed class RetiradaEnvasesViewTests
{
    [AvaloniaFact]
    public void RetiradaEnvasesWindow_se_construye_y_muestra_con_una_fila_real_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000"
        });
        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var diaRetiradaHoy = DiasSemana.Codigos[((int)DateTime.Today.DayOfWeek + 6) % 7]; // LU=0..DO=6
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, diaRetiradaHoy, 1),
            usuarioQueEjecutaId: null);
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
        var servicioEnvases = new ServicioEnvases(repositorioEnvases, repositorioTratamientos, repositorioPacientes, auditoria);
        var servicioListadoRetirada = new ServicioListadoRetirada(
            repositorioPacientes, new RepositorioContactos(conexion), repositorioTratamientos, repositorioMedicamentos,
            repositorioEnvases, repositorioFarmacia, new ComprobadorCoberturaSpdNulo(), auditoria);

        var servicioMedicamentos = new ServicioMedicamentos(repositorioMedicamentos, auditoria);
        var vm = new RetiradaEnvasesViewModel(servicioListadoRetirada, servicioEnvases, servicioMedicamentos, usuarioActualId: null);
        var ventana = AnfitrionDeVista.Anfitrion(new RetiradaEnvasesView { DataContext = vm });

        ventana.Show();

        // Spec 005 FR-513: al abrir el registro, las unidades vienen del catálogo.
        vm.PrepararRegistroCommand.Execute(Assert.Single(vm.Filas));
        Assert.Equal(28, vm.UnidadesIniciales);
        Assert.False(vm.Unidades.OfrecerGuardar);

        // El lector USB teclea la cadena y un Intro sobre el campo de escaneo; después el foco pasa a
        // Unidades y otro Intro guarda. Se prueba con teclas de verdad, no llamando a los comandos.
        Dispatcher.UIThread.RunJobs();
        var campoEscaneo = ventana.GetVisualDescendants().OfType<TextBox>().Single(t => t.Watermark == "Escanear envase (spec-012)");
        campoEscaneo.Focus();
        vm.CodigoEscaneado = $"0108470006950647{Gs}17301231{Gs}10LOTE7{Gs}21SERIE7";
        ventana.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("SERIE7", vm.Serie);
        var unidades = ventana.GetVisualDescendants().OfType<NumericUpDown>().Single(n => n.Name == "CampoUnidades");
        Assert.True(unidades.IsKeyboardFocusWithin);

        ventana.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        Dispatcher.UIThread.RunJobs();

        var envase = Assert.Single(servicioEnvases.ListarEnCustodiaDePaciente(paciente.Id));
        Assert.Equal(28, envase.UnidadesIniciales);
        Assert.Equal(OrigenEnvase.Escaneado, envase.Origen);
        Assert.Equal("LOTE7", envase.Lote);
    }

    private const char Gs = '\u001D';
}
