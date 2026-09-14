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

/// <summary>Spec 004 FR-402, revisado el 2026-09-14: la dosis de cada toma se teclea, con el
/// vocabulario cerrado de catorce valores. Antes era una lista que enseñaba los nombres internos del
/// programa («UnoYMedio»).</summary>
public sealed class PautasTecleadasTests
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
        servicios.Medicamentos.Crear(new DatosAltaMedicamento("654321", "Enalapril 20 mg"), null);

        var vm = new TratamientoViewModel(servicios.Tratamientos, servicios.Medicamentos, servicios.Comunicaciones,
            servicios.Documentos, servicios.Medicos, paciente.Id, usuarioActualId: null);
        return (vm, conexion);
    }

    private static void ElegirMedicamento(TratamientoViewModel vm)
    {
        vm.SelectorMedicamento.Texto = "654321";
        vm.SelectorMedicamento.ElegirCommand.Execute(Assert.Single(vm.SelectorMedicamento.Resultados));
    }

    [AvaloniaFact]
    public void Se_teclea_la_pauta_con_mas_y_se_guarda_el_valor_exacto()
    {
        var (vm, conexion) = Montar();
        using var c = conexion;
        var ventana = AnfitrionDeVista.Anfitrion(new TratamientoView { DataContext = vm });
        ventana.Show();

        ElegirMedicamento(vm);
        vm.PautaD = "1/2";
        vm.PautaC = "1 + 1/2";   // con espacios: se tolera la forma, no el valor
        vm.PautaN = "3";
        Assert.False(vm.HayPautaErronea);

        vm.GuardarCommand.Execute(null);

        var tratamiento = Assert.Single(vm.Vigentes).Tratamiento;
        Assert.Equal(FraccionDosis.Media, tratamiento.PautaD);
        Assert.Null(tratamiento.PautaA);                    // vacío = sin toma, no «0»
        Assert.Equal(FraccionDosis.UnoYMedio, tratamiento.PautaC);
        Assert.Equal(FraccionDosis.Tres, tratamiento.PautaN);
    }

    /// <summary>FR-402 y CA-710: nunca un decimal libre. Se avisa mientras se teclea y no se guarda, con
    /// un mensaje que dice qué toma, qué se tecleó y qué valores valen.</summary>
    [AvaloniaFact]
    public void Un_decimal_se_avisa_al_teclear_y_no_se_guarda()
    {
        var (vm, conexion) = Montar();
        using var c = conexion;

        ElegirMedicamento(vm);
        vm.PautaA = "0,5";

        Assert.True(vm.HayPautaErronea);
        vm.GuardarCommand.Execute(null);

        Assert.Empty(vm.Vigentes);
        Assert.Contains("almuerzo", vm.Mensaje);
        Assert.Contains("0,5", vm.Mensaje);
        Assert.Contains("1+1/2", vm.Mensaje);
    }

    [AvaloniaFact]
    public void Al_cambiar_la_pauta_se_carga_con_la_notacion_que_se_teclea()
    {
        var (vm, conexion) = Montar();
        using var c = conexion;
        ElegirMedicamento(vm);
        vm.PautaD = "1+1/4";
        vm.GuardarCommand.Execute(null);

        vm.PrepararCambioDePautaCommand.Execute(vm.Vigentes.Single());

        Assert.Equal("1+1/4", vm.PautaD);
        Assert.Null(vm.PautaA);
    }
}
