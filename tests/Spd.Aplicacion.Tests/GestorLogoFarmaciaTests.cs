using Spd.Infraestructura;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class GestorLogoFarmaciaTests
{
    [Fact]
    public void GuardarNuevoLogo_archiva_el_logo_anterior_con_fecha_antes_de_sustituirlo()
    {
        var carpetaInstalacion = Directory.CreateTempSubdirectory("spd-logo-test-").FullName;
        try
        {
            var gestor = new GestorLogoFarmacia(carpetaInstalacion);
            var logoOriginal = Path.Combine(carpetaInstalacion, "original.png");
            File.WriteAllText(logoOriginal, "logo-v1");

            var rutaGuardada1 = gestor.GuardarNuevoLogo(logoOriginal, rutaLogoActual: null);
            File.WriteAllText(rutaGuardada1, "logo-v1-guardado"); // simula el contenido ya persistido

            var logoNuevo = Path.Combine(carpetaInstalacion, "nuevo.png");
            File.WriteAllText(logoNuevo, "logo-v2");
            gestor.GuardarNuevoLogo(logoNuevo, rutaLogoActual: rutaGuardada1);

            var carpetaHistorico = Path.Combine(carpetaInstalacion, "logo", "historico");
            Assert.True(Directory.Exists(carpetaHistorico));
            Assert.Single(Directory.GetFiles(carpetaHistorico));
        }
        finally
        {
            Directory.Delete(carpetaInstalacion, recursive: true);
        }
    }
}
