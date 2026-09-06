namespace Spd.Dominio;

/// <summary>Consentimiento informado para el servicio de SPD (Spec 002 FR-210..215; Anexo I.B
/// del PNT I). La aplicación no firma nada: registra la fecha en que el papel quedó firmado y, en
/// su caso, la de revocación (Art. II). Nunca se borra (Art. III).</summary>
public sealed class Consentimiento
{
    public int Id { get; set; }
    public required int PacienteId { get; set; }
    public TipoConsentimiento Tipo { get; set; }
    /// <summary>Contacto firmante cuando <see cref="Tipo"/> es Representante (FR-210).</summary>
    public int? ContactoId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateOnly? FechaFirma { get; set; }
    public DateOnly? FechaRevocacion { get; set; }
    public string? MotivoRevocacion { get; set; }
    public DateTime? ImpresoEn { get; set; }

    /// <summary>Firmado y no revocado (FR-213/214).</summary>
    public bool Vigente => FechaFirma is not null && FechaRevocacion is null;
}
