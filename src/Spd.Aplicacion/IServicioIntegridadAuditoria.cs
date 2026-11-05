using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Comprobación de la cadena de sellos de la auditoría (Spec 017 FR-1702). Solo Administrador.</summary>
public interface IServicioIntegridadAuditoria
{
    /// <summary>Recorre toda la auditoría y comprueba sus sellos. Deja traza de la comprobación y de su
    /// resultado (Art. VII.6), después de comprobar, para que la propia traza no altere lo comprobado.</summary>
    ResultadoCadenaAuditoria Comprobar(int? administradorQueEjecutaId);
}
