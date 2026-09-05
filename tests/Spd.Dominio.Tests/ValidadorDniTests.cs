using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public sealed class ValidadorDniTests
{
    [Theory]
    [InlineData("12345678Z")]
    [InlineData("X1234567L")]
    public void EsValido_acepta_dni_y_nie_con_letra_de_control_correcta(string dni)
        => Assert.True(ValidadorDni.EsValido(dni));

    [Theory]
    [InlineData("12345678A")] // letra de control incorrecta
    [InlineData("1234567Z")] // longitud incorrecta
    [InlineData("ABCDEFGHZ")] // no son dígitos
    [InlineData("")]
    public void EsValido_rechaza_formato_o_letra_incorrectos(string dni)
        => Assert.False(ValidadorDni.EsValido(dni));
}
