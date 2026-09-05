namespace Spd.Dominio;

/// <summary>Persona vinculada a un paciente: familiar, representante legal, persona autorizada o
/// cuidador (FR-020). La baja es lógica, nunca se elimina (Art. III.1).</summary>
public sealed class Contacto
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public TipoContacto Tipo { get; set; }
    public required string Nombre { get; set; }
    public required string Apellidos { get; set; }
    public string? Dni { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool EsPrincipal { get; set; }
    public bool RetiraMedicacion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? FechaBaja { get; set; }
}
