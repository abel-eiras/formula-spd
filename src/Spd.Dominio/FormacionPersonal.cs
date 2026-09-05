namespace Spd.Dominio;

/// <summary>Formación o curso recibido por un usuario (FR-920). Sin distinción de categoría
/// profesional (Art. VII.4, research.md Decisión 1 de Spec 009). Nunca se sustituye una entrada
/// anterior: todas se acumulan (FR-921, Art. III).</summary>
public sealed class FormacionPersonal
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public required string NombreCurso { get; set; }
    public string? EntidadOrganizadora { get; set; }
    public DateOnly Fecha { get; set; }
    public bool Acreditado { get; set; }
}
