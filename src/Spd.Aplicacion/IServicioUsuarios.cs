using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Gestión de usuarios y acceso (FR-040..FR-045).</summary>
public interface IServicioUsuarios
{
    IReadOnlyList<Usuario> ListarActivos();
    ResultadoAltaUsuario CrearUsuario(DatosAltaUsuario datos, int? administradorQueEjecutaId);
    void DarDeBaja(int usuarioId, int? usuarioQueEjecutaId);
    void CambiarPassword(int usuarioId, string passwordNueva);

    /// <returns>La nueva contraseña provisional en claro, para comunicársela al usuario (FR-044).</returns>
    string ResetearPassword(int usuarioId, int administradorId);
    ResultadoLogin IntentarLogin(string login, string password);
    void DesbloquearUsuario(int usuarioId, int administradorId);
}
