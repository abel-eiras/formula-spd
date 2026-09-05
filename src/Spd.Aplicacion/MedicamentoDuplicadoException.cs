using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Un CN ya existe activo (FR-306, CA-303). A diferencia de `PacienteDuplicadoException`
/// de Spec 001, este es un bloqueo real, no un aviso: no hay forma de "confirmar y continuar",
/// porque el CN es una clave natural única, no una coincidencia de datos personales.</summary>
public sealed class MedicamentoDuplicadoException(Medicamento existente, string mensaje) : Exception(mensaje)
{
    public Medicamento Existente { get; } = existente;
}
