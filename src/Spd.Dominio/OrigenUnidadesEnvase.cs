namespace Spd.Dominio;

/// <summary>Trazabilidad de cómo se rellenó `unidades_envase` (FR-311). Puramente informativo:
/// nunca bloquea la edición manual.</summary>
public enum OrigenUnidadesEnvase
{
    Manual,
    ImportadoRegex
}
