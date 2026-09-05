namespace Spd.Aplicacion;

/// <summary>Una fila `(CN, Nombre)` extraída del fichero de nomenclátor (FR-320, research.md
/// Decisión 5 de Spec 003). PrincipioActivo y Laboratorio son opcionales: se rellenan solo si el
/// fichero descargado tiene esas columnas.</summary>
public sealed record FilaNomenclator(string Cn, string Nombre, string? PrincipioActivo = null, string? Laboratorio = null);
