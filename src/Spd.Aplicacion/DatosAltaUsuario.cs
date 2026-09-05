using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de entrada para dar de alta un usuario (FR-040/FR-041).</summary>
public sealed record DatosAltaUsuario(
    string Nombre, string Apellidos, string Login, string? PasswordProvisional,
    Rol Rol, string? CargoPnt, string? Colegiado);
