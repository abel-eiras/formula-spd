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

public class AspectoParecidoFirmaTests
{
    private static SpdLinea Linea(int id, string desc) => new()
    {
        SpdId = 1, TratamientoId = id, MedicamentoId = id, SnapNombre = $"M{id}", SnapCn = "1",
        SnapDescTexto = desc, SnapPautaD = FraccionDosis.Uno, SnapDiasSemana = "1111111"
    };

    [Fact]
    public void Solo_color_sin_forma_no_basta_para_avisar()
        => Assert.Empty(AspectoParecido.Detectar([Linea(1, "blanco"), Linea(2, "blanco")]));

    [Fact]
    public void Solo_forma_sin_color_no_basta_para_avisar()
        => Assert.Empty(AspectoParecido.Detectar([Linea(1, "comprimido redondo"), Linea(2, "comprimido redondo")]));

    [Fact]
    public void Tres_medicamentos_iguales_dan_tres_parejas()
        => Assert.Equal(3, AspectoParecido.Detectar([
            Linea(1, "comprimido blanco"), Linea(2, "comprimido blanco"), Linea(3, "comprimido blanco")]).Count);

    [Fact]
    public void Una_linea_excluida_no_cuenta()
    {
        var excluida = Linea(2, "comprimido blanco");
        excluida.EstadoLinea = EstadoLinea.Excluida;
        Assert.Empty(AspectoParecido.Detectar([Linea(1, "comprimido blanco"), excluida]));
    }
}
