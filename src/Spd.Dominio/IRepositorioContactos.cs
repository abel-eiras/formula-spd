namespace Spd.Dominio;

/// <summary>Acceso a los contactos de un paciente. La baja es lógica (Art. III.1).</summary>
public interface IRepositorioContactos
{
    Contacto? ObtenerPorId(int id);
    IReadOnlyList<Contacto> ListarDePaciente(int pacienteId, bool incluirBaja);
    int Crear(Contacto contacto);
    void Actualizar(Contacto contacto);
}
