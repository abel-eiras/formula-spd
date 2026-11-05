namespace Spd.Dominio;

/// <summary>Resultado de interpretar un escaneo (spec-012 FR-1201/FR-1204/FR-1210).</summary>
public enum EstadoLecturaGs1
{
    /// <summary>Una única interpretación válida: se aplica.</summary>
    Reconocido,
    /// <summary>Varias interpretaciones válidas (el lector se comió el separador GS entre dos campos
    /// variables): no se adivina, se enseñan las candidatas y la persona elige mirando la caja.</summary>
    Ambiguo,
    /// <summary>Ninguna interpretación válida: introducción manual, sin bloquear nada.</summary>
    NoReconocido
}

/// <summary>Lo que se ha entendido de un escaneo. <see cref="Datos"/> solo existe si es
/// <see cref="EstadoLecturaGs1.Reconocido"/>; <see cref="Candidatas"/> solo si es ambiguo.</summary>
public sealed record LecturaGs1(EstadoLecturaGs1 Estado, DatosEnvaseEscaneado? Datos, IReadOnlyList<DatosEnvaseEscaneado> Candidatas)
{
    public static readonly LecturaGs1 NoReconocida = new(EstadoLecturaGs1.NoReconocido, null, []);

    /// <summary>Identidad del medicamento cuando todas las interpretaciones coinciden en GTIN y CN, que
    /// es lo normal en una lectura ambigua (los campos de longitud fija no se confunden): sirve para
    /// identificar el medicamento aunque el lote todavía esté por confirmar. Null si no hay lectura o si
    /// las candidatas no coinciden.</summary>
    public DatosEnvaseEscaneado? IdentidadComun => Estado switch
    {
        EstadoLecturaGs1.Reconocido => Datos,
        EstadoLecturaGs1.Ambiguo when Candidatas.Select(c => (c.Gtin, c.CodigoNacional)).Distinct().Count() == 1 => Candidatas[0],
        _ => null
    };
}

/// <summary>
/// Interpreta la cadena que un lector de codigo de barras USB (emulacion de teclado) entrega al escanear
/// un DataMatrix GS1 de un envase de medicamento (spec-012 FR-1200/FR-1201). Nacio como prototipo aislado
/// en prototipos/lector-datamatrix, validado contra codigos reales y los patrones oficiales de
/// SEVeM-0108.03 "Pruebas de validacion de escaneres" antes de integrarse aqui.
///
/// Ante la duda NO adivina (FR-1210): si sin separador GS hay varias formas válidas de partir la cadena,
/// devuelve todas para que la persona confirme el lote mirando la caja.
/// </summary>
public static class LectorGs1DataMatrix
{
    private const char SeparadorGs = '\u001D';
    private const int LongitudGtin = 14;
    private const int LongitudFechaGs1 = 6;
    private const int LongitudMaximaCampoVariable = 20;
    private const int LongitudMaximaCn = 10;

    // Identificadores de simbología que algunos lectores anteponen (ISO/IEC 15424): ]d2 DataMatrix GS1,
    // ]C1 GS1-128, ]Q3 QR GS1. No forman parte del contenido.
    private static readonly string[] PrefijosSimbologia = ["]d2", "]C1", "]Q3"];

    /// <summary>Atajo para quien solo quiere una lectura inequívoca: los datos si es
    /// <see cref="EstadoLecturaGs1.Reconocido"/>, null en cualquier otro caso. Nunca lanza.</summary>
    public static DatosEnvaseEscaneado? Leer(string cadenaEscaneada, string? separadorSustituto = null)
        => Interpretar(cadenaEscaneada, separadorSustituto).Datos;

