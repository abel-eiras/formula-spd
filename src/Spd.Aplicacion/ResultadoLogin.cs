using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Resultado de un intento de inicio de sesión (FR-045).</summary>
public sealed record ResultadoLogin(bool Exito, bool Bloqueado, Usuario? Usuario, string? Mensaje)
{
    public static ResultadoLogin Exitoso(Usuario usuario) => new(true, false, usuario, null);

    public static ResultadoLogin Fallido(string mensaje) => new(false, false, null, mensaje);

    public static ResultadoLogin ParaUsuarioBloqueado()
        => new(false, true, null, "Usuario bloqueado. Contacte con un Administrador.");
}
