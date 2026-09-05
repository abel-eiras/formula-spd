namespace Spd.Dominio;

/// <summary>Lectura de temperatura y humedad del obrador (FR-900), rutinaria o ligada a una
/// preparación (`SpdId`, nulo hasta que exista Spec 006). `FueraDeRango` se congela al crear, no
/// se recalcula si cambia la configuración después (Art. IV, CA-900).</summary>
public sealed class RegistroAmbiental
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public double Temperatura { get; set; }
    public double Humedad { get; set; }
    public int UsuarioId { get; set; }
    public string? Observaciones { get; set; }
    public int? SpdId { get; set; }
    public bool FueraDeRango { get; set; }
}
