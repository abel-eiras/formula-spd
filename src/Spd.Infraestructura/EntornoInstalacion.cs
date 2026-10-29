using System.Runtime.InteropServices;

namespace Spd.Infraestructura;

/// <summary>Dónde está instalada esta copia de Fórmula SPD y para qué plataforma se empaquetó (FR-053).
///
/// Es un dato y no una consulta hecha dentro del instalador para poder probar la instalación en una
/// carpeta temporal sin tocar el binario que ejecuta los tests. En producción se usa <see cref="Actual"/>.
/// Los nombres de plataforma, de ejecutable y de extensión son los mismos que pone
/// `publicar/empaquetar.sh`: si cambian allí, cambian aquí.</summary>
public sealed record EntornoInstalacion(string CarpetaInstalacion, string? RutaEjecutableEnUso, string? Plataforma)
{
    /// <summary>Carpeta de trabajo de la descarga, dentro de la instalación: así el paquete extraído está en
    /// el mismo disco y sustituir un fichero es un renombrado, no una copia a medias.</summary>
    public string CarpetaTemporal => Path.Combine(CarpetaInstalacion, "actualizacion");

    public string NombreEjecutable => Plataforma == "windows-x64" ? "FormulaSPD.exe" : "FormulaSPD";

    public string ExtensionPaquete => Plataforma == "linux-x64" ? ".tar.gz" : ".zip";

    public static EntornoInstalacion Actual()
        => new(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory), Environment.ProcessPath, PlataformaActual());

    /// <summary>La arquitectura del **proceso**, no la del sistema: un Intel bajo Rosetta sigue siendo el
    /// paquete de Intel, que es el que está instalado.</summary>
    public static string? PlataformaActual()
    {
        var arquitectura = RuntimeInformation.ProcessArchitecture;
        if (OperatingSystem.IsWindows() && arquitectura == Architecture.X64) return "windows-x64";
        if (OperatingSystem.IsLinux() && arquitectura == Architecture.X64) return "linux-x64";
        if (OperatingSystem.IsMacOS() && arquitectura == Architecture.Arm64) return "macos-apple-silicon";
        if (OperatingSystem.IsMacOS() && arquitectura == Architecture.X64) return "macos-intel";
        return null;
    }

    /// <summary>`FormulaSPD-0.2.0-windows-x64.zip`. Se compara el final y no el nombre entero porque la
    /// versión del nombre es la del .csproj, sin la `v` de la etiqueta.</summary>
    public bool EsPaqueteDeEstaPlataforma(string nombreFichero)
        => Plataforma is not null
           && nombreFichero.StartsWith("FormulaSPD-", StringComparison.Ordinal)
           && nombreFichero.EndsWith($"-{Plataforma}{ExtensionPaquete}", StringComparison.Ordinal);

    /// <summary>Solo se puede sustituir el ejecutable empaquetado, dentro de su carpeta. Con `dotnet run` el
    /// proceso es `dotnet` y la carpeta es `bin/`: «instalar» ahí mezclaría un paquete con una compilación
    /// de desarrollo.</summary>
    public string? MotivoParaNoInstalar()
    {
        if (Plataforma is null)
            return "Este sistema no tiene paquete de actualización automática. Descarga la versión nueva a mano.";

        var esElEmpaquetado = RutaEjecutableEnUso is not null
            && string.Equals(Path.GetFileName(RutaEjecutableEnUso), NombreEjecutable, StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(RutaEjecutableEnUso) ?? ""),
                Path.TrimEndingDirectorySeparator(CarpetaInstalacion),
                StringComparison.OrdinalIgnoreCase);
        return esElEmpaquetado
            ? null
            : $"La instalación desde la aplicación solo funciona en la versión empaquetada ({NombreEjecutable} dentro de su carpeta). "
              + "Esta copia se está ejecutando de otra forma; descarga la versión nueva a mano.";
    }
}
