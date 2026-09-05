using Avalonia;
using Serilog;
using System;
using System.IO;

namespace Spd.Presentacion;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ConfigurarLogging();
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // La carpeta de logs vive dentro de la carpeta de instalación (Art. VI.4): junto al ejecutable.
    private static void ConfigurarLogging()
    {
        var rutaLogs = Path.Combine(AppContext.BaseDirectory, "logs", "log-.txt");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(rutaLogs, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
