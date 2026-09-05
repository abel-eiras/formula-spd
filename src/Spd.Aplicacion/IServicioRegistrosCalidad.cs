using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Registros de calidad de uso diario: ambiental, limpieza, formación y residuos
/// (FR-900..FR-930, FR-950). Toda escritura registra en auditoría (Art. VII.6).</summary>
public interface IServicioRegistrosCalidad
{
    RegistroAmbiental RegistrarAmbiental(DatosRegistroAmbiental datos, int? usuarioQueEjecutaId);
    IReadOnlyList<RegistroAmbiental> ListarAmbiental();

    RegistroLimpieza RegistrarLimpieza(TipoLimpieza tipo, string? observaciones, int? usuarioQueEjecutaId);
    IReadOnlyList<RegistroLimpieza> ListarLimpieza();

    FormacionPersonal RegistrarFormacion(DatosFormacion datos, int? usuarioQueEjecutaId);
    IReadOnlyList<FormacionPersonal> ListarFormacion(int usuarioId);

    RecogidaResiduos RegistrarRecogidaResiduos(DatosRecogidaResiduos datos, int? usuarioQueEjecutaId);
    IReadOnlyList<RecogidaResiduos> ListarRecogidaResiduos();

    ResultadoAvisoRegistros ComprobarAvisos();
}
