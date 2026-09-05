using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a los usuarios vía Dapper/SQLite. La baja es lógica (Art. III.1).</summary>
public sealed class RepositorioUsuarios(SqliteConnection conexion) : IRepositorioUsuarios
{
    private const string Columnas = """
        id, nombre, apellidos, login, hash_password, rol, cargo_pnt, colegiado,
        firma_abreviada, activo, fecha_baja, debe_cambiar_password,
        intentos_fallidos_consecutivos, bloqueado
        """;

    public Usuario? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<UsuarioFila>(
            $"SELECT {Columnas} FROM Usuario WHERE id = @id", new { id })?.AUsuario();

    public Usuario? ObtenerPorLogin(string login)
        => conexion.QuerySingleOrDefault<UsuarioFila>(
            $"SELECT {Columnas} FROM Usuario WHERE login = @login", new { login })?.AUsuario();

    public IReadOnlyList<Usuario> ListarActivos()
        => conexion.Query<UsuarioFila>(
                $"SELECT {Columnas} FROM Usuario WHERE activo = 1 ORDER BY apellidos, nombre")
            .Select(f => f.AUsuario())
            .ToList();

    public int ContarAdministradoresActivos()
        => conexion.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Usuario WHERE activo = 1 AND rol = 'ADMINISTRADOR'");

    public int Crear(Usuario usuario)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO Usuario (
                nombre, apellidos, login, hash_password, rol, cargo_pnt, colegiado,
                firma_abreviada, activo, debe_cambiar_password
            ) VALUES (
                @Nombre, @Apellidos, @Login, @HashPassword, @Rol, @CargoPnt, @Colegiado,
                @FirmaAbreviada, @Activo, @DebeCambiarPassword
            ) RETURNING id
            """,
            DesdeUsuario(usuario));

    public void Actualizar(Usuario usuario)
        => conexion.Execute(
            """
            UPDATE Usuario SET
                nombre = @Nombre, apellidos = @Apellidos, login = @Login,
                hash_password = @HashPassword, rol = @Rol, cargo_pnt = @CargoPnt,
                colegiado = @Colegiado, firma_abreviada = @FirmaAbreviada, activo = @Activo,
                fecha_baja = @FechaBaja, debe_cambiar_password = @DebeCambiarPassword,
                intentos_fallidos_consecutivos = @IntentosFallidosConsecutivos,
                bloqueado = @Bloqueado, modificado_en = @ModificadoEn
            WHERE id = @Id
            """,
            new
            {
                usuario.Id, usuario.Nombre, usuario.Apellidos, usuario.Login, usuario.HashPassword,
                Rol = TextoRol(usuario.Rol), usuario.CargoPnt, usuario.Colegiado,
                usuario.FirmaAbreviada, Activo = usuario.Activo ? 1 : 0, usuario.FechaBaja,
                DebeCambiarPassword = usuario.DebeCambiarPassword ? 1 : 0,
                usuario.IntentosFallidosConsecutivos, Bloqueado = usuario.Bloqueado ? 1 : 0,
                ModificadoEn = DateTime.UtcNow.ToString("o")
            });

    private static string TextoRol(Rol rol) => rol == Rol.Administrador ? "ADMINISTRADOR" : "ELABORADOR";

    private static object DesdeUsuario(Usuario usuario) => new
    {
        usuario.Nombre, usuario.Apellidos, usuario.Login, usuario.HashPassword,
        Rol = TextoRol(usuario.Rol), usuario.CargoPnt, usuario.Colegiado, usuario.FirmaAbreviada,
        Activo = usuario.Activo ? 1 : 0, DebeCambiarPassword = usuario.DebeCambiarPassword ? 1 : 0
    };

    /// <summary>Fila 1:1 con las columnas leídas de Usuario. Evita depender de la conversión
    /// automática de enums de Dapper (Art. XI.2: explícito antes que una abstracción que oculte
    /// el mapeo real).</summary>
    private sealed record UsuarioFila(
        long Id, string Nombre, string Apellidos, string Login, string HashPassword, string Rol,
        string? CargoPnt, string? Colegiado, string? FirmaAbreviada, long Activo, string? FechaBaja,
        long DebeCambiarPassword, long IntentosFallidosConsecutivos, long Bloqueado)
    {
        public Usuario AUsuario() => new()
        {
            Id = (int)Id,
            Nombre = Nombre,
            Apellidos = Apellidos,
            Login = Login,
            HashPassword = HashPassword,
            Rol = Rol == "ADMINISTRADOR" ? Dominio.Rol.Administrador : Dominio.Rol.Elaborador,
            CargoPnt = CargoPnt,
            Colegiado = Colegiado,
            FirmaAbreviada = FirmaAbreviada,
            Activo = Activo == 1,
            FechaBaja = FechaBaja is null ? null : DateTime.Parse(FechaBaja),
            DebeCambiarPassword = DebeCambiarPassword == 1,
            IntentosFallidosConsecutivos = (int)IntentosFallidosConsecutivos,
            Bloqueado = Bloqueado == 1
        };
    }
}