    /// <summary>
    /// Interpreta la cadena escaneada. Nunca lanza excepción por un formato inesperado (FR-1204).
    /// <paramref name="separadorSustituto"/> es el carácter que un lector en modo teclado manda en lugar
    /// del GS, configurado en «Lector de códigos» (FR-1211): se trata exactamente como un GS.
    /// </summary>
    public static LecturaGs1 Interpretar(string? cadenaEscaneada, string? separadorSustituto = null)
    {
        var cadena = Normalizar(cadenaEscaneada ?? string.Empty, separadorSustituto);
        if (cadena.Length == 0) return LecturaGs1.NoReconocida;

        var interpretaciones = cadena.Contains(SeparadorGs)
            ? LeerConSeparadorGs(cadena)
            : LeerSinSeparadorGs(cadena);

        var validas = interpretaciones
            .Select(ConstruirResultado)
            .OfType<DatosEnvaseEscaneado>()
            .Distinct()
            .ToList();

        return validas.Count switch
        {
            0 => LecturaGs1.NoReconocida,
            1 => new LecturaGs1(EstadoLecturaGs1.Reconocido, validas[0], []),
            _ => new LecturaGs1(EstadoLecturaGs1.Ambiguo, null, validas)
        };
    }

    private static string Normalizar(string cadena, string? separadorSustituto)
    {
        var resultado = cadena.TrimEnd('\r', '\n');
        foreach (var prefijo in PrefijosSimbologia)
        {
            if (resultado.StartsWith(prefijo, StringComparison.Ordinal)) { resultado = resultado[prefijo.Length..]; break; }
        }
        if (!string.IsNullOrEmpty(separadorSustituto))
            resultado = resultado.Replace(separadorSustituto, SeparadorGs.ToString(), StringComparison.Ordinal);
        // Un GS al principio es el FNC1 inicial que algunos lectores transmiten: no separa nada.
        return resultado.TrimStart(SeparadorGs);
    }

    // Ruta preferente: el lector transmite el separador GS tal como exige el estandar GS1.
    // Cada segmento entre separadores puede contener varios AI de longitud fija seguidos
    // de, como mucho, un AI de longitud variable que ocupa el resto del segmento.
    private static List<Dictionary<string, string>> LeerConSeparadorGs(string cadena)
    {
        var campos = new Dictionary<string, string>();
        foreach (var segmento in cadena.Split(SeparadorGs, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!ConsumirSegmento(segmento, campos)) return [];
        }
        return [campos];
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
    // caso habitual segun SEVeM-0108.03 "Pruebas de validacion de escaneres". Sin separador, el limite
    // entre los campos de longitud variable (10, 21, 712) es ambiguo por definicion del propio estandar
    // GS1: se prueban todas las descomposiciones posibles dentro de los limites de longitud de GS1.
    // Las que no pueden dar un resultado (falta un campo obligatorio, fecha imposible, GTIN con el digito
    // de control mal) se descartan en ConstruirResultado; si sobrevive mas de una, la lectura es ambigua
    // y se ofrecen todas (FR-1210) en vez de descartar el codigo.
    private static List<Dictionary<string, string>> LeerSinSeparadorGs(string cadena)
    {
        var encontrados = new List<Dictionary<string, string>>();
        Buscar(cadena, new Dictionary<string, string>(), encontrados);
        return encontrados;
    }

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

    private static DatosEnvaseEscaneado? ConstruirResultado(Dictionary<string, string> campos)
    {
        if (!campos.TryGetValue("01", out var gtin) || !Gs1.DigitoControlCorrecto(gtin)) return null;
        if (!campos.TryGetValue("10", out var lote)) return null;
        if (!campos.TryGetValue("21", out var serie)) return null;
        if (!campos.TryGetValue("17", out var fecha) || fecha.Length != LongitudFechaGs1 || !fecha.All(char.IsAsciiDigit))
            return null;

        var caducidad = new FechaCaducidadGs1(2000 + int.Parse(fecha[..2]), int.Parse(fecha[2..4]), int.Parse(fecha[4..6]));
        if (!caducidad.EsValida) return null;
        campos.TryGetValue("712", out var cn);

        return new DatosEnvaseEscaneado(gtin, lote, caducidad, serie, cn);
    }
}
