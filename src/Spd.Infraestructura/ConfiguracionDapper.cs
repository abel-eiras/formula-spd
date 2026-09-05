using System.Runtime.CompilerServices;

namespace Spd.Infraestructura;

/// <summary>Configura el mapeo snake_case → PascalCase de Dapper al cargar el ensamblado.</summary>
internal static class ConfiguracionDapper
{
#pragma warning disable CA2255 // Deliberado: única forma de garantizar el mapeo de Dapper antes de cualquier query, sin repetir la llamada en cada composition root (app y tests)
    [ModuleInitializer]
    internal static void Inicializar() => Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
#pragma warning restore CA2255
}
