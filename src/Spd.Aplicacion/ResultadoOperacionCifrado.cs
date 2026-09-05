namespace Spd.Aplicacion;

/// <summary>Resultado de una operación de cifrado sin frase de recuperación asociada
/// (`DesactivarCifrado`, FR-1011). Un fallo (contraseña incorrecta) es un resultado esperable, no
/// una excepción de programación.</summary>
public sealed record ResultadoOperacionCifrado(bool Exito, string? Motivo)
{
    public static ResultadoOperacionCifrado Exitoso() => new(true, null);

    public static ResultadoOperacionCifrado Fallido(string motivo) => new(false, motivo);
}
