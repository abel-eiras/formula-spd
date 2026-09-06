namespace Spd.Dominio;

/// <summary>Un blíster semanal 7×4 (FR-600..604). Siempre "un blíster = un SPD"; dos blísteres de
/// la misma sesión son dos filas independientes agrupadas solo por <see cref="SesionId"/> (CA-611:
/// no existe una tabla de negocio "Sesión"). Nombrada en mayúsculas (no `Spd`) porque coincide con
/// el espacio de nombres raíz del proyecto y colisionaría en la resolución de nombres de C#.</summary>
public sealed class SPD
{
    public int Id { get; set; }
    public required string NumRegistro { get; set; }
    public int CorrelativoNumRegistro { get; set; }
    public required int PacienteId { get; set; }
    public int Version { get; set; } = 1;
    public required Guid SesionId { get; set; }
    public DateOnly ValidezDesde { get; set; }
    public DateOnly ValidezHasta { get; set; }
    public DateTime? FechaPreparacion { get; set; }
    public int? MaterialId { get; set; }
    public int? RegistroAmbientalId { get; set; }
    public required int ElaboradorId { get; set; }
    public int? VerificadorId { get; set; }
    public DateTime? FechaVerificacion { get; set; }
    public string? ExcepcionVerificadorMotivo { get; set; }
    public ResultadoVerificacion? ResultadoVerificacion { get; set; }
    public int? EntregadorId { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string? EntregadoA { get; set; }
    public bool PrimeraEntrega { get; set; }
    public bool? SpdAnteriorRecogido { get; set; }
    public string? UnidadesNoAdministradas { get; set; }
    public string? ObservacionesAdherencia { get; set; }
    public bool CambiosMedicacionPreguntado { get; set; }
    public string? ObservacionesEtiqueta { get; set; }
    public EstadoSpd Estado { get; set; } = EstadoSpd.Borrador;
    public string? MotivoAnulacion { get; set; }
    public DateTime? ImpresoFichaEn { get; set; }
    public DateTime? ImpresoEtiquetasEn { get; set; }
    public DateTime? ImpresoInstruccionesEn { get; set; }
}
