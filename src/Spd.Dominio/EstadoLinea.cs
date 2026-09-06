namespace Spd.Dominio;

/// <summary>Estado de una línea de SPD (FR-614, FR-673). `EnvasePendiente` bloquea solo esa línea
/// en la continuidad (research.md Decisión 5 de Spec 006).</summary>
public enum EstadoLinea
{
    Normal,
    Excluida,
    EnvasePendiente
}
