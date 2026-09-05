namespace Spd.Aplicacion;

/// <summary>Valores por defecto de retirada y rangos ambientales (FR-020..FR-022). Solo afectan a
/// entidades nuevas o sin personalización propia — esta spec no toca ninguna entidad de Paciente,
/// eso es responsabilidad de Spec 001/005/006/009 al leer estos valores.</summary>
public sealed record DatosValoresDefecto(
    string DiaRetiradaDefecto,
    int NBlisteresDefecto,
    int DiasAntelacionListado,
    double TempMin,
    double TempMax,
    double HrMin,
    double HrMax,
    int UmbralReutilizacionLecturaAmbientalHoras);
