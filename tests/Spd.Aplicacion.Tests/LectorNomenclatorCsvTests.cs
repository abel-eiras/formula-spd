using Spd.Infraestructura;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class LectorNomenclatorCsvTests
{
    private static string EscribirCsvTemporal(string contenido)
    {
        var ruta = Path.GetTempFileName();
        File.WriteAllText(ruta, contenido);
        return ruta;
    }

    [Fact]
    public void Leer_extrae_filas_de_un_csv_con_cabecera_CN_Nombre()
    {
        var ruta = EscribirCsvTemporal("CN,Nombre\n654321,Paracetamol 1g\n111111,Ibuprofeno 600mg\n");

        var resultado = new LectorNomenclatorCsv().Leer(ruta);

        Assert.True(resultado.Exito);
        Assert.Equal(2, resultado.Filas.Count);
        Assert.Contains(resultado.Filas, f => f.Cn == "654321" && f.Nombre == "Paracetamol 1g");
    }

    [Fact]
    public void Leer_devuelve_error_explicito_si_faltan_las_columnas_esperadas()
    {
        var ruta = EscribirCsvTemporal("Codigo,Descripcion\n654321,Paracetamol 1g\n");

        var resultado = new LectorNomenclatorCsv().Leer(ruta);

        Assert.False(resultado.Exito);
        Assert.NotNull(resultado.Error);
        Assert.Empty(resultado.Filas);
    }
}
