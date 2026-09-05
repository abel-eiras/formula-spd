namespace Spd.Aplicacion;

/// <summary>Campos que se editan en el sitio sin cerrar/abrir fila (FR-411): no son
/// clínicamente relevantes para la instantánea de un SPD.</summary>
public sealed record DatosNoClinicos(
    string? ConocimientoCumplimiento, string? Incidencias, string? Intervencion, int? AjusteUnidadesManual);
