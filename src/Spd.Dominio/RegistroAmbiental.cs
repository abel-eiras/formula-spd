namespace Spd.Dominio;

/// <summary>Lectura de temperatura/humedad de la zona de preparación (FR-630/631, Art. I.3: "toda
/// preparación registra temperatura y humedad"). `FueraRango` se congela contra
/// `Farmacia.TempMin/Max`/`HrMin/Max` vigentes en el momento del registro (Art. IV.3, mismo patrón
/// que Spec 009 usó para sus propios registros).</summary>
public sealed class RegistroAmbiental
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public double Temperatura { get; set; }
    public double Humedad { get; set; }
    public bool FueraRango { get; set; }
    public int? UsuarioId { get; set; }
}
