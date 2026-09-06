namespace Spd.Dominio;

/// <summary>Acceso a EvaluacionIdoneidad. Solo alta (FR-202, Art. III).</summary>
public interface IRepositorioEvaluacionesIdoneidad
{
    int Crear(EvaluacionIdoneidad evaluacion);
    EvaluacionIdoneidad? ObtenerPorId(int id);
    IReadOnlyList<EvaluacionIdoneidad> ListarDePaciente(int pacienteId);
    /// <summary>La más reciente del paciente, o null si nunca se le evaluó.</summary>
    EvaluacionIdoneidad? ObtenerVigente(int pacienteId);
}
