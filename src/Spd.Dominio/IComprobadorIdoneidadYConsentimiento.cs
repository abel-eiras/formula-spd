namespace Spd.Dominio;

/// <summary>Punto de extensión para FR-602/Constitución Art. I.3: "no se prepara un SPD para un
/// paciente sin consentimiento vigente y evaluación de idoneidad APTO". La entidad real de
/// idoneidad/consentimiento es de Spec 002, que no existe todavía (research.md Decisión 1 de
/// Spec 006) — Spec 002 deberá registrar aquí su propia implementación sin que
/// `ServicioPreparacion` cambie.</summary>
public interface IComprobadorIdoneidadYConsentimiento
{
    bool Aprobado(int pacienteId);
}
