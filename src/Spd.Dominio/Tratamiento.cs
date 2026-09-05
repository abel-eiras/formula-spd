namespace Spd.Dominio;

/// <summary>Un medicamento que toma un paciente, versionado en la propia tabla: un cambio
/// clínicamente relevante cierra esta fila (`Estado = Finalizado`, `FechaFin`) y crea una nueva en
/// vez de editarla (Art. IV.3/IV.4, FR-410, research.md Decisión 1 de Spec 004).</summary>
public sealed class Tratamiento
{
    public int Id { get; set; }
    public required int PacienteId { get; set; }
    public required int MedicamentoId { get; set; }
    public bool EnSpd { get; set; } = true;
    public string? ProblemaSalud { get; set; }
    public int? MedicoId { get; set; }
    public FraccionDosis? PautaD { get; set; }
    public FraccionDosis? PautaA { get; set; }
    public FraccionDosis? PautaC { get; set; }
    public FraccionDosis? PautaN { get; set; }
    public string? PautaTexto { get; set; }
    public string DiasSemana { get; set; } = "1111111";
    public string? Via { get; set; }
    public string? Momento { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public DateOnly FechaPrescripcionInicial { get; set; }
    public TipoTratamiento Tipo { get; set; } = TipoTratamiento.Cronico;
    public string? ConocimientoCumplimiento { get; set; }
    public string? Incidencias { get; set; }
    public string? Intervencion { get; set; }
    public EstadoTratamiento Estado { get; set; } = EstadoTratamiento.Activo;
    public int? AjusteUnidadesManual { get; set; }
}
