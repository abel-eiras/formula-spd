using System.Linq;
using System.Text;

namespace LectorDataMatrix;

/// <summary>
/// Interpreta la cadena que un lector de codigo de barras USB (emulacion de teclado) entrega al escanear
/// un DataMatrix GS1 de un envase de medicamento, segun el formato descrito en spec-012.
/// Ver README.md de este prototipo para el porque de las dos rutas de lectura.
/// </summary>
public static class LectorGs1DataMatrix
{
    private const char SeparadorGs = '\u001D';
    private const int LongitudGtin = 14;
    private const int LongitudFechaGs1 = 6;
    private const int LongitudMaximaCampoVariable = 20;
    private const int LongitudMaximaCn = 10;

    /// <summary>
    /// Intenta interpretar la cadena escaneada. Devuelve null si no se reconoce (FR-1204: no bloquea,
    /// la pantalla debe permitir introduccion manual en ese caso), nunca lanza excepcion por un formato inesperado.
    /// </summary>
    public static DatosEnvaseEscaneado? Leer(string cadenaEscaneada)
    {
        var cadena = cadenaEscaneada.TrimEnd('\r', '\n');
        if (cadena.Length == 0) return null;

        var campos = cadena.Contains(SeparadorGs)
            ? LeerConSeparadorGs(cadena)
            : LeerSinSeparadorGs(cadena);

        return ConstruirResultado(campos);
    }

    // Ruta preferente: el lector transmite el separador GS tal como exige el estandar GS1.
    // Cada segmento entre separadores puede contener varios AI de longitud fija seguidos
    // de, como mucho, un AI de longitud variable que ocupa el resto del segmento.
    private static Dictionary<string, string>? LeerConSeparadorGs(string cadena)
    {
        var campos = new Dictionary<string, string>();
        foreach (var segmento in cadena.Split(SeparadorGs, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!ConsumirSegmento(segmento, campos)) return null;
        }
        return campos;
    }

    private static bool ConsumirSegmento(string segmento, Dictionary<string, string> campos)
    {
        var resto = segmento;
        while (resto.Length > 0)
        {
            if (EsAiLongitudFija(resto, "01", LongitudGtin, campos, ref resto)) continue;
            if (EsAiLongitudFija(resto, "17", LongitudFechaGs1, campos, ref resto)) continue;
            if (EsAiVariable(resto, "10", campos, ref resto)) continue;
            if (EsAiVariable(resto, "21", campos, ref resto)) continue;
            if (EsAiVariable(resto, "712", campos, ref resto)) continue;
            return false; // AI no reconocido: el codigo no se puede interpretar.
        }
        return true;
    }

    private static bool EsAiLongitudFija(string resto, string ai, int longitudContenido,
        Dictionary<string, string> campos, ref string restoActualizado)
    {
        if (!resto.StartsWith(ai) || campos.ContainsKey(ai) || resto.Length < ai.Length + longitudContenido)
            return false;
        campos[ai] = resto.Substring(ai.Length, longitudContenido);
        restoActualizado = resto[(ai.Length + longitudContenido)..];
        return true;
    }

    // Un AI de longitud variable, dentro de un segmento ya delimitado por GS, ocupa siempre
    // el resto del segmento (es la razon de ser del propio separador).
    private static bool EsAiVariable(string resto, string ai, Dictionary<string, string> campos,
        ref string restoActualizado)
    {
        if (!resto.StartsWith(ai) || campos.ContainsKey(ai)) return false;
        campos[ai] = resto[ai.Length..];
        restoActualizado = "";
        return true;
    }

