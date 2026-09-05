namespace Spd.Dominio;

/// <summary>Persona con acceso a la aplicación. Nunca se elimina, solo se da de baja (Art. III.1).</summary>
public sealed class Usuario
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Apellidos { get; set; }
    public required string Login { get; set; }
    public required string HashPassword { get; set; }
    public Rol Rol { get; set; }
    public string? CargoPnt { get; set; }
    public string? Colegiado { get; set; }
    public string? FirmaAbreviada { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? FechaBaja { get; set; }
    public bool DebeCambiarPassword { get; set; }
    public int IntentosFallidosConsecutivos { get; set; }
    public bool Bloqueado { get; set; }
}
