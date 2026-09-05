namespace Spd.Aplicacion;

/// <summary>Una fila cruda de la importación, ya mapeada a los cuatro campos mínimos de FR-571,
/// antes de resolver el medicamento/paciente/envase.</summary>
public sealed record FilaImportacionCruda(string? Cn, string? NumSerie, string? Lote, string? Caducidad);

/// <summary>Parsea texto tabulado (pegado, FR-570) o CSV (fichero, research.md Decisión 7) en
/// filas mapeadas a CN/serie/lote/caducidad según <c>PerfilImportacionTratamiento.Mapeo</c>, cuyos
/// campos contienen el índice de columna (base 0) elegido en la vista previa de mapeo (FR-571).</summary>
public static class ParserLineasImportacion
{
    public static IReadOnlyList<FilaImportacionCruda> ParsearTexto(string textoTabulado, Dominio.MapeoColumnasImportacion mapeo, bool tieneCabecera = false)
        => ParsearFilas(DividirEnFilas(textoTabulado, "\t"), mapeo, tieneCabecera);

    public static IReadOnlyList<FilaImportacionCruda> ParsearCsv(string contenidoCsv, Dominio.MapeoColumnasImportacion mapeo, string separador, bool tieneCabecera)
        => ParsearFilas(DividirEnFilas(contenidoCsv, separador), mapeo, tieneCabecera);

    private static IReadOnlyList<string[]> DividirEnFilas(string texto, string separador)
        => texto.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(linea => linea.Split(separador))
            .ToList();

    private static IReadOnlyList<FilaImportacionCruda> ParsearFilas(IReadOnlyList<string[]> filas, Dominio.MapeoColumnasImportacion mapeo, bool tieneCabecera)
    {
        var indiceCn = int.Parse(mapeo.Cn);
        var indiceSerie = int.Parse(mapeo.NumSerie);
        var indiceLote = int.Parse(mapeo.Lote);
        var indiceCaducidad = int.Parse(mapeo.Caducidad);

        return (tieneCabecera ? filas.Skip(1) : filas)
            .Select(f => new FilaImportacionCruda(
                ValorEn(f, indiceCn), ValorEn(f, indiceSerie), ValorEn(f, indiceLote), ValorEn(f, indiceCaducidad)))
            .ToList();
    }

    private static string? ValorEn(string[] fila, int indice)
        => indice >= 0 && indice < fila.Length && !string.IsNullOrWhiteSpace(fila[indice]) ? fila[indice].Trim() : null;
}
