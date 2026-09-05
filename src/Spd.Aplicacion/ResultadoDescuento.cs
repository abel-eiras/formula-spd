using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Resultado de aplicar el descuento de FR-520/521 a un tratamiento.</summary>
public sealed record ResultadoDescuento(int UnidadesDescontadas, IReadOnlyList<AsignacionEnvase> Asignaciones);

public sealed record AsignacionEnvase(Envase Envase, int UnidadesTomadas);
