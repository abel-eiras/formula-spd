using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Evaluación de idoneidad y consentimiento informado del paciente, y la transición
/// EVALUACION→ACTIVO que ambos desencadenan (Spec 002 FR-200..215; Art. I.3).</summary>
public interface IServicioIdoneidadConsentimiento
{
    EstadoIdoneidadConsentimiento Consultar(int pacienteId);

    ResultadoEvaluacion RegistrarEvaluacion(int pacienteId, DatosEvaluacionIdoneidad datos, int? usuarioId);

    Consentimiento CrearConsentimiento(int pacienteId, TipoConsentimiento tipo, int? contactoId, int? usuarioId);

    Contacto CrearRepresentante(int pacienteId, DatosContactoRepresentante datos, int? usuarioId);

    ResultadoConsentimiento RegistrarFirma(int consentimientoId, DateOnly fechaFirma, int? usuarioId);

    ResultadoConsentimiento Revocar(int consentimientoId, DateOnly fecha, string motivo, int? usuarioId);

    /// <summary>ACTIVO → SUSPENDIDO a petición de la pantalla tras una sugerencia (FR-214); nunca automático.</summary>
    void Suspender(int pacienteId, int? usuarioId);
}
