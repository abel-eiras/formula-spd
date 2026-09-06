namespace Spd.Aplicacion;

/// <summary>Valores por defecto de retirada y rangos ambientales (FR-020..FR-022) y años de
/// retención antes de la purga (Spec 010 FR-1020). Solo afectan a entidades nuevas o sin
/// personalización propia — esta spec no toca ninguna entidad de Paciente.</summary>
public sealed record DatosValoresDefecto(
    string DiaRetiradaDefecto,
    int NBlisteresDefecto,
    int DiasAntelacionListado,
    double TempMin,
    double TempMax,
    double HrMin,
    double HrMax,
    int UmbralReutilizacionLecturaAmbientalHoras,
    int AniosRetencionPurga = 5);
