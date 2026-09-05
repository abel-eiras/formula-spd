namespace Spd.Dominio;

/// <summary>Catálogo de médicos, reutilizable desde cualquier ficha (Art. IV.1, FR-030). Editar
/// sus datos se refleja de inmediato en toda referencia; nunca se copian en Paciente (FR-035).</summary>
public sealed class Medico
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Apellidos { get; set; }
    public string? Colegiado { get; set; }
    public string Especialidad { get; set; } = "Medicina de familia";
    public string? Centro { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
    public string BusquedaNormalizada { get; set; } = string.Empty;
}
