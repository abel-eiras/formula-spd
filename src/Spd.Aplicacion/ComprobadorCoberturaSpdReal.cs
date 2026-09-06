using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Implementación real de FR-530/CA-502 (Spec 005): excluye del listado de retirada a un
/// paciente que ya tiene un SPD en PREPARADO o VERIFICADO cuya validez cubre su próxima retirada.
/// Sustituye a `ComprobadorCoberturaSpdNulo` ahora que la entidad SPD existe (Spec 006,
/// research.md Decisión 3 de Spec 005).</summary>
public sealed class ComprobadorCoberturaSpdReal(IRepositorioSpd repositorioSpd) : IComprobadorCoberturaSpd
{
    public bool YaCubierta(int pacienteId, DateOnly proximaRetirada)
        => repositorioSpd.Listar(null, pacienteId, null)
            .Any(s => (s.Estado == EstadoSpd.Preparado || s.Estado == EstadoSpd.Verificado)
                      && proximaRetirada >= s.ValidezDesde && proximaRetirada <= s.ValidezHasta);
}
