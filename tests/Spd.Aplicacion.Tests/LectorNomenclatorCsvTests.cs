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

    [Fact]
    public void Leer_reconoce_la_cabecera_real_del_nomenclator_oficial_y_respeta_comillas()
    {
        var ruta = EscribirCsvTemporal(
            "Código Nacional,Nombre del producto farmacéutico,Tipo de fármaco,Nombre genérico efecto y " +
            "accesorio,Código del laboratorio ofertante,Nombre del laboratorio ofertante,Estado,Fecha de " +
            "alta en el nomenclátor,Fecha de baja en el nomenclátor,Aportación del beneficiario,Principio " +
            "activo o asociación de principios activos,Precio venta al público con IVA\n" +
            "650004,\"DEPAKINE 500 mg comprimidos gastrorresistentes, 20 comprimidos\",Medicamento Etica,,345," +
            "\"SANOFI AVENTIS, S.A\",ALTA,29/10/2004,,ESPECIAL,VALPROATO SODIO,2.5\n");

        var resultado = new LectorNomenclatorCsv().Leer(ruta);

        Assert.True(resultado.Exito);
        var fila = Assert.Single(resultado.Filas);
        Assert.Equal("650004", fila.Cn);
        // La coma dentro del campo entre comillas no debe partir la columna del nombre.
        Assert.Equal("DEPAKINE 500 mg comprimidos gastrorresistentes, 20 comprimidos", fila.Nombre);
        Assert.Equal("VALPROATO SODIO", fila.PrincipioActivo);
        Assert.Equal("SANOFI AVENTIS, S.A", fila.Laboratorio);
    }

    [Fact]
    public void Leer_deja_PrincipioActivo_y_Laboratorio_a_null_si_el_fichero_no_tiene_esas_columnas()
    {
        var ruta = EscribirCsvTemporal("CN,Nombre\n654321,Paracetamol 1g\n");

        var resultado = new LectorNomenclatorCsv().Leer(ruta);

        var fila = Assert.Single(resultado.Filas);
        Assert.Null(fila.PrincipioActivo);
        Assert.Null(fila.Laboratorio);
    }
}
