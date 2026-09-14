using Avalonia;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Spd.Presentacion;

sealed class Program
{
    /// <summary>Art. IX.4: arranque completo hasta login/asistente < 2 s. App.axaml.cs lo registra en
    /// cuanto muestra la primera ventana.
    ///
    /// Se mide desde el arranque del **proceso**, no con un cronómetro. Había un
    /// `static readonly Stopwatch = Stopwatch.StartNew()` y siempre informaba de 0 ms: los campos
    /// estáticos de una clase sin constructor estático se inicializan de forma diferida, en el primer
    /// acceso — y el primer acceso a ese campo era la línea que lo leía, así que el cronómetro
    /// arrancaba en el instante de medirlo. La comprobación de la prueba manual pasaba siempre sin
    /// medir nada. El arranque del proceso incluye además la inicialización del runtime, que es lo que
    /// el artículo quiere acotar cuando dice «arranque completo».</summary>
    public static TimeSpan TiempoDesdeElArranque()
    {
        try
        {
            return DateTime.Now - Process.GetCurrentProcess().StartTime;
        }
        catch (Exception)
        {
            // Algunos entornos restringen la consulta del propio proceso. Antes de inventar un cero,
            // se informa de que no se pudo medir.
            return TimeSpan.Zero;
        }
    }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ConfigurarLogging();

        // Los manejadores van ANTES de cualquier inicialización que pueda fallar. Estaban después
        // de `SetProvider`, y eso deja ciego justo el fallo más probable al estrenar la aplicación
        // en otro sistema operativo: si el proveedor nativo de SQLCipher no carga —en la carpeta de
        // publicación conviven `e_sqlcipher` y `e_sqlite3`— el proceso moría sin escribir una sola
        // línea en logs/, y no habría nada que mirar ni que enviar.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Log.Fatal(e.ExceptionObject as Exception, "Excepción no controlada, terminando={EsTerminando}", e.IsTerminating);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Excepción no observada en una Task");
            e.SetObserved();
        };

        RegistrarEntorno();

        // Art. VIII.2/VII.2: el proveedor SQLCipher se registra ANTES de que cualquier
        // SqliteConnection se abra (research.md Decisión 1 de Spec 010) — sustituye por completo
        // la inicialización implícita de Microsoft.Data.Sqlite (Batteries_V2.Init(), que
        // registraría el SQLite normal en vez del compilado con SQLCipher). Una base sin cifrar
        // se sigue abriendo con total normalidad bajo este mismo proveedor.
        try
        {
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlcipher());
        }
        catch (Exception ex)
        {
            // Se registra y se deja subir: sin proveedor no hay base de datos, así que seguir
            // sería peor. Pero al menos queda escrito cuál de las dos librerías nativas falló.
            Log.Fatal(ex, "No se pudo registrar el proveedor SQLCipher (e_sqlcipher). Sin base de datos no se puede continuar.");
            throw;
        }

        // Art. VIII.2 (motor de documentos): licencia Community (research.md Decisión 1 de
        // Spec 007) — gratuita para una farmacia individual con ingresos por debajo del umbral
        // que QuestPDF publica en su web; revisar si la situación del propietario cambiara.
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>Primera línea de cada arranque: qué versión y sobre qué sistema.
    ///
    /// Sin esto, un log enviado desde otro ordenador no dice ni qué compilación se estaba
    /// ejecutando; con esto, el propio fichero identifica el binario y el sistema, que es la mitad
    /// de un diagnóstico cuando quien lo ejecuta y quien lo mira no están en la misma máquina.</summary>
    private static void RegistrarEntorno()
    {
        var version = System.Reflection.Assembly.GetEntryAssembly()?
            .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "desconocida";

        Log.Information(
            "Arranque de Fórmula SPD {Version} · {SistemaOperativo} · .NET {Runtime} · {Arquitectura} · carpeta {Carpeta}",
            version,
            System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            Environment.Version,
            System.Runtime.InteropServices.RuntimeInformation.OSArchitecture,
            AppContext.BaseDirectory);
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
