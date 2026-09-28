using LectorDataMatrix;
using Xunit;

namespace LectorDataMatrix.Tests;

public class LectorGs1DataMatrixTests
{
    // Codigos reales escaneados por Abel con su lector (2026-09-28), pegados sin separador GS visible.

    [Fact]
    public void CodigoReal1_incluye_codigo_nacional_via_AI712()
    {
        var resultado = LectorGs1DataMatrix.Leer(
            "010541378799931617301031103012801214000LQG554DTM17128702619");

        Assert.NotNull(resultado);
        Assert.Equal("05413787999316", resultado!.Gtin);
        Assert.Equal("3012801", resultado.Lote);
        Assert.Equal(new FechaCaducidadGs1(2030, 10, 31), resultado.Caducidad);
        Assert.Equal("4000LQG554DTM1", resultado.NumeroSerie);
        Assert.Equal("8702619", resultado.CodigoNacional);
    }

    [Fact]
    public void CodigoReal2_sin_codigo_nacional_orden_AI_01_21_17_10()
    {
        var resultado = LectorGs1DataMatrix.Leer(
            "010847000695064721172342612146291728073110RA0462");

        Assert.NotNull(resultado);
        Assert.Equal("08470006950647", resultado!.Gtin);
        Assert.Equal("17234261214629", resultado.NumeroSerie);
        Assert.Equal(new FechaCaducidadGs1(2028, 7, 31), resultado.Caducidad);
        Assert.Equal("RA0462", resultado.Lote);
        Assert.Null(resultado.CodigoNacional);
    }

    [Fact]
    public void CodigoReal3_sin_codigo_nacional_orden_AI_01_17_10_21()
    {
        var resultado = LectorGs1DataMatrix.Leer(
            "01084700076426641727123010B010B2178339902223173");

        Assert.NotNull(resultado);
        Assert.Equal("08470007642664", resultado!.Gtin);
        Assert.Equal(new FechaCaducidadGs1(2027, 12, 30), resultado.Caducidad);
        Assert.Equal("B010B", resultado.Lote);
        Assert.Equal("78339902223173", resultado.NumeroSerie);
        Assert.Null(resultado.CodigoNacional);
    }

    // Casos oficiales de SEVeM-0108.03 "Pruebas de validacion de escaneres" (v1.0), con el separador GS
    // real (0x1D) tal como especifica el documento. Cada uno reproduce un fallo de configuracion distinto
    // que un escaner puede introducir; confirman que la ruta con separador funciona incluso con contenido
    // con mayusculas/minusculas, simbolos y espacios en lote/serie.

    private const char Gs = '\u001D';

    [Fact]
    public void SevemMayusculasMinusculas()
    {
        var cadena = $"01084365715201041721010010LETRASGRANDES{Gs}21letraspequenas{Gs}7127166559";
        var resultado = LectorGs1DataMatrix.Leer(cadena);

        Assert.NotNull(resultado);
        Assert.Equal("08436571520104", resultado!.Gtin);
        Assert.Equal("LETRASGRANDES", resultado.Lote);
        Assert.Equal("letraspequenas", resultado.NumeroSerie);
        Assert.Equal(new FechaCaducidadGs1(2021, 1, 0), resultado.Caducidad);
        Assert.True(resultado.Caducidad.DiaSinEspecificar);
    }

    [Fact]
    public void SevemCaracteresEspeciales()
    {
        var cadena = $"01084365715201041721010010////_ _ _ _{Gs}21----....{Gs}7127166559";
        var resultado = LectorGs1DataMatrix.Leer(cadena);

        Assert.NotNull(resultado);
        Assert.Equal("////_ _ _ _", resultado!.Lote);
        Assert.Equal("----....", resultado.NumeroSerie);
    }

    [Fact]
    public void SevemLetrasIntercambiadasYZ()
    {
        var cadena = $"01084365715201041721010010ZZZZZ{Gs}21YYYYY{Gs}7127166559";
        var resultado = LectorGs1DataMatrix.Leer(cadena);

        Assert.NotNull(resultado);
        Assert.Equal("ZZZZZ", resultado!.Lote);
        Assert.Equal("YYYYY", resultado.NumeroSerie);
    }

    [Fact]
    public void SevemInversionDeColores()
    {
        var cadena = $"010843657152010417210100101234567890{Gs}21ABCDEF{Gs}7127166559";
        var resultado = LectorGs1DataMatrix.Leer(cadena);

        Assert.NotNull(resultado);
        Assert.Equal("1234567890", resultado!.Lote);
        Assert.Equal("ABCDEF", resultado.NumeroSerie);
    }

    // FR-1204: un codigo que no se puede interpretar no bloquea, simplemente no se reconoce.

    [Theory]
    [InlineData("")]
    [InlineData("no es un datamatrix")]
    [InlineData("999999999999999999999999999999")]
    public void Cadena_no_reconocida_devuelve_null(string cadena)
    {
        Assert.Null(LectorGs1DataMatrix.Leer(cadena));
    }
}
