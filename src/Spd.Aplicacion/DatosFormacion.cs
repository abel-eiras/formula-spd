namespace Spd.Aplicacion;

/// <summary>Datos de entrada para una formación del personal (FR-920). Sin distinción de
/// categoría profesional (Art. VII.4).</summary>
public sealed record DatosFormacion(int UsuarioId, string NombreCurso, string? EntidadOrganizadora, DateOnly Fecha, bool Acreditado);
