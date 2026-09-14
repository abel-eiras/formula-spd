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
    /// <summary>La versión que realmente se ejecuta, no una escrita a mano: clavada en el código, un binario
    /// recién actualizado seguía diciendo que era la anterior y volvía a ofrecer la misma actualización.</summary>
    private static string VersionInstalada => VersionAplicacion.Texto;

    public string VersionInstaladaTexto => $"Versión instalada: {VersionInstalada}";

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
