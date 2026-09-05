using System.Text;
using Spd.Aplicacion;

namespace Spd.Infraestructura;

/// <summary>Lee el CSV de nomenclátor ya descargado (research.md Decisión 5 de Spec 003).
/// Reconoce tanto la cabecera mínima de pruebas (`CN`, `Nombre`) como la cabecera real del
/// nomenclátor oficial de facturación (`Código Nacional`, `Nombre del producto farmacéutico`,
/// y opcionalmente `Nombre del laboratorio ofertante` / `Principio activo o asociación de
/// principios activos`). El fichero oficial cita entre comillas los campos que contienen comas
/// (nombres de producto, razón social del laboratorio), así que el parseo respeta el formato CSV
/// con comillas (RFC 4180), no un `Split(',')` ingenuo. Simplificación deliberada: sin perfiles
/// de mapeo columna a columna configurables, sin soporte de saltos de línea dentro de un campo.
/// Spec 011 lo sustituirá por un `PerfilImportacion` configurable sin cambiar `ILectorNomenclator`.</summary>
public sealed class LectorNomenclatorCsv : ILectorNomenclator
{
    private static readonly string[] CabecerasCn = ["CN", "Código Nacional"];
    private static readonly string[] CabecerasNombre = ["Nombre", "Nombre del producto farmacéutico"];
    private static readonly string[] CabecerasPrincipioActivo = ["Principio activo o asociación de principios activos"];
    private static readonly string[] CabecerasLaboratorio = ["Nombre del laboratorio ofertante"];

    public ResultadoLecturaNomenclator Leer(string rutaFichero)
    {
        var lineas = File.ReadAllLines(rutaFichero);
        if (lineas.Length == 0)
        {
            return ResultadoLecturaNomenclator.Fallido("El fichero de nomenclátor está vacío.");
        }

        var cabecera = ParsearLineaCsv(lineas[0]);
        var indiceCn = IndiceDeCualquiera(cabecera, CabecerasCn);
        var indiceNombre = IndiceDeCualquiera(cabecera, CabecerasNombre);
        if (indiceCn < 0 || indiceNombre < 0)
        {
            return ResultadoLecturaNomenclator.Fallido(
                "El fichero no tiene las columnas 'CN'/'Código Nacional' y 'Nombre'/'Nombre del " +
                "producto farmacéutico' esperadas (research.md Decisión 5).");
        }

        var indicePrincipioActivo = IndiceDeCualquiera(cabecera, CabecerasPrincipioActivo);
        var indiceLaboratorio = IndiceDeCualquiera(cabecera, CabecerasLaboratorio);
        var indiceMaximoRequerido = Math.Max(indiceCn, indiceNombre);

        var filas = lineas.Skip(1)
            .Where(linea => !string.IsNullOrWhiteSpace(linea))
            .Select(ParsearLineaCsv)
            .Where(campos => campos.Length > indiceMaximoRequerido)
            .Select(campos => new FilaNomenclator(
                campos[indiceCn].Trim(),
                campos[indiceNombre].Trim(),
                ValorOpcional(campos, indicePrincipioActivo),
                ValorOpcional(campos, indiceLaboratorio)))
            .ToList();

        return ResultadoLecturaNomenclator.Exitoso(filas);
    }

    private static string? ValorOpcional(string[] campos, int indice)
        => indice >= 0 && indice < campos.Length && !string.IsNullOrWhiteSpace(campos[indice])
            ? campos[indice].Trim()
            : null;

    private static int IndiceDeCualquiera(string[] cabecera, string[] nombresPosibles)
        => nombresPosibles.Select(nombre => Array.IndexOf(cabecera, nombre)).FirstOrDefault(i => i >= 0, -1);

    // Parseo mínimo de una línea CSV con comillas (RFC 4180): una coma dentro de un campo entre
    // comillas no separa columnas, y "" dentro de un campo entre comillas es una comilla literal.
    private static string[] ParsearLineaCsv(string linea)
    {
        var campos = new List<string>();
        var actual = new StringBuilder();
        var dentroDeComillas = false;

        for (var i = 0; i < linea.Length; i++)
        {
            var c = linea[i];
            if (dentroDeComillas)
            {
                if (c == '"')
                {
                    if (i + 1 < linea.Length && linea[i + 1] == '"')
                    {
                        actual.Append('"');
                        i++;
                    }
                    else
                    {
                        dentroDeComillas = false;
                    }
                }
                else
                {
                    actual.Append(c);
                }
            }
            else if (c == '"')
            {
                dentroDeComillas = true;
            }
            else if (c == ',')
            {
                campos.Add(actual.ToString());
                actual.Clear();
            }
            else
            {
                actual.Append(c);
            }
        }

        campos.Add(actual.ToString());
        return campos.ToArray();
    }
}
