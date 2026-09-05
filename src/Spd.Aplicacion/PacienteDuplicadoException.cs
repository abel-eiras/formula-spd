using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Aviso de que ya existe otro paciente no dado de baja con el mismo DNI o CIP (FR-004,
/// CA-003). No bloquea: el llamador puede reintentar con `DatosAltaPaciente.ConfirmarDuplicado = true`.</summary>
public sealed class PacienteDuplicadoException(Paciente existente, string mensaje) : Exception(mensaje)
{
    public Paciente Existente { get; } = existente;
}
