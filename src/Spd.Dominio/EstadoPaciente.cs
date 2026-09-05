namespace Spd.Dominio;

/// <summary>Estados de la ficha del paciente (FR-006). Las transiciones válidas viven en
/// <see cref="Paciente.TransicionValida"/>.</summary>
public enum EstadoPaciente
{
    Evaluacion,
    Activo,
    Suspendido,
    Baja
}
