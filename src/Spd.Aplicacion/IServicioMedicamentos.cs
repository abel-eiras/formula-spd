using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Catálogo de medicamentos: alta mínima, edición, descripción física versionada,
/// unidades por envase y búsqueda (FR-300..FR-311). Toda escritura registra en auditoría (Art. VII.6).</summary>
public interface IServicioMedicamentos
{
    Medicamento? ObtenerPorId(int id);
    Medicamento? ObtenerPorCn(string cn);
    /// <summary>FR-305. Con menos de dos caracteres devuelve vacío, y nunca más de <paramref name="limite"/>
    /// resultados: con el nomenclátor entero en el catálogo, listar miles no ayuda a encontrar ninguno.</summary>
    IReadOnlyList<Medicamento> Buscar(string fragmento, int limite = 50);

    /// <summary>Confirmación de aptitud SPD **en bloque** al elaborar (Art. I.3, decisión del propietario del
    /// 2026-09-14): marca aptos los indicados que estén sin confirmar y devuelve cuántos. Nunca cambia un
    /// «no apto» explícito. Cada medicamento confirmado queda en auditoría con quién lo confirmó.</summary>
    int ConfirmarAptitudSpd(IReadOnlyCollection<int> medicamentoIds, int? usuarioQueEjecutaId);
    Medicamento Crear(DatosAltaMedicamento datos, int? usuarioQueEjecutaId);
    void ActualizarDatos(Medicamento medicamento, int? usuarioQueEjecutaId);
    string ProponerDescripcionTexto(DatosDescripcionFisica datos);
    void ActualizarDescripcionFisica(int medicamentoId, DatosDescripcionFisica datos, int? usuarioQueEjecutaId);
    IReadOnlyList<VersionDescripcionFisica> ListarHistorialDescripcion(int medicamentoId);
    void ActualizarUnidadesEnvase(int medicamentoId, int unidadesEnvase, int? usuarioQueEjecutaId);
    void DarDeBaja(int medicamentoId, int? usuarioQueEjecutaId);
}
