namespace Spd.Aplicacion;

/// <summary>Motor único de generación de documentos PDF (FR-700..713, FR-740..743).</summary>
public interface IServicioGeneracionDocumentos
{
    ResultadoGeneracionDocumento GenerarFichaSpd(int spdId, int? usuarioQueEjecutaId);

    ResultadoGeneracionDocumento GenerarEtiquetaAnverso(int spdId, int? usuarioQueEjecutaId);

    ResultadoGeneracionDocumento GenerarEtiquetaReverso(int spdId, int? usuarioQueEjecutaId);

    ResultadoGeneracionDocumento GenerarInstrucciones(int spdId, int? usuarioQueEjecutaId);

    ResultadoGeneracionDocumento GenerarFichaPaciente(int pacienteId, int? usuarioQueEjecutaId);
}
