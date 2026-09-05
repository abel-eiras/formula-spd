using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Asistente;

public partial class PasoRutasView : UserControl
{
    public PasoRutasView() => InitializeComponent();

    // FR-030/031: elegir la carpeta con un explorador nativo en vez de escribir/pegar la ruta a mano.
    private async void ExplorarRutaBackup_Click(object? sender, RoutedEventArgs e)
    {
        var carpeta = await ElegirCarpetaAsync("Selecciona la carpeta de copias de seguridad");
        if (carpeta is not null && DataContext is AsistentePrimerArranqueViewModel vm)
        {
            vm.RutaBackup = carpeta;
        }
    }

    private async void ExplorarRutaDocumentos_Click(object? sender, RoutedEventArgs e)
    {
        var carpeta = await ElegirCarpetaAsync("Selecciona la carpeta de documentos generados");
        if (carpeta is not null && DataContext is AsistentePrimerArranqueViewModel vm)
        {
            vm.RutaDocumentosGenerados = carpeta;
        }
    }

    private async System.Threading.Tasks.Task<string?> ElegirCarpetaAsync(string titulo)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        var carpetas = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = titulo,
            AllowMultiple = false
        });

        return carpetas.Count > 0 ? carpetas[0].Path.LocalPath : null;
    }
}
