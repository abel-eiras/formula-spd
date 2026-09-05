namespace Spd.Aplicacion;

/// <summary>Resultado de generar un backup (FR-1000/FR-1001/FR-1003). Un fallo (ruta inaccesible,
/// disco lleno) es un resultado esperable, no una excepción de programación — la Presentación
/// decide cómo mostrarlo (CA-1001 exige un aviso visible, no un log que nadie revisa).</summary>
public sealed record ResultadoBackup(bool Exito, string? RutaZip, string? Motivo)
{
    public static ResultadoBackup Exitoso(string rutaZip) => new(true, rutaZip, null);

    public static ResultadoBackup Fallido(string motivo) => new(false, null, motivo);
}
