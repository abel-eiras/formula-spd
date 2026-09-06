using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Spd.Presentacion.Ayuda;

namespace Spd.Presentacion.ViewModels;

/// <summary>Ventana de ayuda (Spec 014): dos secciones, búsqueda y apartado seleccionado. El
/// contenido lo pinta la vista con <see cref="RenderizadorMarkdown"/>.</summary>
public sealed partial class AyudaViewModel : ViewModelBase
{
    public IndiceAyuda Indice { get; }

    [ObservableProperty] private string? _textoBusqueda;
    [ObservableProperty] private ObservableCollection<EntradaAyuda> _procedimiento;
    [ObservableProperty] private ObservableCollection<EntradaAyuda> _uso;
    [ObservableProperty] private ObservableCollection<EntradaAyuda> _resultados = [];
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HayBusqueda))] private bool _buscando;
    [ObservableProperty] private EntradaAyuda? _entradaSeleccionada;

    public bool HayBusqueda => Buscando;

    public AyudaViewModel(IndiceAyuda indice)
    {
        Indice = indice;
        _procedimiento = new ObservableCollection<EntradaAyuda>(indice.Procedimiento);
        _uso = new ObservableCollection<EntradaAyuda>(indice.Uso);
    }

    public void Seleccionar(string? seccion, string? id)
    {
        if (seccion is null || id is null) { EntradaSeleccionada = null; return; }
        EntradaSeleccionada = Indice.Obtener(seccion, id) ?? EntradaSeleccionada;
    }

    partial void OnTextoBusquedaChanged(string? value)
    {
        Buscando = !string.IsNullOrWhiteSpace(value);
        Resultados = new ObservableCollection<EntradaAyuda>(Buscando ? Indice.Buscar(value) : []);
        if (Buscando && Resultados.Count > 0 && EntradaSeleccionada is null) EntradaSeleccionada = Resultados.First();
    }
}
