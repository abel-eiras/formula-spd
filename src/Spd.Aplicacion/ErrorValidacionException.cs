namespace Spd.Aplicacion;

/// <summary>Faltan campos obligatorios en la entrada de un caso de uso (p. ej. un paso del asistente, FR-001).</summary>
public sealed class ErrorValidacionException(string mensaje) : Exception(mensaje);
