using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class ReglaAptitudSpdTests
{
    [Theory]
    [InlineData(FormaFarmaceutica.Comprimido)]
    [InlineData(FormaFarmaceutica.ComprimidoLiberacionProlongada)]
    [InlineData(FormaFarmaceutica.Capsula)]
    [InlineData(FormaFarmaceutica.CapsulaLiberacionProlongada)]
    [InlineData(FormaFarmaceutica.Gragea)]
    [InlineData(FormaFarmaceutica.Pastilla)]
    [InlineData(FormaFarmaceutica.Pildora)]
    public void PorDefecto_es_apto_para_todas_las_formas_salvo_OtraNoApta(FormaFarmaceutica forma)
        => Assert.True(ReglaAptitudSpd.PorDefecto(forma));

    [Fact]
    public void PorDefecto_no_es_apto_para_OtraNoApta()
        => Assert.False(ReglaAptitudSpd.PorDefecto(FormaFarmaceutica.OtraNoApta));
}
