using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Spd.Dominio;

namespace Spd.Presentacion.Ayuda;

/// <summary>Un apartado de la ayuda (Spec 014). `Seccion` es `uso` o `procedimiento`; `Id` es el
/// contrato estable que usan F1, los enlaces cruzados y "¿Por qué?" (research.md Decisión 2).</summary>
public sealed record EntradaAyuda(string Seccion, string Id, int Orden, string Titulo, string Contenido)
{
    public string SeccionTitulo => Seccion == "uso" ? "Uso de la aplicación" : "Procedimiento del servicio SPD";
}

/// <summary>Carga los apartados embebidos `Ayuda/{seccion}__{NNN}-{id}.md` (FR-1405) y ofrece
/// búsqueda por texto sin tildes ni mayúsculas (FR-1404, research.md Decisión 5).</summary>
public sealed class IndiceAyuda
{
    private static readonly Regex PatronNombre = new(@"(uso|procedimiento)__(\d{3})-([a-z0-9\-]+)\.md$", RegexOptions.Compiled);
    private static readonly Lazy<IndiceAyuda> Instancia = new(() => new IndiceAyuda(CargarEmbebidas()));

    public static IndiceAyuda Global => Instancia.Value;

    public IReadOnlyList<EntradaAyuda> Todas { get; }
    public IReadOnlyList<EntradaAyuda> Uso => Todas.Where(e => e.Seccion == "uso").ToList();
    public IReadOnlyList<EntradaAyuda> Procedimiento => Todas.Where(e => e.Seccion == "procedimiento").ToList();

    public IndiceAyuda(IEnumerable<EntradaAyuda> entradas)
        => Todas = entradas.OrderBy(e => e.Seccion == "procedimiento" ? 0 : 1).ThenBy(e => e.Orden).ToList();

    public EntradaAyuda? Obtener(string seccion, string id)
        => Todas.FirstOrDefault(e => e.Seccion == seccion && e.Id == id);

    /// <summary>Coincidencia en el título puntúa más que en el contenido; vacío devuelve todo.</summary>
    public IReadOnlyList<EntradaAyuda> Buscar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return Todas;
        var clave = Normalizador.QuitarTildesYMayusculas(texto.Trim());
        return Todas
            .Select(e => (Entrada: e, Puntos:
                (Normalizador.QuitarTildesYMayusculas(e.Titulo).Contains(clave) ? 2 : 0) +
                (Normalizador.QuitarTildesYMayusculas(e.Contenido).Contains(clave) ? 1 : 0)))
            .Where(x => x.Puntos > 0)
            .OrderByDescending(x => x.Puntos).ThenBy(x => x.Entrada.Seccion == "procedimiento" ? 0 : 1).ThenBy(x => x.Entrada.Orden)
            .Select(x => x.Entrada)
            .ToList();
    }

    /// <summary>Ids `[[seccion:id]]` referenciados en el contenido de una entrada.</summary>
    public static IEnumerable<(string Seccion, string Id)> EnlacesDe(EntradaAyuda entrada)
        => Regex.Matches(entrada.Contenido, @"\[\[(uso|procedimiento):([a-z0-9\-]+)\]\]")
            .Select(m => (m.Groups[1].Value, m.Groups[2].Value));

    private static IEnumerable<EntradaAyuda> CargarEmbebidas()
    {
        var ensamblado = typeof(IndiceAyuda).Assembly;
        foreach (var nombre in ensamblado.GetManifestResourceNames())
        {
            var m = PatronNombre.Match(nombre);
            if (!m.Success) continue;
            using var flujo = ensamblado.GetManifestResourceStream(nombre)!;
            using var lector = new StreamReader(flujo);
            yield return Parsear(m.Groups[1].Value, int.Parse(m.Groups[2].Value), m.Groups[3].Value, lector.ReadToEnd());
        }
    }

    public static EntradaAyuda Parsear(string seccion, int orden, string id, string markdown)
    {
        var lineas = markdown.Replace("\r\n", "\n").Split('\n');
        var indiceTitulo = Array.FindIndex(lineas, l => l.StartsWith("# "));
        var titulo = indiceTitulo >= 0 ? lineas[indiceTitulo][2..].Trim() : id;
        var contenido = string.Join("\n", lineas.Where((_, i) => i != indiceTitulo)).Trim();
        return new EntradaAyuda(seccion, id, orden, titulo, contenido);
    }
}
