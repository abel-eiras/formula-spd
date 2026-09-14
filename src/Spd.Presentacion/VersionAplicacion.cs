using System.Reflection;

namespace Spd.Presentacion;

/// <summary>La versión que se está ejecutando, leída del ensamblado (la declara `<Version>` en el .csproj).
///
/// Va en el título de las ventanas porque las primeras versiones son **betas** que prueban farmacias:
/// quien informa de un fallo tiene que poder decir qué versión tenía delante sin buscarla.</summary>
public static class VersionAplicacion
{
    public const string Nombre = "Fórmula SPD";

    /// <summary>Sin los metadatos de compilación (`+hash`): «0.1.0-beta».</summary>
    public static string Texto { get; } =
        typeof(VersionAplicacion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0]
        ?? typeof(VersionAplicacion).Assembly.GetName().Version?.ToString(3)
        ?? "0.0.0";

    public static string TituloPrincipal => $"{Nombre} {Texto} — Sistemas Personalizados de Dosificación";

    public static string TituloLogin => $"{Nombre} {Texto} — Iniciar sesión";
}
