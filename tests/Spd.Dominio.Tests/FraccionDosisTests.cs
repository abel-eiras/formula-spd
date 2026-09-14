using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

/// <summary>El vocabulario cerrado de dosis (FR-402), ampliado el 2026-09-14 a los catorce valores que
/// enumeró el propietario, y tecleable en vez de elegirse de una lista.</summary>
public sealed class FraccionDosisTests
{
    [Theory]
    [InlineData(FraccionDosis.Cero, 0, "0", "0")]
    [InlineData(FraccionDosis.UnCuarto, 3, "1/4", "1/4")]
    [InlineData(FraccionDosis.UnTercio, 4, "1/3", "1/3")]
    [InlineData(FraccionDosis.Media, 6, "1/2", "1/2")]
    [InlineData(FraccionDosis.DosTercios, 8, "2/3", "2/3")]
    [InlineData(FraccionDosis.TresCuartos, 9, "3/4", "3/4")]
    [InlineData(FraccionDosis.Uno, 12, "1", "1")]
    [InlineData(FraccionDosis.UnoYCuarto, 15, "1 1/4", "1+1/4")]
    [InlineData(FraccionDosis.UnoYTercio, 16, "1 1/3", "1+1/3")]
    [InlineData(FraccionDosis.UnoYMedio, 18, "1 1/2", "1+1/2")]
    [InlineData(FraccionDosis.UnoYDosTercios, 20, "1 2/3", "1+2/3")]
    [InlineData(FraccionDosis.UnoYTresCuartos, 21, "1 3/4", "1+3/4")]
    [InlineData(FraccionDosis.Dos, 24, "2", "2")]
    [InlineData(FraccionDosis.Tres, 36, "3", "3")]
    public void Doceavos_notacion_impresa_y_notacion_de_entrada(
        FraccionDosis fraccion, int doceavos, string impreso, string entrada)
    {
        Assert.Equal(doceavos, fraccion.Doceavos());
        Assert.Equal(doceavos / 12m, fraccion.Valor());
        Assert.Equal(impreso, fraccion.Texto());
        Assert.Equal(entrada, fraccion.TextoEntrada());
    }

    [Fact]
    public void Son_exactamente_los_catorce_valores_que_enumero_el_propietario()
    {
        Assert.Equal(14, Enum.GetValues<FraccionDosis>().Length);
        Assert.Equal(14, FraccionDosisExtensiones.Admitidas.Distinct().Count());
        Assert.Equal("0, 1, 2, 3, 1/4, 1/2, 1/3, 3/4, 2/3, 1+1/2, 1+1/4, 1+1/3, 1+2/3, 1+3/4",
            FraccionDosisExtensiones.TextoAdmitidas);
    }

    /// <summary>Art. IV.3. Estos nombres están guardados como texto en tratamientos e instantáneas de
    /// SPD ya entregados. Si alguien renombra uno «para que quede más bonito», ese SPD deja de poder
    /// leerse; este test lo impide.</summary>
    [Theory]
    [InlineData("Cero")]
    [InlineData("UnCuarto")]
    [InlineData("UnTercio")]
    [InlineData("Media")]
    [InlineData("DosTercios")]
    [InlineData("TresCuartos")]
    [InlineData("Uno")]
    [InlineData("UnoYCuarto")]
    [InlineData("UnoYMedio")]
    public void Los_nombres_ya_guardados_se_siguen_leyendo_Art_IV_3(string nombreGuardado)
        => Assert.True(Enum.TryParse<FraccionDosis>(nombreGuardado, out _));

    [Theory]
    [InlineData("1+1/2", FraccionDosis.UnoYMedio)]
    [InlineData("1 + 1/2", FraccionDosis.UnoYMedio)]
    [InlineData("1 1/2", FraccionDosis.UnoYMedio)]
    [InlineData("  1/3 ", FraccionDosis.UnTercio)]
    [InlineData("0", FraccionDosis.Cero)]
    [InlineData("3", FraccionDosis.Tres)]
    [InlineData("1+2/3", FraccionDosis.UnoYDosTercios)]
    [InlineData("1+3/4", FraccionDosis.UnoYTresCuartos)]
    public void Acepta_el_vocabulario_y_solo_es_tolerante_con_la_forma(string tecleado, FraccionDosis esperada)
    {
        Assert.True(FraccionDosisExtensiones.TryParsear(tecleado, out var leida));
        Assert.Equal(esperada, leida);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Vacio_es_sin_dosis_en_esa_toma_no_cero(string? tecleado)
    {
        Assert.True(FraccionDosisExtensiones.TryParsear(tecleado, out var leida));
        Assert.Null(leida);
    }

    /// <summary>FR-402 y CA-710: nunca un decimal libre. «0,5» valdría lo mismo que 1/2, pero «0,33» no
    /// vale 1/3, y aceptar uno invita a teclear el otro. «11/2» es exactamente el error de lectura que
    /// la notación con «+» evita.</summary>
    [Theory]
    [InlineData("0,5")]
    [InlineData("0.5")]
    [InlineData("0,33")]
    [InlineData("4")]
    [InlineData("2+1/2")]
    [InlineData("1/5")]
    [InlineData("media")]
    [InlineData("1½")]
    [InlineData("11/2")]
    public void Rechaza_decimales_y_valores_fuera_del_vocabulario_FR_402_CA_710(string tecleado)
    {
        Assert.False(FraccionDosisExtensiones.TryParsear(tecleado, out var leida));
        Assert.Null(leida);
    }

    [Fact]
    public void Cada_valor_se_vuelve_a_leer_desde_su_propia_notacion_impresa_y_de_entrada()
    {
        foreach (var fraccion in FraccionDosisExtensiones.Admitidas)
        {
            Assert.True(FraccionDosisExtensiones.TryParsear(fraccion.TextoEntrada(), out var desdeEntrada));
            Assert.Equal(fraccion, desdeEntrada);
            Assert.True(FraccionDosisExtensiones.TryParsear(fraccion.Texto(), out var desdeImpreso));
            Assert.Equal(fraccion, desdeImpreso);
        }
    }
}
