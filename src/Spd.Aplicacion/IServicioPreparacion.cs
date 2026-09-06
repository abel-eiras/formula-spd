using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Ciclo de vida completo del blíster: sesión de preparación, verificación, entrega,
/// continuidad y reelaboración (FR-600..695, FR-6120..6127).</summary>
public interface IServicioPreparacion
{
    IReadOnlyList<SPD> CrearSesion(int pacienteId, int elaboradorId);

    RegistroAmbiental ObtenerOCrearLecturaAmbiental(double? temperatura, double? humedad, int? usuarioId);

    MaterialAcondicionamiento CrearMaterial(string descripcion, string lote, DateOnly fechaEntrada);

    IReadOnlyList<MaterialAcondicionamiento> ListarMaterialesActivos();

    void AsignarMaterial(int spdId, int materialId);

    void ExcluirLinea(int lineaId, string motivo, int? usuarioId);

    Envase RegistrarEnvaseDesdeLinea(int lineaId, DatosAltaEnvase datos, int? usuarioId);

    void PasarAPreparado(int spdId, int registroAmbientalId, int? usuarioId);

    SPD Verificar(int spdId, int verificadorId, ChecklistVerificacion checklist, string? excepcionMotivo, int? usuarioId);

    IReadOnlyList<SPD> RegistrarEntrega(DatosEntregaSpd datos, IReadOnlyList<int> spdIds, int? usuarioId);

    IReadOnlyList<SPD> PrepararSiguiente(int pacienteId, int elaboradorId, out ResultadoContinuidad resultado);

    SPD Reelaborar(int spdId, OrigenSolicitudReelaboracion origen, string motivo, int usuarioId);

    void RegistrarImpresion(int spdId, TipoDocumentoSpd tipo, int? usuarioId);

    void Anular(int spdId, string motivo, int? usuarioId);

    IReadOnlyList<SPD> ListarPorFiltro(FiltrosPreparaciones filtros);

    /// <summary>Spec 007 FR-720: paciente ACTIVO con idoneidad, tratamiento en SPD sin líneas
    /// pendientes de revisión y saldo suficiente en custodia para todas ellas.</summary>
    ResultadoEnvasesAlDia ComprobarEnvasesAlDia(int pacienteId);

    IReadOnlyList<SpdLinea> ListarLineas(int spdId);
}
