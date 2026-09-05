namespace Spd.Dominio;

/// <summary>Comunicación al médico (presentación, incidencia o llamada telefónica, FR-800..807).
/// Inmutable tras guardarse salvo <see cref="Respuesta"/>/<see cref="FechaRespuesta"/> (Art. III).</summary>
public sealed class ComunicacionMedico
{
    public int Id { get; set; }
    public required int PacienteId { get; set; }
    public required int MedicoId { get; set; }
    public TipoComunicacionMedico Tipo { get; set; }
    public DateOnly Fecha { get; set; }
    public string? IncidenciasDetectadas { get; set; }
    public string? Propuesta { get; set; }
    public string? Respuesta { get; set; }
    public DateOnly? FechaRespuesta { get; set; }
    public int? FarmaceuticoId { get; set; }

    /// <summary>FR-806, research.md Decisión 3: solo Presentación e Incidencia generan documento;
    /// Telefono es un registro interno sin plantilla formal.</summary>
    public bool EsImprimible => Tipo != TipoComunicacionMedico.Telefono;
}
