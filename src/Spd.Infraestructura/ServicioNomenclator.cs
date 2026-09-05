using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Descarga del fichero del nomenclátor desde una URL configurable (research.md
/// Decisión 5). Solo descarga; el parseo es de Spec 003/011.</summary>
public sealed class ServicioNomenclator(HttpClient httpClient, IRegistradorAuditoria auditoria)
    : IServicioNomenclator
{
    public async Task<ResultadoDescargaNomenclator> DescargarNomenclatorAsync(
        string urlNomenclator, string rutaDestino, int? administradorQueEjecutaId)
    {
        try
        {
            using var respuesta = await httpClient.GetAsync(urlNomenclator);
            respuesta.EnsureSuccessStatusCode();

            await using var flujoDestino = File.Create(rutaDestino);
            await respuesta.Content.CopyToAsync(flujoDestino);

            auditoria.Registrar(administradorQueEjecutaId, "DESCARGA_NOMENCLATOR", "Sistema", null, "exito");
            return new ResultadoDescargaNomenclator(true, rutaDestino, null, DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
        {
            auditoria.Registrar(administradorQueEjecutaId, "DESCARGA_NOMENCLATOR", "Sistema", null, $"fallo={ex.Message}");
            return new ResultadoDescargaNomenclator(false, null, ex.Message, DateTime.UtcNow);
        }
    }
}
