using System.Net;

namespace Spd.Aplicacion.Tests;

/// <summary>Handler de prueba: si <see cref="LanzarFalloDeRed"/> es true, simula que la conexión
/// no responde (sin depender de red real, CA-005); si no, devuelve <see cref="RespuestaJson"/>.</summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    public bool LanzarFalloDeRed { get; set; }
    public string RespuestaJson { get; set; } = "{}";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (LanzarFalloDeRed)
        {
            throw new HttpRequestException("Simulación: la conexión no responde.");
        }

        var respuesta = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RespuestaJson)
        };
        return Task.FromResult(respuesta);
    }
}
