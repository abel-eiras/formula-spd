namespace Spd.Aplicacion;

/// <summary>Aviso informativo de registro atrasado en el panel de inicio (FR-950). Nunca
/// bloquea nada.</summary>
public sealed record ResultadoAvisoRegistros(bool AvisoAmbiental, bool AvisoLimpieza, int DiasSinAmbiental, int DiasSinLimpiezaRutinaria);
