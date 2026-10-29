using System.Security.Cryptography;
using System.Text;
using Spd.Infraestructura;

namespace Spd.Aplicacion.Tests;

/// <summary>FR-053: la firma de `SHA256SUMS.txt` es lo que impide instalar un programa que no publicó el
/// proyecto. Claves efímeras: la privada real nunca entra en los tests ni en el repositorio.</summary>
public sealed class VerificadorFirmaActualizacionesTests
{
    private static readonly byte[] Sumas = Encoding.UTF8.GetBytes(
        "0a1b2c  FormulaSPD-0.2.0-windows-x64.zip\n3d4e5f  FormulaSPD-0.2.0-linux-x64.tar.gz\n");

    /// <summary>Lo mismo que hace la CI con `openssl dgst -sha256 -sign`: firma DER.</summary>
    internal static byte[] Firmar(ECDsa clave, byte[] datos)
        => clave.SignData(datos, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);

    [Fact]
    public void Una_firma_hecha_con_la_clave_privada_correspondiente_es_valida()
    {
        using var clave = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        Assert.True(VerificadorFirmaActualizaciones.FirmaValida(Sumas, Firmar(clave, Sumas), clave.ExportSubjectPublicKeyInfoPem()));
    }

    [Fact]
    public void Si_se_manipula_el_fichero_de_sumas_la_firma_deja_de_ser_valida()
    {
        using var clave = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var firma = Firmar(clave, Sumas);
        var manipuladas = (byte[])Sumas.Clone();
        manipuladas[0] = (byte)'f';

        Assert.False(VerificadorFirmaActualizaciones.FirmaValida(manipuladas, firma, clave.ExportSubjectPublicKeyInfoPem()));
    }

    [Fact]
    public void Una_firma_hecha_con_otra_clave_no_es_valida()
    {
        using var clave = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var otra = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        Assert.False(VerificadorFirmaActualizaciones.FirmaValida(Sumas, Firmar(otra, Sumas), clave.ExportSubjectPublicKeyInfoPem()));
    }

    [Fact]
    public void Una_firma_malformada_es_invalida_y_no_una_excepcion()
    {
        using var clave = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        Assert.False(VerificadorFirmaActualizaciones.FirmaValida(Sumas, [1, 2, 3], clave.ExportSubjectPublicKeyInfoPem()));
    }

    [Fact]
    public void La_clave_publica_oficial_se_puede_importar_y_no_valida_firmas_de_otra_clave()
    {
        using var otra = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        Assert.False(VerificadorFirmaActualizaciones.FirmaValida(
            Sumas, Firmar(otra, Sumas), VerificadorFirmaActualizaciones.ClavePublicaOficial));
    }

    /// <summary>La CI verifica la firma con `publicar/clave-publica-actualizaciones.pem` y la aplicación con la
    /// constante: si alguien cambiara una sin la otra, la CI publicaría releases que ninguna copia aceptaría.</summary>
    [Fact]
    public void La_clave_publica_embebida_es_la_misma_que_usa_la_CI()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (directorio is not null && !File.Exists(Path.Combine(directorio.FullName, "publicar", "clave-publica-actualizaciones.pem")))
            directorio = directorio.Parent;
        Assert.NotNull(directorio);

        var deLaCi = File.ReadAllText(Path.Combine(directorio!.FullName, "publicar", "clave-publica-actualizaciones.pem"));
        Assert.Equal(deLaCi.ReplaceLineEndings("\n").Trim(), VerificadorFirmaActualizaciones.ClavePublicaOficial.ReplaceLineEndings("\n").Trim());
    }

    [Theory]
    [InlineData("FormulaSPD-0.2.0-windows-x64.zip", "0a1b2c")]
    [InlineData("FormulaSPD-0.2.0-linux-x64.tar.gz", "3d4e5f")]
    [InlineData("FormulaSPD-0.2.0-macos-intel.zip", null)]
    public void SumaDe_lee_el_formato_de_sha256sum(string nombre, string? esperada)
        => Assert.Equal(esperada, VerificadorFirmaActualizaciones.SumaDe(Encoding.UTF8.GetString(Sumas), nombre));

    [Fact]
    public void SumaDe_admite_el_asterisco_del_modo_binario_y_finales_de_linea_de_windows()
        => Assert.Equal("abc", VerificadorFirmaActualizaciones.SumaDe("abc *paquete.zip\r\n", "paquete.zip"));
}
