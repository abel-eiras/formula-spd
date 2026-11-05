using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Comprueba que nadie ha tocado la auditoría por fuera de la aplicación (Spec 017).</summary>
public sealed class ServicioIntegridadAuditoria(IRepositorioAuditoria repositorio, IRegistradorAuditoria auditoria)
    : IServicioIntegridadAuditoria
{
    public ResultadoCadenaAuditoria Comprobar(int? administradorQueEjecutaId)
    {
        var resultado = CadenaAuditoria.Verificar(repositorio.ListarEnOrden());
        auditoria.Registrar(administradorQueEjecutaId, "COMPROBACION", "Auditoria", null,
            resultado.Integra
                ? $"integra: {resultado.Selladas} selladas, {resultado.SinSellar} anteriores al sellado"
                : $"PROBLEMA en la entrada {resultado.PrimeraEntradaConProblema}: {resultado.Problema}");
        return resultado;
    }
}
