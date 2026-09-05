using System.Security.Cryptography;
using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Gestión de usuarios y acceso (FR-040..FR-045). Todo método de escritura registra en
/// auditoría (Art. VII.6).</summary>
public sealed class ServicioUsuarios(
    IRepositorioUsuarios repositorio,
    IHasheadorPassword hasheador,
    IRegistradorAuditoria auditoria) : IServicioUsuarios
{
    private const int IntentosFallidosPermitidos = 5;

    public IReadOnlyList<Usuario> ListarActivos() => repositorio.ListarActivos();

    public ResultadoAltaUsuario CrearUsuario(DatosAltaUsuario datos, int? administradorQueEjecutaId)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre) || string.IsNullOrWhiteSpace(datos.Apellidos)
            || string.IsNullOrWhiteSpace(datos.Login))
        {
            throw new ErrorValidacionException("Faltan campos obligatorios del usuario (FR-040).");
        }

        var passwordProvisional = string.IsNullOrWhiteSpace(datos.PasswordProvisional)
            ? GenerarPasswordProvisional()
            : datos.PasswordProvisional;

        var usuario = new Usuario
        {
            Nombre = datos.Nombre,
            Apellidos = datos.Apellidos,
            Login = datos.Login,
            HashPassword = hasheador.Hashear(passwordProvisional),
            Rol = datos.Rol,
            CargoPnt = datos.CargoPnt,
            Colegiado = datos.Colegiado,
            DebeCambiarPassword = true // FR-041: siempre, tanto si se genera como si se introduce a mano
        };
        usuario.Id = repositorio.Crear(usuario);

        auditoria.Registrar(administradorQueEjecutaId, "ALTA", "Usuario", usuario.Id, null);
        return new ResultadoAltaUsuario(usuario, passwordProvisional);
    }

    public void DarDeBaja(int usuarioId, int? usuarioQueEjecutaId)
    {
        var usuario = ObtenerOLanzar(usuarioId);
        ReglaUnicoAdministrador.ValidarBaja(usuario, repositorio.ContarAdministradoresActivos());

        usuario.Activo = false;
        usuario.FechaBaja = DateTime.UtcNow;
        repositorio.Actualizar(usuario);

        auditoria.Registrar(usuarioQueEjecutaId, "BAJA", "Usuario", usuarioId, null);
    }

    public void CambiarPassword(int usuarioId, string passwordNueva)
    {
        var usuario = ObtenerOLanzar(usuarioId);
        usuario.HashPassword = hasheador.Hashear(passwordNueva);
        usuario.DebeCambiarPassword = false;
        repositorio.Actualizar(usuario);

        auditoria.Registrar(usuarioId, "CAMBIO_PASSWORD", "Usuario", usuarioId, null);
    }

    public string ResetearPassword(int usuarioId, int administradorId)
    {
        var usuario = ObtenerOLanzar(usuarioId);
        var passwordProvisional = GenerarPasswordProvisional();
        usuario.HashPassword = hasheador.Hashear(passwordProvisional);
        usuario.DebeCambiarPassword = true;
        repositorio.Actualizar(usuario);

        auditoria.Registrar(administradorId, "RESETEO_PASSWORD", "Usuario", usuarioId, null);
        return passwordProvisional;
    }

    public ResultadoLogin IntentarLogin(string login, string password)
    {
        var usuario = repositorio.ObtenerPorLogin(login);
        if (usuario is null || !usuario.Activo)
        {
            return ResultadoLogin.Fallido("Usuario o contraseña incorrectos.");
        }
        if (usuario.Bloqueado)
        {
            return ResultadoLogin.ParaUsuarioBloqueado();
        }

        var passwordCorrecta = hasheador.Verificar(password, usuario.HashPassword);
        RegistrarIntentoLogin(usuario, passwordCorrecta);

        return passwordCorrecta
            ? ResultadoLogin.Exitoso(usuario)
            : ResultadoLogin.Fallido("Usuario o contraseña incorrectos.");
    }

    public void DesbloquearUsuario(int usuarioId, int administradorId)
    {
        var administrador = ObtenerOLanzar(administradorId);
        if (administrador.Rol != Rol.Administrador)
        {
            throw new ErrorValidacionException("Solo un Administrador puede desbloquear usuarios.");
        }

        var usuario = ObtenerOLanzar(usuarioId);
        usuario.Bloqueado = false;
        usuario.IntentosFallidosConsecutivos = 0;
        repositorio.Actualizar(usuario);

        auditoria.Registrar(administradorId, "DESBLOQUEO", "Usuario", usuarioId, null);
    }

    /// <summary>Actualiza el contador de intentos fallidos y bloquea al llegar al umbral (FR-045),
    /// sin expiración automática (Clarifications, Q2). Todo intento, éxito o fallo, se audita (Art. VII.6).</summary>
    private void RegistrarIntentoLogin(Usuario usuario, bool exito)
    {
        if (exito)
        {
            usuario.IntentosFallidosConsecutivos = 0;
            repositorio.Actualizar(usuario);
            auditoria.Registrar(usuario.Id, "LOGIN", "Usuario", usuario.Id, null);
            return;
        }

        usuario.IntentosFallidosConsecutivos++;
        if (usuario.IntentosFallidosConsecutivos >= IntentosFallidosPermitidos)
        {
            usuario.Bloqueado = true;
        }
        repositorio.Actualizar(usuario);
        auditoria.Registrar(usuario.Id, "LOGIN_FALLIDO", "Usuario", usuario.Id, null);
    }

    private Usuario ObtenerOLanzar(int usuarioId)
        => repositorio.ObtenerPorId(usuarioId)
           ?? throw new ErrorValidacionException($"No existe el usuario {usuarioId}.");

    private static string GenerarPasswordProvisional()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(9)).Replace('/', '-').Replace('+', '_');
}
