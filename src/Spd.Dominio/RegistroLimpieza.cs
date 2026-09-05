namespace Spd.Dominio;

/// <summary>Registro de limpieza de la zona de preparación (FR-910), de un clic desde la
/// pantalla de preparación cuando el tipo es PRE/POST_PREPARACION (FR-911).</summary>
public sealed class RegistroLimpieza
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int UsuarioId { get; set; }
    public TipoLimpieza Tipo { get; set; }
    public string? Observaciones { get; set; }
}
