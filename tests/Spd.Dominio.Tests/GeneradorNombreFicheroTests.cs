using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class GeneradorNombreFicheroTests
{
    [Fact]
    public void Generar_devuelve_el_nombre_largo_por_defecto_CA_700()
    {
        var nombre = GeneradorNombreFichero.Generar(
            "Ficha de preparación", "FICHA", "María", "López Vidal", "F-000123", new DateOnly(2026, 9, 4));

        Assert.Equal("Ficha de preparación. María López Vidal.04092026.pdf", nombre);
    }

    [Fact]
    public void Generar_usa_el_nombre_corto_si_supera_120_caracteres_CA_701()
    {
        var apellidosMuyLargos = new string('A', 130);

        var nombre = GeneradorNombreFichero.Generar(
            "Ficha de preparación", "FICHA", "María", apellidosMuyLargos, "F-000123", new DateOnly(2026, 9, 4));

        Assert.Equal("FICHA_F-000123_04092026.pdf", nombre);
    }

    [Fact]
    public void Generar_usa_el_nombre_corto_si_el_paciente_no_tiene_apellidos()
    {
        var nombre = GeneradorNombreFichero.Generar(
            "Ficha de preparación", "FICHA", "María", null, "F-000123", new DateOnly(2026, 9, 4));

        Assert.Equal("FICHA_F-000123_04092026.pdf", nombre);
    }

    [Fact]
    public void Generar_usa_el_nombre_corto_si_colisiona_con_uno_ya_usado_en_el_lote()
    {
        var yaUsados = new HashSet<string> { "Ficha de preparación. María López Vidal.04092026.pdf" };

        var nombre = GeneradorNombreFichero.Generar(
            "Ficha de preparación", "FICHA", "María", "López Vidal", "F-000123", new DateOnly(2026, 9, 4), yaUsados);

        Assert.Equal("FICHA_F-000123_04092026.pdf", nombre);
    }
}
