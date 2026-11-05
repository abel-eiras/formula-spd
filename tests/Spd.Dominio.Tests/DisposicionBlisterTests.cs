using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

/// <summary>Spec 016: fecha de cada columna y toma de cada fila según la disposición del blíster.</summary>
public class DisposicionBlisterTests
{
    private static readonly DateOnly Jueves = new(2026, 9, 10);

    [Fact]
    public void Cronologico_empieza_por_el_primer_dia_de_validez()
    {
        var fechas = DisposicionBlister.PorDefecto.FechasDeColumnas(Jueves);
        Assert.Equal(Jueves, fechas[0]);
        Assert.Equal(Jueves.AddDays(6), fechas[6]);
    }

    [Fact]
    public void Convencional_va_de_lunes_a_domingo_dentro_del_periodo()
    {
        var fechas = new DisposicionBlister(InicioBlister.Convencional, false, false).FechasDeColumnas(Jueves);
        Assert.Equal(DayOfWeek.Monday, fechas[0].DayOfWeek);
        Assert.Equal(DayOfWeek.Sunday, fechas[6].DayOfWeek);
        Assert.Equal(new DateOnly(2026, 9, 14), fechas[0]);
        Assert.All(fechas, f => Assert.InRange(f, Jueves, Jueves.AddDays(6)));
    }

    [Fact]
    public void Convencional_empezando_en_lunes_coincide_con_el_cronologico()
    {
        var lunes = new DateOnly(2026, 9, 7);
        Assert.Equal(DisposicionBlister.PorDefecto.FechasDeColumnas(lunes),
            new DisposicionBlister(InicioBlister.Convencional, false, false).FechasDeColumnas(lunes));
    }

    [Fact]
    public void Derecha_a_izquierda_invierte_las_columnas()
    {
        var fechas = new DisposicionBlister(InicioBlister.Cronologico, true, false).FechasDeColumnas(Jueves);
        Assert.Equal(Jueves, fechas[6]);
        Assert.Equal(Jueves.AddDays(6), fechas[0]);
    }

    [Theory]
    [InlineData(false, 0, 0)]
    [InlineData(false, 3, 3)]
    [InlineData(true, 0, 3)]
    [InlineData(true, 3, 0)]
    public void Noche_arriba_invierte_las_tomas(bool nocheArriba, int fila, int toma)
    {
        Assert.Equal(toma, new DisposicionBlister(InicioBlister.Cronologico, false, nocheArriba).TomaDeFila(fila));
    }
}
