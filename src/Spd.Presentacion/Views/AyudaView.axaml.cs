using System.ComponentModel;
using Avalonia.Controls;
using Spd.Presentacion.Ayuda;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views;

public partial class AyudaView : UserControl
{
    public AyudaView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is not AyudaViewModel vm) return;
            vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(AyudaViewModel.EntradaSeleccionada)) Pintar(vm); };
            Pintar(vm);
        };
    }

    private void Pintar(AyudaViewModel vm)
    {
        var panel = this.FindControl<ContentControl>("PanelContenido")!;
        if (vm.EntradaSeleccionada is null)
        {
            panel.Content = RenderizadorMarkdown.Renderizar(
                "# Ayuda de SPD\n\nElige un apartado a la izquierda o busca por texto. **F1** en cualquier pantalla abre el apartado del procedimiento que le corresponde.\n\n" +
                "- **Procedimiento del servicio SPD**: el circuito completo, paso a paso, con lo que exige el PNT y lo que genera la aplicación.\n" +
                "- **Uso de la aplicación**: qué hace cada pantalla y cada botón.",
                vm.Indice, vm.Seleccionar);
            return;
        }
        var e = vm.EntradaSeleccionada;
        panel.Content = RenderizadorMarkdown.Renderizar($"# {e.Titulo}\n\n{e.Contenido}", vm.Indice, vm.Seleccionar);
    }
}
