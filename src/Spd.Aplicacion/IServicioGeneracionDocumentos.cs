namespace Spd.Aplicacion;

/// <summary>Motor único de generación de documentos PDF (FR-700..713, FR-740..743). Cada
/// documento contiene, como mínimo, los elementos que el PNT exige para su anexo (Art. I.2;
/// cruce campo a campo en docs/analisis-resources.md §2.3).</summary>
public interface IServicioGeneracionDocumentos
{
    /// <summary>Anexo I.G — ficha de preparación, control y entrega (código FICHA).</summary>
    ResultadoGeneracionDocumento GenerarFichaSpd(int spdId, int? usuarioQueEjecutaId);

    /// <summary>Anexo I.F — etiqueta anverso (ETQ-A).</summary>
    ResultadoGeneracionDocumento GenerarEtiquetaAnverso(int spdId, int? usuarioQueEjecutaId);

    /// <summary>Anexo I.F — etiqueta reverso (ETQ-R).</summary>
    ResultadoGeneracionDocumento GenerarEtiquetaReverso(int spdId, int? usuarioQueEjecutaId);

    /// <summary>Anexo I.H — hoja de instrucciones al paciente (INSTR).</summary>
    ResultadoGeneracionDocumento GenerarInstrucciones(int spdId, int? usuarioQueEjecutaId);

    /// <summary>Anexo I.E — ficha del paciente (FICHA-PAC).</summary>
    ResultadoGeneracionDocumento GenerarFichaPaciente(int pacienteId, int? usuarioQueEjecutaId);

    /// <summary>Anexo I.D — información sobre protección de datos entregada al paciente junto
    /// con el consentimiento (RGPD). Se rellena con los datos de la farmacia y del paciente.</summary>
    ResultadoGeneracionDocumento GenerarInformacionProteccionDatos(int pacienteId, int? usuarioQueEjecutaId);

    /// <summary>Anexo I.B — consentimiento informado (CONSENT), para el paciente o su
    /// representante (Spec 002 FR-212). Marca <c>impreso_en</c> en el consentimiento.</summary>
    ResultadoGeneracionDocumento GenerarConsentimiento(int consentimientoId, int? usuarioQueEjecutaId);
}
