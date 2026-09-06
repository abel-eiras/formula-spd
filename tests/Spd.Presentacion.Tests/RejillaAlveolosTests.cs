using System;
using System.Linq;
using Spd.Dominio;
using Spd.Presentacion.Preparacion;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 015 CA-1531: la rejilla coloca cada pauta en el alvéolo que le toca. Es la
/// traducción que hoy hace mentalmente quien llena el blíster, y es donde se falla.</summary>
public sealed class RejillaAlveolosTests
{
    private const int Desayuno = 0;
    private const int Almuerzo = 1;
    private const int Cena = 2;
    private const int Noche = 3;

    private static SpdLinea Linea(
        string nombre, FraccionDosis? d = null, FraccionDosis? a = null, FraccionDosis? c = null,
        FraccionDosis? n = null, string dias = "1111111", EstadoLinea estado = EstadoLinea.Normal) => new()
    {
        SpdId = 1, TratamientoId = 1, MedicamentoId = 1,
        SnapNombre = nombre, SnapCn = "654321", SnapDiasSemana = dias,
        SnapPautaD = d, SnapPautaA = a, SnapPautaC = c, SnapPautaN = n,
        EstadoLinea = estado
    };

    private static Alveolo Celda(System.Collections.Generic.IReadOnlyList<Alveolo> rejilla, int dia, int toma)
        => rejilla.Single(x => x.Dia == dia && x.Toma == toma);

    /// <summary>Un blíster que empieza en lunes: la máscara de días y las columnas coinciden.</summary>
    private static readonly DateOnly LunesDeSeptiembre = new(2026, 9, 7);

    [Fact]
    public void Refleja_la_pauta_y_los_dias_CA_1531()
    {
        // ½-0-1-0 todos los días.
        var rejilla = MapaAlveolos.Construir(
            [Linea("Enalapril", d: FraccionDosis.Media, c: FraccionDosis.Uno)], LunesDeSeptiembre);

        Assert.Equal(MapaAlveolos.Dias * MapaAlveolos.Tomas, rejilla.Count);

        for (var dia = 0; dia < MapaAlveolos.Dias; dia++)
        {
            Assert.Equal("E 1/2", Celda(rejilla, dia, Desayuno).Texto);
            Assert.Equal("E 1", Celda(rejilla, dia, Cena).Texto);
            Assert.True(Celda(rejilla, dia, Almuerzo).Vacio);
            Assert.True(Celda(rejilla, dia, Noche).Vacio);
        }
    }

    [Fact]
    public void Los_dias_sin_toma_quedan_vacios_CA_1531()
    {
        // Lunes, miércoles y viernes: máscara desde lunes.
        var rejilla = MapaAlveolos.Construir(
            [Linea("Furosemida", d: FraccionDosis.Uno, dias: "1010100")], LunesDeSeptiembre);

        Assert.Equal("F 1", Celda(rejilla, 0, Desayuno).Texto);
        Assert.True(Celda(rejilla, 1, Desayuno).Vacio);
        Assert.Equal("F 1", Celda(rejilla, 2, Desayuno).Texto);
        Assert.True(Celda(rejilla, 3, Desayuno).Vacio);
        Assert.Equal("F 1", Celda(rejilla, 4, Desayuno).Texto);
        Assert.True(Celda(rejilla, 5, Desayuno).Vacio);
        Assert.True(Celda(rejilla, 6, Desayuno).Vacio);
    }

    /// <summary>El caso que un cálculo ingenuo se come: un blíster que **no** empieza en lunes. La
    /// máscara siempre empieza en lunes, pero la primera columna es el primer día de validez.</summary>
    [Fact]
    public void Un_blister_que_empieza_en_jueves_coloca_los_dias_donde_toca()
    {
        var jueves = new DateOnly(2026, 9, 10);
        Assert.Equal(DayOfWeek.Thursday, jueves.DayOfWeek);

        // Solo lunes.
        var rejilla = MapaAlveolos.Construir([Linea("Metformina", d: FraccionDosis.Uno, dias: "1000000")], jueves);

        // Jueves, viernes, sábado, domingo vacíos; el lunes cae en la quinta columna.
        for (var dia = 0; dia < 4; dia++) Assert.True(Celda(rejilla, dia, Desayuno).Vacio);
        Assert.Equal("M 1", Celda(rejilla, 4, Desayuno).Texto);
        Assert.True(Celda(rejilla, 5, Desayuno).Vacio);
        Assert.True(Celda(rejilla, 6, Desayuno).Vacio);

        // Y las cabeceras lo dicen: la primera columna es jueves.
        var columnas = MapaAlveolos.Columnas(jueves);
        Assert.StartsWith("JU", columnas[0]);
        Assert.StartsWith("LU", columnas[4]);
    }

    [Fact]
    public void Dos_lineas_comparten_alveolo_CA_1531()
    {
        var rejilla = MapaAlveolos.Construir(
            [Linea("Enalapril", d: FraccionDosis.Media), Linea("Omeprazol", d: FraccionDosis.Uno)],
            LunesDeSeptiembre);

        var celda = Celda(rejilla, 0, Desayuno);
        Assert.Equal(2, celda.Contenido.Count);
        Assert.Contains(celda.Contenido, c => c.Medicamento == "Enalapril" && c.Fraccion == "1/2");
        Assert.Contains(celda.Contenido, c => c.Medicamento == "Omeprazol" && c.Fraccion == "1");
        Assert.Equal("E 1/2  O 1", celda.Texto);
    }

    [Fact]
    public void Una_linea_excluida_no_llena_ningun_alveolo()
    {
        // FR-640: una línea excluida no está en el blíster, y por tanto no se llena.
        var rejilla = MapaAlveolos.Construir(
            [Linea("Sintrom", d: FraccionDosis.Uno, estado: EstadoLinea.Excluida)], LunesDeSeptiembre);

        Assert.All(rejilla, celda => Assert.True(celda.Vacio));
    }
}
