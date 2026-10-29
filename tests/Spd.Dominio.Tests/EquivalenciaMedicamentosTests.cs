using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class EquivalenciaMedicamentosTests
{
    [Theory]
    [InlineData(null, null, false)]
    [InlineData("", " ", false)]
    [InlineData("GH-1", " gh-1 ", true)]
    [InlineData("GH-1", "GH-2", false)]
    public void Solo_equivale_identidad_persistida_o_grupo_explicito(string? primero, string? segundo, bool esperado)
    {
        var a = new Medicamento { Cn = "111111", Nombre = "Marca A", GrupoHomogeneo = primero };
        var b = new Medicamento { Cn = "222222", Nombre = "Marca B", GrupoHomogeneo = segundo };
        Assert.Equal(esperado, a.EsEquivalenteA(b)); // Los dos Id=0 no son la misma identidad.
        a.Id = 1; b.Id = 2;
        Assert.Equal(esperado, a.EsEquivalenteA(b));
        b.Id = 1;
        Assert.True(a.EsEquivalenteA(b));
    }
}
