namespace Spd.Aplicacion;

/// <summary>Una fila `(CN, Nombre)` extraída del fichero de nomenclátor (FR-320, research.md
/// Decisión 5 de Spec 003).</summary>
public sealed record FilaNomenclator(string Cn, string Nombre);
