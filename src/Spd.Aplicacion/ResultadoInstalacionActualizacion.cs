namespace Spd.Aplicacion;

/// <summary>Resultado de instalar una actualización (FR-053). Un fallo —sin conexión, firma inválida,
/// carpeta de solo lectura— es un resultado esperable, no una excepción: la pantalla lo explica y la
/// aplicación sigue funcionando con la versión que ya tenía (FR-052).
///
/// <see cref="SoloManual"/> distingue las releases que no se pueden instalar desde la aplicación
/// (publicadas sin firma, o esta copia no es la empaquetada): ahí la salida es descargar a mano.</summary>
public sealed record ResultadoInstalacionActualizacion(
    bool Exito, string? VersionInstalada, string? RutaEjecutable, string? Motivo, bool SoloManual)
{
    public static ResultadoInstalacionActualizacion Exitosa(string version, string rutaEjecutable)
        => new(true, version, rutaEjecutable, null, false);

    public static ResultadoInstalacionActualizacion Fallida(string motivo) => new(false, null, null, motivo, false);

    public static ResultadoInstalacionActualizacion SoloADescargarAMano(string motivo) => new(false, null, null, motivo, true);
}
