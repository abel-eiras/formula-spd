namespace Spd.Aplicacion;

/// <summary>Datos mínimos para dar de alta un medicamento (FR-302, CA-300). El resto de campos de
/// FR-300 se completan después con `IServicioMedicamentos.ActualizarDatos`.</summary>
public sealed record DatosAltaMedicamento(string Cn, string Nombre);
