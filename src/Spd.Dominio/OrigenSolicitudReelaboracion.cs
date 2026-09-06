namespace Spd.Dominio;

/// <summary>Quién pidió el cambio de medicación que motiva una reelaboración (FR-6121).</summary>
public enum OrigenSolicitudReelaboracion
{
    Paciente,
    Familiar,
    Medico,
    Farmaceutico,
    Otro
}
