namespace Spd.Aplicacion;

/// <summary>Las ocho preguntas SÍ/NO de la verificación final del Anexo I.G del PNT I
/// (Spec 006 FR-651 enumeraba cinco; corrección 2026-09-06, el PNT manda — Art. I.1). Todas
/// obligatorias para el resultado APTO. En orden del anexo:
/// 1 contenido correcto (comprobado con la ficha del paciente); 2 instrucciones del fabricante y
/// PNT seguidas; 3 etiqueta coincide con la ficha de preparación; 4 etiqueta coincide con la ficha
/// del paciente a fecha de hoy; 5 trazabilidad envase original → DDP; 6 periodo de validez en el
/// DDP; 7 hoja de instrucciones cumplimentada; 8 sin alteraciones visibles (integridad).</summary>
public sealed record ChecklistVerificacion(
    bool Contenido,
    bool FabricanteYPnt,
    bool EtiquetaDatos,
    bool EtiquetaFichaPaciente,
    bool Trazabilidad,
    bool EtiquetaValidez,
    bool Instrucciones,
    bool Aspecto)
{
    public bool TodosAptos =>
        Contenido && FabricanteYPnt && EtiquetaDatos && EtiquetaFichaPaciente
        && Trazabilidad && EtiquetaValidez && Instrucciones && Aspecto;

    /// <summary>Atajo para tests y para el lote (Spec 007 FR-722): todo conforme.</summary>
    public static ChecklistVerificacion TodoConforme { get; } = new(true, true, true, true, true, true, true, true);
}
