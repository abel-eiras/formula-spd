namespace Spd.Aplicacion;

/// <summary>Resultado de cambiar la contraseña maestra (FR-1012). Genera una MEK y una frase de
/// recuperación nuevas — `PRAGMA rekey`, no `sqlcipher_export` (research.md Decisión 1/3).</summary>
public sealed record ResultadoCambioContrasena(bool Exito, string[]? FraseRecuperacionNueva, string? Motivo)
{
    public static ResultadoCambioContrasena Exitoso(string[] fraseRecuperacionNueva) => new(true, fraseRecuperacionNueva, null);

    public static ResultadoCambioContrasena Fallido(string motivo) => new(false, null, motivo);
}
