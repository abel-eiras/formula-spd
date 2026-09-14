using System.Net.Http.Json;
using System.Text.Json;
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
    /// <summary>La lista de releases, no `releases/latest`: esa ruta **omite las preliberaciones**, y las
    /// primeras versiones son betas. Con `latest` una farmacia probadora nunca se habría enterado de la
    /// siguiente beta.</summary>
    private const string RutaReleases = "repos/abel-eiras/formula-spd/releases?per_page=30";

    public async Task<ResultadoComprobacionActualizacion> ComprobarActualizacionesAsync(
        string versionInstalada, int? administradorQueEjecutaId)
    {
        try
        {
            // La API de GitHub rechaza las peticiones sin User-Agent.
            using var peticion = new HttpRequestMessage(HttpMethod.Get, RutaReleases);
            peticion.Headers.UserAgent.ParseAdd("SPD-farmacia");
            peticion.Headers.Accept.ParseAdd("application/vnd.github+json");
            using var respuesta = await httpClient.SendAsync(peticion);
            respuesta.EnsureSuccessStatusCode();
            var releases = await respuesta.Content.ReadFromJsonAsync<List<ReleaseGitHub>>() ?? [];

            // La más alta de las publicadas; los borradores no cuentan.
            ReleaseGitHub? release = null;
            foreach (var candidata in releases.Where(r => !r.Draft && !string.IsNullOrWhiteSpace(r.TagName)))
            {
                if (release is null || EsMasNueva(candidata.TagName, release.TagName)) release = candidata;
            }

            var hayNueva = EsMasNueva(release?.TagName, versionInstalada);
            Registrar(administradorQueEjecutaId, $"hayNueva={hayNueva}");
            return new ResultadoComprobacionActualizacion(
                true, hayNueva, release?.TagName, release?.HtmlUrl, null, DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            Registrar(administradorQueEjecutaId, $"fallo={ex.Message}");
            return new ResultadoComprobacionActualizacion(false, false, null, null, ex.Message, DateTime.UtcNow);
        }
    }

    /// <summary>¿La release publicada es **posterior** a la instalada?
    ///
    /// Antes esto era `tagName != versionInstalada`, que es distinto y peor: con la etiqueta `v0.1.0` y la
    /// versión instalada `0.1.0` la aplicación habría ofrecido actualizarse a su propia versión, para
    /// siempre, y una etiqueta *anterior* también se habría anunciado como nueva.
    ///
    /// Se comparan como versiones semánticas: se admite la `v` de las etiquetas, se ignoran los metadatos
    /// de compilación (`+hash`) y **una preliberación va antes que su versión final**: quien tiene la
    /// `0.1.0-beta` debe ver la `0.1.0` como novedad, y la `0.1.0-beta.2` también. Si alguna no se puede
    /// interpretar (`beta-gallega`), se cae a la comparación textual: menos precisa, pero honesta.</summary>
    public static bool EsMasNueva(string? etiquetaRelease, string? versionInstalada)
    {
        if (string.IsNullOrWhiteSpace(etiquetaRelease)) return false;
        if (string.IsNullOrWhiteSpace(versionInstalada)) return true;

        if (VersionSemantica.TryLeer(etiquetaRelease, out var deLaRelease)
            && VersionSemantica.TryLeer(versionInstalada, out var instalada))
        {
            return VersionSemantica.Comparar(deLaRelease, instalada) > 0;
        }

        return !string.Equals(etiquetaRelease.Trim(), versionInstalada.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Lo justo de SemVer 2.0 para ordenar releases: núcleo numérico y preliberación.</summary>
    private readonly record struct VersionSemantica(Version Nucleo, string[] Preliberacion)
    {
        public static bool TryLeer(string valor, out VersionSemantica version)
        {
            version = default;
            var limpio = valor.Trim().TrimStart('v', 'V');
            var mas = limpio.IndexOf('+');
            if (mas >= 0) limpio = limpio[..mas];

            var guion = limpio.IndexOf('-');
            var nucleo = guion >= 0 ? limpio[..guion] : limpio;
            var preliberacion = guion >= 0 ? limpio[(guion + 1)..].Split('.') : [];
            if (!Version.TryParse(nucleo, out var numerica) || preliberacion.Any(string.IsNullOrEmpty)) return false;

            version = new VersionSemantica(numerica, preliberacion);
            return true;
        }

        public static int Comparar(VersionSemantica a, VersionSemantica b)
        {
            var porNucleo = a.Nucleo.CompareTo(b.Nucleo);
            if (porNucleo != 0) return porNucleo;

            // Sin preliberación es la versión final, que va después de cualquiera de sus betas.
            if (a.Preliberacion.Length == 0 || b.Preliberacion.Length == 0)
                return b.Preliberacion.Length.CompareTo(a.Preliberacion.Length) switch { 0 => 0, var x => x };

            for (var i = 0; i < Math.Min(a.Preliberacion.Length, b.Preliberacion.Length); i++)
            {
                var (x, y) = (a.Preliberacion[i], b.Preliberacion[i]);
                var xNum = long.TryParse(x, out var nx);
                var yNum = long.TryParse(y, out var ny);
                var orden = (xNum, yNum) switch
                {
                    (true, true) => nx.CompareTo(ny),
                    (true, false) => -1,   // los identificadores numéricos van antes que los de texto
                    (false, true) => 1,
                    _ => string.CompareOrdinal(x, y)
                };
                if (orden != 0) return orden;
            }

            // «beta» va antes que «beta.2»: con los mismos primeros campos, gana la que tiene más.
            return a.Preliberacion.Length.CompareTo(b.Preliberacion.Length);
        }
    }

    private void Registrar(int? administradorId, string detalle)
        => auditoria.Registrar(administradorId, "COMPROBAR_ACTUALIZACIONES", "Sistema", null, detalle);

    private sealed record ReleaseGitHub(
        [property: JsonPropertyName("tag_name")] string? TagName,
        [property: JsonPropertyName("html_url")] string? HtmlUrl,
        [property: JsonPropertyName("draft")] bool Draft);
}
