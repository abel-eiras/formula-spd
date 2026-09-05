using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Asignación y descuento de envases (FR-520/521): agota primero el envase con menos
/// unidades restantes; el sobrante nunca se descarta.</summary>
public sealed class ServicioAsignacionEnvases(
    IRepositorioEnvases repositorioEnvases, IRepositorioTratamientos repositorioTratamientos, IRegistradorAuditoria auditoria)
    : IServicioAsignacionEnvases
{
    public ResultadoDescuento Descontar(int tratamientoId, int? unidadesOverride, DateOnly caducidadMinima, int? usuarioQueEjecutaId)
    {
        var tratamiento = repositorioTratamientos.ObtenerPorId(tratamientoId)
            ?? throw new ErrorValidacionException($"No existe el tratamiento {tratamientoId}.");

        var unidadesADescontar = unidadesOverride ?? CalculadoraUnidadesADescontar.Calcular(tratamiento);

        // FR-520: ya vienen ordenados por unidades_restantes asc, caducidad asc (RepositorioEnvases).
        var candidatos = repositorioEnvases
            .ListarEnCustodiaDePacienteYMedicamento(tratamiento.PacienteId, tratamiento.MedicamentoId)
            .Where(e => e.Caducidad is { } caducidad && caducidad >= caducidadMinima)
            .ToList();

        if (candidatos.Sum(e => e.UnidadesRestantes ?? 0) < unidadesADescontar)
            throw new ErrorValidacionException(
                "No hay envases suficientes en custodia con caducidad válida para completar el descuento.");

        var asignaciones = new List<AsignacionEnvase>();
        var pendiente = unidadesADescontar;
        foreach (var envase in candidatos)
        {
            if (pendiente <= 0) break;

            var disponibles = envase.UnidadesRestantes ?? 0;
            var tomadas = Math.Min(disponibles, pendiente);

            envase.UnidadesRestantes = disponibles - tomadas;
            // FR-521: el sobrante permanece EN_CUSTODIA; solo se agota cuando llega a 0.
            if (envase.UnidadesRestantes == 0) envase.Estado = EstadoEnvase.Agotado;
            repositorioEnvases.Actualizar(envase);

            asignaciones.Add(new AsignacionEnvase(envase, tomadas));
            pendiente -= tomadas;
        }

        auditoria.Registrar(usuarioQueEjecutaId, "DESCUENTO_ENVASE", "Tratamiento", tratamientoId, $"unidades={unidadesADescontar}");
        return new ResultadoDescuento(unidadesADescontar, asignaciones);
    }
}
