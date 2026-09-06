using System.Text.RegularExpressions;
using Spd.Presentacion;
using Spd.Presentacion.Ayuda;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>Spec 014: el índice de ayuda se carga de los recursos embebidos y es coherente (ids
/// referenciados existen, toda ventana registrada tiene apartado, búsqueda sin tildes).</summary>
public sealed class IndiceAyudaTests
{
    private static readonly IndiceAyuda Indice = IndiceAyuda.Global;

    [Fact]
    public void Carga_las_dos_secciones_con_titulo_y_contenido_CA_1400()
    {
        Assert.True(Indice.Procedimiento.Count >= 13, $"Procedimiento: {Indice.Procedimiento.Count}");
        Assert.True(Indice.Uso.Count >= 13, $"Uso: {Indice.Uso.Count}");
        Assert.All(Indice.Todas, e =>
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Titulo), e.Id);
            Assert.True(e.Contenido.Length > 200, $"{e.Seccion}:{e.Id} demasiado corto");
        });
        Assert.Equal("Procedimiento del servicio SPD", Indice.Procedimiento[0].SeccionTitulo);
        Assert.Equal("Uso de la aplicación", Indice.Uso[0].SeccionTitulo);
    }

    [Fact]
    public void Todos_los_enlaces_internos_apuntan_a_apartados_existentes()
    {
        foreach (var entrada in Indice.Todas)
            foreach (var (seccion, id) in IndiceAyuda.EnlacesDe(entrada))
                Assert.True(Indice.Obtener(seccion, id) is not null, $"{entrada.Seccion}:{entrada.Id} enlaza a {seccion}:{id}, que no existe");
    }

    [Fact]
    public void Toda_ventana_registrada_tiene_apartado_de_procedimiento_y_de_uso_FR_1401_1412()
    {
        foreach (var (ventana, id) in AyudaContextual.SeccionPorVentana)
            Assert.True(Indice.Obtener("procedimiento", id) is not null, $"{ventana} → procedimiento:{id}");
        foreach (var (ventana, id) in AyudaContextual.UsoPorVentana)
            Assert.True(Indice.Obtener("uso", id) is not null, $"{ventana} → uso:{id}");
        Assert.Equal(AyudaContextual.SeccionPorVentana.Keys.OrderBy(k => k), AyudaContextual.UsoPorVentana.Keys.OrderBy(k => k));
    }

    [Fact]
    public void Cada_apartado_de_procedimiento_tiene_los_tres_bloques_FR_1411_o_es_un_indice()
    {
        var conBloques = Indice.Procedimiento.Where(e => Regex.IsMatch(e.Contenido, @"## Qué hago en la aplicación")).ToList();
        Assert.True(conBloques.Count >= 9, $"Apartados con los tres bloques: {conBloques.Count}");
        Assert.All(conBloques, e =>
        {
            Assert.Contains("## Qué exige el PNT", e.Contenido);
            Assert.Contains("## Qué pasa si se omite", e.Contenido);
        });
    }

    [Fact]
    public void Buscar_verificador_devuelve_uso_y_procedimiento_sin_tildes_CA_1404()
    {
        var resultados = Indice.Buscar("VERIFICADÓR");   // tildes y mayúsculas no importan
        Assert.Contains(resultados, e => e.Seccion == "procedimiento" && e.Id == "verificacion");
        Assert.Contains(resultados, e => e.Seccion == "uso" && e.Id == "preparacion");
        Assert.Equal("verificacion", resultados[0].Id);   // coincidencia en el título va primero
        Assert.Empty(Indice.Buscar("zzzz-no-existe"));
        Assert.Equal(Indice.Todas.Count, Indice.Buscar("").Count);
    }

    [Fact]
    public void El_checklist_de_documentacion_cubre_los_momentos_del_servicio_CA_1403()
    {
        var doc = Indice.Obtener("procedimiento", "documentacion-y-conservacion")!;
        foreach (var momento in new[] { "## En el alta", "## Antes de preparar", "## Para entregar", "## Conservación y plazos" })
            Assert.Contains(momento, doc.Contenido);
        Assert.Contains("consentimiento vigente", doc.Contenido);
        Assert.Contains("idoneidad APTO", doc.Contenido);
    }

    [Fact]
    public void Parsear_extrae_titulo_y_contenido()
    {
        var e = IndiceAyuda.Parsear("uso", 5, "x", "# Título\n\nCuerpo **a** [[uso:inicio]]");
        Assert.Equal("Título", e.Titulo);
        Assert.StartsWith("Cuerpo", e.Contenido);
        Assert.Single(IndiceAyuda.EnlacesDe(e));
    }
}
