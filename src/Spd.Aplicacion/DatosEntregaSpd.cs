namespace Spd.Aplicacion;

/// <summary>Datos de una entrega (FR-661), comunes a todos los SPD que cubra (uno o dos
/// blísteres).</summary>
public sealed record DatosEntregaSpd(
    DateOnly Fecha, string EntregadoA, bool PrimeraEntrega, bool? SpdAnteriorRecogido,
    string? UnidadesNoAdministradas, string? ObservacionesAdherencia, bool CambiosMedicacionPreguntado,
    string? ObservacionesEtiqueta);
