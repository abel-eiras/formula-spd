using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Fecha y motivo exigidos al pasar un paciente a BAJA (FR-007). `Detalle` solo se usa
/// cuando `Motivo` es <see cref="MotivoBaja.Otro"/>.</summary>
public sealed record DatosBaja(DateTime Fecha, MotivoBaja Motivo, string? Detalle);
