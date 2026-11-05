using Spd.Dominio;

namespace Spd.Aplicacion;

public sealed record DatosCentro(string Nombre, string? Direccion, string? Telefono, string? PersonaContacto);

/// <summary>Un blíster que va en el albarán de un centro (Spec 019 FR-1904).</summary>
public sealed record BlisterParaAlbaran(int SpdId, string NumRegistro, int PacienteId, string NumFicha, string Paciente,
    DateOnly ValidezDesde, DateOnly ValidezHasta);

/// <summary>Centros (residencias, centros de día) y su albarán de entrega (Spec 019). Toda escritura
/// deja traza en auditoría (Art. VII.6).</summary>
public interface IServicioCentros
{
    Centro Crear(DatosCentro datos, int? usuarioId);
    void Actualizar(int centroId, DatosCentro datos, int? usuarioId);
    void DarDeBaja(int centroId, string motivo, int? usuarioId);
    IReadOnlyList<Centro> Listar(bool incluirBaja);

    void AsignarPaciente(int pacienteId, int? centroId, int? usuarioId);
    IReadOnlyList<Paciente> PacientesDe(int centroId);

    /// <summary>Blísteres VERIFICADOS (listos para entregar) de los pacientes del centro.</summary>
    IReadOnlyList<BlisterParaAlbaran> BlisteresParaAlbaran(int centroId);
}

/// <summary>Una fila de la lista de reposición (Spec 019 FR-1910): un medicamento en SPD de un
/// paciente que se agota dentro del horizonte con lo que hay en custodia.</summary>
public sealed record FilaReposicion(
    int PacienteId, string Paciente, string NumFicha, string? Centro, string? Medico,
    int MedicamentoId, string Medicamento, string Cn,
    int UnidadesDisponibles, decimal ConsumoSemanal, int DiasCobertura, DateOnly FechaAgotamiento);

/// <summary>Lista de reposición (Spec 019 FR-1910..1912): calculada, no almacenada.</summary>
public interface IServicioReposicion
{
    IReadOnlyList<FilaReposicion> Obtener(DateOnly hoy, int horizonteDias, int? centroId = null);
}

/// <summary>Documentos de centros y reposición (Spec 019). Cada impresión deja traza (Art. II.2).</summary>
public interface IServicioDocumentosCentros
{
    ResultadoGeneracionDocumento GenerarAlbaran(int centroId, IReadOnlyList<int> spdIds, int? usuarioId);
    ResultadoGeneracionDocumento GenerarListaReposicion(IReadOnlyList<FilaReposicion> filas, int horizonteDias, DateOnly hoy, int? usuarioId);
}
