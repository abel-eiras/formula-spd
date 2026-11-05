using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

public class LectorGs1DataMatrixTests
{
    // Codigos reales escaneados por Abel, pegados sin separador GS visible (prototipos/lector-datamatrix).

    [Fact]
    public void CodigoReal1_incluye_codigo_nacional_via_AI712()
    {
        var resultado = LectorGs1DataMatrix.Leer(
            "010541378799931617301031103012801214000LQG554DTM17128702619");

        Assert.NotNull(resultado);
        Assert.Equal("05413787999316", resultado!.Gtin);
        Assert.Equal("3012801", resultado.Lote);
        Assert.Equal(new FechaCaducidadGs1(2030, 10, 31), resultado.Caducidad);
        Assert.Equal("4000LQG554DTM1", resultado.NumeroSerie);
        Assert.Equal("8702619", resultado.CodigoNacional);
    }

    [Fact]
    public void CodigoReal2_sin_codigo_nacional_orden_AI_01_21_17_10()
    {
        var resultado = LectorGs1DataMatrix.Leer(
            "010847000695064721172342612146291728073110RA0462");

        Assert.NotNull(resultado);
        Assert.Equal("08470006950647", resultado!.Gtin);
        Assert.Equal("17234261214629", resultado.NumeroSerie);
        Assert.Equal(new FechaCaducidadGs1(2028, 7, 31), resultado.Caducidad);
        Assert.Equal("RA0462", resultado.Lote);
        Assert.Null(resultado.CodigoNacional);
    }

    [Fact]
    public void CodigoReal3_sin_codigo_nacional_orden_AI_01_17_10_21()
    {
        var resultado = LectorGs1DataMatrix.Leer(
            "01084700076426641727123010B010B2178339902223173");

        Assert.NotNull(resultado);
        Assert.Equal("08470007642664", resultado!.Gtin);
        Assert.Equal(new FechaCaducidadGs1(2027, 12, 30), resultado.Caducidad);
        Assert.Equal("B010B", resultado.Lote);
        Assert.Equal("78339902223173", resultado.NumeroSerie);
        Assert.Null(resultado.CodigoNacional);
    }

    [Fact]
    public void CodigoReal4_orden_AI_01_17_10_21()
    {
        var resultado = LectorGs1DataMatrix.Leer(
            "01084700097166391728103110412036X214000JFRH2XAN6S");

        Assert.NotNull(resultado);
        Assert.Equal("08470009716639", resultado!.Gtin);
        Assert.Equal(new FechaCaducidadGs1(2028, 10, 31), resultado.Caducidad);
        Assert.Equal("412036X", resultado.Lote);
        Assert.Equal("4000JFRH2XAN6S", resultado.NumeroSerie);
        Assert.Null(resultado.CodigoNacional);
    }

    // Sin el filtro de EsDecomposicionCompleta, la busqueda sin separador GS encontraba dos formas de
    // consumir la cadena entera (una se comia la fecha de caducidad dentro del lote) y el codigo se
    // descartaba como ambiguo pese a tener una unica interpretacion realmente valida.
    [Fact]
    public void CodigoReal5_descarta_descomposicion_incompleta_antes_de_juzgar_ambiguedad()
    {
        var resultado = LectorGs1DataMatrix.Leer(
            "0108470006543870219436795376788101250529317260228");

        Assert.NotNull(resultado);
        Assert.Equal("08470006543870", resultado!.Gtin);
        Assert.Equal("12505293", resultado.Lote);
        Assert.Equal("9436795376788", resultado.NumeroSerie);
        Assert.Equal(new FechaCaducidadGs1(2026, 2, 28), resultado.Caducidad);
        Assert.Null(resultado.CodigoNacional);
    }

    // Casos oficiales de SEVeM-0108.03 "Pruebas de validacion de escaneres" (v1.0), con el separador GS
    // real (0x1D) tal como especifica el documento. Cada uno reproduce un fallo de configuracion distinto
    // que un escaner puede introducir; confirman que la ruta con separador funciona incluso con contenido
    // con mayusculas/minusculas, simbolos y espacios en lote/serie.