    // Ruta de respaldo: el lector no transmite (o transmite de forma no estandar) el separador GS,
    // caso habitual segun SEVeM-0108.03 "Pruebas de validacion de escaneres" (el valor GS puede no
    // visualizarse nada segun la configuracion del escaner). Sin separador, el limite entre los campos
    // de longitud variable (10, 21, 712) es ambiguo por definicion del propio estandar GS1: se prueban
    // todas las descomposiciones posibles dentro de los limites de longitud de GS1 y solo se acepta el
    // resultado si hay una unica forma de consumir toda la cadena. Ante cualquier ambiguedad (cero o
    // varias descomposiciones validas) se considera no reconocido, igual que un codigo realmente ilegible.
    //
    // Antes de juzgar la ambiguedad se descartan las descomposiciones que nunca podrian dar un resultado
    // valido por faltarles un campo obligatorio (01/10/17/21): un escaneo real (2026-09-28) demostraba
    // que la busqueda encontraba dos formas de consumir la cadena entera, pero una de ellas se comia la
    // fecha de caducidad dentro del lote y por tanto jamas habria podido construir un resultado. Sin este
    // filtro, esa descomposicion inutil bastaba para declarar el codigo ambiguo y descartar la unica
    // interpretacion realmente valida.
    private static Dictionary<string, string>? LeerSinSeparadorGs(string cadena)
    {
        var encontrados = new List<Dictionary<string, string>>();
        Buscar(cadena, new Dictionary<string, string>(), encontrados);
        var completos = encontrados.Where(EsDecomposicionCompleta).ToList();
        return completos.Count == 1 ? completos[0] : null;
    }

    private static bool EsDecomposicionCompleta(Dictionary<string, string> campos) =>
        campos.ContainsKey("01") && campos.ContainsKey("10") &&
        campos.ContainsKey("17") && campos.ContainsKey("21");

    private static void Buscar(string resto, Dictionary<string, string> campos, List<Dictionary<string, string>> encontrados)
    {
        if (resto.Length == 0)
        {
            encontrados.Add(new Dictionary<string, string>(campos));
            return;
        }
        if (resto.Length < 2) return;

        ProbarAiLongitudFija(resto, "01", LongitudGtin, campos, encontrados);
        ProbarAiLongitudFija(resto, "17", LongitudFechaGs1, campos, encontrados);
        ProbarAiVariable(resto, "10", LongitudMaximaCampoVariable, campos, encontrados);
        ProbarAiVariable(resto, "21", LongitudMaximaCampoVariable, campos, encontrados);
        ProbarAiVariable(resto, "712", LongitudMaximaCn, campos, encontrados);
    }

    private static void ProbarAiLongitudFija(string resto, string ai, int longitudContenido,
        Dictionary<string, string> campos, List<Dictionary<string, string>> encontrados)
    {
        if (!resto.StartsWith(ai) || campos.ContainsKey(ai) || resto.Length < ai.Length + longitudContenido)
            return;
        campos[ai] = resto.Substring(ai.Length, longitudContenido);
        Buscar(resto[(ai.Length + longitudContenido)..], campos, encontrados);
        campos.Remove(ai);
    }

    private static void ProbarAiVariable(string resto, string ai, int longitudMaxima,
        Dictionary<string, string> campos, List<Dictionary<string, string>> encontrados)
    {
        if (!resto.StartsWith(ai) || campos.ContainsKey(ai)) return;
        var cuerpo = resto[ai.Length..];
        var maximo = Math.Min(longitudMaxima, cuerpo.Length);
        for (var longitud = maximo; longitud >= 1; longitud--)
        {
            campos[ai] = cuerpo[..longitud];
            Buscar(cuerpo[longitud..], campos, encontrados);
            campos.Remove(ai);
        }
    }

    private static DatosEnvaseEscaneado? ConstruirResultado(Dictionary<string, string>? campos)
    {
        if (campos is null) return null;
        if (!campos.TryGetValue("01", out var gtin)) return null;
        if (!campos.TryGetValue("10", out var lote)) return null;
        if (!campos.TryGetValue("21", out var serie)) return null;
        if (!campos.TryGetValue("17", out var fecha) || fecha.Length != LongitudFechaGs1) return null;

        var anio = 2000 + int.Parse(fecha[..2]);
        var mes = int.Parse(fecha[2..4]);
        var dia = int.Parse(fecha[4..6]);
        campos.TryGetValue("712", out var cn);

        return new DatosEnvaseEscaneado(gtin, lote, new FechaCaducidadGs1(anio, mes, dia), serie, cn);
    }
}
