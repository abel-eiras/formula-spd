using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class CalculadoraProximaRetiradaTests
{
    [Fact]
    public void Sin_spd_previo_es_el_primer_dia_a_partir_de_hoy_incluido_Q1()
    {
        var hoy = new DateOnly(2026, 9, 7); // lunes
        Assert.Equal(hoy, CalculadoraProximaRetirada.Calcular("LU", null, hoy));
    }

    [Fact]
    public void Sin_spd_previo_si_hoy_no_es_el_dia_avanza_al_siguiente()
    {
        var hoy = new DateOnly(2026, 9, 7); // lunes
        Assert.Equal(new DateOnly(2026, 9, 10), CalculadoraProximaRetirada.Calcular("JU", null, hoy));
    }

    [Fact]
    public void Con_spd_previo_es_el_primer_dia_estrictamente_posterior_a_la_validez()
    {
        var ultimaValidez = new DateOnly(2026, 9, 10); // jueves
        var proxima = CalculadoraProximaRetirada.Calcular("JU", ultimaValidez, new DateOnly(2026, 9, 1));
        Assert.Equal(new DateOnly(2026, 9, 17), proxima);
    }
}
