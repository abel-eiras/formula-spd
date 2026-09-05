namespace Spd.Aplicacion;

/// <summary>Datos de entrada para un registro ambiental (FR-900/FR-902). `FechaHora` nulo se
/// prerrellena a "ahora"; editable pasando un valor explícito.</summary>
public sealed record DatosRegistroAmbiental(double Temperatura, double Humedad, string? Observaciones, DateTime? FechaHora = null);
