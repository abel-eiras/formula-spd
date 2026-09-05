namespace Spd.Dominio;

/// <summary>Motivo de salida a SIGRE de un envase (FR-540).</summary>
public enum MotivoSalidaEnvase
{
    CeseTratamiento,
    CambioTratamiento,
    Caducado,
    Deteriorado,
    Fallecimiento,
    BajaPaciente,
    Otro
}
