using Spd.Infraestructura;

namespace Spd.Aplicacion.Tests;

/// <summary>FR-053: la sustitución sobre una «instalación» en una carpeta temporal. O se sustituye todo o
/// se deja todo como estaba, y los datos de la farmacia no se tocan nunca.</summary>
public sealed class SustitucionFicherosInstalacionTests
{
    private const string Exe = "FormulaSPD.exe";

    private static (string Origen, string Instalacion, string Respaldo) Preparar(CarpetaTemporalDePrueba carpeta)
    {
        carpeta.Escribir(Path.Combine("instalacion", Exe), "exe viejo");
        carpeta.Escribir(Path.Combine("instalacion", "LEEME.txt"), "leeme viejo");
        carpeta.Escribir(Path.Combine("instalacion", "spd.db"), "datos de la farmacia");
        carpeta.Escribir(Path.Combine("instalacion", "logs", "log-20260929.txt"), "log");

        carpeta.Escribir(Path.Combine("nueva", Exe), "exe nuevo");
        carpeta.Escribir(Path.Combine("nueva", "LEEME.txt"), "leeme nuevo");
        carpeta.Escribir(Path.Combine("nueva", "licencias", "LICENSE.txt"), "MIT");
        return (carpeta.Sub("nueva"), carpeta.Sub("instalacion"), carpeta.Sub("respaldo"));
    }

    [Fact]
    public void Sustituye_los_ficheros_del_paquete_y_aparta_el_ejecutable_como_anterior()
    {
        using var carpeta = new CarpetaTemporalDePrueba();
        var (origen, instalacion, respaldo) = Preparar(carpeta);

        SustitucionFicherosInstalacion.Sustituir(origen, instalacion, Exe, respaldo);

        Assert.Equal("exe nuevo", File.ReadAllText(Path.Combine(instalacion, Exe)));
        Assert.Equal("exe viejo", File.ReadAllText(Path.Combine(instalacion, Exe + ".anterior")));
        Assert.Equal("leeme nuevo", File.ReadAllText(Path.Combine(instalacion, "LEEME.txt")));
        Assert.Equal("MIT", File.ReadAllText(Path.Combine(instalacion, "licencias", "LICENSE.txt")));
    }

    [Fact]
    public void No_toca_nada_que_no_venga_en_el_paquete()
    {
        using var carpeta = new CarpetaTemporalDePrueba();
        var (origen, instalacion, respaldo) = Preparar(carpeta);

        SustitucionFicherosInstalacion.Sustituir(origen, instalacion, Exe, respaldo);

        Assert.Equal("datos de la farmacia", File.ReadAllText(Path.Combine(instalacion, "spd.db")));
        Assert.Equal("log", File.ReadAllText(Path.Combine(instalacion, "logs", "log-20260929.txt")));
    }

    [Fact]
    public void Un_anterior_de_una_actualizacion_previa_se_sobrescribe()
    {
        using var carpeta = new CarpetaTemporalDePrueba();
        var (origen, instalacion, respaldo) = Preparar(carpeta);
        carpeta.Escribir(Path.Combine("instalacion", Exe + ".anterior"), "exe de hace dos versiones");

        SustitucionFicherosInstalacion.Sustituir(origen, instalacion, Exe, respaldo);

        Assert.Equal("exe viejo", File.ReadAllText(Path.Combine(instalacion, Exe + ".anterior")));
    }

    /// <summary>Se fuerza un fallo a mitad: en la instalación hay un *fichero* llamado `licencias`, así que
    /// no se puede crear la carpeta del mismo nombre. Para entonces el ejecutable ya se había sustituido.</summary>
    [Fact]
    public void Si_falla_a_mitad_deshace_el_renombrado_y_deja_la_version_anterior()
    {
        using var carpeta = new CarpetaTemporalDePrueba();
        var (origen, instalacion, respaldo) = Preparar(carpeta);
        carpeta.Escribir(Path.Combine("instalacion", "licencias"), "un fichero que estorba");

        var error = Assert.Throws<SustitucionFallidaException>(
            () => SustitucionFicherosInstalacion.Sustituir(origen, instalacion, Exe, respaldo));

        Assert.Contains("versión anterior tal como estaba", error.Message);
        Assert.Equal("exe viejo", File.ReadAllText(Path.Combine(instalacion, Exe)));
        Assert.False(File.Exists(Path.Combine(instalacion, Exe + ".anterior")));
        Assert.Equal("leeme viejo", File.ReadAllText(Path.Combine(instalacion, "LEEME.txt")));
        Assert.Equal("datos de la farmacia", File.ReadAllText(Path.Combine(instalacion, "spd.db")));
    }

    [Fact]
    public void En_linux_y_macos_el_ejecutable_y_los_scripts_quedan_con_permiso_de_ejecucion()
    {
        if (OperatingSystem.IsWindows()) return;
        using var carpeta = new CarpetaTemporalDePrueba();
        carpeta.Escribir(Path.Combine("nueva", "FormulaSPD"), "binario");
        carpeta.Escribir(Path.Combine("nueva", "crear-acceso-directo.sh"), "#!/bin/sh");
        carpeta.Escribir(Path.Combine("nueva", "LEEME.txt"), "leeme");
        Directory.CreateDirectory(carpeta.Sub("instalacion"));

        SustitucionFicherosInstalacion.Sustituir(carpeta.Sub("nueva"), carpeta.Sub("instalacion"), "FormulaSPD", carpeta.Sub("respaldo"));

        Assert.True(File.GetUnixFileMode(carpeta.Sub("instalacion", "FormulaSPD")).HasFlag(UnixFileMode.UserExecute));
        Assert.True(File.GetUnixFileMode(carpeta.Sub("instalacion", "crear-acceso-directo.sh")).HasFlag(UnixFileMode.OtherExecute));
        Assert.False(File.GetUnixFileMode(carpeta.Sub("instalacion", "LEEME.txt")).HasFlag(UnixFileMode.UserExecute));
    }
}
