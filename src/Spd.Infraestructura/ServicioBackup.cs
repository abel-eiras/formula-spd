using System.IO.Compression;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Copia de seguridad íntegra (Art. VI.6, FR-1000..1004): `VACUUM INTO` + zip +
/// rotación calculada del propio nombre de fichero (research.md Decisión 5/6 de Spec 010).
/// `_carpetaInstalacion` es donde vive `config.json` (Art. VI.4), inyectable para tests.</summary>
public sealed partial class ServicioBackup(
    SqliteConnection conexion, IRepositorioFarmacia repositorioFarmacia, IRegistradorAuditoria auditoria,
    string? carpetaInstalacion = null)
    : IServicioBackup
{
    private const int MaximoDiarios = 30;
    private const int MaximoMensuales = 12;

    private readonly string _carpetaInstalacion = carpetaInstalacion ?? AppContext.BaseDirectory;

    [GeneratedRegex(@"^spd-(\d{8})-(\d{4})\.zip$")]
    private static partial Regex PatronNombreBackup();

    public ResultadoBackup GenerarBackup(int? usuarioQueEjecutaId, bool esAutomatico)
    {
        var farmacia = repositorioFarmacia.Obtener();
        if (string.IsNullOrWhiteSpace(farmacia?.RutaBackup))
        {
            return Registrar(esAutomatico, usuarioQueEjecutaId, ResultadoBackup.Fallido(
                "No hay ninguna ruta de backup configurada (Configuración / Farmacia)."));
        }

        try
        {
            Directory.CreateDirectory(farmacia.RutaBackup);

            var ahora = DateTime.Now;
            var nombreZip = $"spd-{ahora:yyyyMMdd-HHmm}.zip";
            var rutaZipTemporal = Path.Combine(Path.GetTempPath(), $"{nombreZip}.tmp");
            var rutaDbTemporal = Path.Combine(Path.GetTempPath(), $"spd-vacuum-{Guid.NewGuid():N}.db");

            try
            {
                // Comilla simple duplicada: la ruta es nuestra (carpeta temporal + guid), pero se
                // escapa igualmente por higiene — VACUUM INTO no admite parámetros ligados en
                // todas las versiones de SQLite.
                conexion.Execute($"VACUUM INTO '{rutaDbTemporal.Replace("'", "''")}';");

                using (var zip = ZipFile.Open(rutaZipTemporal, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(rutaDbTemporal, "spd.db");
                    var rutaConfig = Path.Combine(_carpetaInstalacion, "config.json");
                    if (File.Exists(rutaConfig))
                    {
                        zip.CreateEntryFromFile(rutaConfig, "config.json");
                    }
                }

                var rutaZipFinal = Path.Combine(farmacia.RutaBackup, nombreZip);
                File.Move(rutaZipTemporal, rutaZipFinal, overwrite: true);

                AplicarRotacion(farmacia.RutaBackup);

                return Registrar(esAutomatico, usuarioQueEjecutaId, ResultadoBackup.Exitoso(rutaZipFinal));
            }
            finally
            {
                if (File.Exists(rutaDbTemporal)) File.Delete(rutaDbTemporal);
                if (File.Exists(rutaZipTemporal)) File.Delete(rutaZipTemporal);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SqliteException)
        {
            return Registrar(esAutomatico, usuarioQueEjecutaId, ResultadoBackup.Fallido(ex.Message));
        }
    }

    public IReadOnlyList<BackupInfo> ListarBackups()
    {
        var farmacia = repositorioFarmacia.Obtener();
        if (string.IsNullOrWhiteSpace(farmacia?.RutaBackup) || !Directory.Exists(farmacia.RutaBackup))
        {
            return [];
        }

        var backups = LeerBackups(farmacia.RutaBackup).ToList();
        var mensuales = new HashSet<string>(CandidatosMensuales(backups).Select(b => b.NombreFichero));

        return backups
            .Select(b => b with { EsMensual = mensuales.Contains(b.NombreFichero) })
            .OrderByDescending(b => b.FechaHora)
            .ToList();
    }

    public ResultadoRestauracion RestaurarDesdeZip(string rutaZip, string carpetaDestino, int administradorQueEjecutaId)
    {
        try
        {
            Directory.CreateDirectory(carpetaDestino);
            ZipFile.ExtractToDirectory(rutaZip, carpetaDestino, overwriteFiles: true);
            auditoria.Registrar(administradorQueEjecutaId, "RESTAURAR_BACKUP", "Sistema", null, rutaZip);
            return ResultadoRestauracion.Exitoso();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            return ResultadoRestauracion.Fallido(ex.Message);
        }
    }

    private ResultadoBackup Registrar(bool esAutomatico, int? usuarioQueEjecutaId, ResultadoBackup resultado)
    {
        var accion = esAutomatico ? "BACKUP_AUTOMATICO" : "BACKUP_MANUAL";
        var detalle = resultado.Exito ? resultado.RutaZip : $"fallo={resultado.Motivo}";
        auditoria.Registrar(usuarioQueEjecutaId, accion, "Sistema", null, detalle);
        return resultado;
    }

    // FR-1002: 30 diarios (los más recientes en general) + 12 mensuales (el backup más antiguo de
    // cada uno de los 12 meses más recientes con backups, "promovido" para no perderlo cuando cae
    // fuera de la ventana de 30 diarios). Se elimina cualquier fichero de backup que no esté en
    // ninguno de los dos conjuntos — la única eliminación automática de todo el sistema (Art. III.2),
    // y solo de ficheros de backup, nunca de `spd.db` ni de ninguna tabla.
    private static void AplicarRotacion(string rutaBackup)
    {
        var backups = LeerBackups(rutaBackup).OrderByDescending(b => b.FechaHora).ToList();

        var aConservarDiarios = backups.Take(MaximoDiarios).Select(b => b.NombreFichero);
        var aConservarMensuales = CandidatosMensuales(backups).Select(b => b.NombreFichero);
        var aConservar = new HashSet<string>(aConservarDiarios.Concat(aConservarMensuales));

        foreach (var backup in backups.Where(b => !aConservar.Contains(b.NombreFichero)))
        {
            File.Delete(Path.Combine(rutaBackup, backup.NombreFichero));
        }
    }

    // El backup más antiguo de cada uno de los 12 meses más recientes con backups — "el primero
    // de cada mes se promueve a mensual" (FR-1002).
    private static IEnumerable<BackupInfo> CandidatosMensuales(IEnumerable<BackupInfo> backups)
        => backups
            .GroupBy(b => new { b.FechaHora.Year, b.FechaHora.Month })
            .Select(g => g.OrderBy(b => b.FechaHora).First())
            .OrderByDescending(b => b.FechaHora)
            .Take(MaximoMensuales);

    private static IEnumerable<BackupInfo> LeerBackups(string rutaBackup)
        => Directory.EnumerateFiles(rutaBackup, "spd-*.zip")
            .Select(ruta => (Ruta: ruta, Nombre: Path.GetFileName(ruta)))
            .Select(f => (f.Ruta, f.Nombre, Coincide: PatronNombreBackup().Match(f.Nombre)))
            .Where(f => f.Coincide.Success)
            .Select(f => new BackupInfo(
                f.Nombre,
                DateTime.ParseExact($"{f.Coincide.Groups[1].Value}{f.Coincide.Groups[2].Value}", "yyyyMMddHHmm", null),
                false,
                new FileInfo(f.Ruta).Length));
}
