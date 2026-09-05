namespace Spd.Dominio;

/// <summary>Lista cerrada de motivos de baja de un paciente (FR-007). <c>Otro</c> exige texto
/// libre en <see cref="Paciente.MotivoBajaDetalle"/>.</summary>
public enum MotivoBaja
{
    Fallecimiento,
    Renuncia,
    Traslado,
    HospitalizacionProlongada,
    CriterioFarmaceutico,
    Otro
}
