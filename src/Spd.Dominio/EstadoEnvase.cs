namespace Spd.Dominio;

/// <summary>Ciclo de vida de un envase en custodia (FR-511). Sin estado "devuelto al stock": esa
/// operación no existe (FR-544, CA-509).</summary>
public enum EstadoEnvase
{
    EnCustodia,
    Agotado,
    ResiduoSigre,
    EntregadoPaciente
}
