namespace Spd.Dominio;

/// <summary>Punto de extensión para FR-530/CA-502: excluir del listado de retirada a un paciente
/// que ya tiene un SPD PREPARADO/VERIFICADO cuya validez cubre su próxima retirada. La entidad SPD
/// es de Spec 006, que no existe todavía (research.md Decisión 3 de Spec 005) — Spec 006 deberá
/// registrar aquí su propia implementación real sin que `ServicioListadoRetirada` cambie.</summary>
public interface IComprobadorCoberturaSpd
{
    bool YaCubierta(int pacienteId, DateOnly proximaRetirada);
}
