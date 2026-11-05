namespace Spd.Dominio;

/// <summary>Lectura de la auditoría para comprobar su cadena de sellos (Spec 017). Sin escritura: la
/// única escritura es <see cref="IRegistradorAuditoria.Registrar"/>.</summary>
public interface IRepositorioAuditoria
{
    IReadOnlyList<EntradaAuditoria> ListarEnOrden();
}
