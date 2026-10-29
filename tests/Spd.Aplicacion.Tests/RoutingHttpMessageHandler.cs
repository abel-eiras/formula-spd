using System.Net;

namespace Spd.Aplicacion.Tests;

/// <summary>Handler de prueba que responde por URL absoluta, para simular a la vez la API de GitHub y la
/// descarga de los ficheros de una release sin red real. Lo que no está registrado responde 404, y cada
/// petición queda anotada para comprobar qué se llegó a descargar.</summary>
internal sealed class RoutingHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, byte[]> _respuestas = new();

    public List<string> Peticiones { get; } = [];

    public void Responder(string url, byte[] contenido) => _respuestas[url] = contenido;

    public void Responder(string url, string contenido) => Responder(url, System.Text.Encoding.UTF8.GetBytes(contenido));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri!.ToString();
        Peticiones.Add(url);
        var respuesta = _respuestas.TryGetValue(url, out var contenido)
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(contenido) }
            : new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
        return Task.FromResult(respuesta);
    }
}
