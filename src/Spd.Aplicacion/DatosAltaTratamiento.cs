using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de entrada para dar de alta un tratamiento (FR-400) o abrir una versión nueva al
/// cambiar la pauta (FR-410). `MedicoId` a null hace que el servicio prerrellene el médico de
/// cabecera del paciente (CA-400).</summary>
public sealed record DatosAltaTratamiento(
    int MedicamentoId, bool EnSpd, string? ProblemaSalud, int? MedicoId,
    FraccionDosis? PautaD, FraccionDosis? PautaA, FraccionDosis? PautaC, FraccionDosis? PautaN,
    string? PautaTexto, string DiasSemana, string? Via, string? Momento,
    DateOnly FechaInicio, TipoTratamiento Tipo);
