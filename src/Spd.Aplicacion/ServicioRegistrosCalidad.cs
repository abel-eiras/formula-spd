using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Registros de calidad de uso diario: ambiental, limpieza, formación y residuos
/// (FR-900..FR-930, FR-950). Toda escritura registra en auditoría (Art. VII.6).</summary>
public sealed class ServicioRegistrosCalidad(
    IRepositorioRegistrosCalidad repositorio, IRepositorioFarmacia repositorioFarmacia, IRegistradorAuditoria auditoria)
    : IServicioRegistrosCalidad
{
    public RegistroAmbiental RegistrarAmbiental(DatosRegistroAmbiental datos, int? usuarioQueEjecutaId)
    {
        var farmacia = repositorioFarmacia.Obtener()
            ?? throw new ErrorValidacionException("No hay configuración de farmacia (Spec 000).");

        var registro = new RegistroAmbiental
        {
            FechaHora = datos.FechaHora ?? DateTime.UtcNow,
            Temperatura = datos.Temperatura,
            Humedad = datos.Humedad,
            UsuarioId = usuarioQueEjecutaId ?? 0,
            Observaciones = datos.Observaciones,
            // Congelado al registrar con los rangos vigentes; nunca se recalcula (Art. IV, CA-900).
            FueraDeRango = datos.Temperatura < farmacia.TempMin || datos.Temperatura > farmacia.TempMax
                           || datos.Humedad < farmacia.HrMin || datos.Humedad > farmacia.HrMax
        };
        registro.Id = repositorio.Crear(registro);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA", "RegistroAmbiental", registro.Id, null);
        return registro;
    }

    public IReadOnlyList<RegistroAmbiental> ListarAmbiental() => repositorio.ListarAmbiental();

    public RegistroLimpieza RegistrarLimpieza(TipoLimpieza tipo, string? observaciones, int? usuarioQueEjecutaId)
    {
        var registro = new RegistroLimpieza
        {
            Fecha = DateTime.UtcNow,
            UsuarioId = usuarioQueEjecutaId ?? 0,
            Tipo = tipo,
            Observaciones = observaciones
        };
        registro.Id = repositorio.Crear(registro);

        auditoria.Registrar(usuarioQueEjecutaId, "ALTA", "RegistroLimpieza", registro.Id, null);
        return registro;
    }

    public IReadOnlyList<RegistroLimpieza> ListarLimpieza() => repositorio.ListarLimpieza();

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

    public ResultadoAvisoRegistros ComprobarAvisos()
    {
        var farmacia = repositorioFarmacia.Obtener()
            ?? throw new ErrorValidacionException("No hay configuración de farmacia (Spec 000).");
        var ahora = DateTime.UtcNow;

        var diasSinAmbiental = DiasDesdeElUltimo(repositorio.ListarAmbiental().Select(r => r.FechaHora), ahora);
        var diasSinLimpieza = DiasDesdeElUltimo(
            repositorio.ListarLimpieza().Where(r => r.Tipo == TipoLimpieza.Rutinaria).Select(r => r.Fecha), ahora);

        return new ResultadoAvisoRegistros(
            diasSinAmbiental > farmacia.UmbralDiasAvisoCalidad,
            diasSinLimpieza > farmacia.UmbralDiasAvisoCalidad,
            diasSinAmbiental, diasSinLimpieza);
    }

    private static int DiasDesdeElUltimo(IEnumerable<DateTime> fechas, DateTime ahora)
    {
        var ultima = fechas.OrderByDescending(f => f).Cast<DateTime?>().FirstOrDefault();
        return ultima is null ? int.MaxValue : (int)(ahora - ultima.Value).TotalDays;
    }
}
