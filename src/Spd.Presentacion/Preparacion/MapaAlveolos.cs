using System;
using System.Collections.Generic;
using System.Linq;
using Spd.Dominio;

namespace Spd.Presentacion.Preparacion;

/// <summary>Lo que va en un alvéolo: un medicamento y su fracción de dosis.</summary>
public sealed record ContenidoAlveolo(string Medicamento, string Inicial, string Fraccion)
{
    public override string ToString() => $"{Inicial} {Fraccion}";
}

/// <summary>Un alvéolo del blíster: un día y una toma, con lo que le corresponde (puede ser más de
/// un medicamento, o ninguno).</summary>
public sealed record Alveolo(int Dia, int Toma, IReadOnlyList<ContenidoAlveolo> Contenido)
{
    public bool Vacio => Contenido.Count == 0;
    public string Texto => string.Join("  ", Contenido.Select(c => c.ToString()));
}

/// <summary>La rejilla completa: las etiquetas de las siete columnas y los veintiocho alvéolos.
///
/// Las columnas no son siempre LU–DO: un blíster que empieza en jueves tiene el jueves en la primera
/// columna. Poner una cabecera fija de lunes a domingo encima de esos datos sería mentir sobre qué
/// día es cada alvéolo, que es justo el error que esta pantalla existe para evitar.</summary>
public sealed record RejillaAlveolosDatos(IReadOnlyList<string> Columnas, IReadOnlyList<Alveolo> Celdas);

/// <summary>La rejilla del dispositivo (Spec 015 FR-1541).
///
/// **Q2, resuelta con lo que ya dice el dominio y no por conjetura**: el modelo es 7 días × 4 tomas
/// (desayuno, almuerzo, cena y noche). No es una elección de interfaz: `Tratamiento` y `SpdLinea`
/// tienen exactamente cuatro pautas —`PautaD`, `PautaA`, `PautaC`, `PautaN`—, `DiasSemana` es una
/// máscara de siete caracteres y la validez de un blíster va de un día a seis días después. Cualquier
/// otro número de tomas exigiría cambiar el dominio, no esta rejilla.
///
/// Lo que resuelve: hoy el llenado se hace mirando una lista de líneas y traduciendo mentalmente
/// "½-0-1-0, lunes a viernes" a la posición física de cada alvéolo. Esa traducción mental es
/// justamente donde se equivoca una persona cansada.</summary>
public static class MapaAlveolos
{
    public const int Dias = 7;
    public const int Tomas = 4;

    public static readonly string[] NombresTomas = ["Desayuno", "Almuerzo", "Cena", "Noche"];

    /// <summary>Códigos de día en el orden en que los recorre `DayOfWeek` empezando en lunes.</summary>
    private static readonly string[] CodigosDia = ["LU", "MA", "MI", "JU", "VI", "SA", "DO"];

    /// <summary>Rejilla lista para pintar: cabeceras y celdas coherentes entre sí.</summary>
    public static RejillaAlveolosDatos Rejilla(IEnumerable<SpdLinea> lineas, DateOnly? validezDesde = null)
        => new(Columnas(validezDesde), Construir(lineas, validezDesde));

    /// <summary>Etiqueta de cada columna. Con fecha de validez lleva también el día del mes, para que
    /// no haya duda de a qué día concreto corresponde cada alvéolo.</summary>
    public static IReadOnlyList<string> Columnas(DateOnly? validezDesde)
    {
        var columnas = new string[Dias];
        for (var d = 0; d < Dias; d++)
        {
            if (validezDesde is { } inicio)
            {
                var fecha = inicio.AddDays(d);
                columnas[d] = $"{CodigosDia[((int)fecha.DayOfWeek + 6) % 7]} {fecha:dd/MM}";
            }
            else
            {
                columnas[d] = CodigosDia[d];
            }
        }
        return columnas;
    }

    /// <summary>Construye la rejilla desde las líneas del blíster. Se usa la **instantánea** de la
    /// línea (`Snap*`), no el catálogo actual: el Art. IV.3 exige que un blíster ya elaborado se
    /// siga leyendo como se elaboró.</summary>
    public static IReadOnlyList<Alveolo> Construir(IEnumerable<SpdLinea> lineas, DateOnly? validezDesde = null)
    {
        var acumulado = new List<ContenidoAlveolo>[Dias, Tomas];
        for (var d = 0; d < Dias; d++)
            for (var t = 0; t < Tomas; t++)
                acumulado[d, t] = [];

        foreach (var linea in lineas)
        {
            // Las líneas excluidas no se llenan: no están en el blíster (FR-640).
            if (linea.EstadoLinea == EstadoLinea.Excluida) continue;

            var pautas = new[] { linea.SnapPautaD, linea.SnapPautaA, linea.SnapPautaC, linea.SnapPautaN };

            for (var d = 0; d < Dias; d++)
            {
                if (!DiaActivo(linea.SnapDiasSemana, d, validezDesde)) continue;

                for (var t = 0; t < Tomas; t++)
                {
                    if (pautas[t] is not { } fraccion || fraccion == FraccionDosis.Cero) continue;
                    acumulado[d, t].Add(new ContenidoAlveolo(
                        linea.SnapNombre, Inicial(linea.SnapNombre), fraccion.TextoEntrada()));
                }
            }
        }

        // Se devuelve por **tomas** y luego por días: es el orden en que se lee el dispositivo
        // físico —una fila por toma, una columna por día— y el que espera la rejilla al pintarlo.
        var rejilla = new List<Alveolo>(Dias * Tomas);
        for (var t = 0; t < Tomas; t++)
            for (var d = 0; d < Dias; d++)
                rejilla.Add(new Alveolo(d, t, acumulado[d, t]));
        return rejilla;
    }

    /// <summary>`SnapDiasSemana` es una máscara de siete caracteres que empieza en **lunes**. Si el
    /// blíster no empieza en lunes, la columna `d` de la rejilla es el día natural correspondiente,
    /// no el índice de la máscara: sin esta corrección un tratamiento de "solo lunes" aparecería en
    /// la casilla equivocada de un blíster que empieza en jueves.</summary>
    private static bool DiaActivo(string mascara, int diaDelBlister, DateOnly? validezDesde)
    {
        if (string.IsNullOrEmpty(mascara)) return true;

        var indice = validezDesde is { } inicio
            ? ((int)inicio.AddDays(diaDelBlister).DayOfWeek + 6) % 7
            : diaDelBlister;

        return indice < mascara.Length && mascara[indice] == '1';
    }

    /// <summary>La inicial identifica el medicamento dentro del alvéolo sin llenarlo de texto; el
    /// nombre completo sigue estando en el propio contenido para la ayuda emergente y la lista.</summary>
    private static string Inicial(string nombre)
        => string.IsNullOrWhiteSpace(nombre) ? "?" : nombre.Trim()[..1].ToUpperInvariant();

    /// <summary>La fecha real de cada columna, cuando se conoce el inicio de validez.</summary>
    public static DateOnly? FechaDe(int diaDelBlister, DateOnly? validezDesde)
        => validezDesde?.AddDays(diaDelBlister);
}
