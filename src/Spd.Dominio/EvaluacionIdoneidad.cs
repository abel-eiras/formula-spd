namespace Spd.Dominio;

/// <summary>Evaluación de la idoneidad del paciente para el servicio de SPD (Spec 002 FR-200..204;
/// bloque "EVALUACIÓN IDONEIDAD" del Anexo I.E del PNT I). Solo se añade, nunca se modifica: la
/// más reciente es la vigente (FR-202, Art. III).</summary>
public sealed class EvaluacionIdoneidad
{
    /// <summary>Los siete criterios de inclusión orientativos del PNT I §4.1, literales.</summary>
    public static readonly string[] TextosCriterios =
    [
        "Paciente polimedicado, especialmente con pautas posológicas complejas",
        "Persona mayor que vive sola, o dependiente sin cuidador o persona de referencia que le ayude con la medicación",
        "Deficiencia cognitiva o demencia que le impide seguir adecuadamente su tratamiento",
        "Problemas de adherencia terapéutica",
        "Incluido en un programa concertado con la Administración que incluye el SPD",
        "Expresa dificultades para la correcta gestión de su medicación",
        "Susceptible de beneficiarse del SPD a criterio del médico prescriptor o del farmacéutico",
    ];

    /// <summary>Las dos condiciones que el PNT I §4.1 marca como importantes.</summary>
    public static readonly string[] TextosCondiciones =
    [
        "Está motivado y dispuesto a gestionar su medicación, por sí mismo o a través de la persona cuidadora",
        "Puede manejar correctamente el DDP: destreza manual y agudeza visual suficientes (paciente o cuidador)",
    ];

    public int Id { get; set; }
    public required int PacienteId { get; set; }
    public DateTime Fecha { get; set; }
    public int? FarmaceuticoId { get; set; }
    public bool Criterio1 { get; set; }
    public bool Criterio2 { get; set; }
    public bool Criterio3 { get; set; }
    public bool Criterio4 { get; set; }
    public bool Criterio5 { get; set; }
    public bool Criterio6 { get; set; }
    public bool Criterio7 { get; set; }
    public bool CondicionMotivacion { get; set; }
    public bool CondicionDestreza { get; set; }
    public string? Observaciones { get; set; }
    public ResultadoIdoneidad Resultado { get; set; }

    public bool[] Criterios => [Criterio1, Criterio2, Criterio3, Criterio4, Criterio5, Criterio6, Criterio7];

    public bool AlgunCriterio => Criterios.Any(c => c);

    /// <summary>Propuesta de la aplicación (Art. V.1; research.md Decisión 1): el PNT no fija
    /// fórmula, así que la propuesta solo recoge lo que el propio PNT llama "importante": sin
    /// motivación o sin capacidad de manejar el DDP no procede, y sin ningún criterio de inclusión
    /// tampoco. El farmacéutico decide el resultado final.</summary>
    public ResultadoIdoneidad ResultadoPropuesto()
        => AlgunCriterio && CondicionMotivacion && CondicionDestreza ? ResultadoIdoneidad.Apto : ResultadoIdoneidad.NoApto;

    /// <summary>FR-204: las observaciones justifican un NO_APTO o una decisión que contradice la
    /// propuesta (Art. I.2, trazabilidad de la decisión profesional).</summary>
    public bool RequiereObservaciones => Resultado == ResultadoIdoneidad.NoApto || Resultado != ResultadoPropuesto();
}
