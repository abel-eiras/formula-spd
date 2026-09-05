namespace Spd.Dominio;

/// <summary>FR-420. `PendienteRevision` bloquea nueva preparación (FR-421, disparo automático
/// diferido a Spec 006 — research.md Decisión 3 de Spec 004).</summary>
public enum EstadoTratamiento
{
    Activo,
    Suspendido,
    Finalizado,
    PendienteRevision
}
