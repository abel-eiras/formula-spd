using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Comprobación de actualizaciones vía GitHub Releases (research.md Decisión 3,
/// Clarifications Q1). <paramref name="httpClient"/> debe tener su <c>BaseAddress</c> apuntando a
/// <c>https://api.github.com/</c> en producción; en tests se apunta a una dirección que no
/// responde para simular el fallo (CA-005 aplicado también aquí, no solo al nomenclátor).</summary>
public sealed class ServicioActualizaciones(HttpClient httpClient, IRegistradorAuditoria auditoria)
    : IServicioActualizaciones
{
    private const string RutaReleases = "repos/abel-eiras/spd/releases/latest";

    public async Task<ResultadoComprobacionActualizacion> ComprobarActualizacionesAsync(
        string versionInstalada, int? administradorQueEjecutaId)
    {
        try
        {
            using var respuesta = await httpClient.GetAsync(RutaReleases);
            respuesta.EnsureSuccessStatusCode();
            var release = await respuesta.Content.ReadFromJsonAsync<ReleaseGitHub>();

            var hayNueva = release?.TagName is not null && release.TagName != versionInstalada;
            Registrar(administradorQueEjecutaId, $"hayNueva={hayNueva}");
            return new ResultadoComprobacionActualizacion(
                true, hayNueva, release?.TagName, release?.HtmlUrl, null, DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Registrar(administradorQueEjecutaId, $"fallo={ex.Message}");
            return new ResultadoComprobacionActualizacion(false, false, null, null, ex.Message, DateTime.UtcNow);
        }
    }

    private void Registrar(int? administradorId, string detalle)
        => auditoria.Registrar(administradorId, "COMPROBAR_ACTUALIZACIONES", "Sistema", null, detalle);

    private sealed record ReleaseGitHub(
        [property: JsonPropertyName("tag_name")] string? TagName,
        [property: JsonPropertyName("html_url")] string? HtmlUrl);
}
