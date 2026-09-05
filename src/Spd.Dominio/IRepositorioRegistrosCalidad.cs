namespace Spd.Dominio;

/// <summary>Acceso a los registros de calidad de uso diario (Art. III.1: solo alta, nunca
/// actualización ni eliminación).</summary>
public interface IRepositorioRegistrosCalidad
{
    int Crear(FormacionPersonal formacion);
    IReadOnlyList<FormacionPersonal> ListarFormacion(int usuarioId);

    int Crear(RecogidaResiduos recogida);
    IReadOnlyList<RecogidaResiduos> ListarRecogidaResiduos();
}
