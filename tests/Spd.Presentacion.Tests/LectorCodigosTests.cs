using Spd.Dominio;
using Spd.Presentacion.ViewModels;
using Xunit;

namespace Spd.Presentacion.Tests;

/// <summary>spec-012 FR-1210..1212: elegir entre lecturas ambiguas y prueba del lector.</summary>
public sealed class LectorCodigosTests
{
    [Fact]
    public void Lectura_ambigua_enseña_candidatas_y_aplica_solo_la_elegida()
    {
        var lectura = LectorGs1DataMatrix.Interpretar("01084700069506471728073110AB21CD21EF");
        var vm = new LecturaAmbiguaViewModel();
        DatosEnvaseEscaneado? aplicada = null;

        vm.Mostrar(lectura, d => aplicada = d);

        Assert.True(vm.Visible);
        Assert.Equal(2, vm.Candidatas.Count);
        Assert.Null(aplicada);
        var elegida = Assert.Single(vm.Candidatas, c => c.Datos.Lote == "AB");
        vm.ElegirCommand.Execute(elegida);
        Assert.Equal("CD21EF", aplicada!.NumeroSerie);
        Assert.False(vm.Visible);
    }

    [Fact]
    public void Descartar_no_aplica_nada()
    {
        var vm = new LecturaAmbiguaViewModel();
        var aplicada = false;
        vm.Mostrar(LectorGs1DataMatrix.Interpretar("01084700069506471728073110AB21CD21EF"), _ => aplicada = true);
        vm.DescartarCommand.Execute(null);
        Assert.False(aplicada);
        Assert.False(vm.Visible);
    }

    [Fact]
    public void Prueba_del_lector_hace_visibles_los_caracteres_de_control()
    {
        Assert.Equal("10AB⟨GS⟩21CD⟨CR⟩", LectorCodigosViewModel.HacerVisible("10AB\u001D21CD\r"));
    }

    [Fact]
    public void Diagnostico_distingue_GS_sustituto_y_ausencia()
    {
        Assert.Contains("llega bien", LectorCodigosViewModel.Diagnosticar("01\u001D21", null));
        Assert.Contains("sustituto «~»", LectorCodigosViewModel.Diagnosticar("01~21", "~"));
        Assert.Contains("sin separador GS", LectorCodigosViewModel.Diagnosticar("0121", null));
    }

    [Fact]
    public void Descripcion_de_la_lectura_dice_que_el_CN_sale_del_GTIN()
    {
        var texto = LectorCodigosViewModel.Describir(
            LectorGs1DataMatrix.Interpretar("010847000695064721172342612146291728073110RA0462"));
        Assert.Contains("CN 695064 (deducido del GTIN)", texto);
        Assert.Contains("Lote RA0462", texto);
    }
}
