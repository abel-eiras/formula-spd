using Avalonia.Headless.XUnit;
using Spd.Presentacion.Ayuda;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views;
using Xunit;

namespace Spd.Presentacion.Tests;

public sealed class AyudaWindowTests
{
    [AvaloniaFact]
    public void AyudaWindow_se_abre_en_el_indice_y_en_un_apartado_sin_lanzar_CA_1401()
    {
        var indice = new AyudaWindow(IndiceAyuda.Global, null, null);
        indice.Show();
        Assert.Null(((AyudaViewModel)indice.DataContext!).EntradaSeleccionada);

        var verificacion = new AyudaWindow(IndiceAyuda.Global, "procedimiento", "verificacion");
        verificacion.Show();
        Assert.Equal("verificacion", ((AyudaViewModel)verificacion.DataContext!).EntradaSeleccionada!.Id);
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
