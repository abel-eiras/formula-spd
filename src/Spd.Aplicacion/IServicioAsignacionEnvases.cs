namespace Spd.Aplicacion;

/// <summary>Asignación y descuento de envases (FR-520/521), invocable por Spec 006 al pasar un
/// SPD a PREPARADO sin que este contrato cambie (research.md Decisión 5 de Spec 005).</summary>
public interface IServicioAsignacionEnvases
{
    ResultadoDescuento Descontar(int tratamientoId, int? unidadesOverride, DateOnly caducidadMinima, int? usuarioQueEjecutaId);
}
