namespace Spd.Aplicacion;

/// <summary>Una fila del listado de retirada (FR-531/FR-532): un paciente y un medicamento en SPD
/// con faltantes (o sin ellos, si `filtros.SoloConFaltantes = false`).</summary>
public sealed record FilaListadoRetirada(
    int PacienteId, string Apellidos, string Nombre, string? Cip, string? DniRetirada, bool AvisoSinDni,
    int MedicamentoId, string MedicamentoNombre, string Cn,
    int Necesarias, int Disponibles, int Faltan, int? EnvasesARetirar,
    DateOnly ProximaRetirada, int NBlisteres);

/// <summary>Filtros de la pantalla "Retirada de envases" (FR-534). `SoloConFaltantes = true` es
/// el valor por defecto.</summary>
public sealed record FiltrosListadoRetirada(bool SoloConFaltantes = true, string? DiaRetirada = null, int? PacienteId = null);
