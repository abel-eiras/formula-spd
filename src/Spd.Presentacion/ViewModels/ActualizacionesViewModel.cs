using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Configuración / Actualizaciones (FR-050). Nunca se comprueba automáticamente al
/// arrancar (Art. VI.3): solo el botón "Comprobar actualizaciones" la invoca.</summary>
public sealed partial class ActualizacionesViewModel(IServicioActualizaciones servicio, int? administradorActualId)
    : ViewModelBase
{
    // Sin esquema formal de versión todavía (fuera de alcance de esta spec); placeholder explícito.
    private const string VersionInstalada = "0.1.0";

    [ObservableProperty] private string? _mensaje;
    [ObservableProperty] private bool _comprobando;

    [RelayCommand]
    private async Task ComprobarAsync()
    {
        Comprobando = true;
        try
        {
            var resultado = await servicio.ComprobarActualizacionesAsync(VersionInstalada, administradorActualId);
            Mensaje = Describir(resultado);
        }
        finally
        {
            Comprobando = false;
        }
    }

    private static string Describir(ResultadoComprobacionActualizacion resultado)
    {
        if (!resultado.Exito)
        {
            return $"No se pudo comprobar: {resultado.Motivo}";
        }
        return resultado.HayNueva
            ? $"Hay una versión nueva disponible: {resultado.VersionDisponible} — {resultado.UrlDescarga}"
            : "Ya tienes la última versión.";
    }
}
