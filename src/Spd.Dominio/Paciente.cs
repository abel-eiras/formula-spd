namespace Spd.Dominio;

/// <summary>Ficha del paciente (Anexo 2, cara anterior — FR-002). Nunca se elimina, solo cambia
/// de estado (Art. III.1).</summary>
public sealed class Paciente
{
    public int Id { get; set; }
    public required string NumFicha { get; set; }
    public int CorrelativoNumFicha { get; set; }
    public DateTime FechaAltaFicha { get; set; }
    public required string Nombre { get; set; }
    public required string Apellidos { get; set; }
    public string? Sexo { get; set; }
    public string? Dni { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? NumSs { get; set; }
    public string? Cip { get; set; }
    public string? Direccion { get; set; }
    public string? Cp { get; set; }
    public string? Poblacion { get; set; }
    public string? Telefono1 { get; set; }
    public string? Telefono2 { get; set; }
    public string? Email { get; set; }
    public int? MedicoId { get; set; }
    public string? EnfermedadesCronicas { get; set; }
    public string? Alergias { get; set; }
    public string? Observaciones { get; set; }
    public bool PictogramaComidas { get; set; }
    public string? IdentificadorVisual { get; set; }
    public required string DiaRetirada { get; set; }
    public int NBlisteres { get; set; } = 1;
    public EstadoPaciente Estado { get; set; } = EstadoPaciente.Evaluacion;
    public DateTime? FechaBaja { get; set; }
    public MotivoBaja? MotivoBaja { get; set; }
    public string? MotivoBajaDetalle { get; set; }
    public string BusquedaNormalizada { get; set; } = string.Empty;

    /// <summary>Transiciones permitidas de FR-006. Reactivar desde BAJA exige una nueva
    /// evaluación (Spec 002), por eso vuelve a EVALUACION y no directamente a ACTIVO.</summary>
    public bool TransicionValida(EstadoPaciente nuevo) => (Estado, nuevo) switch
    {
        (EstadoPaciente.Evaluacion, EstadoPaciente.Activo) => true,
        (EstadoPaciente.Activo, EstadoPaciente.Suspendido) => true,
        (EstadoPaciente.Suspendido, EstadoPaciente.Activo) => true,
        (EstadoPaciente.Baja, EstadoPaciente.Evaluacion) => true,
        (_, EstadoPaciente.Baja) => true,
        _ => false
    };
}
