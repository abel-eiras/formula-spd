using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestApplication(typeof(Spd.Presentacion.Tests.TestAppBuilder))]

namespace Spd.Presentacion.Tests;

/// <summary>Aplicación mínima para tests headless de Avalonia: solo carga el tema, sin tocar base
/// de datos ni red (a diferencia de la App real).</summary>
public sealed class HeadlessTestApp : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<HeadlessTestApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
