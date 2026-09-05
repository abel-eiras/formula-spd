namespace Spd.Aplicacion;

/// <summary>Resultado de restaurar desde un `.zip` de backup (FR-1004).</summary>
public sealed record ResultadoRestauracion(bool Exito, string? Motivo)
{
    public static ResultadoRestauracion Exitoso() => new(true, null);

    public static ResultadoRestauracion Fallido(string motivo) => new(false, motivo);
}
