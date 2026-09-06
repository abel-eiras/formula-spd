using System;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;

namespace Spd.Presentacion.Ayuda;

/// <summary>Render mínimo del Markdown de la ayuda (research.md Decisión 1): `#`/`##`/`###`,
/// párrafos, listas `-` y `1.`, `**negrita**`, y enlaces internos `[[seccion:id]]` que se pintan
/// como botones de navegación.</summary>
public static class RenderizadorMarkdown
{
    private static readonly Regex Negrita = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
    private static readonly Regex Enlace = new(@"\[\[(uso|procedimiento):([a-z0-9\-]+)\]\]", RegexOptions.Compiled);

    public static Control Renderizar(string markdown, IndiceAyuda indice, Action<string, string> navegar)
    {
        var panel = new StackPanel { Spacing = 6, Margin = new Thickness(4) };
        var parrafo = new System.Text.StringBuilder();

        void CerrarParrafo()
        {
            if (parrafo.Length == 0) return;
            panel.Children.Add(Linea(parrafo.ToString().Trim(), indice, navegar, 13, FontWeight.Normal, 0));
            parrafo.Clear();
        }

        foreach (var cruda in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var linea = cruda.TrimEnd();
            if (linea.Length == 0) { CerrarParrafo(); continue; }
            if (linea.StartsWith("### ")) { CerrarParrafo(); panel.Children.Add(Linea(linea[4..], indice, navegar, 14, FontWeight.Bold, 6)); continue; }
            if (linea.StartsWith("## ")) { CerrarParrafo(); panel.Children.Add(Linea(linea[3..], indice, navegar, 16, FontWeight.Bold, 10)); continue; }
            if (linea.StartsWith("# ")) { CerrarParrafo(); panel.Children.Add(Linea(linea[2..], indice, navegar, 19, FontWeight.Bold, 10)); continue; }
            if (linea.StartsWith("- ") || linea.StartsWith("* ") || Regex.IsMatch(linea, @"^\d+\. "))
            {
                CerrarParrafo();
                var vineta = Regex.IsMatch(linea, @"^\d+\. ") ? linea[..linea.IndexOf(' ')] : "•";
                var texto = linea[(linea.IndexOf(' ') + 1)..];
                var fila = new DockPanel { Margin = new Thickness(12, 0, 0, 0) };
                var marca = new TextBlock { Text = vineta, Width = 18, FontSize = 13 };
                DockPanel.SetDock(marca, Dock.Left);
                fila.Children.Add(marca);
                fila.Children.Add(Linea(texto, indice, navegar, 13, FontWeight.Normal, 0));
                panel.Children.Add(fila);
                continue;
            }
            parrafo.Append(linea).Append(' ');
        }
        CerrarParrafo();
        return panel;
    }

    private static Control Linea(string texto, IndiceAyuda indice, Action<string, string> navegar, double tamano, FontWeight peso, double margenSuperior)
    {
        // Los enlaces internos se separan en botones; el texto restante lleva negritas en línea.
        var contenedor = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, margenSuperior, 0, 0) };
        var ultimo = 0;
        foreach (Match m in Enlace.Matches(texto))
        {
            if (m.Index > ultimo) contenedor.Children.Add(BloqueTexto(texto[ultimo..m.Index], tamano, peso));
            var seccion = m.Groups[1].Value;
            var id = m.Groups[2].Value;
            var destino = indice.Obtener(seccion, id);
            var boton = new Button
            {
                Content = "→ " + (destino?.Titulo ?? id),
                FontSize = tamano - 1,
                Padding = new Thickness(6, 1),
                Margin = new Thickness(2, 0)
            };
            boton.Click += (_, _) => navegar(seccion, id);
            contenedor.Children.Add(boton);
            ultimo = m.Index + m.Length;
        }
        if (ultimo < texto.Length) contenedor.Children.Add(BloqueTexto(texto[ultimo..], tamano, peso));
        return contenedor;
    }

    private static TextBlock BloqueTexto(string texto, double tamano, FontWeight peso)
    {
        var bloque = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = tamano, FontWeight = peso, MaxWidth = 720 };
        var ultimo = 0;
        foreach (Match m in Negrita.Matches(texto))
        {
            if (m.Index > ultimo) bloque.Inlines!.Add(new Run(texto[ultimo..m.Index]));
            var negrita = new Bold();
            negrita.Inlines.Add(new Run(m.Groups[1].Value));
            bloque.Inlines!.Add(negrita);
            ultimo = m.Index + m.Length;
        }
        if (ultimo < texto.Length) bloque.Inlines!.Add(new Run(texto[ultimo..]));
        return bloque;
    }
}
