namespace Spd.Dominio;

/// <summary>Implementación por defecto de <see cref="IComprobadorIdoneidadYConsentimiento"/>
/// mientras no existe Spec 002: siempre aprueba. Sustituir por la implementación real al mergear
/// Spec 002 (research.md Decisión 1 de Spec 006).</summary>
public sealed class ComprobadorIdoneidadYConsentimientoNulo : IComprobadorIdoneidadYConsentimiento
{
    public bool Aprobado(int pacienteId) => true;
}
