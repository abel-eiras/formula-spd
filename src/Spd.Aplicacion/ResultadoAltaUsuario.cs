using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Resultado de dar de alta un usuario (FR-041): la contraseña provisional se devuelve en
/// claro una sola vez, para que quien lo crea pueda comunicársela — nunca se vuelve a poder leer
/// (Art. VII.1, solo se guarda el hash).</summary>
public sealed record ResultadoAltaUsuario(Usuario Usuario, string PasswordProvisional);
