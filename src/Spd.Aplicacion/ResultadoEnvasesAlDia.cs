namespace Spd.Aplicacion;

/// <summary>Columna "Envases al día" del listado de preparaciones (Spec 007 FR-720): SÍ si la
/// próxima sesión del paciente puede prepararse sin intervención manual; si NO, el motivo.</summary>
public sealed record ResultadoEnvasesAlDia(bool AlDia, string? Motivo);
