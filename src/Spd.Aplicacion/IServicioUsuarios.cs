using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Gestión de usuarios y acceso (FR-040..FR-045).</summary>
public interface IServicioUsuarios
{
    IReadOnlyList<Usuario> ListarActivos();
    Usuario CrearUsuario(DatosAltaUsuario datos, int? administradorQueEjecutaId);
    void DarDeBaja(int usuarioId, int? usuarioQueEjecutaId);
    void CambiarPassword(int usuarioId, string passwordNueva);
    void ResetearPassword(int usuarioId, int administradorId);
    ResultadoLogin IntentarLogin(string login, string password);
    void DesbloquearUsuario(int usuarioId, int administradorId);
}
