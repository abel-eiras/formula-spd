using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class ExtractorUnidadesEnvaseTests
{
    [Fact]
    public void Extraer_devuelve_el_grupo_de_captura_como_entero()
    {
        Assert.Equal(28, ExtractorUnidadesEnvase.Extraer("COMP 30 mg 28 UDS", @"(\d+)\s*UDS"));
    }

    [Fact]
    public void Extraer_devuelve_null_si_no_hay_coincidencia()
    {
        Assert.Null(ExtractorUnidadesEnvase.Extraer("JBE 150 ml", @"(\d+)\s*UDS"));
    }

    [Fact]
    public void Probar_cuenta_aciertos_y_fallos_sobre_una_muestra_CA_1101()
    {
        var resultado = ExtractorUnidadesEnvase.Probar(
            ["COMP 30 mg 28 UDS", "JBE 150 ml", "COMP 500 mg 20 UDS"], @"(\d+)\s*UDS");

        Assert.Equal(2, resultado.Aciertos);
        Assert.Equal(1, resultado.Fallos);
        Assert.Equal(28, resultado.Filas[0].UnidadesExtraidas);
        Assert.Null(resultado.Filas[1].UnidadesExtraidas);
        Assert.Equal(20, resultado.Filas[2].UnidadesExtraidas);
    }
}
