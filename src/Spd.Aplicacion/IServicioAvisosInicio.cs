namespace Spd.Aplicacion;

/// <summary>El área de la aplicación donde se resuelve un aviso (Spec 015 FR-1511). Es
/// deliberadamente un concepto de aplicación, no de interfaz: la capa de aplicación sabe *dónde se
/// arregla* cada aviso (en el listado de retirada, en las preparaciones, en los registros de
/// calidad), y es la interfaz la que traduce eso a su propia navegación.</summary>
public enum AreaAviso
{
    Retirada,
    Preparaciones,
    Calidad
}

/// <summary>Un aviso del panel de inicio (Spec 006 FR-691, Spec 009 FR-950). Informativo, nunca
/// bloquea nada. Desde Spec 015 lleva además a dónde se resuelve: <see cref="Area"/> más, cuando
/// aplica, el paciente al que se refiere (`PacienteId` para identificarlo y `NombrePaciente` para
/// prefiltrar el listado de destino sin tener que volver a consultar el repositorio).</summary>
public sealed record AvisoInicio(
    string Tipo,
    string Texto,
    int? PacienteId,
    AreaAviso Area,
    string? NombrePaciente = null);

/// <summary>Los cuatro indicadores de la cabecera del panel de inicio (Spec 015 FR-1510). Los tres
/// primeros son recuentos de avisos; el cuarto es un plazo, no un recuento: `null` significa que
/// hay lectura ambiental reciente y no hay nada que mirar.</summary>
public sealed record IndicadoresInicio(
    int Faltantes,
    int SinEntregar,
    int SesionesAMedias,
    int? DiasSinLecturaAmbiental);

public interface IServicioAvisosInicio
{
    IReadOnlyList<AvisoInicio> Obtener(DateOnly hoy);

    IndicadoresInicio ObtenerIndicadores(DateOnly hoy);
}
