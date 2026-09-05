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
}
