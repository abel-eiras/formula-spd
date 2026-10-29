using Spd.Infraestructura;

namespace Spd.Aplicacion.Tests;

/// <summary>FR-053: qué paquete corresponde a cada plataforma (mismos nombres que `publicar/empaquetar.sh`)
/// y cuándo esta copia no se puede actualizar sola.</summary>
public sealed class EntornoInstalacionTests
{
    private static readonly string[] Assets =
    [
        "FormulaSPD-0.2.0-windows-x64.zip",
        "FormulaSPD-0.2.0-linux-x64.tar.gz",
        "FormulaSPD-0.2.0-macos-apple-silicon.zip",
        "FormulaSPD-0.2.0-macos-intel.zip",
        "SHA256SUMS.txt",
        "SHA256SUMS.txt.sig",
    ];

    [Theory]
    [InlineData("windows-x64", "FormulaSPD-0.2.0-windows-x64.zip", "FormulaSPD.exe")]
    [InlineData("linux-x64", "FormulaSPD-0.2.0-linux-x64.tar.gz", "FormulaSPD")]
    [InlineData("macos-apple-silicon", "FormulaSPD-0.2.0-macos-apple-silicon.zip", "FormulaSPD")]
    [InlineData("macos-intel", "FormulaSPD-0.2.0-macos-intel.zip", "FormulaSPD")]
    public void Cada_plataforma_elige_su_paquete_y_su_ejecutable(string plataforma, string paqueteEsperado, string ejecutable)
    {
        var entorno = new EntornoInstalacion("/opt/spd", null, plataforma);

        Assert.Equal([paqueteEsperado], Assets.Where(entorno.EsPaqueteDeEstaPlataforma));
        Assert.Equal(ejecutable, entorno.NombreEjecutable);
    }

    [Fact]
    public void Sin_plataforma_conocida_no_hay_paquete_ni_instalacion()
    {
        var entorno = new EntornoInstalacion("/opt/spd", "/opt/spd/FormulaSPD", null);

        Assert.Empty(Assets.Where(entorno.EsPaqueteDeEstaPlataforma));
        Assert.NotNull(entorno.MotivoParaNoInstalar());
    }

    [Fact]
    public void La_version_empaquetada_dentro_de_su_carpeta_se_puede_instalar()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "spd");
        var entorno = new EntornoInstalacion(carpeta, Path.Combine(carpeta, "FormulaSPD.exe"), "windows-x64");

        Assert.Null(entorno.MotivoParaNoInstalar());
    }

    /// <summary>Con `dotnet run` el proceso es `dotnet` (o `Spd.Presentacion`) y la carpeta es `bin/`.</summary>
    [Theory]
    [InlineData("/usr/lib/dotnet/dotnet")]
    [InlineData("/repo/src/Spd.Presentacion/bin/Debug/net8.0/Spd.Presentacion")]
    [InlineData("/otra/carpeta/FormulaSPD")]
    [InlineData(null)]
    public void Fuera_de_la_version_empaquetada_se_rechaza_con_un_motivo_claro(string? procesoEnMarcha)
    {
        var entorno = new EntornoInstalacion("/opt/spd", procesoEnMarcha, "linux-x64");

        Assert.Contains("versión empaquetada", entorno.MotivoParaNoInstalar());
    }
}
