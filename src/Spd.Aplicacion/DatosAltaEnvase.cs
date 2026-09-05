using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Datos de entrada para registrar un envase en custodia (FR-510-514).</summary>
public sealed record DatosAltaEnvase(
    int PacienteId, int MedicamentoId, string Serie, string Lote, DateOnly Caducidad,
    int UnidadesIniciales, OrigenEnvase Origen);

/// <summary>Datos de entrada para registrar una entrega fuera de blíster (FR-543): serie y lote
/// opcionales, sin unidades ni caducidad porque no se custodia.</summary>
public sealed record DatosEntregaFueraBlister(
    int PacienteId, int MedicamentoId, string? Serie, string? Lote, string EntregadoA);
