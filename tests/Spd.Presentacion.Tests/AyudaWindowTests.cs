using Avalonia.Headless.XUnit;
using Spd.Presentacion.Ayuda;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 014: la ayuda es ahora una sección del marco (Spec 015), no una ventana. Se prueba
/// el ViewModel y que la vista se realice con cualquier apartado.</summary>
public sealed class AyudaWindowTests
{
    [AvaloniaFact]
    public void La_ayuda_abre_en_el_indice_y_en_un_apartado_concreto_CA_1401()
    {
        var indice = new AyudaViewModel(IndiceAyuda.Global);
        Assert.Null(indice.EntradaSeleccionada);

        var verificacion = new AyudaViewModel(IndiceAyuda.Global);
        verificacion.Seleccionar("procedimiento", "verificacion");
        Assert.Equal("verificacion", verificacion.EntradaSeleccionada!.Id);

        var ventana = AnfitrionDeVista.Anfitrion(new AyudaView { DataContext = verificacion });
        ventana.Show();
    }

    [AvaloniaFact]
    public void Renderizar_todos_los_apartados_no_lanza()
    {
        foreach (var entrada in IndiceAyuda.Global.Todas)
            RenderizadorMarkdown.Renderizar(entrada.Contenido, IndiceAyuda.Global, (_, _) => { });
    }

    [AvaloniaFact]
    public void Buscar_desde_el_viewmodel_filtra_y_selecciona_el_primero_CA_1404()
    {
        var vm = new AyudaViewModel(IndiceAyuda.Global) { TextoBusqueda = "verificador" };
        Assert.True(vm.HayBusqueda);
        Assert.NotEmpty(vm.Resultados);
        Assert.Equal(vm.Resultados[0], vm.EntradaSeleccionada);
        vm.TextoBusqueda = "";
        Assert.False(vm.HayBusqueda);
    }
}
