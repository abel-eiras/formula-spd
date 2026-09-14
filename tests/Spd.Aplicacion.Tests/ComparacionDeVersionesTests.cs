using Spd.Infraestructura;
using Xunit;

namespace Spd.Aplicacion.Tests;

/// <summary>La comparación de versiones del comprobador de actualizaciones (FR-050).
///
/// Existe por un defecto que solo se habría visto al publicar la primera release: la comprobación era
/// `tagName != versionInstalada`, comparación textual. Con la etiqueta `v0.1.0` —la forma habitual de
/// etiquetar en GitHub— y la versión instalada `0.1.0`, la aplicación habría ofrecido actualizarse a
/// su propia versión indefinidamente. Y una etiqueta anterior también se habría anunciado como nueva.</summary>
public sealed class ComparacionDeVersionesTests
{
    [Theory]
    // La misma versión no es una novedad, con o sin la `v` de la etiqueta.
    [InlineData("0.1.0", "0.1.0", false)]
    [InlineData("v0.1.0", "0.1.0", false)]
    [InlineData("V0.1.0", "0.1.0", false)]
    // Posterior sí.
    [InlineData("v0.2.0", "0.1.0", true)]
    [InlineData("v1.0.0", "0.9.9", true)]
    [InlineData("0.1.1", "0.1.0", true)]
    // Anterior no: publicar por error una release vieja no debe pedir "actualizar" hacia atrás.
    [InlineData("v0.1.0", "0.2.0", false)]
    [InlineData("v0.9.0", "1.0.0", false)]
    // Sufijos de preliberación y metadatos de compilación no rompen la comparación.
    [InlineData("v0.2.0-beta", "0.1.0", true)]
    [InlineData("v0.1.0+abc123", "0.1.0", false)]
    // Betas (SemVer): una preliberación va antes que su versión final y que las betas siguientes.
    [InlineData("v0.1.0", "0.1.0-beta", true)]
    [InlineData("v0.1.0-beta", "0.1.0-beta", false)]
    [InlineData("v0.1.0-beta", "0.1.0-beta+67504d6", false)]
    [InlineData("v0.1.0-beta", "0.1.0", false)]
    [InlineData("v0.1.0-beta.2", "0.1.0-beta", true)]
    [InlineData("v0.1.0-beta.10", "0.1.0-beta.9", true)]
    [InlineData("v0.1.0-beta.9", "0.1.0-beta.10", false)]
    [InlineData("v0.1.0-rc.1", "0.1.0-beta.3", true)]
    [InlineData("v0.2.0-beta", "0.1.0-beta.7", true)]
    public void Solo_una_version_posterior_cuenta_como_novedad(string etiqueta, string instalada, bool esperado)
        => Assert.Equal(esperado, ServicioActualizaciones.EsMasNueva(etiqueta, instalada));

    [Fact]
    public void Sin_etiqueta_no_hay_novedad_y_sin_version_instalada_si()
    {
        Assert.False(ServicioActualizaciones.EsMasNueva(null, "0.1.0"));
        Assert.False(ServicioActualizaciones.EsMasNueva("   ", "0.1.0"));
        // Si no se sabe qué está instalado, se informa de la release y decide el administrador.
        Assert.True(ServicioActualizaciones.EsMasNueva("v0.1.0", null));
    }

    [Fact]
    public void Una_etiqueta_que_no_es_una_version_cae_a_la_comparacion_textual()
    {
        // No se inventa un orden que no existe: distinta = hay algo nuevo que mirar; igual = nada.
        Assert.True(ServicioActualizaciones.EsMasNueva("beta-gallega", "0.1.0"));
        Assert.False(ServicioActualizaciones.EsMasNueva("beta-gallega", "beta-gallega"));
        Assert.False(ServicioActualizaciones.EsMasNueva("BETA-GALLEGA", "beta-gallega"));
    }
}
