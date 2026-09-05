namespace Spd.Dominio;

/// <summary>Mapeo de columnas reutilizable para importar tratamiento+envase (FR-570-572).</summary>
public sealed class PerfilImportacionTratamiento
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public OrigenImportacionTratamiento Origen { get; set; }
    public string? Separador { get; set; }
    public bool? TieneCabecera { get; set; }
    public required MapeoColumnasImportacion Mapeo { get; set; }
}

/// <summary>Columnas de origen mapeadas a los campos mínimos exigidos por FR-571.</summary>
public sealed record MapeoColumnasImportacion(string Cn, string NumSerie, string Lote, string Caducidad);
