using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Views.Pacientes;
using Spd.Presentacion.ViewModels;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Regresión F5 de `/speckit-analyze` (Spec 001): mismo patrón de `ListBox`+`ItemTemplate`
/// con comando de ancestro que causó el SIGABRT de Spec 000; se fuerza la realización con un
/// envase real en custodia.</summary>
public sealed class DepositoViewTests
{
    [AvaloniaFact]
    public void DepositoView_se_construye_y_muestra_con_un_envase_real_sin_lanzar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();

        var repositorioFarmacia = new RepositorioFarmacia(conexion);
        repositorioFarmacia.Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra",
            Telefono = "986000000", PrefijoNumFicha = "F-"
        });
        var auditoria = new RegistradorAuditoria(conexion);
        var repositorioPacientes = new RepositorioPacientes(conexion);
        var servicioPacientes = new ServicioPacientes(repositorioPacientes, repositorioFarmacia, auditoria);
        var paciente = servicioPacientes.Crear(
            new DatosAltaPaciente("José", "Núñez", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, null),
            usuarioQueEjecutaId: null);

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
        repositorioEnvases.Crear(new Envase
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, Serie = "S1",
            Caducidad = new DateOnly(2030, 1, 1), UnidadesIniciales = 28, UnidadesRestantes = 28
        });

        var servicioEnvases = new ServicioEnvases(repositorioEnvases, repositorioTratamientos, repositorioPacientes, auditoria, new RepositorioMedicamentos(conexion));
        var servicioMedicamentos = new ServicioMedicamentos(repositorioMedicamentos, auditoria);
        var servicioTratamientosParaImportacion = new ServicioTratamientos(repositorioTratamientos, repositorioPacientes, auditoria);
        var servicioImportacion = new ServicioImportacionTratamientoEnvase(
            repositorioMedicamentos, repositorioTratamientos, repositorioEnvases,
            new RepositorioPerfilesImportacionTratamiento(conexion), servicioTratamientosParaImportacion, auditoria);
        var ventana = AnfitrionDeVista.Anfitrion(new DepositoView
        {
            DataContext = new DepositoViewModel(servicioEnvases, servicioMedicamentos, servicioImportacion, paciente.Id, usuarioActualId: null)
        });

        var vista = (DepositoView)ventana.Content!;
        var vm = (DepositoViewModel)vista.DataContext!;
        var primera = vm.EnCustodia.Single();
        for (var i = 2; i <= 12; i++) vm.EnCustodia.Add(primera with { Id = i, NombreMedicamento = $"Medicamento {i}" });
        ventana.Height = 500;
        ventana.Show();
        AnfitrionDeVista.ComprobarScrollHastaElUltimo(ventana, vista.FindControl<ListBox>("ListaCustodia")!);

        vm.PrepararOtraCajaCommand.Execute(primera);
        Assert.Equal(medicamento.Cn, vm.Cn);
        Assert.Equal(28, vm.UnidadesIniciales);
        vm.Serie = "NUEVA-1"; vm.Lote = "LOTE-1"; vm.Caducidad = new DateTimeOffset(2030, 1, 31, 0, 0, 0, TimeSpan.Zero);
        vm.RegistrarEnvaseCommand.Execute(null);
        Assert.Equal(medicamento.Cn, vm.Cn);
        Assert.Equal(28, vm.UnidadesIniciales);
        Assert.Equal(string.Empty, vm.Serie); Assert.Null(vm.Lote); Assert.Null(vm.Caducidad);
        vm.Serie = "NUEVA-2"; vm.Lote = "LOTE-2"; vm.Caducidad = new DateTimeOffset(2031, 2, 28, 0, 0, 0, TimeSpan.Zero);
        vm.RegistrarEnvaseCommand.Execute(null);
        Assert.Equal(3, vm.EnCustodia.Count);
        Assert.Equal("LOTE-1", vm.EnCustodia.Single(e => e.Envase.Serie == "NUEVA-1").Envase.Lote);
        Assert.Equal(new DateOnly(2031, 2, 28), vm.EnCustodia.Single(e => e.Envase.Serie == "NUEVA-2").Envase.Caducidad);
        ventana.Close();
    }
}
