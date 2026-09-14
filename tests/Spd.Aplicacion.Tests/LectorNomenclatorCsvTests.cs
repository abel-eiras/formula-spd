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

    /// <summary>El nomenclátor real mezcla medicamentos, efectos y accesorios y códigos de
    /// facturación. Los nombres son copia literal del fichero de septiembre de 2026.</summary>
    [Fact]
    public void Distingue_medicamentos_de_accesorios_y_bajas_de_altas()
    {
        var ruta = EscribirCsvTemporal(
            "Código Nacional,Nombre del producto farmacéutico,Tipo de fármaco,Nombre genérico efecto y accesorio,Estado\n" +
            "650004,\"DEPAKINE 500 mg comprimidos gastrorresistentes, 20 comprimidos\",Medicamento Etica,,ALTA\n" +
            "700001,LAMOTRIGINA KERN PHARMA 100MG 56 COMPR DISPER EFG,Medicamento Generico,,BAJA GENERAL\n" +
            "700002,OLMETEC PLUS 20/12,Medicamento Etica,,SUSPENSION TEMPORAL GENERAL\n" +
            "400011,MODERMA FLEX ABIERTA PLANA MINI OPACA 15-55MM 30U,,BOLSAS ILEOST RES SINT MIC FIL,ALTA\n");

        var filas = new LectorNomenclatorCsv().Leer(ruta).Filas;

        Assert.True(filas.Single(f => f.Cn == "650004") is { EsMedicamento: true, EstaDeBaja: false });
        Assert.True(filas.Single(f => f.Cn == "700001") is { EsMedicamento: true, EstaDeBaja: true });
        // Una suspensión temporal no es una baja: el medicamento sigue existiendo.
        Assert.True(filas.Single(f => f.Cn == "700002") is { EsMedicamento: true, EstaDeBaja: false });
        // Tipo vacío con la columna presente: es un accesorio, no un medicamento.
        var accesorio = filas.Single(f => f.Cn == "400011");
        Assert.Equal(string.Empty, accesorio.TipoFarmaco);
        Assert.False(accesorio.EsMedicamento);
    }

    [Fact]
    public void Sin_columnas_de_tipo_ni_estado_todo_es_medicamento_en_alta()
    {
        var ruta = EscribirCsvTemporal("CN,Nombre\n654321,Paracetamol 1g\n");

        var fila = Assert.Single(new LectorNomenclatorCsv().Leer(ruta).Filas);

        Assert.Null(fila.TipoFarmaco);
        Assert.Null(fila.Estado);
        Assert.True(fila.EsMedicamento);
        Assert.False(fila.EstaDeBaja);
    }
}
