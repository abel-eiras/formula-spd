namespace Spd.Aplicacion;

/// <summary>Datos de entrada para una recogida de residuos no SIGRE (FR-930).</summary>
public sealed record DatosRecogidaResiduos(DateOnly Fecha, string EmpresaGestora, string? Observaciones);
