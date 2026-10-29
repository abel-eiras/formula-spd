namespace Spd.Infraestructura;

/// <summary>Coloca los ficheros de una versión nueva, ya verificada y extraída, sobre la carpeta de
/// instalación (FR-053), de forma que o se sustituye todo o se deja todo como estaba.
///
/// Solo se tocan los ficheros que vienen en el paquete: la base de datos, los logs, las copias y los
/// documentos no están en él y por tanto nunca se tocan (Art. III, Art. VI.4).
///
/// El ejecutable en marcha no se puede sobrescribir en Windows, pero sí **renombrar**: se aparta como
/// `FormulaSPD.exe.anterior` y el nuevo ocupa su sitio; el proceso actual sigue funcionando con el fichero
/// renombrado hasta que se cierra. El `.anterior` se conserva tras instalar: es la vuelta atrás manual si
/// la versión nueva no arrancara.</summary>
public static class SustitucionFicherosInstalacion
{
    public const string SufijoAnterior = ".anterior";

    /// <summary>Lanza <see cref="SustitucionFallidaException"/> si algo falla, después de haber deshecho lo
    /// ya hecho; su mensaje ya está escrito para el usuario.</summary>
    /// <param name="carpetaRespaldo">Donde se apartan los demás ficheros sustituidos mientras dura la
    /// operación, para poder deshacerla. Es temporal: se borra al terminar.</param>
    public static void Sustituir(string origen, string carpetaInstalacion, string nombreEjecutable, string carpetaRespaldo)
    {
        var hechos = new List<(string Destino, string? Anterior)>();
        try
        {
            foreach (var relativa in FicherosEnOrden(origen, nombreEjecutable))
            {
                var destino = Path.Combine(carpetaInstalacion, relativa);
                var anterior = Apartar(destino, relativa, nombreEjecutable, carpetaRespaldo);
                hechos.Add((destino, anterior));
                Directory.CreateDirectory(Path.GetDirectoryName(destino)!);
                File.Move(Path.Combine(origen, relativa), destino);
            }
            DarPermisoDeEjecucion(carpetaInstalacion, hechos.Select(h => h.Destino), nombreEjecutable);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var restaurado = Deshacer(hechos);
            throw new SustitucionFallidaException(restaurado
                ? $"No se pudieron sustituir los ficheros del programa ({ex.Message}). Se ha dejado la versión anterior tal como estaba."
                : $"No se pudieron sustituir los ficheros del programa ({ex.Message}) y tampoco restaurar todos. "
                  + $"Si la aplicación no arranca, renombra {nombreEjecutable}{SufijoAnterior} a {nombreEjecutable}.", ex);
        }
    }

    /// <summary>El ejecutable primero: es lo que más importa deshacer si algo falla después, y así el
    /// orden no depende de cómo enumere los ficheros cada sistema.</summary>
    private static IEnumerable<string> FicherosEnOrden(string origen, string nombreEjecutable)
        => Directory.EnumerateFiles(origen, "*", SearchOption.AllDirectories)
            .Select(fichero => Path.GetRelativePath(origen, fichero))
            .OrderByDescending(relativa => relativa == nombreEjecutable)
            .ThenBy(relativa => relativa, StringComparer.Ordinal)
            .ToList();

    /// <summary>Quita de en medio el fichero actual, si lo hay, y devuelve dónde quedó.</summary>
    private static string? Apartar(string destino, string relativa, string nombreEjecutable, string carpetaRespaldo)
    {
        if (!File.Exists(destino)) return null;

        var anterior = relativa == nombreEjecutable
            ? destino + SufijoAnterior
            : Path.Combine(carpetaRespaldo, relativa);
        Directory.CreateDirectory(Path.GetDirectoryName(anterior)!);
        File.Move(destino, anterior, overwrite: true);
        return anterior;
    }

    /// <summary>En Linux y macOS el zip no conserva el bit de ejecución: sin él, ni el programa ni los
    /// scripts de acceso directo arrancarían tras actualizar.</summary>
    private static void DarPermisoDeEjecucion(string carpetaInstalacion, IEnumerable<string> colocados, string nombreEjecutable)
    {
        if (OperatingSystem.IsWindows()) return;

        const UnixFileMode permiso755 = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
            | UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute;
        foreach (var ruta in colocados)
        {
            var esEjecutable = ruta == Path.Combine(carpetaInstalacion, nombreEjecutable)
                || ruta.EndsWith(".sh", StringComparison.Ordinal) || ruta.EndsWith(".command", StringComparison.Ordinal);
            if (esEjecutable) File.SetUnixFileMode(ruta, permiso755);
        }
    }

    /// <summary>En orden inverso: quita lo nuevo y devuelve cada fichero apartado a su sitio. Sigue aunque
    /// un paso falle, para restaurar todo lo posible; devuelve si lo consiguió entero.</summary>
    private static bool Deshacer(List<(string Destino, string? Anterior)> hechos)
    {
        var todoRestaurado = true;
        for (var i = hechos.Count - 1; i >= 0; i--)
        {
            var (destino, anterior) = hechos[i];
            try
            {
                if (File.Exists(destino)) File.Delete(destino);
                if (anterior is not null) File.Move(anterior, destino);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                todoRestaurado = false;
            }
        }
        return todoRestaurado;
    }
}

/// <summary>Un fallo al sustituir los ficheros del programa, con un mensaje que dice en qué estado quedó
/// la instalación. Tipo propio para no confundirlo con un fallo de descarga, que no ha tocado nada.</summary>
public sealed class SustitucionFallidaException(string mensaje, Exception causa) : IOException(mensaje, causa);
