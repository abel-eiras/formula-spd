using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Serilog;

namespace Spd.Presentacion;

/// <summary>Reinicio después de instalar una actualización (FR-053).
///
/// El ejecutable nuevo no se lanza desde la pantalla de Actualizaciones sino desde `Program.Main`, **después**
/// de que la aplicación haya terminado de cerrarse. Si se lanzara antes, el proceso nuevo abriría `spd.db`
/// —y aplicaría migraciones— mientras el viejo aún la tiene abierta y todavía está cerrando la sesión.</summary>
public static class ReinicioTrasActualizacion
{
    /// <summary>El ejecutable que hay que abrir al salir; <c>null</c> si no se ha instalado nada.</summary>
    public static string? EjecutableNuevo { get; private set; }

    /// <summary>La instalación acaba de hacer su propia copia de seguridad: la de cierre sería idéntica.</summary>
    public static bool Solicitado => EjecutableNuevo is not null;

    /// <summary>Anota el reinicio y cierra la ventana principal, que sigue su cierre ordenado de siempre.</summary>
    public static void Solicitar(string rutaEjecutable)
    {
        EjecutableNuevo = rutaEjecutable;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } ventana })
        {
            ventana.Close();
        }
    }

    /// <summary>Lo llama `Program.Main` cuando la aplicación ya se ha cerrado del todo.</summary>
    public static void LanzarSiSeSolicito()
    {
        if (EjecutableNuevo is null) return;
        try
        {
            Process.Start(new ProcessStartInfo(EjecutableNuevo)
            {
                WorkingDirectory = Path.GetDirectoryName(EjecutableNuevo)!,
                UseShellExecute = false
            });
            Log.Information("Reinicio tras actualizar: lanzado {Ejecutable}", EjecutableNuevo);
        }
        catch (Exception ex)
        {
            // La versión nueva ya está instalada; si no arranca sola, el usuario la abre como siempre.
            Log.Error(ex, "Reinicio tras actualizar: no se pudo lanzar {Ejecutable}", EjecutableNuevo);
        }
    }
}
