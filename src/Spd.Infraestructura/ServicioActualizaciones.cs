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

            var hayNueva = EsMasNueva(release?.TagName, versionInstalada);
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

    /// <summary>¿La release publicada es **posterior** a la instalada?
    ///
    /// Antes esto era `tagName != versionInstalada`, que es distinto y peor: cualquier etiqueta que no
    /// coincidiera carácter a carácter contaba como novedad. Con la etiqueta `v0.1.0` y la versión
    /// instalada `0.1.0` —la forma habitual de etiquetar en GitHub— la aplicación habría ofrecido
    /// actualizarse a su propia versión, para siempre. Y una etiqueta *anterior* también se habría
    /// anunciado como nueva.
    ///
    /// Se admite la `v` inicial de las etiquetas y se comparan como versiones. Si alguna no se puede
    /// interpretar (una etiqueta como `beta-gallega`), se cae a la comparación textual anterior: es
    /// menos preciso, pero es honesto y no se inventa un orden que no existe.</summary>
    public static bool EsMasNueva(string? etiquetaRelease, string? versionInstalada)
    {
        if (string.IsNullOrWhiteSpace(etiquetaRelease)) return false;
        if (string.IsNullOrWhiteSpace(versionInstalada)) return true;

        if (Version.TryParse(Normalizar(etiquetaRelease), out var deLaRelease)
            && Version.TryParse(Normalizar(versionInstalada), out var instalada))
        {
            return deLaRelease > instalada;
        }

        return !string.Equals(etiquetaRelease.Trim(), versionInstalada.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Quita la `v` de las etiquetas y lo que venga tras `+` o `-` (metadatos de compilación
    /// y sufijos de preliberación), que `Version` no sabe interpretar.</summary>
    private static string Normalizar(string valor)
    {
        var limpio = valor.Trim().TrimStart('v', 'V');
        var corte = limpio.IndexOfAny(['+', '-']);
        return corte >= 0 ? limpio[..corte] : limpio;
    }

    private void Registrar(int? administradorId, string detalle)
        => auditoria.Registrar(administradorId, "COMPROBAR_ACTUALIZACIONES", "Sistema", null, detalle);

    private sealed record ReleaseGitHub(
        [property: JsonPropertyName("tag_name")] string? TagName,
        [property: JsonPropertyName("html_url")] string? HtmlUrl);
}
