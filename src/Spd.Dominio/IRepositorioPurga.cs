namespace Spd.Dominio;

/// <summary>La única eliminación física de la aplicación (constitución Art. III.2; Spec 010
/// FR-1022): un paciente con baja antigua y toda su cadena de datos, en una transacción. Nunca
/// toca Auditoria (Art. III.3).</summary>
public interface IRepositorioPurga
{
    /// <returns>Filas eliminadas en total, contando todas las tablas.</returns>
    int PurgarPaciente(int pacienteId);
}
