using System.Formats.Tar;
using System.IO.Compression;
using Spd.Infraestructura;

namespace Spd.Aplicacion.Tests;

/// <summary>FR-053: extracción de los dos formatos que genera `publicar/empaquetar.sh` y protección frente a
/// rutas que escapan de la carpeta temporal («zip-slip»).</summary>
public sealed class ExtractorPaqueteActualizacionTests
{
    /// <summary>Un paquete con la misma forma que los reales: carpeta raíz `FormulaSPD-…/` con el ejecutable,
    /// el LEEME y las licencias.</summary>
    internal static string CrearCarpetaDePaquete(CarpetaTemporalDePrueba carpeta, string nombreRaiz, string ejecutable, string contenidoEjecutable)
    {
        carpeta.Escribir(Path.Combine("origen", nombreRaiz, ejecutable), contenidoEjecutable);
        carpeta.Escribir(Path.Combine("origen", nombreRaiz, "LEEME.txt"), "leeme nuevo");
        carpeta.Escribir(Path.Combine("origen", nombreRaiz, "licencias", "LICENSE.txt"), "MIT");
        return carpeta.Sub("origen", nombreRaiz);
    }

    internal static string CrearZip(CarpetaTemporalDePrueba carpeta, string raiz, string nombreZip)
    {
        var ruta = carpeta.Sub(nombreZip);
        ZipFile.CreateFromDirectory(raiz, ruta, CompressionLevel.Fastest, includeBaseDirectory: true);
        return ruta;
    }

    internal static string CrearTarGz(CarpetaTemporalDePrueba carpeta, string raiz, string nombre)
    {
        var ruta = carpeta.Sub(nombre);
        using (var fichero = File.Create(ruta))
        using (var gzip = new GZipStream(fichero, CompressionLevel.Fastest))
        {
            TarFile.CreateFromDirectory(raiz, gzip, includeBaseDirectory: true);
        }
        return ruta;
    }

    [Fact]
    public void Extrae_un_zip_y_devuelve_la_carpeta_raiz_con_el_ejecutable()
    {
        using var carpeta = new CarpetaTemporalDePrueba();
        var raiz = CrearCarpetaDePaquete(carpeta, "FormulaSPD-0.2.0-windows-x64", "FormulaSPD.exe", "exe nuevo");
        var zip = CrearZip(carpeta, raiz, "FormulaSPD-0.2.0-windows-x64.zip");

        var extraida = ExtractorPaqueteActualizacion.Extraer(zip, carpeta.Sub("extraido"), "FormulaSPD.exe");

        Assert.Equal(carpeta.Sub("extraido", "FormulaSPD-0.2.0-windows-x64"), extraida);
        Assert.Equal("exe nuevo", File.ReadAllText(Path.Combine(extraida!, "FormulaSPD.exe")));
        Assert.Equal("MIT", File.ReadAllText(Path.Combine(extraida!, "licencias", "LICENSE.txt")));
    }

    [Fact]
    public void Extrae_un_tar_gz_y_devuelve_la_carpeta_raiz_con_el_ejecutable()
    {
        using var carpeta = new CarpetaTemporalDePrueba();
        var raiz = CrearCarpetaDePaquete(carpeta, "FormulaSPD-0.2.0-linux-x64", "FormulaSPD", "binario nuevo");
        var tar = CrearTarGz(carpeta, raiz, "FormulaSPD-0.2.0-linux-x64.tar.gz");

        var extraida = ExtractorPaqueteActualizacion.Extraer(tar, carpeta.Sub("extraido"), "FormulaSPD");

        Assert.Equal("binario nuevo", File.ReadAllText(Path.Combine(extraida!, "FormulaSPD")));
        Assert.True(File.Exists(Path.Combine(extraida!, "licencias", "LICENSE.txt")));
    }

    [Fact]
    public void Un_paquete_sin_el_ejecutable_devuelve_null()
    {
        using var carpeta = new CarpetaTemporalDePrueba();
        var raiz = CrearCarpetaDePaquete(carpeta, "FormulaSPD-0.2.0-windows-x64", "OtroPrograma.exe", "x");
        var zip = CrearZip(carpeta, raiz, "paquete.zip");

        Assert.Null(ExtractorPaqueteActualizacion.Extraer(zip, carpeta.Sub("extraido"), "FormulaSPD.exe"));
    }

    [Fact]
    public void Un_zip_con_una_ruta_que_sale_de_la_carpeta_se_rechaza_sin_escribir_fuera()
    {
        using var carpeta = new CarpetaTemporalDePrueba();
        var zip = carpeta.Sub("malicioso.zip");
        using (var archivo = ZipFile.Open(zip, ZipArchiveMode.Create))
        {
            using var escritor = new StreamWriter(archivo.CreateEntry("../spd.db").Open());
            escritor.Write("pisado");
        }

        Assert.Throws<InvalidDataException>(() => ExtractorPaqueteActualizacion.Extraer(zip, carpeta.Sub("extraido"), "FormulaSPD.exe"));
        Assert.False(File.Exists(carpeta.Sub("spd.db")));
    }

    [Fact]
    public void Un_tar_gz_con_una_ruta_que_sale_de_la_carpeta_se_rechaza_sin_escribir_fuera()
    {
        using var carpeta = new CarpetaTemporalDePrueba();
        var tar = carpeta.Sub("malicioso.tar.gz");
        using (var fichero = File.Create(tar))
        using (var gzip = new GZipStream(fichero, CompressionLevel.Fastest))
        using (var escritor = new TarWriter(gzip))
        {
            var entrada = new PaxTarEntry(TarEntryType.RegularFile, "../../spd.db")
            {
                DataStream = new MemoryStream("pisado"u8.ToArray())
            };
            escritor.WriteEntry(entrada);
        }

        Assert.Throws<InvalidDataException>(() => ExtractorPaqueteActualizacion.Extraer(tar, carpeta.Sub("a", "extraido"), "FormulaSPD"));
        Assert.False(File.Exists(carpeta.Sub("spd.db")));
    }

    [Theory]
    [InlineData("../fuera.txt")]
    [InlineData("dentro/../../fuera.txt")]
    [InlineData("/etc/passwd")]
    public void RutaDentroDe_rechaza_lo_que_escapa_de_la_carpeta(string entrada)
    {
        using var carpeta = new CarpetaTemporalDePrueba();

        Assert.Throws<InvalidDataException>(() => ExtractorPaqueteActualizacion.RutaDentroDe(carpeta.Sub("extraido"), entrada));
    }

    [Fact]
    public void RutaDentroDe_acepta_rutas_normales_aunque_lleven_puntos()
    {
        using var carpeta = new CarpetaTemporalDePrueba();

        var ruta = ExtractorPaqueteActualizacion.RutaDentroDe(carpeta.Sub("extraido"), "raiz/./licencias/../LEEME.txt");

        Assert.Equal(carpeta.Sub("extraido", "raiz", "LEEME.txt"), ruta);
    }
}
