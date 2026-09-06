namespace Spd.Dominio;

/// <summary>Resultado de la evaluación de idoneidad (Spec 002 FR-201): decisión del
/// farmacéutico, con propuesta calculada por <see cref="EvaluacionIdoneidad.ResultadoPropuesto"/>.</summary>
public enum ResultadoIdoneidad
{
    Apto,
    NoApto
}