    private const char Gs = '\u001D';

    [Fact]
    public void SevemMayusculasMinusculas()
    {
        var cadena = $"01084365715201041721010010LETRASGRANDES{Gs}21letraspequenas{Gs}7127166559";
        var resultado = LectorGs1DataMatrix.Leer(cadena);

        Assert.NotNull(resultado);
        Assert.Equal("08436571520104", resultado!.Gtin);
        Assert.Equal("LETRASGRANDES", resultado.Lote);
        Assert.Equal("letraspequenas", resultado.NumeroSerie);
        Assert.Equal(new FechaCaducidadGs1(2021, 1, 0), resultado.Caducidad);
        Assert.True(resultado.Caducidad.DiaSinEspecificar);
    }

    [Fact]
    public void SevemCaracteresEspeciales()
    {
        var cadena = $"01084365715201041721010010////_ _ _ _{Gs}21----....{Gs}7127166559";
        var resultado = LectorGs1DataMatrix.Leer(cadena);

        Assert.NotNull(resultado);
        Assert.Equal("////_ _ _ _", resultado!.Lote);
        Assert.Equal("----....", resultado.NumeroSerie);
    }

    [Fact]
    public void SevemLetrasIntercambiadasYZ()
    {
        var cadena = $"01084365715201041721010010ZZZZZ{Gs}21YYYYY{Gs}7127166559";
        var resultado = LectorGs1DataMatrix.Leer(cadena);

        Assert.NotNull(resultado);
        Assert.Equal("ZZZZZ", resultado!.Lote);
        Assert.Equal("YYYYY", resultado.NumeroSerie);
    }

    [Fact]
    public void SevemInversionDeColores()
    {
        var cadena = $"010843657152010417210100101234567890{Gs}21ABCDEF{Gs}7127166559";
        var resultado = LectorGs1DataMatrix.Leer(cadena);

        Assert.NotNull(resultado);
        Assert.Equal("1234567890", resultado!.Lote);
        Assert.Equal("ABCDEF", resultado.NumeroSerie);
    }

    // FR-1204: un codigo que no se puede interpretar no bloquea, simplemente no se reconoce.

    [Theory]
    [InlineData("")]
    [InlineData("no es un datamatrix")]
    [InlineData("999999999999999999999999999999")]
    public void Cadena_no_reconocida_devuelve_null(string cadena)
    {
        Assert.Null(LectorGs1DataMatrix.Leer(cadena));
    }

    // FechaCaducidadGs1.ComoFecha(): dia "00" (GS1, sin dia especifico) se interpreta como el ultimo
    // dia del mes; el campo sigue siendo editable en pantalla si el pico farmaceutico lo requiere.

    [Fact]
    public void ComoFecha_con_dia_especifico_lo_respeta()
    {
        Assert.Equal(new DateOnly(2030, 10, 31), new FechaCaducidadGs1(2030, 10, 31).ComoFecha());
    }

    [Fact]
    public void ComoFecha_con_dia_sin_especificar_usa_ultimo_dia_del_mes()
    {
        Assert.Equal(new DateOnly(2021, 1, 31), new FechaCaducidadGs1(2021, 1, 0).ComoFecha());
        Assert.Equal(new DateOnly(2028, 2, 29), new FechaCaducidadGs1(2028, 2, 0).ComoFecha()); // bisiesto
    }
}

/// <summary>spec-012 v0.5: CN deducido del GTIN (FR-1209), lecturas ambiguas con candidatas (FR-1210),
/// separador sustituto e identificadores de simbología (FR-1211).</summary>
public class LectorGs1DataMatrixAmpliacionTests
{
    private const char Gs = '\u001D';

    [Fact]
    public void Gtin_espanol_da_el_CN_de_seis_digitos()
    {
        var datos = LectorGs1DataMatrix.Leer("010847000695064721172342612146291728073110RA0462")!;
        Assert.Null(datos.CodigoNacional);
        Assert.Equal("695064", datos.CnIdentificado);
    }

