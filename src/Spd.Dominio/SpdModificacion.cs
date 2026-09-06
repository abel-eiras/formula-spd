namespace Spd.Dominio;

/// <summary>Historial de una reelaboración (Art. III.4, FR-6121). Guarda copia íntegra de lo
/// anterior: nada se pierde, todo queda reconstruible.</summary>
public sealed class SpdModificacion
{
    public int Id { get; set; }
    public required int SpdId { get; set; }
    public int VersionAnterior { get; set; }
    public int VersionNueva { get; set; }
    public DateTime Fecha { get; set; }
    public required int UsuarioId { get; set; }
    public OrigenSolicitudReelaboracion OrigenSolicitud { get; set; }
    public required string Motivo { get; set; }
    public required string ResumenCambios { get; set; }
    public required string LineasSnapshotAnterior { get; set; }
}
