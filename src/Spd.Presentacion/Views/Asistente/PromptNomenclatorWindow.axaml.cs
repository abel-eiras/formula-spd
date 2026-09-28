using System;
using Avalonia.Controls;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Asistente;

/// <summary>Se ofrece una sola vez, justo tras el asistente de primer arranque (FR-000). Descargar
/// el nomenclátor no es automático ni obligatorio (Art. VI.3): se pregunta explícitamente y
/// "Continuar" está siempre disponible, se haya descargado o no (CA-005).</summary>
public partial class PromptNomenclatorWindow : Window
{
    public event Action? Continuado;

    public PromptNomenclatorWindow(NomenclatorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        this.FindControl<Button>("BotonContinuar")!.Click += (_, _) => Continuado?.Invoke();
    }

    // Constructor sin parámetros exigido por el compilador de XAML del previsualizador.
    public PromptNomenclatorWindow() => InitializeComponent();
}
