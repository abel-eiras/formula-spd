using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Copia de seguridad íntegra y consistente (Art. VI.6, FR-1000..1004). Nunca abre red
/// (Art. VI.1): trabaja solo con `Farmacia.RutaBackup`, ya configurada (Spec 000 FR-030).</summary>
public interface IServicioBackup
{
    ResultadoBackup GenerarBackup(int? usuarioQueEjecutaId, bool esAutomatico);

    IReadOnlyList<BackupInfo> ListarBackups();

    ResultadoRestauracion RestaurarDesdeZip(string rutaZip, string carpetaDestino, int administradorQueEjecutaId);
}
