using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

/// <summary>Spec 003 FR-312: propuesta de descripción física desde el texto de la ficha técnica.</summary>
public class AnalizadorDescripcionFisicaTests
{
    [Fact]
    public void Comprimido_recubierto_con_marca_ranura_y_diametro()
    {
        var p = AnalizadorDescripcionFisica.Analizar(
            "Comprimidos recubiertos con película, de color blanco, redondos, biconvexos, con la inscripción \"M 20\" grabada en una cara y ranurados en la otra. Diámetro: 8 mm.");

        Assert.Equal("comprimido recubierto redondo", p.Forma);
        Assert.Equal("blanco", p.Color);
        Assert.Equal("en una cara", p.Ranura);
        Assert.Equal("M 20", p.Serigrafia);
        Assert.Equal("8 mm", p.Tamano);
        Assert.Contains(PropuestaDescripcionFisica.AvisoRanura, p.Avisos);
    }

    [Fact]
    public void Blanco_o_casi_blanco_no_se_simplifica()
    {
        var p = AnalizadorDescripcionFisica.Analizar("Comprimido oblongo, blanco o casi blanco, de 10 x 5 mm.");
        Assert.Equal("blanco o casi blanco", p.Color);
        Assert.Equal("comprimido oblongo", p.Forma);
        Assert.Equal("10 × 5 mm", p.Tamano);
    }

    [Fact]
    public void Capsula_bicolor_avisa()
    {
        var p = AnalizadorDescripcionFisica.Analizar("Cápsulas duras de gelatina con tapa azul y cuerpo amarilla.");
        Assert.Equal("cápsula dura", p.Forma);
        Assert.Equal("azul y amarillo", p.Color);
        Assert.Contains(p.Avisos, a => a.Contains("más de un color"));
        Assert.Null(p.Ranura);
    }

    [Fact]
    public void Sin_ranura_explicita_no_propone_ranura()
    {
        var p = AnalizadorDescripcionFisica.Analizar("Comprimido redondo de color rosa, sin ranura.");
        Assert.Null(p.Ranura);
        Assert.Equal("rosa", p.Color);
    }

    [Fact]
    public void Comillas_sin_hablar_de_marca_no_son_serigrafia()
    {
        Assert.Null(AnalizadorDescripcionFisica.Analizar("Comprimido \"tipo A\" blanco.").Serigrafia);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Solución oral transparente.")]
    public void Sin_datos_de_aspecto_no_inventa(string texto)
    {
        var p = AnalizadorDescripcionFisica.Analizar(texto);
        Assert.True(p.Vacia);
        Assert.Contains(PropuestaDescripcionFisica.AvisoSinDatos, p.Avisos);
    }
}
