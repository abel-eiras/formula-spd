namespace Spd.Aplicacion;

/// <summary>Resultado de activar el cifrado (FR-1010). `FraseRecuperacion` se muestra una única
/// vez para que el administrador la imprima o guarde (CA-1003); no se conserva en memoria más
/// allá de esta llamada.</summary>
public sealed record ResultadoActivarCifrado(bool Exito, string[]? FraseRecuperacion, string? Motivo)
{
    public static ResultadoActivarCifrado Exitoso(string[] fraseRecuperacion) => new(true, fraseRecuperacion, null);

    public static ResultadoActivarCifrado Fallido(string motivo) => new(false, null, motivo);
}
