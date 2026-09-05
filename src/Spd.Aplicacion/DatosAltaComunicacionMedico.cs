using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de entrada para dar de alta una comunicación al médico (FR-800/801/802).
/// `MedicoId` a null hace que el servicio prerrellene el médico de cabecera del paciente
/// (CA-800).</summary>
public sealed record DatosAltaComunicacionMedico(
    int PacienteId, int? MedicoId, TipoComunicacionMedico Tipo, string? IncidenciasDetectadas, string? Propuesta);
