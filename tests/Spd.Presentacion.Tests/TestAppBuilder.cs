using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestApplication(typeof(Spd.Presentacion.Tests.TestAppBuilder))]

namespace Spd.Presentacion.Tests;

/// <summary>Aplicación mínima para tests headless de Avalonia: carga el mismo tema que la aplicación
/// real (paleta, tipografías y estilos, Spec 015) pero sin tocar base de datos ni red. Si aquí no se
/// cargara el tema, los tests de vista no probarían lo que ve el usuario.</summary>
public sealed class HeadlessTestApp : Application
{
    public override void Initialize()
    {
        Resources.MergedDictionaries.Add(
            new ResourceInclude((System.Uri?)null) { Source = new System.Uri("avares://Spd.Presentacion/Estilos/Paleta.axaml") });
        Resources.MergedDictionaries.Add(
            new ResourceInclude((System.Uri?)null) { Source = new System.Uri("avares://Spd.Presentacion/Estilos/Tipografia.axaml") });
        Styles.Add(new FluentTheme());
        Styles.Add(new StyleInclude((System.Uri?)null) { Source = new System.Uri("avares://Spd.Presentacion/Estilos/Controles.axaml") });
    }
}

public static class TestAppBuilder
{
    /// <summary>`UseHeadlessDrawing = false` hace que los tests usen el renderizado real (Skia) en
    /// vez del dibujo simulado: el simulado no sabe cargar las fuentes embebidas de la aplicación
    /// (Spec 015) y cualquier vista con IBM Plex fallaría por una limitación del entorno de prueba,
    /// no por un defecto. Además, así los tests ejercitan el mismo camino de texto que el usuario.</summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<HeadlessTestApp>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
