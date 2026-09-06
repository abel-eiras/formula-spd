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

    /// <summary>Spec 006 FR-682 (resuelto por el PNT I §4.4.1: la hoja es "en cada entrega"): una
    /// sola hoja para todos los blísteres de la sesión si su contenido es idéntico, con el periodo
    /// de validez completo; si difieren, lanza para que se imprima una por blíster.</summary>
    ResultadoGeneracionDocumento GenerarInstruccionesSesion(Guid sesionId, int? usuarioQueEjecutaId);

    /// <summary>Anexo I.E — ficha del paciente (FICHA-PAC).</summary>
    ResultadoGeneracionDocumento GenerarFichaPaciente(int pacienteId, int? usuarioQueEjecutaId);

    /// <summary>Anexo I.D — información sobre protección de datos entregada al paciente junto
    /// con el consentimiento (RGPD). Se rellena con los datos de la farmacia y del paciente.</summary>
    ResultadoGeneracionDocumento GenerarInformacionProteccionDatos(int pacienteId, int? usuarioQueEjecutaId);

    /// <summary>Anexo I.B — consentimiento informado (CONSENT), para el paciente o su
    /// representante (Spec 002 FR-212). Marca <c>impreso_en</c> en el consentimiento.</summary>
    ResultadoGeneracionDocumento GenerarConsentimiento(int consentimientoId, int? usuarioQueEjecutaId);

    /// <summary>Anexo I.C — carta al médico (Spec 008 FR-806): `CARTA-PRES` para una comunicación
    /// de presentación (texto literal del anexo) y `CARTA-INC` para una de incidencia (mismo
    /// encabezado y cierre, cuerpo con incidencias y propuesta). Una comunicación telefónica no
    /// genera documento.</summary>
    ResultadoGeneracionDocumento GenerarCartaMedico(int comunicacionId, int? usuarioQueEjecutaId);
}
