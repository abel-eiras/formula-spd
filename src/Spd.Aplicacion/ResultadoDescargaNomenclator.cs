namespace Spd.Aplicacion;

/// <summary>Resultado de descargar el nomenclátor (FR-051/FR-052). No parsea el contenido — eso
/// es responsabilidad de Spec 003/011.</summary>
public sealed record ResultadoDescargaNomenclator(bool Exito, string? RutaDestino, string? Motivo, DateTime FechaUtc);
