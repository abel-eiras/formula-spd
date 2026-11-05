using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Alertas de calidad y trazabilidad inversa (Spec 018). Ante una retirada de lote, dice en
/// segundos qué cajas hay en el depósito, qué blísteres sin entregar hay que rehacer y a qué pacientes
/// se les entregó un blíster con unidades de ese lote: es para lo que se registra el lote de cada caja
/// (PNT, trazabilidad; Spec 005 FR-510).</summary>
public sealed class ServicioAlertasCalidad(
    IRepositorioAlertasCalidad repositorio, IRepositorioMedicamentos repositorioMedicamentos, IRegistradorAuditoria auditoria)
    : IServicioAlertasCalidad
{
    private static readonly char[] SeparadoresLotes = [',', ';', '\n', '\r'];

    public IReadOnlyList<AlertaCalidad> Registrar(DatosAltaAlertaCalidad datos, int? usuarioId)
    {
        if (string.IsNullOrWhiteSpace(datos.Referencia))
            throw new ErrorValidacionException("Indica la referencia de la alerta (número de la nota de la AEMPS, del laboratorio o del Colegio).");
        var cn = AlertaCalidad.NormalizarCn(datos.Cn);
        if (cn.Length != 6)
            throw new ErrorValidacionException("El CN debe tener seis dígitos (o siete con el dígito de control).");
        var lotes = datos.Lotes.Split(SeparadoresLotes, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => AlertaCalidad.NormalizarLote(l).Length > 0)
            .DistinctBy(AlertaCalidad.NormalizarLote)
            .ToList();
        if (lotes.Count == 0)
            throw new ErrorValidacionException("Indica al menos un lote afectado.");

        var creadas = new List<AlertaCalidad>();
        foreach (var lote in lotes)
        {
            var alerta = new AlertaCalidad
            {
                Referencia = datos.Referencia.Trim(), FechaAlerta = datos.FechaAlerta, Cn = cn, Lote = lote,
                Descripcion = string.IsNullOrWhiteSpace(datos.Descripcion) ? null : datos.Descripcion.Trim()
            };
            alerta.Id = repositorio.Crear(alerta, usuarioId);
            auditoria.Registrar(usuarioId, "ALTA", "AlertaCalidad", alerta.Id, $"{alerta.Referencia}: CN {cn}, lote {lote}");
            creadas.Add(alerta);
        }
        return creadas;
    }

    public void Cerrar(int alertaId, string motivo, DateOnly hoy, int? usuarioId)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ErrorValidacionException("Indica el motivo del cierre (por ejemplo, «cajas retiradas y blísteres rehechos»).");
        var alerta = repositorio.ObtenerPorId(alertaId) ?? throw new ErrorValidacionException("La alerta no existe.");
        if (!alerta.Activa) throw new ErrorValidacionException("La alerta ya está cerrada.");
        repositorio.Cerrar(alertaId, hoy, motivo.Trim(), usuarioId);
        auditoria.Registrar(usuarioId, "CIERRE", "AlertaCalidad", alertaId, motivo.Trim());
    }

    public IReadOnlyList<AlertaCalidadFila> Listar(bool soloActivas)
        => repositorio.Listar(soloActivas)
            .Select(a => new AlertaCalidadFila(a, repositorioMedicamentos.ObtenerPorCn(a.Cn)?.Nombre))
            .ToList();

    public TrazabilidadAlerta Trazar(int alertaId)
    {
        var alerta = repositorio.ObtenerPorId(alertaId) ?? throw new ErrorValidacionException("La alerta no existe.");
        return new TrazabilidadAlerta(alerta, repositorio.EnvasesAfectados(alerta), repositorio.BlisteresAfectados(alerta));
    }
}
