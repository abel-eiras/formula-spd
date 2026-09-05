namespace Spd.Dominio;

/// <summary>Acceso a los usuarios de la aplicación. La baja es lógica, nunca hay eliminación física (Art. III.1).</summary>
public interface IRepositorioUsuarios
{
    Usuario? ObtenerPorId(int id);
    Usuario? ObtenerPorLogin(string login);
    IReadOnlyList<Usuario> ListarActivos();
    int ContarAdministradoresActivos();
    int Crear(Usuario usuario);
    void Actualizar(Usuario usuario);
}
