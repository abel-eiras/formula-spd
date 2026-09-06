using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Consulta de médicos para la interfaz (Spec 015 FR-1542).
///
/// Existe porque hasta ahora el médico y el verificador se elegían **tecleando su número de id** en
/// un campo numérico. Nadie sabe de memoria que la Dra. Vidal es el 7, y equivocarse de dígito
/// atribuye una comunicación al médico que no es. La lista por nombre es la corrección.</summary>
public interface IServicioMedicos
{
    IReadOnlyList<Medico> ListarActivos();

    Medico? ObtenerPorId(int id);
}

public sealed class ServicioMedicos(IRepositorioMedicos repositorio) : IServicioMedicos
{
    public IReadOnlyList<Medico> ListarActivos() => repositorio.ListarActivos();

    public Medico? ObtenerPorId(int id) => repositorio.ObtenerPorId(id);
}
