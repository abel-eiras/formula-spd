using System.Formats.Tar;
using System.IO.Compression;

namespace Spd.Infraestructura;

/// <summary>Descomprime un paquete de actualización (FR-053): `.zip` en Windows y macOS, `.tar.gz` en Linux,
/// los mismos que genera `publicar/empaquetar.sh`. Solo con el BCL (Art. VIII.2: sin paquetes nuevos).
///
/// Toda ruta del paquete se comprueba antes de escribir: una entrada `../../spd.db` («zip-slip») podría, si
/// no, escribir fuera de la carpeta temporal y pisar los datos de la farmacia. La firma ya garantiza que el
/// paquete es nuestro, pero esta comprobación no cuesta nada y no depende de que la CI no se equivoque.</summary>
public static class ExtractorPaqueteActualizacion
{
    /// <summary>Extrae en <paramref name="destino"/> y devuelve la carpeta que contiene
    /// <paramref name="nombreEjecutable"/> (los paquetes llevan una carpeta raíz `FormulaSPD-…/`), o
    /// <c>null</c> si el paquete no trae el ejecutable.</summary>
    public static string? Extraer(string rutaPaquete, string destino, string nombreEjecutable)
    {
        Directory.CreateDirectory(destino);
        if (rutaPaquete.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) ExtraerZip(rutaPaquete, destino);
        else if (rutaPaquete.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)) ExtraerTarGz(rutaPaquete, destino);
        else throw new InvalidDataException($"formato de paquete desconocido: {Path.GetFileName(rutaPaquete)}");

        return CarpetaConEjecutable(destino, nombreEjecutable);
    }

    /// <summary>Ruta absoluta de una entrada del paquete, garantizando que queda dentro de
    /// <paramref name="destino"/>. Una ruta absoluta o con `..` que escape lanza.</summary>
    public static string RutaDentroDe(string destino, string nombreEntrada)
    {
        var raiz = Path.GetFullPath(destino);
        if (!Path.EndsInDirectorySeparator(raiz)) raiz += Path.DirectorySeparatorChar;
        var ruta = Path.GetFullPath(Path.Combine(raiz, nombreEntrada));
        if (!ruta.StartsWith(raiz, StringComparison.Ordinal) && ruta + Path.DirectorySeparatorChar != raiz)
            throw new InvalidDataException($"el paquete contiene una ruta fuera de su carpeta: {nombreEntrada}");
        return ruta;
    }

    private static void ExtraerZip(string rutaPaquete, string destino)
    {
        using var zip = ZipFile.OpenRead(rutaPaquete);
        foreach (var entrada in zip.Entries)
        {
            var ruta = RutaDentroDe(destino, entrada.FullName);
            if (entrada.FullName.EndsWith('/') || entrada.FullName.EndsWith('\\'))
            {
                Directory.CreateDirectory(ruta);
                continue;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
            entrada.ExtractToFile(ruta, overwrite: true);
        }
    }

    /// <summary>Solo directorios y ficheros normales: un enlace simbólico en el paquete podría apuntar
    /// fuera de la carpeta, y los paquetes de Fórmula SPD no llevan ninguno.</summary>
    private static void ExtraerTarGz(string rutaPaquete, string destino)
    {
        using var fichero = File.OpenRead(rutaPaquete);
        using var gzip = new GZipStream(fichero, CompressionMode.Decompress);
        using var lector = new TarReader(gzip);
        while (lector.GetNextEntry() is { } entrada)
        {
            if (entrada.EntryType is not (TarEntryType.Directory or TarEntryType.RegularFile or TarEntryType.V7RegularFile))
                continue;

            var ruta = RutaDentroDe(destino, entrada.Name);
            if (entrada.EntryType == TarEntryType.Directory)
            {
                Directory.CreateDirectory(ruta);
                continue;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
            entrada.ExtractToFile(ruta, overwrite: true);
        }
    }

    /// <summary>La raíz del paquete o su única carpeta de primer nivel; se descartan carpetas añadidas por el
    /// compresor (el `__MACOSX` de los zip hechos en un Mac).</summary>
    private static string? CarpetaConEjecutable(string destino, string nombreEjecutable)
    {
        if (File.Exists(Path.Combine(destino, nombreEjecutable))) return destino;
        return Directory.EnumerateDirectories(destino)
            .FirstOrDefault(carpeta => File.Exists(Path.Combine(carpeta, nombreEjecutable)));
    }
}
