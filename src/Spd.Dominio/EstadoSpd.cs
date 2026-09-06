namespace Spd.Dominio;

/// <summary>Ciclo de vida de un blíster (FR-604). Un SPD = un blíster, nunca una hoja para
/// dos.</summary>
public enum EstadoSpd
{
    Borrador,
    Preparado,
    Verificado,
    Entregado,
    Anulado
}
