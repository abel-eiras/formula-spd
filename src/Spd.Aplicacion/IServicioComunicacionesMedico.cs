using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Comunicaciones al médico (FR-800..807). Inmutables salvo la respuesta (Art. III).</summary>
public interface IServicioComunicacionesMedico
{
    ComunicacionMedico Crear(DatosAltaComunicacionMedico datos, int? usuarioQueEjecutaId);

    /// <summary>FR-804: no guarda nada; solo resuelve paciente/médico prescriptor para prerrellenar
    /// el formulario. El alta real, con incidencias y propuesta ya redactadas, se hace con
    /// <see cref="Crear"/> (FR-807: las comunicaciones son un hecho registrado, no un borrador).</summary>
    DatosAltaComunicacionMedico PrepararDesdeTratamiento(int tratamientoId);

    /// <summary>FR-805 (research.md Decisión 2): mismo contrato que
    /// <see cref="PrepararDesdeTratamiento"/> para el punto de extensión hacia Spec 006.</summary>
    DatosAltaComunicacionMedico PrepararDesdeAvisoCambioReferido(int pacienteId, int medicoId);

    void RegistrarRespuesta(int comunicacionId, string respuesta, DateOnly fechaRespuesta, int? usuarioQueEjecutaId);

    IReadOnlyList<ComunicacionMedico> ListarDePaciente(int pacienteId);
}
