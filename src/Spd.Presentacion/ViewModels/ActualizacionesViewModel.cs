using System.Reflection;
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
    /// <summary>Se lee del ensamblado en vez de estar escrita a mano. Clavada en el código, un
    /// binario recién actualizado seguía diciendo que era la 0.1.0 y volvía a ofrecer la misma
    /// actualización una y otra vez; ahora la versión que se compara es la que realmente se ejecuta.
    /// La declara `<Version>` en el .csproj.</summary>
    private static string VersionInstalada =>
        System.Reflection.Assembly.GetEntryAssembly()?
            .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0]
        ?? System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3)
        ?? "0.0.0";

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
