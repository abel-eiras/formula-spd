using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Catálogo de medicamentos: alta mínima, edición, descripción física versionada,
/// unidades por envase y búsqueda (FR-300..FR-311). Toda escritura registra en auditoría (Art. VII.6).</summary>
public interface IServicioMedicamentos
{
    Medicamento? ObtenerPorId(int id);
    Medicamento? ObtenerPorCn(string cn);
    IReadOnlyList<Medicamento> Buscar(string fragmento);
    Medicamento Crear(DatosAltaMedicamento datos, int? usuarioQueEjecutaId);
    void ActualizarDatos(Medicamento medicamento, int? usuarioQueEjecutaId);
    string ProponerDescripcionTexto(DatosDescripcionFisica datos);
    void ActualizarDescripcionFisica(int medicamentoId, DatosDescripcionFisica datos, int? usuarioQueEjecutaId);
    IReadOnlyList<VersionDescripcionFisica> ListarHistorialDescripcion(int medicamentoId);
    void ActualizarUnidadesEnvase(int medicamentoId, int unidadesEnvase, int? usuarioQueEjecutaId);
    void DarDeBaja(int medicamentoId, int? usuarioQueEjecutaId);
}
