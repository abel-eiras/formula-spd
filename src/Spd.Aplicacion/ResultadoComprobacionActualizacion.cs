namespace Spd.Aplicacion;

/// <summary>Resultado de comprobar si hay una versión nueva del software (FR-050/FR-052).</summary>
public sealed record ResultadoComprobacionActualizacion(
    bool Exito, bool HayNueva, string? VersionDisponible, string? UrlDescarga, string? Motivo, DateTime FechaComprobacion);
