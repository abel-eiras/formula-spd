using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Resultado de una purga (Spec 010 FR-1022): lo que queda tras el borrado es solo la
/// fila de auditoría con el número de ficha y el nombre.</summary>
public sealed record ResultadoPurga(string NumFicha, string NombreCompleto, int FilasEliminadas);

/// <summary>Purga manual de pacientes con baja antigua (Spec 010 §4.3, FR-1020..1024). Solo
/// Administrador, paciente a paciente, con su contraseña; nunca automática (Art. III.2).</summary>
public interface IServicioPurga
{
    /// <summary>Pacientes en BAJA con fecha de baja anterior a hoy menos los años de retención (FR-1020).</summary>
    IReadOnlyList<Paciente> ListarPurgables(DateOnly hoy);

    /// <summary>Verifica la contraseña del administrador (FR-1021), la antigüedad de la baja y que no
    /// queden envases en custodia (FR-1024); audita con número de ficha y nombre antes de borrar (FR-1022).</summary>
    ResultadoPurga Purgar(int pacienteId, int administradorId, string passwordAdministrador, DateOnly hoy);
}
