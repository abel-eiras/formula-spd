using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de alta o edición de un médico (FR-030).</summary>
public sealed record DatosMedico(
    string Nombre,
    string Apellidos,
    string? Colegiado = null,
    string? Especialidad = null,
    string? Centro = null,
    string? Telefono = null,
    string? Email = null,
    string? Direccion = null);

/// <summary>Alta de un médico. `Aviso` no bloquea: FR-034 advierte de un posible duplicado y **el
/// usuario decide**, porque dos médicos pueden llamarse igual y el mismo médico puede pasar consulta
/// en dos centros.</summary>
public sealed record ResultadoAltaMedico(Medico Medico, string? Aviso);

/// <summary>El catálogo de médicos (Spec 001 US2, FR-030..FR-037).
///
/// Hasta ahora solo existía el repositorio: no había forma de dar de alta un médico desde la
/// aplicación, así que todo campo que referenciaba a uno estaba condenado a quedarse vacío.</summary>
public interface IServicioMedicos
{
    /// <summary>FR-032: busca por fragmento de apellidos, nombre o centro, sin tildes ni mayúsculas.
    /// Con menos de dos caracteres devuelve vacío: no es una búsqueda, es traer el catálogo entero.</summary>
    IReadOnlyList<Medico> Buscar(string? fragmento);

    IReadOnlyList<Medico> ListarActivos();

    Medico? ObtenerPorId(int id);

    ResultadoAltaMedico Crear(DatosMedico datos, int? usuarioQueEjecutaId);

    Medico Actualizar(int id, DatosMedico datos, int? usuarioQueEjecutaId);

    /// <summary>FR-036: solo si no es médico de cabecera de ningún paciente activo o en evaluación.
    /// Si lo es, la excepción **lista esos pacientes**, para que se sepa a quién hay que reasignar
    /// antes de poder dar de baja.</summary>
    void DarDeBaja(int id, int? usuarioQueEjecutaId);

    /// <summary>FR-037: cuántos pacientes activos o en evaluación lo tienen de cabecera.</summary>
    int ContarPacientesDeCabecera(int medicoId);
}
