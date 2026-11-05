using Spd.Dominio;
using Xunit;

namespace Spd.Dominio.Tests;

/// <summary>Spec 017: la cadena de sellos detecta modificar, borrar, reordenar o intercalar entradas.</summary>
public class CadenaAuditoriaTests
{
    private static List<EntradaAuditoria> Cadena(int n, int sinSellarAntes = 0)
    {
        var lista = new List<EntradaAuditoria>();
        for (var i = 0; i < sinSellarAntes; i++)
            lista.Add(new EntradaAuditoria(lista.Count + 1, $"2026-01-0{i + 1}", 1, "ALTA", "Paciente", i, null, null, null));
        var previo = CadenaAuditoria.Genesis;
        for (var i = 0; i < n; i++)
        {
            var fecha = $"2026-11-05T10:00:0{i}Z";
            var hash = CadenaAuditoria.CalcularHash(previo, fecha, 1, "MODIFICACION", "Paciente", i, $"detalle {i}");
            lista.Add(new EntradaAuditoria(lista.Count + 1, fecha, 1, "MODIFICACION", "Paciente", i, $"detalle {i}", previo, hash));
            previo = hash;
        }
        return lista;
    }

    [Fact]
    public void Cadena_intacta_es_integra_y_cuenta_las_anteriores_al_sellado()
    {
        var r = CadenaAuditoria.Verificar(Cadena(4, sinSellarAntes: 2));
        Assert.True(r.Integra);
        Assert.Equal(4, r.Selladas);
        Assert.Equal(2, r.SinSellar);
    }

    [Fact]
    public void Modificar_el_detalle_se_detecta()
    {
        var c = Cadena(3);
        c[1] = c[1] with { Detalle = "otro" };
        var r = CadenaAuditoria.Verificar(c);
        Assert.False(r.Integra);
        Assert.Equal(c[1].Id, r.PrimeraEntradaConProblema);
        Assert.Contains("modificado", r.Problema);
    }

    [Fact]
    public void Borrar_una_entrada_intermedia_se_detecta()
    {
        var c = Cadena(3);
        c.RemoveAt(1);
        Assert.Contains("no enlaza", CadenaAuditoria.Verificar(c).Problema);
    }

    [Fact]
    public void Reordenar_se_detecta()
    {
        var c = Cadena(3);
        (c[0], c[1]) = (c[1], c[0]);
        Assert.False(CadenaAuditoria.Verificar(c).Integra);
    }

    [Fact]
    public void Una_entrada_sin_sellar_despues_del_sellado_es_una_insercion_externa()
    {
        var c = Cadena(2);
        c.Add(new EntradaAuditoria(99, "2026-11-06", null, "ALTA", "X", null, null, null, null));
        Assert.Contains("por fuera", CadenaAuditoria.Verificar(c).Problema);
    }

    [Fact]
    public void Nulo_y_texto_vacio_dan_hashes_distintos()
    {
        Assert.NotEqual(
            CadenaAuditoria.CalcularHash(CadenaAuditoria.Genesis, "f", null, "A", "E", null, null),
            CadenaAuditoria.CalcularHash(CadenaAuditoria.Genesis, "f", null, "A", "E", null, ""));
    }
}

public class CadenaAuditoriaGenesisTests
{
    [Fact]
    public void La_primera_entrada_enlaza_con_el_genesis()
    {
        Assert.Equal(CadenaAuditoria.Genesis, CadenaAuditoria.HashPrevioPara(null));
        Assert.Equal("abc", CadenaAuditoria.HashPrevioPara("abc"));
        Assert.Equal(new string('0', 64), CadenaAuditoria.Genesis);
    }

    [Fact]
    public void Primera_entrada_sellada_que_no_enlaza_con_el_genesis_se_detecta()
    {
        var hash = CadenaAuditoria.CalcularHash("otro", "f", null, "A", "E", null, null);
        var r = CadenaAuditoria.Verificar([new EntradaAuditoria(1, "f", null, "A", "E", null, null, "otro", hash)]);
        Assert.False(r.Integra);
    }
}
