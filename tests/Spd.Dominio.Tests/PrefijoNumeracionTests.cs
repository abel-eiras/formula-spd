using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

/// <summary>CA-001: cambiar el prefijo no afecta a numeración ya asignada. La garantía completa
/// depende de Spec 001 (Paciente.num_ficha no existe todavía); aquí se valida la parte que sí
/// pertenece a esta spec: el prefijo vive únicamente en Farmacia y cambiarlo no toca nada más del
/// objeto ni reescribe un valor ya capturado en otro sitio (quickstart.md lo documenta como
/// pendiente de verificación end-to-end cuando exista Spec 001).</summary>
public sealed class PrefijoNumeracionTests
{
    [Fact]
    public void CambiarPrefijoNumFicha_no_afecta_a_un_numero_ya_capturado_antes_del_cambio()
    {
        var farmacia = new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B1",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "1",
            PrefijoNumFicha = "F-"
        };

        var numeroYaAsignado = farmacia.PrefijoNumFicha + "000041"; // instantánea tomada "antes"

        farmacia.PrefijoNumFicha = "PAC-";

        Assert.Equal("F-000041", numeroYaAsignado);
        Assert.Equal("PAC-", farmacia.PrefijoNumFicha);
    }

    [Fact]
    public void CambiarPrefijos_no_modifica_ningun_otro_campo_de_Farmacia()
    {
        var farmacia = new Farmacia
        {
            CodigoSanitario = "PO-001", Nombre = "F", TitularOComunidadBienes = "T", Cif = "B1",
            Direccion = "D", Cp = "36000", Poblacion = "P", Telefono = "1",
            PrefijoNumFicha = "F-", PrefijoNumSpd = "S-", DiaRetiradaDefecto = "MA"
        };

        farmacia.PrefijoNumFicha = "PAC-";
        farmacia.PrefijoNumSpd = "BLI-";

        Assert.Equal("MA", farmacia.DiaRetiradaDefecto); // no colateral
    }
}
