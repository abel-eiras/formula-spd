using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Control de cambios del PNT y control de copias, exclusivo de Administrador
/// (FR-940..FR-942). Toda escritura registra en auditoría (Art. VII.6).</summary>
public interface IServicioControlDocumental
{
    ControlCambiosPNT RegistrarCambioPnt(DatosCambioPnt datos, int administradorQueEjecutaId);
    IReadOnlyList<ControlCambiosPNT> ListarCambiosPnt(int usuarioQueEjecutaId);

    ControlCopias RegistrarCopia(DatosCopia datos, int administradorQueEjecutaId);
    IReadOnlyList<ControlCopias> ListarCopias(int usuarioQueEjecutaId);
}
