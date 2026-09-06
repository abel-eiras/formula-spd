using System.Linq;
using Avalonia.Headless.XUnit;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;
using Spd.Infraestructura.Migraciones;
using Spd.Presentacion.Navegacion;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 015 CA-1510/1511: el panel de inicio muestra los cuatro indicadores y cada aviso
/// lleva a donde se resuelve.</summary>
public sealed class InicioViewTests
{
    private static (InicioViewModel Vm, Navegador Navegador, SqliteConnection Conexion) Crear()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        new AplicadorMigraciones(conexion).Aplicar();
        new RepositorioFarmacia(conexion).Crear(new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "Farmacia de Prueba", TitularOComunidadBienes = "Titular",
            Cif = "B00000000", Direccion = "Calle Falsa 1", Cp = "36000", Poblacion = "Pontevedra", Telefono = "986000000"
        });

        var servicios = FabricaServiciosTest.Todos(conexion);
        var navegador = new Navegador(esAdministrador: true);
        return (new InicioViewModel(servicios.Avisos, navegador), navegador, conexion);
    }

    [AvaloniaFact]
    public void La_vista_de_inicio_se_realiza_con_indicadores_y_avisos_sin_lanzar()
    {
        var (vm, _, conexion) = Crear();
        using var c = conexion;

        // Una base recién creada no tiene lecturas ambientales: hay al menos ese aviso, con lo que
        // la plantilla de la lista se realiza de verdad (regresión F5) y no se prueba en vacío.
        Assert.Contains(vm.Avisos, a => a.Tipo == "Ambiental");
        Assert.True(vm.HayAvisos);

        var ventana = AnfitrionDeVista.Anfitrion(new InicioView { DataContext = vm });
        ventana.Show();
    }

    [AvaloniaFact]
    public void Sin_lectura_ambiental_el_indicador_lo_dice_y_el_aviso_lleva_a_calidad_CA_1511()
    {
        var (vm, navegador, conexion) = Crear();
        using var c = conexion;

        Assert.True(vm.AmbientalAtrasado);
        Assert.NotEqual("al día", vm.TextoAmbiental);

        var ambiental = vm.Avisos.Single(a => a.Tipo == "Ambiental");
        Assert.Equal(Seccion.Calidad, ambiental.Destino.Seccion);

        vm.IrAlAvisoCommand.Execute(ambiental);
        Assert.Equal(Seccion.Calidad, navegador.Actual.Seccion);
    }

    [AvaloniaFact]
    public void Las_acciones_rapidas_navegan_a_su_seccion()
    {
        var (vm, navegador, conexion) = Crear();
        using var c = conexion;

        vm.IrACommand.Execute("Preparaciones");
        Assert.Equal(Seccion.Preparaciones, navegador.Actual.Seccion);

        vm.IrACommand.Execute("Retirada");
        Assert.Equal(Seccion.Retirada, navegador.Actual.Seccion);
    }
}
