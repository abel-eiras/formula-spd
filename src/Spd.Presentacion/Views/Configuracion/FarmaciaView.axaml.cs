using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Spd.Presentacion.ViewModels;

namespace Spd.Presentacion.Views.Configuracion;

public partial class FarmaciaView : UserControl
{
    public FarmaciaView() => InitializeComponent();

    // FR-030/031: elegir la carpeta con un explorador nativo en vez de escribir/pegar la ruta a mano.
    private async void ExplorarRutaBackup_Click(object? sender, RoutedEventArgs e)
    {
        var carpeta = await ElegirCarpetaAsync("Selecciona la carpeta de copias de seguridad");
        if (carpeta is not null && DataContext is FarmaciaViewModel vm)
        {
            vm.RutaBackup = carpeta;
        }
    }

    private async void ExplorarRutaDocumentos_Click(object? sender, RoutedEventArgs e)
    {
        var carpeta = await ElegirCarpetaAsync("Selecciona la carpeta de documentos generados");
        if (carpeta is not null && DataContext is FarmaciaViewModel vm)
        {
            vm.RutaDocumentosGenerados = carpeta;
        }
    }

    // FR-011: elegir el fichero de imagen del logo con un explorador nativo.
    private async void ExplorarLogo_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var ficheros = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Selecciona la imagen del logo",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new("Imágenes") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp"] }
            }
        });

        if (ficheros.Count > 0 && DataContext is FarmaciaViewModel vm)
        {
            vm.RutaNuevoLogo = ficheros[0].Path.LocalPath;
        }
    }

    private async Task<string?> ElegirCarpetaAsync(string titulo)
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
