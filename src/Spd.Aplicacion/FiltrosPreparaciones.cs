using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Filtros de la pantalla "Preparaciones" (FR-690).</summary>
public sealed record FiltrosPreparaciones(EstadoSpd? Estado = null, int? PacienteId = null, int? ElaboradorId = null);
