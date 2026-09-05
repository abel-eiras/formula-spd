namespace Spd.Dominio;

/// <summary>Implementación por defecto de <see cref="IComprobadorCoberturaSpd"/> mientras no
/// existe la entidad SPD (Spec 006): nunca excluye a nadie del listado de retirada por este
/// motivo. Sustituir por la implementación real al mergear Spec 006 (research.md Decisión 3 de
/// Spec 005).</summary>
public sealed class ComprobadorCoberturaSpdNulo : IComprobadorCoberturaSpd
{
    public bool YaCubierta(int pacienteId, DateOnly proximaRetirada) => false;
}
