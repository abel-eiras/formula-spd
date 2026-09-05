using System;
using Avalonia.Controls;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Asistente;

public partial class AsistentePrimerArranqueWindow : Window
{
    /// <summary>Se dispara cuando el asistente termina, para que App.axaml.cs abra la ventana principal.</summary>
    public event Action? AsistenteFinalizado;

    public AsistentePrimerArranqueWindow(IServicioAsistentePrimerArranque servicio)
    {
        InitializeComponent();

        var viewModel = new AsistentePrimerArranqueViewModel(servicio);
        viewModel.AsistenteFinalizado += () => AsistenteFinalizado?.Invoke();
        DataContext = viewModel;
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public AsistentePrimerArranqueWindow() => InitializeComponent();
}
