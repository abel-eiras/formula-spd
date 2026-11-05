using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

/// <summary>Spec 018: cuándo una alerta alcanza a una caja.</summary>
public class AlertaCalidadTests
{
    private static AlertaCalidad Alerta(string cn = "654321", string lote = "AB-123") => new()
    {
        Referencia = "R 1/2026", FechaAlerta = new DateOnly(2026, 11, 1), Cn = cn, Lote = lote
    };

    [Theory]
    [InlineData("654321", "AB-123")]
    [InlineData("654321", "ab 123")]
    [InlineData("654321", "AB123")]
    [InlineData("6543217", "AB123")]   // CN con su dígito de control
    public void Coincide_ignorando_guiones_espacios_mayusculas_y_digito_de_control(string cn, string lote)
    {
        Assert.True(Alerta().Afecta(cn, lote));
    }

    [Theory]
    [InlineData("654321", "AB124")]
    [InlineData("654320", "AB123")]
    [InlineData("654321", null)]
    [InlineData("654321", "")]
    public void No_coincide_con_otro_lote_otro_CN_o_sin_lote(string cn, string? lote)
    {
        Assert.False(Alerta().Afecta(cn, lote));
    }

    [Fact]
    public void Traza_sin_nada_en_circulacion_no_requiere_accion()
    {
        var traza = new TrazabilidadAlerta(Alerta(),
            [new EnvaseAfectado(1, 1, "P", "S", "AB123", EstadoEnvase.Agotado, 0)],
            [new SpdAfectado(1, "F-1", 1, "P", EstadoSpd.Anulado, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 7), "AB123")]);
        Assert.False(traza.RequiereAccion);
    }
}