    [Fact]
    public void Gtin_extranjero_sin_712_no_da_CN()
    {
        Assert.Null(CodigoNacionalEspanol.DesdeGtin("05413787999316"));
    }

    [Fact]
    public void AI712_con_digito_de_control_del_CN_se_reduce_a_seis_digitos()
    {
        var datos = LectorGs1DataMatrix.Leer("010541378799931617301031103012801214000LQG554DTM17128702619")!;
        Assert.Equal("870261", datos.CnIdentificado);
        Assert.Equal(["8702619", "870261"], datos.CodigosNacionalesCandidatos);
    }

    [Theory]
    [InlineData("08470006950647", "695064")]
    [InlineData("8470006950647", "695064")]   // EAN-13 lineal de los medicamentos sin receta
    public void CN_desde_GTIN14_y_EAN13(string codigo, string cn)
    {
        Assert.Equal(cn, CodigoNacionalEspanol.DesdeGtin(codigo));
    }

    [Fact]
    public void GTIN_espanol_con_digito_de_control_mal_no_da_CN()
    {
        Assert.Null(CodigoNacionalEspanol.DesdeGtin("08470006950646"));
    }

    [Fact]
    public void Sin_GS_con_dos_interpretaciones_devuelve_las_candidatas_sin_elegir()
    {
        var lectura = LectorGs1DataMatrix.Interpretar("01084700069506471728073110AB21CD21EF");

        Assert.Equal(EstadoLecturaGs1.Ambiguo, lectura.Estado);
        Assert.Null(lectura.Datos);
        Assert.Contains(lectura.Candidatas, c => c is { Lote: "AB21CD", NumeroSerie: "EF" });
        Assert.Contains(lectura.Candidatas, c => c is { Lote: "AB", NumeroSerie: "CD21EF" });
        Assert.Equal("08470006950647", lectura.IdentidadComun!.Gtin);
        Assert.Null(LectorGs1DataMatrix.Leer("01084700069506471728073110AB21CD21EF"));
    }

    [Fact]
    public void Separador_sustituto_se_trata_como_GS()
    {
        var lectura = LectorGs1DataMatrix.Interpretar("01084700069506471728073110AB21CD~21EF", "~");

        Assert.Equal(EstadoLecturaGs1.Reconocido, lectura.Estado);
        Assert.Equal("AB21CD", lectura.Datos!.Lote);
        Assert.Equal("EF", lectura.Datos.NumeroSerie);
    }

    [Fact]
    public void Identificador_de_simbologia_y_FNC1_inicial_se_ignoran()
    {
        var datos = LectorGs1DataMatrix.Leer($"]d2{Gs}01084700069506471728073110AB21CD{Gs}21EF");
        Assert.NotNull(datos);
        Assert.Equal("AB21CD", datos!.Lote);
    }

    [Fact]
    public void Fecha_imposible_no_se_acepta()
    {
        Assert.Equal(EstadoLecturaGs1.NoReconocido,
            LectorGs1DataMatrix.Interpretar($"01084700069506471728133110AB{Gs}21EF").Estado);
    }

    [Fact]
    public void GTIN_con_digito_de_control_mal_no_se_acepta()
    {
        Assert.Null(LectorGs1DataMatrix.Leer($"01084700069506461728073110AB{Gs}21EF"));
    }
}

public class Gs1Tests
{
    [Theory]
    [InlineData("08470006950647", true)]
    [InlineData("8470006950647", true)]
    [InlineData("08470006950646", false)]
    [InlineData("0847000695064A", false)]
    [InlineData("1234567", false)]
    [InlineData("", false)]
    public void Digito_de_control(string codigo, bool correcto)
        => Assert.Equal(correcto, Gs1.DigitoControlCorrecto(codigo));

    [Theory]
    [InlineData("7127293", "712729")]
    [InlineData("712729", "712729")]
    [InlineData(" ", null)]
    [InlineData(null, null)]
    public void CN_sin_digito_de_control(string? entrada, string? esperado)
        => Assert.Equal(esperado, CodigoNacionalEspanol.SinDigitoDeControl(entrada));
}
