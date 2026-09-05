namespace Spd.Dominio;

/// <summary>Acceso al catálogo de médicos (Art. IV.1).</summary>
public interface IRepositorioMedicos
{
    Medico? ObtenerPorId(int id);

    /// <summary>Busca médicos activos por fragmento ya normalizado (FR-032). El mínimo de 2
    /// caracteres lo exige la capa de Aplicación, no el repositorio.</summary>
    IReadOnlyList<Medico> Buscar(string fragmentoNormalizado);

    IReadOnlyList<Medico> ListarActivos();
    int Crear(Medico medico);
    void Actualizar(Medico medico);
}
