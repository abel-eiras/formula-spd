using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary><paramref name="httpClient"/> debe tener su <c>BaseAddress</c> apuntando a
/// <c>https://cima.aemps.es/cima/rest/</c> en producción (CIMA REST API v1.23, AEMPS). Un CN sin
/// resultado en CIMA (HTTP 204) no es un fallo: significa que ese CN no es un medicamento
/// registrado (p. ej. una fórmula magistral normalizada, que CIMA no indexa), y el alta sigue
/// siendo manual como hasta ahora.</summary>
public sealed class ServicioConsultaCima(HttpClient httpClient, IRegistradorAuditoria auditoria)
    : IServicioConsultaCima
{
    // Vocabulario de formaFarmaceuticaSimplificada confirmado contra la API real (maestra=13):
    // solo estos cuatro valores tienen equivalente exacto en el catálogo cerrado de FR-300; el
    // resto (formas líquidas, efervescentes, bucodispersables, vaginales…) caen a OtraNoApta por
    // defecto, igual que ya dicta FR-301 para esas mismas formas. Gragea/Pastilla/Píldora no
    // existen en el vocabulario de CIMA: siempre quedan de selección manual.
    private static readonly Dictionary<string, FormaFarmaceutica> MapaFormaSimplificada = new(StringComparer.OrdinalIgnoreCase)
    {
        ["COMPRIMIDO"] = FormaFarmaceutica.Comprimido,
        ["COMPRIMIDO LIBERACION MODIFICADA"] = FormaFarmaceutica.ComprimidoLiberacionProlongada,
        ["CAPSULA"] = FormaFarmaceutica.Capsula,
        ["CAPSULA LIBERACION MODIFICADA"] = FormaFarmaceutica.CapsulaLiberacionProlongada,
    };

    public async Task<ResultadoConsultaCima> ConsultarPorCnAsync(string cn)
    {
        try
        {
            using var respuesta = await httpClient.GetAsync($"medicamento?cn={Uri.EscapeDataString(cn)}");
            if (respuesta.StatusCode == HttpStatusCode.NoContent)
            {
                auditoria.Registrar(null, "CONSULTA_CIMA", "Medicamento", null, $"cn={cn} no_encontrado");
                return ResultadoConsultaCima.NoEncontrado();
            }

            respuesta.EnsureSuccessStatusCode();
            var medicamento = await respuesta.Content.ReadFromJsonAsync<MedicamentoCima>();
            if (medicamento is null)
            {
                return ResultadoConsultaCima.NoEncontrado();
            }

            var presentacion = medicamento.Presentaciones?.FirstOrDefault();
            var nombre = presentacion?.Nombre ?? medicamento.Nombre ?? cn;
            var forma = medicamento.FormaFarmaceuticaSimplificada?.Nombre is { } nombreForma
                && MapaFormaSimplificada.TryGetValue(nombreForma, out var formaMapeada)
                ? formaMapeada
                : (FormaFarmaceutica?)null;

            auditoria.Registrar(null, "CONSULTA_CIMA", "Medicamento", null, $"cn={cn} encontrado");
            return ResultadoConsultaCima.Exitoso(
                nombre, medicamento.PrincipiosActivos, medicamento.Laboratorio, forma, ExtraerEnvaseIndicado(presentacion?.Nombre));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            auditoria.Registrar(null, "CONSULTA_CIMA", "Medicamento", null, $"cn={cn} fallo={ex.Message}");
            return ResultadoConsultaCima.Fallido(ex.Message);
        }
    }

    // CIMA no tiene un campo estructurado para el tipo de envase; a veces lo menciona entre
    // paréntesis al final del nombre de la presentación (p. ej. "…14 cápsulas (Blister)"). Es una
    // pista informativa, no un dato fiable al 100%: no todas las presentaciones lo incluyen.
    private static string? ExtraerEnvaseIndicado(string? nombrePresentacion)
    {
        if (nombrePresentacion is null) return null;
        var inicio = nombrePresentacion.LastIndexOf('(');
        var fin = nombrePresentacion.LastIndexOf(')');
        return inicio >= 0 && fin > inicio ? nombrePresentacion[(inicio + 1)..fin] : null;
    }

    private sealed record MedicamentoCima(
        [property: JsonPropertyName("nombre")] string? Nombre,
        [property: JsonPropertyName("pactivos")] string? PrincipiosActivos,
        [property: JsonPropertyName("labtitular")] string? Laboratorio,
        [property: JsonPropertyName("formaFarmaceuticaSimplificada")] FormaFarmaceuticaCima? FormaFarmaceuticaSimplificada,
        [property: JsonPropertyName("presentaciones")] PresentacionCima[]? Presentaciones);

    private sealed record FormaFarmaceuticaCima([property: JsonPropertyName("nombre")] string? Nombre);

    private sealed record PresentacionCima([property: JsonPropertyName("nombre")] string? Nombre);
}
