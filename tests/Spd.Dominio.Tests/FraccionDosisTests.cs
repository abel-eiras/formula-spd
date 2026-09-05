using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class FraccionDosisTests
{
    [Theory]
    [InlineData(FraccionDosis.Cero, 0, "0")]
    [InlineData(FraccionDosis.UnCuarto, 0.25, "1/4")]
    [InlineData(FraccionDosis.Media, 0.5, "1/2")]
    [InlineData(FraccionDosis.TresCuartos, 0.75, "3/4")]
    [InlineData(FraccionDosis.Uno, 1, "1")]
    [InlineData(FraccionDosis.UnoYCuarto, 1.25, "1 1/4")]
    [InlineData(FraccionDosis.UnoYMedio, 1.5, "1 1/2")]
    public void Valor_y_Texto_son_correctos(FraccionDosis fraccion, double valorEsperado, string textoEsperado)
    {
        Assert.Equal((decimal)valorEsperado, fraccion.Valor());
        Assert.Equal(textoEsperado, fraccion.Texto());
    }

    [Fact]
    public void Expone_exactamente_9_valores_del_vocabulario_cerrado()
    {
        Assert.Equal(9, Enum.GetValues<FraccionDosis>().Length);
    }
}
