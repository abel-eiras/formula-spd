namespace Spd.Dominio;

/// <summary>Un intento de verificación de un blíster (FR-651). Se conservan todas: reelaborar
/// archiva la anterior, nunca la sobrescribe.</summary>
public sealed class SpdVerificacion
{
    public int Id { get; set; }
    public required int SpdId { get; set; }
    public required int VerificadorId { get; set; }
    public DateTime Fecha { get; set; }
    public bool VerifAspecto { get; set; }
    public bool VerifEtiquetaDatos { get; set; }
    public bool VerifEtiquetaValidez { get; set; }
    public bool VerifInstrucciones { get; set; }
    public bool VerifContenido { get; set; }
    // Las tres preguntas del Anexo I.G que Spec 006 FR-651 no recogía (migración 0010).
    public bool VerifFabricantePnt { get; set; }
    public bool VerifEtiquetaFichaPaciente { get; set; }
    public bool VerifTrazabilidad { get; set; }
    public ResultadoVerificacion Resultado { get; set; }
    public string? ExcepcionMotivo { get; set; }
}
