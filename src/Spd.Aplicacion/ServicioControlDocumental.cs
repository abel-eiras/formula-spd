using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Control de cambios del PNT y control de copias, exclusivo de Administrador
/// (FR-940..FR-942, research.md Decisión 4 de Spec 009). La comprobación de rol se hace aquí,
/// no en la pantalla (Art. VII.4), tanto para escribir como para consultar (CA-903).</summary>
public sealed class ServicioControlDocumental(
    IRepositorioControlDocumental repositorio, IRepositorioUsuarios repositorioUsuarios, IRegistradorAuditoria auditoria)
    : IServicioControlDocumental
{
    public ControlCambiosPNT RegistrarCambioPnt(DatosCambioPnt datos, int administradorQueEjecutaId)
    {
        ExigirAdministrador(administradorQueEjecutaId);

        var cambio = new ControlCambiosPNT
        {
            Documento = datos.Documento,
            Version = datos.Version,
            DescripcionCambio = datos.DescripcionCambio,
            Fecha = datos.Fecha,
            RedactadoPor = datos.RedactadoPor,
            RevisadoPor = datos.RevisadoPor,
            AprobadoPor = datos.AprobadoPor
        };
        cambio.Id = repositorio.Crear(cambio);

        auditoria.Registrar(administradorQueEjecutaId, "ALTA", "ControlCambiosPNT", cambio.Id, null);
        return cambio;
    }

    public IReadOnlyList<ControlCambiosPNT> ListarCambiosPnt(int usuarioQueEjecutaId)
    {
        ExigirAdministrador(usuarioQueEjecutaId);
        return repositorio.ListarCambiosPnt();
    }

    public ControlCopias RegistrarCopia(DatosCopia datos, int administradorQueEjecutaId)
    {
        ExigirAdministrador(administradorQueEjecutaId);

        var copia = new ControlCopias
        {
            Documento = datos.Documento, NumCopia = datos.NumCopia, UsuarioId = datos.UsuarioId, Fecha = datos.Fecha
        };
        copia.Id = repositorio.Crear(copia);

        auditoria.Registrar(administradorQueEjecutaId, "ALTA", "ControlCopias", copia.Id, null);
        return copia;
    }

    public IReadOnlyList<ControlCopias> ListarCopias(int usuarioQueEjecutaId)
    {
        ExigirAdministrador(usuarioQueEjecutaId);
        return repositorio.ListarCopias();
    }

    private void ExigirAdministrador(int usuarioId)
    {
        var usuario = repositorioUsuarios.ObtenerPorId(usuarioId);
        if (usuario is null || usuario.Rol != Rol.Administrador)
        {
            throw new ErrorValidacionException("Solo un Administrador puede acceder al control documental (FR-942).");
        }
    }
}
