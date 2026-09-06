using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Perfiles genéricos de importación/exportación (FR-1100).</summary>
public sealed class ServicioPerfilesImportacion(IRepositorioPerfilesImportacion repositorio, IRegistradorAuditoria auditoria)
    : IServicioPerfilesImportacion
{
    public PerfilImportacion Crear(DatosAltaPerfilImportacion datos, int? usuarioQueEjecutaId)
    {
        var perfil = new PerfilImportacion
        {
            Nombre = datos.Nombre,
            Tipo = datos.Tipo,
            Separador = datos.Separador,
            Codificacion = datos.Codificacion,
            TieneCabecera = datos.TieneCabecera,
            Mapeo = datos.Mapeo,
            RegexUnidadesEnvase = datos.RegexUnidadesEnvase
        };
        perfil.Id = repositorio.Crear(perfil);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA_PERFIL_IMPORTACION", "PerfilImportacion", perfil.Id, $"tipo={datos.Tipo}");
        return perfil;
    }

    public void Actualizar(int perfilId, DatosAltaPerfilImportacion datos, int? usuarioQueEjecutaId)
    {
        var perfil = repositorio.ObtenerPorId(perfilId)
            ?? throw new ErrorValidacionException($"No existe el perfil {perfilId}.");

        perfil.Tipo = datos.Tipo;
        perfil.Separador = datos.Separador;
        perfil.Codificacion = datos.Codificacion;
        perfil.TieneCabecera = datos.TieneCabecera;
        perfil.Mapeo = datos.Mapeo;
        perfil.RegexUnidadesEnvase = datos.RegexUnidadesEnvase;
        repositorio.Actualizar(perfil);

        auditoria.Registrar(usuarioQueEjecutaId, "ACTUALIZAR_PERFIL_IMPORTACION", "PerfilImportacion", perfilId, null);
    }

    public PerfilImportacion? ObtenerPorNombre(string nombre) => repositorio.ObtenerPorNombre(nombre);

    public IReadOnlyList<PerfilImportacion> ListarPorTipo(TipoPerfilImportacion tipo) => repositorio.ListarPorTipo(tipo);
}
