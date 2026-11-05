using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

/// <summary>Spec 006 FR-699: aviso de aspecto parecido en el mismo alvéolo.</summary>
public class AspectoParecidoTests
{
    private static SpdLinea Linea(int medicamentoId, string nombre, string? desc, FraccionDosis? d = null,
        FraccionDosis? n = null, string dias = "1111111") => new()
    {
        SpdId = 1, TratamientoId = medicamentoId, MedicamentoId = medicamentoId, SnapNombre = nombre, SnapCn = "1",
        SnapDescTexto = desc, SnapPautaD = d, SnapPautaN = n, SnapDiasSemana = dias
    };

    [Fact]
    public void Misma_forma_y_color_en_la_misma_toma_avisa()
    {
        var avisos = AspectoParecido.Detectar([
            Linea(1, "Enalapril", "comprimido blanco redondo, serigrafía E20", d: FraccionDosis.Uno),
            Linea(2, "Amlodipino", "Comprimido redondo blanco", d: FraccionDosis.Uno)]);
        var aviso = Assert.Single(avisos);
        Assert.Contains("Enalapril y Amlodipino", aviso);
        Assert.Contains("desayuno", aviso);
    }

    [Fact]
    public void Distinto_color_no_avisa()
    {
        Assert.Empty(AspectoParecido.Detectar([
            Linea(1, "A", "comprimido blanco redondo", d: FraccionDosis.Uno),
            Linea(2, "B", "comprimido rosa redondo", d: FraccionDosis.Uno)]));
    }

    [Fact]
    public void Tomas_distintas_no_avisa()
    {
        Assert.Empty(AspectoParecido.Detectar([
            Linea(1, "A", "comprimido blanco redondo", d: FraccionDosis.Uno),
            Linea(2, "B", "comprimido blanco redondo", n: FraccionDosis.Uno)]));
    }

    [Fact]
    public void Dias_que_no_coinciden_no_avisa()
    {
        Assert.Empty(AspectoParecido.Detectar([
            Linea(1, "A", "comprimido blanco redondo", d: FraccionDosis.Uno, dias: "1000000"),
            Linea(2, "B", "comprimido blanco redondo", d: FraccionDosis.Uno, dias: "0100000")]));
    }

    [Fact]
    public void Sin_descripcion_no_afirma_nada()
    {
        Assert.Empty(AspectoParecido.Detectar([
            Linea(1, "A", null, d: FraccionDosis.Uno),
            Linea(2, "B", "comprimido blanco redondo", d: FraccionDosis.Uno)]));
    }
}
