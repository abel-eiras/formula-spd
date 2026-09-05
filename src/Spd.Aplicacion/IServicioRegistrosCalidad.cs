using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Registros de calidad de uso diario: formación y residuos (FR-920..FR-930). Toda
/// escritura registra en auditoría (Art. VII.6).
///
/// Nota de alcance (2026-09-05): el registro ambiental y de limpieza (FR-900..FR-911) y el aviso
/// de registro atrasado (FR-950) que dependía de ellos se retiraron de esta spec tras prueba
/// manual del usuario — ver PROGRESO.md.</summary>
public interface IServicioRegistrosCalidad
{
    FormacionPersonal RegistrarFormacion(DatosFormacion datos, int? usuarioQueEjecutaId);
    IReadOnlyList<FormacionPersonal> ListarFormacion(int usuarioId);

    RecogidaResiduos RegistrarRecogidaResiduos(DatosRecogidaResiduos datos, int? usuarioQueEjecutaId);
    IReadOnlyList<RecogidaResiduos> ListarRecogidaResiduos();
}
