namespace Spd.Dominio;

/// <summary>Un par campo de la aplicación ↔ columna del fichero externo. En importación, `Columna`
/// es el índice de columna de origen (base 0, como texto); en exportación (FR-1130), el mismo par
/// se lee en sentido inverso: `Columna` es el nombre de la columna de salida (research.md
/// Decisión 3 de Spec 011).</summary>
public sealed record ParCampoColumna(string Campo, string Columna);

/// <summary>Perfil de importación/exportación genérico y reutilizable (FR-1100). Distinto de
/// `PerfilImportacionTratamiento` (Spec 005), que es un perfil especializado a solo cuatro
/// campos.</summary>
public sealed class PerfilImportacion
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public TipoPerfilImportacion Tipo { get; set; }
    public string Separador { get; set; } = ",";
    public string Codificacion { get; set; } = "UTF-8";
    public bool TieneCabecera { get; set; } = true;
    public required IReadOnlyList<ParCampoColumna> Mapeo { get; set; }
    public string? RegexUnidadesEnvase { get; set; }
}
