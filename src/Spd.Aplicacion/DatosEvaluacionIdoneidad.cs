using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Formulario de evaluación de idoneidad (Spec 002 FR-200/201): siete criterios de
/// inclusión, dos condiciones, observaciones y el resultado decidido por el farmacéutico.</summary>
public sealed record DatosEvaluacionIdoneidad(
    bool Criterio1, bool Criterio2, bool Criterio3, bool Criterio4, bool Criterio5, bool Criterio6, bool Criterio7,
    bool CondicionMotivacion, bool CondicionDestreza,
    string? Observaciones, ResultadoIdoneidad Resultado, int? FarmaceuticoId);

/// <summary>Alta en línea de un representante legal o persona autorizada desde la pantalla de
/// consentimiento (Spec 002 FR-211). El DNI es obligatorio (Spec 001 FR-022).</summary>
public sealed record DatosContactoRepresentante(
    TipoContacto Tipo, string Nombre, string Apellidos, string Dni, string? Telefono, string? Email);

/// <summary>Estado de idoneidad y consentimiento de un paciente (Spec 002), para la pantalla.</summary>
public sealed record EstadoIdoneidadConsentimiento(
    Paciente Paciente,
    EvaluacionIdoneidad? EvaluacionVigente,
    Consentimiento? ConsentimientoVigente,
    IReadOnlyList<EvaluacionIdoneidad> Evaluaciones,
    IReadOnlyList<Consentimiento> Consentimientos,
    IReadOnlyList<Contacto> RepresentantesElegibles)
{
    /// <summary>FR-213: evaluación vigente APTO y consentimiento vigente.</summary>
    public bool CumpleParaActivo => EvaluacionVigente?.Resultado == ResultadoIdoneidad.Apto && ConsentimientoVigente is not null;
}

/// <summary>Resultado de registrar una evaluación: si el paciente pasó a ACTIVO (FR-213) o si
/// procede ofrecer SUSPENDIDO (NO_APTO sobre un paciente activo, caso límite de spec.md §7).</summary>
public sealed record ResultadoEvaluacion(EvaluacionIdoneidad Evaluacion, bool PacienteActivado, bool SugerirSuspension);

/// <summary>Resultado de firmar o revocar un consentimiento (FR-213/214).</summary>
public sealed record ResultadoConsentimiento(Consentimiento Consentimiento, bool PacienteActivado, bool SugerirSuspension);
