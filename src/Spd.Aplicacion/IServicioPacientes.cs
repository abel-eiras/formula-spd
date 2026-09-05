using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Ficha de paciente, sus estados y su búsqueda global (FR-001..FR-011). Cualquier
/// Elaborador o Administrador ejecuta todas estas operaciones sin restricción (FR-040, CA-012).</summary>
public interface IServicioPacientes
{
    Paciente Crear(DatosAltaPaciente datos, int? usuarioQueEjecutaId);
    void Actualizar(Paciente paciente, int? usuarioQueEjecutaId);
    void CambiarEstado(int pacienteId, EstadoPaciente nuevoEstado, DatosBaja? datosBaja, int? usuarioQueEjecutaId);
    IReadOnlyList<Paciente> Buscar(string fragmento, EstadoPaciente[]? filtroEstados, int? filtroMedicoId);
    ResultadoValidacionDni ValidarDni(string dni);
    ResultadoValidacionCip ValidarCip(string cip, DateOnly fechaNacimiento, string apellidos, string sexo);
    string AutocompletarCip(DateOnly fechaNacimiento, string apellidos, string sexo);
}
