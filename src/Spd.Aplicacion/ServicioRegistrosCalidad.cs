using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Registros de calidad de uso diario: formación y residuos (FR-920..FR-930). Toda
/// escritura registra en auditoría (Art. VII.6).</summary>
public sealed class ServicioRegistrosCalidad(
    IRepositorioRegistrosCalidad repositorio, IRegistradorAuditoria auditoria) : IServicioRegistrosCalidad
{
    public FormacionPersonal RegistrarFormacion(DatosFormacion datos, int? usuarioQueEjecutaId)
    {
        var formacion = new FormacionPersonal
        {
            UsuarioId = datos.UsuarioId,
            NombreCurso = datos.NombreCurso,
            EntidadOrganizadora = datos.EntidadOrganizadora,
            Fecha = datos.Fecha,
            Acreditado = datos.Acreditado
        };
        formacion.Id = repositorio.Crear(formacion);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA", "FormacionPersonal", formacion.Id, null);
        return formacion;
    }

    public IReadOnlyList<FormacionPersonal> ListarFormacion(int usuarioId) => repositorio.ListarFormacion(usuarioId);

    public RecogidaResiduos RegistrarRecogidaResiduos(DatosRecogidaResiduos datos, int? usuarioQueEjecutaId)
    {
        var recogida = new RecogidaResiduos
        {
            Fecha = datos.Fecha,
            EmpresaGestora = datos.EmpresaGestora,
            UsuarioId = usuarioQueEjecutaId ?? 0,
            Observaciones = datos.Observaciones
        };
        recogida.Id = repositorio.Crear(recogida);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA", "RecogidaResiduos", recogida.Id, null);
        return recogida;
    }

    public IReadOnlyList<RecogidaResiduos> ListarRecogidaResiduos() => repositorio.ListarRecogidaResiduos();
}
