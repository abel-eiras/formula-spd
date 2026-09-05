namespace Spd.Aplicacion;

/// <summary>Descarga manual del fichero del nomenclátor (FR-051). Nunca automática (Art. VI.3).</summary>
public interface IServicioNomenclator
{
    Task<ResultadoDescargaNomenclator> DescargarNomenclatorAsync(
        string urlNomenclator, string rutaDestino, int? administradorQueEjecutaId);
}
