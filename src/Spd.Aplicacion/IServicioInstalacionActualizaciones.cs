namespace Spd.Aplicacion;

/// <summary>Instala desde la propia aplicación la versión que <see cref="IServicioActualizaciones"/> ha
/// encontrado (FR-053). Descarga, verifica la firma y la suma de comprobación, hace copia de seguridad y
/// sustituye los ficheros del programa; nunca toca los datos. Solo se invoca desde el botón «Instalar»
/// tras la confirmación del usuario (Art. VI.2/VI.3): jamás al arrancar ni en segundo plano.</summary>
public interface IServicioInstalacionActualizaciones
{
    /// <param name="etiqueta">La etiqueta de la release tal cual la devolvió la comprobación (`v0.2.0`).</param>
    /// <param name="progreso">Porcentaje (0–100) de la descarga del paquete.</param>
    Task<ResultadoInstalacionActualizacion> InstalarAsync(
        string etiqueta, int? administradorQueEjecutaId, IProgress<int>? progreso, CancellationToken cancelacion);
}
