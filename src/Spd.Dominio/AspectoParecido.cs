namespace Spd.Dominio;

/// <summary>Aviso de medicamentos de aspecto muy parecido en el mismo alvéolo (Spec 006 FR-699). Si dos
/// medicamentos distintos van en la misma toma del mismo día y su descripción física coincide en forma
/// y color, quien llena o verifica no puede distinguirlos a simple vista y debe fijarse en la
/// serigrafía o el tamaño. Es una comprobación visual sobre la instantánea de cada línea (Art. IV.3),
/// no una decisión clínica; nunca bloquea.</summary>
public static class AspectoParecido
{
    private static readonly string[] NombresTomas = ["desayuno", "almuerzo", "cena", "noche"];

    public static IReadOnlyList<string> Detectar(IEnumerable<SpdLinea> lineas)
    {
        var candidatas = lineas
            .Where(l => l.EstadoLinea != EstadoLinea.Excluida)
            .Select(l => (Linea: l, Firma: Firma(l.SnapDescTexto)))
            .Where(x => x.Firma is not null)
            .ToList();

        var avisos = new List<string>();
        for (var i = 0; i < candidatas.Count; i++)
        for (var j = i + 1; j < candidatas.Count; j++)
        {
            var (a, firmaA) = candidatas[i];
            var (b, firmaB) = candidatas[j];
            if (a.MedicamentoId == b.MedicamentoId || firmaA != firmaB || !ComparteDia(a, b)) continue;
            var tomas = TomasComunes(a, b);
            if (tomas.Count == 0) continue;
            avisos.Add($"{a.SnapNombre} y {b.SnapNombre} van juntos ({string.Join(", ", tomas)}) y se parecen mucho ({firmaA}): " +
                       "distínguelos por la serigrafía o el tamaño.");
        }
        return avisos;
    }

    // Forma y color de la descripción. Sin alguno de los dos no se puede afirmar que se parezcan.
    private static string? Firma(string? descripcion)
    {
        var p = AnalizadorDescripcionFisica.Analizar(descripcion);
        return p.Forma is null || p.Color is null ? null : $"{p.Forma} {p.Color}";
    }

    private static bool ComparteDia(SpdLinea a, SpdLinea b)
    {
        static bool Activo(string m, int d) => string.IsNullOrEmpty(m) || (d < m.Length && m[d] == '1');
        return Enumerable.Range(0, 7).Any(d => Activo(a.SnapDiasSemana, d) && Activo(b.SnapDiasSemana, d));
    }

    private static List<string> TomasComunes(SpdLinea a, SpdLinea b)
    {
        static bool Toma(FraccionDosis? f) => f is { } x && x != FraccionDosis.Cero;
        var pa = new[] { a.SnapPautaD, a.SnapPautaA, a.SnapPautaC, a.SnapPautaN };
        var pb = new[] { b.SnapPautaD, b.SnapPautaA, b.SnapPautaC, b.SnapPautaN };
        return Enumerable.Range(0, 4).Where(t => Toma(pa[t]) && Toma(pb[t])).Select(t => NombresTomas[t]).ToList();
    }
}
