using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Instala una versión nueva desde la propia aplicación (FR-053), con el mismo modelo que
/// formula-care: descargar, verificar la firma, copia de seguridad, sustituir y reiniciar (el reinicio lo
/// hace la Presentación cuando la aplicación ya se ha cerrado).
///
/// Orden deliberado: todo lo que puede fallar por la red o por un paquete defectuoso ocurre **antes** de
/// tocar nada, y cualquier fallo en esa parte deja la instalación intacta. Solo con el paquete verificado y
/// extraído se hace la copia de seguridad, y solo si la copia sale bien se sustituyen ficheros. Esa última
/// parte es síncrona: entre el primer renombrado y el último no hay ningún `await`, así que el usuario no
/// puede cerrar la aplicación a medio sustituir.
///
/// Red solo aquí y solo por acción explícita del usuario (Art. VI.2/VI.3).</summary>
public sealed class ServicioInstalacionActualizaciones(
    HttpClient httpClient,
    IServicioBackup servicioBackup,
    IRegistradorAuditoria auditoria,
    EntornoInstalacion entorno,
    string clavePublicaPem) : IServicioInstalacionActualizaciones
{
    private const string RutaReleasePorEtiqueta = "repos/abel-eiras/formula-spd/releases/tags/";
    private const string FicheroSumas = "SHA256SUMS.txt";
    private const string FicheroFirma = "SHA256SUMS.txt.sig";
    private const string NadaCambiado = "No se ha cambiado nada.";

    public async Task<ResultadoInstalacionActualizacion> InstalarAsync(
        string etiqueta, int? administradorQueEjecutaId, IProgress<int>? progreso, CancellationToken cancelacion)
    {
        if (entorno.MotivoParaNoInstalar() is { } motivoEntorno)
            return Registrar(administradorQueEjecutaId, etiqueta, ResultadoInstalacionActualizacion.SoloADescargarAMano(motivoEntorno));

        var temporal = entorno.CarpetaTemporal;
        try
        {
            PrepararCarpetaTemporal(temporal);
            var resultado = await DescargarVerificarEInstalarAsync(etiqueta, administradorQueEjecutaId, temporal, progreso, cancelacion);
            return Registrar(administradorQueEjecutaId, etiqueta, resultado);
        }
        catch (OperationCanceledException) when (cancelacion.IsCancellationRequested)
        {
            return Registrar(administradorQueEjecutaId, etiqueta, ResultadoInstalacionActualizacion.Fallida($"Actualización cancelada. {NadaCambiado}"));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return Registrar(administradorQueEjecutaId, etiqueta, ResultadoInstalacionActualizacion.Fallida(
                $"No se pudo descargar la actualización; comprueba la conexión a internet ({ex.Message}). {NadaCambiado}"));
        }
        catch (UnauthorizedAccessException)
        {
            return Registrar(administradorQueEjecutaId, etiqueta, ResultadoInstalacionActualizacion.SoloADescargarAMano(
                $"No se puede escribir en la carpeta del programa ({entorno.CarpetaInstalacion}). "
                + $"Muévela a una carpeta tuya, como indica el LEEME, o actualiza a mano. {NadaCambiado}"));
        }
        catch (SustitucionFallidaException ex)
        {
            return Registrar(administradorQueEjecutaId, etiqueta, ResultadoInstalacionActualizacion.Fallida(ex.Message));
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException)
        {
            return Registrar(administradorQueEjecutaId, etiqueta, ResultadoInstalacionActualizacion.Fallida(
                $"No se pudo preparar la actualización ({ex.Message}). {NadaCambiado}"));
        }
        finally
        {
            BorrarCarpetaTemporal(temporal);
        }
    }

    private async Task<ResultadoInstalacionActualizacion> DescargarVerificarEInstalarAsync(
        string etiqueta, int? administradorId, string temporal, IProgress<int>? progreso, CancellationToken cancelacion)
    {
        var assets = await ObtenerAssetsAsync(etiqueta, cancelacion);
        var paquete = assets.FirstOrDefault(a => entorno.EsPaqueteDeEstaPlataforma(a.Nombre));
        var sumas = assets.FirstOrDefault(a => a.Nombre == FicheroSumas);
        var firma = assets.FirstOrDefault(a => a.Nombre == FicheroFirma);
        if (paquete is null)
            return ResultadoInstalacionActualizacion.SoloADescargarAMano($"La versión {etiqueta} no tiene paquete para este sistema. {NadaCambiado}");
        if (sumas is null || firma is null)
            return ResultadoInstalacionActualizacion.SoloADescargarAMano(
                $"La versión {etiqueta} se publicó sin firma, así que no se puede instalar desde aquí. Descárgala e instálala a mano. {NadaCambiado}");

        var sumaEsperada = await LeerSumaFirmadaAsync(sumas, firma, paquete.Nombre, cancelacion);
        if (sumaEsperada is null)
            return ResultadoInstalacionActualizacion.Fallida(
                $"La firma de la versión {etiqueta} no es válida: el paquete podría no ser auténtico y no se instalará. {NadaCambiado}");

        var rutaPaquete = Path.Combine(temporal, paquete.Nombre);
        await DescargarPaqueteAsync(paquete, rutaPaquete, progreso, cancelacion);
        var carpetaNueva = await Task.Run(() => VerificarYExtraer(rutaPaquete, sumaEsperada, temporal), cancelacion);
        if (carpetaNueva is null)
            return ResultadoInstalacionActualizacion.Fallida(
                $"El paquete descargado está dañado o incompleto (no coincide con su suma de comprobación). {NadaCambiado}");

        cancelacion.ThrowIfCancellationRequested(); // última ocasión: a partir de aquí ya no se cancela
        return Sustituir(etiqueta, administradorId, carpetaNueva, temporal);
    }

    /// <summary>La copia de seguridad justo antes de sustituir: si la versión nueva diera un problema con la
    /// base de datos al migrarla, esa copia es el estado exacto de antes (Art. VI.6).</summary>
    private ResultadoInstalacionActualizacion Sustituir(string etiqueta, int? administradorId, string carpetaNueva, string temporal)
    {
        var copia = servicioBackup.GenerarBackup(administradorId, esAutomatico: false);
        if (!copia.Exito)
            return ResultadoInstalacionActualizacion.Fallida(
                $"No se pudo hacer la copia de seguridad previa ({copia.Motivo}), así que no se ha instalado nada.");

        SustitucionFicherosInstalacion.Sustituir(
            carpetaNueva, entorno.CarpetaInstalacion, entorno.NombreEjecutable, Path.Combine(temporal, "respaldo"));
        return ResultadoInstalacionActualizacion.Exitosa(etiqueta, Path.Combine(entorno.CarpetaInstalacion, entorno.NombreEjecutable));
    }

    /// <summary>La suma del paquete según un `SHA256SUMS.txt` cuya firma se ha comprobado; <c>null</c> si la
    /// firma no es válida o el paquete no aparece en él (en ambos casos no hay nada fiable con que comparar).</summary>
    private async Task<string?> LeerSumaFirmadaAsync(AssetGitHub sumas, AssetGitHub firma, string nombrePaquete, CancellationToken cancelacion)
    {
        var contenidoSumas = await DescargarBytesAsync(sumas.Url, cancelacion);
        var contenidoFirma = await DescargarBytesAsync(firma.Url, cancelacion);
        if (!VerificadorFirmaActualizaciones.FirmaValida(contenidoSumas, contenidoFirma, clavePublicaPem)) return null;
        return VerificadorFirmaActualizaciones.SumaDe(Encoding.UTF8.GetString(contenidoSumas), nombrePaquete);
    }

    private string? VerificarYExtraer(string rutaPaquete, string sumaEsperada, string temporal)
    {
        var sumaReal = VerificadorFirmaActualizaciones.Sha256DeFichero(rutaPaquete);
        if (!string.Equals(sumaReal, sumaEsperada, StringComparison.OrdinalIgnoreCase)) return null;

        return ExtractorPaqueteActualizacion.Extraer(rutaPaquete, Path.Combine(temporal, "extraido"), entorno.NombreEjecutable)
            ?? throw new InvalidDataException($"el paquete no contiene {entorno.NombreEjecutable}");
    }

    private async Task<List<AssetGitHub>> ObtenerAssetsAsync(string etiqueta, CancellationToken cancelacion)
    {
        using var peticion = Peticion(RutaReleasePorEtiqueta + Uri.EscapeDataString(etiqueta), "application/vnd.github+json");
        using var respuesta = await httpClient.SendAsync(peticion, cancelacion);
        respuesta.EnsureSuccessStatusCode();
        var release = await respuesta.Content.ReadFromJsonAsync<ReleaseGitHub>(cancelacion);
        return release?.Assets?.Where(a => !string.IsNullOrWhiteSpace(a.Nombre) && !string.IsNullOrWhiteSpace(a.Url)).ToList() ?? [];
    }

    private async Task<byte[]> DescargarBytesAsync(string url, CancellationToken cancelacion)
    {
        using var peticion = Peticion(url, "application/octet-stream");
        using var respuesta = await httpClient.SendAsync(peticion, cancelacion);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadAsByteArrayAsync(cancelacion);
    }

    /// <summary>Por trozos, informando del porcentaje: el paquete ronda las decenas de megas y en la conexión
    /// de una farmacia puede tardar lo bastante como para que una barra quieta parezca un cuelgue.</summary>
    private async Task DescargarPaqueteAsync(AssetGitHub paquete, string ruta, IProgress<int>? progreso, CancellationToken cancelacion)
    {
        using var peticion = Peticion(paquete.Url, "application/octet-stream");
        using var respuesta = await httpClient.SendAsync(peticion, HttpCompletionOption.ResponseHeadersRead, cancelacion);
        respuesta.EnsureSuccessStatusCode();
        var total = respuesta.Content.Headers.ContentLength ?? paquete.Tamano;

        await using var origen = await respuesta.Content.ReadAsStreamAsync(cancelacion);
        await using var destino = File.Create(ruta);
        var bufer = new byte[81920];
        long leidos = 0;
        int n;
        while ((n = await origen.ReadAsync(bufer, cancelacion)) > 0)
        {
            await destino.WriteAsync(bufer.AsMemory(0, n), cancelacion);
            leidos += n;
            if (total > 0) progreso?.Report((int)Math.Min(100, leidos * 100 / total));
        }
        progreso?.Report(100);
    }

    /// <summary>La API de GitHub rechaza las peticiones sin User-Agent.</summary>
    private static HttpRequestMessage Peticion(string url, string tipoAceptado)
    {
        var peticion = new HttpRequestMessage(HttpMethod.Get, url);
        peticion.Headers.UserAgent.ParseAdd("SPD-farmacia");
        peticion.Headers.Accept.ParseAdd(tipoAceptado);
        return peticion;
    }

    /// <summary>Si quedó una carpeta de un intento anterior interrumpido, se descarta: todo lo que contiene
    /// se vuelve a descargar y verificar. Crearla es además la prueba de que la carpeta es escribible.</summary>
    private static void PrepararCarpetaTemporal(string temporal)
    {
        BorrarCarpetaTemporal(temporal);
        Directory.CreateDirectory(temporal);
    }

    /// <summary>Es una carpeta de trabajo de la aplicación, no datos del usuario (Art. III no aplica).</summary>
    private static void BorrarCarpetaTemporal(string temporal)
    {
        try
        {
            if (Directory.Exists(temporal)) Directory.Delete(temporal, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // No impide nada: el siguiente intento la vuelve a limpiar.
        }
    }

    /// <summary>Art. VII.6: cada intento queda en auditoría, haya salido bien o mal.</summary>
    private ResultadoInstalacionActualizacion Registrar(int? administradorId, string etiqueta, ResultadoInstalacionActualizacion resultado)
    {
        var detalle = resultado.Exito ? $"version={etiqueta}" : $"version={etiqueta}; fallo={resultado.Motivo}";
        auditoria.Registrar(administradorId, "INSTALAR_ACTUALIZACION", "Sistema", null, detalle);
        return resultado;
    }

    private sealed record ReleaseGitHub([property: JsonPropertyName("assets")] List<AssetGitHub>? Assets);

    private sealed record AssetGitHub(
        [property: JsonPropertyName("name")] string Nombre,
        [property: JsonPropertyName("browser_download_url")] string Url,
        [property: JsonPropertyName("size")] long Tamano);
}
