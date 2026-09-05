using Spd.Dominio;
using Xunit;

namespace Spd.Aplicacion.Tests;

public sealed class ParserLineasImportacionTests
{
    private static readonly MapeoColumnasImportacion Mapeo = new(Cn: "0", NumSerie: "1", Lote: "2", Caducidad: "3");

    [Fact]
    public void ParsearTexto_sin_cabecera_mapea_columnas_por_indice()
    {
        var texto = "654321\tS123\tL1\t2027-01";

        var filas = ParserLineasImportacion.ParsearTexto(texto, Mapeo);

        var fila = Assert.Single(filas);
        Assert.Equal("654321", fila.Cn);
        Assert.Equal("S123", fila.NumSerie);
        Assert.Equal("L1", fila.Lote);
        Assert.Equal("2027-01", fila.Caducidad);
    }

    [Fact]
    public void ParsearTexto_con_varias_lineas_devuelve_una_fila_por_linea()
    {
        var texto = "654321\tS1\tL1\t2027-01\n999999\tS2\tL2\t2027-02";

        var filas = ParserLineasImportacion.ParsearTexto(texto, Mapeo);

        Assert.Equal(2, filas.Count);
        Assert.Equal("999999", filas[1].Cn);
    }

    [Fact]
    public void ParsearCsv_con_cabecera_omite_la_primera_fila()
    {
        var csv = "CN,Serie,Lote,Caducidad\n654321,S1,L1,2027-01-31";

        var filas = ParserLineasImportacion.ParsearCsv(csv, Mapeo, ",", tieneCabecera: true);

        var fila = Assert.Single(filas);
        Assert.Equal("654321", fila.Cn);
    }
}
