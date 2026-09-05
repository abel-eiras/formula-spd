namespace Spd.Dominio;

/// <summary>Un backup ya generado, reconstruido a partir de su nombre de fichero
/// (`spd-aaaammdd-hhmm.zip`, research.md Decisión 6 de Spec 010) — no se persiste en ninguna
/// tabla, es puramente derivable del sistema de ficheros.</summary>
public sealed record BackupInfo(string NombreFichero, DateTime FechaHora, bool EsMensual, long TamanoBytes);
