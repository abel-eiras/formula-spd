using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class ValidadorCipTests
{
    private static readonly DateOnly FechaNacimiento = new(1991, 4, 10);
    private const string Apellidos = "Eiras Espiño";

    [Fact]
    public void Autocompletar_propone_las_posiciones_1_a_11_para_CA_014()
        => Assert.Equal("910410EEIS1", ValidadorCip.Autocompletar(FechaNacimiento, Apellidos, "H"));

    [Fact]
    public void Corresponde_es_verdadero_cuando_el_cip_coincide_con_los_datos()
        => Assert.True(ValidadorCip.Corresponde("910410EEIS1014", FechaNacimiento, Apellidos, "H"));

    [Fact]
    public void Corresponde_avisa_sin_bloquear_cuando_el_sexo_no_coincide_CA_015()
        => Assert.False(ValidadorCip.Corresponde("910410EEIS1014", FechaNacimiento, Apellidos, "M"));

    [Fact]
    public void Corresponde_es_falso_con_longitud_incorrecta()
        => Assert.False(ValidadorCip.Corresponde("910410EEIS1", FechaNacimiento, Apellidos, "H"));
}
