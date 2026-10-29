namespace Spd.Aplicacion.Tests;

/// <summary>Carpeta temporal propia de un test, borrada al terminar: las pruebas del instalador de
/// actualizaciones trabajan sobre una «instalación» de mentira, nunca sobre el binario real.</summary>
internal sealed class CarpetaTemporalDePrueba : IDisposable
{
    public string Ruta { get; } = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "spd-pruebas-" + Guid.NewGuid().ToString("N"))).FullName;

    public string Sub(params string[] partes) => Path.Combine([Ruta, .. partes]);

    public string Escribir(string relativa, string contenido)
    {
        var ruta = Sub(relativa);
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        File.WriteAllText(ruta, contenido);
        return ruta;
    }

    public void Dispose()
    {
        try { Directory.Delete(Ruta, recursive: true); } catch (IOException) { }
    }
}
