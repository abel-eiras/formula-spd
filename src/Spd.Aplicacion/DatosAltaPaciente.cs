using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de entrada para dar de alta un paciente (FR-002/FR-003). `DiaRetirada` y
/// `NBlisteres` se prerrellenan con los valores por defecto de Farmacia cuando vienen a null
/// (FR-002c). `ConfirmarDuplicado` permite reintentar el alta tras el aviso de FR-004/CA-003.</summary>
public sealed record DatosAltaPaciente(
    string Nombre, string Apellidos, string? Sexo, string? Dni, DateOnly? FechaNacimiento,
    string? NumSs, string? Cip, string? Direccion, string? Cp, string? Poblacion,
    string? Telefono1, string? Telefono2, string? Email, int? MedicoId,
    string? EnfermedadesCronicas, string? Alergias, string? Observaciones, bool PictogramaComidas,
    string? IdentificadorVisual, string? DiaRetirada, int? NBlisteres, bool ConfirmarDuplicado = false);
