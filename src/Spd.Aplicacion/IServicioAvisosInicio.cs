namespace Spd.Aplicacion;

/// <summary>Un aviso del panel de inicio (Spec 006 FR-691, Spec 009 FR-950). Informativo, nunca
/// bloquea nada. `PacienteId` permite abrir la ficha desde el aviso cuando aplica.</summary>
public sealed record AvisoInicio(string Tipo, string Texto, int? PacienteId);

public interface IServicioAvisosInicio
{
    IReadOnlyList<AvisoInicio> Obtener(DateOnly hoy);
}
