namespace Spd.Dominio;

/// <summary>Envase en custodia de un paciente, identificado por serie (FR-510). Nunca se elimina
/// ni se reasigna a otro paciente (Art. III, FR-544); solo cambia de estado.</summary>
public sealed class Envase
{
    public int Id { get; set; }
    public required int PacienteId { get; set; }
    public required int MedicamentoId { get; set; }
    public string? Serie { get; set; }
    public string? Lote { get; set; }
    public DateOnly? Caducidad { get; set; }
    public int? UnidadesIniciales { get; set; }
    public int? UnidadesRestantes { get; set; }
    public DateTime FechaEntrada { get; set; }
    public OrigenEnvase Origen { get; set; } = OrigenEnvase.Manual;
    public EstadoEnvase Estado { get; set; } = EstadoEnvase.EnCustodia;
    public DateTime? FechaSalida { get; set; }
    public MotivoSalidaEnvase? MotivoSalida { get; set; }
    public string? MotivoSalidaDetalle { get; set; }
    public string? EntregadoA { get; set; }
}
