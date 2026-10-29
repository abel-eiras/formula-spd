using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Configuración / Actualizaciones (FR-050, FR-053). Nunca se comprueba ni se instala nada
/// automáticamente al arrancar (Art. VI.3): solo los botones «Comprobar» e «Instalar», y este último
/// después de una confirmación que explica lo que va a pasar.</summary>
/// <param name="reiniciar">Recibe el ejecutable nuevo y cierra la aplicación para reabrirla con él
/// (<see cref="ReinicioTrasActualizacion.Solicitar"/>); en los tests, un doble.</param>
public sealed partial class ActualizacionesViewModel(
    IServicioActualizaciones servicio,
    IServicioInstalacionActualizaciones instalacion,
    int? administradorActualId,
    Action<string> reiniciar) : ViewModelBase
{
    /// <summary>La versión que realmente se ejecuta, no una escrita a mano: clavada en el código, un binario
    /// recién actualizado seguía diciendo que era la anterior y volvía a ofrecer la misma actualización.</summary>
    private static string VersionInstalada => VersionAplicacion.Texto;

    private string? _urlRelease;
    private CancellationTokenSource? _cancelacion;

    public string VersionInstaladaTexto => $"Versión instalada: {VersionInstalada}";

    [ObservableProperty] private string? _mensaje;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(PuedeInstalar))]
    private bool _comprobando;

    /// <summary>La etiqueta que se ofrece instalar; <c>null</c> hasta que una comprobación encuentra una nueva.</summary>
    [ObservableProperty, NotifyPropertyChangedFor(nameof(PuedeInstalar), nameof(TextoBotonInstalar), nameof(TextoConfirmacion))]
    private string? _versionParaInstalar;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(PuedeInstalar))]
    private bool _confirmandoInstalacion;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(PuedeInstalar), nameof(PuedeComprobar))]
    private bool _instalando;

    [ObservableProperty] private int _progresoDescarga;

    public bool PuedeComprobar => !Instalando;

    public bool PuedeInstalar => VersionParaInstalar is not null && !Comprobando && !ConfirmandoInstalacion && !Instalando;

    public string TextoBotonInstalar => $"Instalar la versión {VersionParaInstalar}";

    public string TextoConfirmacion =>
        $"Se va a instalar la versión {VersionParaInstalar}. Primero se hará una copia de seguridad en la ruta "
        + "configurada; después se descargará el paquete y se comprobará que es auténtico y que ha llegado entero. "
        + "Si todo va bien, la aplicación se cerrará y se volverá a abrir sola con la versión nueva. "
        + "Los datos de la farmacia no se tocan. Si algo falla, se queda la versión actual tal como está.";

    [RelayCommand]
    private async Task ComprobarAsync()
    {
        Comprobando = true;
        try
        {
            var resultado = await servicio.ComprobarActualizacionesAsync(VersionInstalada, administradorActualId);
            Mensaje = Describir(resultado);
            _urlRelease = resultado.UrlDescarga;
            VersionParaInstalar = resultado is { Exito: true, HayNueva: true } ? resultado.VersionDisponible : null;
        }
        finally
        {
            Comprobando = false;
        }
    }

    [RelayCommand]
    private void PedirConfirmacion() => ConfirmandoInstalacion = true;

    [RelayCommand]
    private void CancelarConfirmacion() => ConfirmandoInstalacion = false;

    [RelayCommand]
    private async Task InstalarAsync()
    {
        if (VersionParaInstalar is null) return;
        ConfirmandoInstalacion = false;
        Instalando = true;
        ProgresoDescarga = 0;
        Mensaje = "Descargando y comprobando la versión nueva. No cierres la aplicación.";
        _cancelacion = new CancellationTokenSource();
        try
        {
            var progreso = new Progress<int>(porcentaje => ProgresoDescarga = porcentaje);
            var resultado = await instalacion.InstalarAsync(VersionParaInstalar, administradorActualId, progreso, _cancelacion.Token);
            if (resultado is { Exito: true, RutaEjecutable: { } ejecutable })
            {
                Mensaje = $"Versión {resultado.VersionInstalada} instalada. La aplicación se reinicia.";
                VersionParaInstalar = null;
                reiniciar(ejecutable);
                return;
            }
            Mensaje = resultado.SoloManual && _urlRelease is not null
                ? $"{resultado.Motivo} Descarga: {_urlRelease}"
                : resultado.Motivo;
        }
        finally
        {
            Instalando = false;
            _cancelacion.Dispose();
            _cancelacion = null;
        }
    }

    /// <summary>Solo tiene efecto mientras se descarga y verifica; la sustitución de ficheros, que dura un
    /// instante, ya no se interrumpe.</summary>
    [RelayCommand]
    private void CancelarInstalacion() => _cancelacion?.Cancel();

    private static string Describir(ResultadoComprobacionActualizacion resultado)
    {
        if (!resultado.Exito)
        {
            return $"No se pudo comprobar: {resultado.Motivo}";
        }
        return resultado.HayNueva
            ? $"Hay una versión nueva disponible: {resultado.VersionDisponible}. Puedes instalarla desde aquí."
            : "Ya tienes la última versión.";
    }
}
