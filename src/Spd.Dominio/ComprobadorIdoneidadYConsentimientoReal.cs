namespace Spd.Dominio;

/// <summary>Implementación real del punto de extensión de Spec 006 (Art. I.3: "no se prepara un
/// SPD para un paciente sin consentimiento vigente y evaluación de idoneidad APTO"). Sustituye a
/// <see cref="ComprobadorIdoneidadYConsentimientoNulo"/> en el arranque (Spec 002, research.md
/// Decisión 7).</summary>
public sealed class ComprobadorIdoneidadYConsentimientoReal(
    IRepositorioEvaluacionesIdoneidad evaluaciones, IRepositorioConsentimientos consentimientos)
    : IComprobadorIdoneidadYConsentimiento
{
    public bool Aprobado(int pacienteId)
        => evaluaciones.ObtenerVigente(pacienteId)?.Resultado == ResultadoIdoneidad.Apto
           && consentimientos.ObtenerVigente(pacienteId) is not null;
}
