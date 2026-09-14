using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class CalculadoraUnidadesADescontarTests
{
    private static Tratamiento Crear(FraccionDosis? d, string diasSemana, int? ajusteUnidadesManual = null) => new()
    {
        PacienteId = 1,
        MedicamentoId = 1,
        PautaD = d,
        DiasSemana = diasSemana,
        FechaInicio = new DateOnly(2026, 1, 1),
        FechaPrescripcionInicial = new DateOnly(2026, 1, 1),
        AjusteUnidadesManual = ajusteUnidadesManual
    };

    [Fact]
    public void Sin_fraccion_la_suma_semanal_exacta_CA_507_negativo()
    {
        var tratamiento = Crear(FraccionDosis.Uno, "1111111");
        Assert.Equal(7, CalculadoraUnidadesADescontar.Calcular(tratamiento));
    }

    [Fact]
    public void Con_fraccion_suma_no_entera_descuenta_entero_mas_uno_CA_507()
    {
        var tratamiento = Crear(FraccionDosis.Media, "1111111");
        Assert.Equal(4, CalculadoraUnidadesADescontar.Calcular(tratamiento));
    }

    [Fact]
    public void Con_fraccion_aunque_la_suma_ya_sea_entera_descuenta_entero_mas_uno_CA_507b()
    {
        var tratamiento = Crear(FraccionDosis.Media, "1111110");
        Assert.Equal(4, CalculadoraUnidadesADescontar.Calcular(tratamiento));
    }

    [Fact]
    public void Ajuste_manual_sobrescribe_el_calculo_CA_507c()
    {
        var tratamiento = Crear(FraccionDosis.Media, "1111111", ajusteUnidadesManual: 5);
        Assert.Equal(5, CalculadoraUnidadesADescontar.Calcular(tratamiento));
    }

    /// <summary>CA-507b con tercios. Un tercio de comprimido en tres días suma **exactamente** un
    /// comprimido, así que la regla da 1 + 1 = 2. Pero `1m/3m` en `decimal` es 0,333…3, y por tres da
    /// 0,999…9: su parte entera es 0 y el cálculo daba 1. Se descontaba una unidad de menos del envase
    /// del paciente, y el listado de retirada creía que había más de lo que había.</summary>
    [Fact]
    public void Un_tercio_en_tres_dias_suma_un_entero_exacto_y_descuenta_dos_CA_507b()
    {
        var tratamiento = Crear(FraccionDosis.UnTercio, "1110000");
        Assert.Equal(2, CalculadoraUnidadesADescontar.Calcular(tratamiento));
    }

    /// <summary>La otra cara del mismo error, y más grave. El listado de retirada estima las unidades
    /// necesarias con `Math.Ceiling(SumaSemanalReal)`. Dos tercios en tres días suman exactamente
    /// dos comprimidos; en `decimal` dan 2,000…1, y el techo lo sube a **3**. Eso es un faltante que
    /// no existe, y un faltante bloquea abrir la sesión de preparación (CA-601).</summary>
    [Fact]
    public void Dos_tercios_en_tres_dias_suman_exactamente_dos_sin_faltante_fantasma_CA_500()
    {
        var tratamiento = Crear(FraccionDosis.DosTercios, "1110000");
        Assert.Equal(2m, CalculadoraUnidadesADescontar.SumaSemanalReal(tratamiento));
        Assert.Equal(2, (int)Math.Ceiling(CalculadoraUnidadesADescontar.SumaSemanalReal(tratamiento)));
    }

    /// <summary>Los valores nuevos siguen la misma regla (Decisión 6), con aritmética exacta.</summary>
    [Theory]
    [InlineData(FraccionDosis.Dos, "1111111", 14)]              // 2 × 7 = 14, sin fracción
    [InlineData(FraccionDosis.Tres, "1010100", 9)]              // 3 × 3 = 9, sin fracción
    [InlineData(FraccionDosis.UnoYTercio, "1110000", 5)]        // 4/3 × 3 = 4 exacto → 4 + 1
    [InlineData(FraccionDosis.UnoYDosTercios, "1110000", 6)]    // 5/3 × 3 = 5 exacto → 5 + 1
    [InlineData(FraccionDosis.UnoYTresCuartos, "1111111", 13)]  // 7/4 × 7 = 12,25 → 12 + 1
    public void Los_valores_ampliados_aplican_la_misma_regla_sin_error_de_redondeo(
        FraccionDosis dosis, string dias, int esperado)
        => Assert.Equal(esperado, CalculadoraUnidadesADescontar.Calcular(Crear(dosis, dias)));
}
