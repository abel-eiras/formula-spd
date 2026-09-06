using System;
using System.Linq;
using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views.Pacientes;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 004 FR-410 / CA-402: la línea temporal del historial de un medicamento.
///
/// El servicio existía y estaba probado desde hace semanas, pero no había pantalla. Y sin pantalla no
/// se podía responder a la pregunta que hace un médico cuando llama: **qué tomaba este paciente y
/// desde cuándo**. Como un cambio de pauta cierra el tratamiento anterior y abre otro (Art. III: nada
/// se reescribe), esa respuesta solo está en el historial.</summary>
public sealed class HistorialTratamientoViewTests
{
    private static (TratamientoViewModel Vm, SqliteConnection Conexion) Montar()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B0",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "9"
        });

        var servicios = FabricaServiciosTest.Todos(conexion);
        var paciente = servicios.Pacientes.Crear(
            new DatosAltaPaciente("Ana", "Ríos", null, "12345678Z", null, null, null, null, null, null,
                null, null, null, null, null, null, null, false, null, null, 1), null);

        var repositorioMedicamentos = new RepositorioMedicamentos(conexion);
        var medicamento = new Medicamento { Cn = "654321", Nombre = "Enalapril 20 mg", UnidadesEnvase = 28 };
        medicamento.Id = repositorioMedicamentos.Crear(medicamento);

        var tratamientos = new RepositorioTratamientos(conexion);
        // Tramo antiguo, ya cerrado: media pastilla en desayuno, solo de lunes a viernes.
        tratamientos.Crear(new Tratamiento
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true,
            PautaD = FraccionDosis.Media, DiasSemana = "1111100",
            FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 6, 30),
            FechaPrescripcionInicial = new DateOnly(2026, 1, 1),
            Estado = EstadoTratamiento.Suspendido,
            Intervencion = "El médico dobla la dosis."
        });
        // Tramo vigente: una entera, todos los días.
        tratamientos.Crear(new Tratamiento
        {
            PacienteId = paciente.Id, MedicamentoId = medicamento.Id, EnSpd = true,
            PautaD = FraccionDosis.Uno, DiasSemana = "1111111",
            FechaInicio = new DateOnly(2026, 7, 1),
            FechaPrescripcionInicial = new DateOnly(2026, 1, 1)
        });

        var vm = new TratamientoViewModel(
            servicios.Tratamientos, servicios.Medicamentos, servicios.Comunicaciones,
            servicios.Documentos, servicios.Medicos, paciente.Id, usuarioActualId: null);
        return (vm, conexion);
    }

    [AvaloniaFact]
    public void El_historial_muestra_los_tramos_del_mas_reciente_al_mas_antiguo_CA_402()
    {
        var (vm, conexion) = Montar();
        using var c = conexion;

        var ventana = AnfitrionDeVista.Anfitrion(new TratamientoView { DataContext = vm });
        ventana.Show();

        // Cerrado, no se muestra hasta que se pide.
        Assert.False(vm.HayHistorial);

        var vigente = Assert.Single(vm.Vigentes);
        vm.VerHistorialCommand.Execute(vigente);

        Assert.True(vm.HayHistorial);
        Assert.Equal("Enalapril 20 mg", vm.HistorialDe);
        Assert.Equal(2, vm.Historial.Count);

        // El más reciente primero, y marcado como el que está en vigor.
        var actual = vm.Historial[0];
        Assert.True(actual.EsVigente);
        Assert.Equal("desde 01/07/2026", actual.Periodo);
        Assert.Equal("1-0-0-0", actual.Pauta);
        Assert.Equal("todos los días", actual.Dias);

        // El antiguo conserva sus fechas, su pauta de entonces y por qué terminó.
        var anterior = vm.Historial[1];
        Assert.False(anterior.EsVigente);
        Assert.Equal("01/01/2026 — 30/06/2026", anterior.Periodo);
        Assert.Equal("1/2-0-0-0", anterior.Pauta);
        Assert.Equal("LU MA MI JU VI", anterior.Dias);
        Assert.True(anterior.TieneMotivo);
        Assert.Contains("dobla la dosis", anterior.Motivo);

        vm.CerrarHistorialCommand.Execute(null);
        Assert.False(vm.HayHistorial);
        Assert.Null(vm.HistorialDe);
    }
}
