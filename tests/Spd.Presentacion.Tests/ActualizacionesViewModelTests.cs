using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Spd.Aplicacion;
using Spd.Presentacion.ViewModels;
using Spd.Presentacion.Views.Configuracion;

namespace Spd.Presentacion.Tests;

/// <summary>FR-053 en la pantalla: el botón de instalar solo aparece tras encontrar una versión nueva, no
/// instala sin confirmación, y al terminar bien pide el reinicio con el ejecutable nuevo.</summary>
public sealed class ActualizacionesViewModelTests
{
    private sealed class ComprobacionFalsa(bool hayNueva) : IServicioActualizaciones
    {
        public Task<ResultadoComprobacionActualizacion> ComprobarActualizacionesAsync(string versionInstalada, int? administradorQueEjecutaId)
            => Task.FromResult(new ResultadoComprobacionActualizacion(
                true, hayNueva, hayNueva ? "v9.0.0" : null, "https://github.com/abel-eiras/formula-spd/releases/tag/v9.0.0", null, DateTime.UtcNow));
    }

    private sealed class InstalacionFalsa(ResultadoInstalacionActualizacion resultado) : IServicioInstalacionActualizaciones
    {
        public List<string> Instaladas { get; } = [];

        public Task<ResultadoInstalacionActualizacion> InstalarAsync(
            string etiqueta, int? administradorQueEjecutaId, IProgress<int>? progreso, CancellationToken cancelacion)
        {
            Instaladas.Add(etiqueta);
            return Task.FromResult(resultado);
        }
    }

    [Fact]
    public async Task Sin_version_nueva_no_se_ofrece_instalar()
    {
        var vm = new ActualizacionesViewModel(
            new ComprobacionFalsa(hayNueva: false), new InstalacionFalsa(ResultadoInstalacionActualizacion.Fallida("x")), 1, _ => { });

        await vm.ComprobarCommand.ExecuteAsync(null);

        Assert.False(vm.PuedeInstalar);
    }

    [Fact]
    public async Task Con_version_nueva_se_confirma_se_instala_y_se_pide_el_reinicio()
    {
        var instalacion = new InstalacionFalsa(ResultadoInstalacionActualizacion.Exitosa("v9.0.0", "/opt/spd/FormulaSPD"));
        var reinicios = new List<string>();
        var vm = new ActualizacionesViewModel(new ComprobacionFalsa(hayNueva: true), instalacion, 1, reinicios.Add);

        await vm.ComprobarCommand.ExecuteAsync(null);
        Assert.True(vm.PuedeInstalar);
        Assert.Equal("Instalar la versión v9.0.0", vm.TextoBotonInstalar);

        vm.PedirConfirmacionCommand.Execute(null);
        Assert.True(vm.ConfirmandoInstalacion);
        Assert.Empty(instalacion.Instaladas);
        Assert.Contains("copia de seguridad", vm.TextoConfirmacion);

        await vm.InstalarCommand.ExecuteAsync(null);

        Assert.Equal(["v9.0.0"], instalacion.Instaladas);
        Assert.Equal(["/opt/spd/FormulaSPD"], reinicios);
    }

    [Fact]
    public async Task Una_release_sin_firma_explica_el_motivo_y_da_el_enlace_de_descarga()
    {
        var reinicios = new List<string>();
        var vm = new ActualizacionesViewModel(
            new ComprobacionFalsa(hayNueva: true),
            new InstalacionFalsa(ResultadoInstalacionActualizacion.SoloADescargarAMano("Se publicó sin firma.")), 1, reinicios.Add);

        await vm.ComprobarCommand.ExecuteAsync(null);
        await vm.InstalarCommand.ExecuteAsync(null);

        Assert.Empty(reinicios);
        Assert.Contains("sin firma", vm.Mensaje);
        Assert.Contains("https://github.com/abel-eiras/formula-spd/releases/tag/v9.0.0", vm.Mensaje);
        Assert.False(vm.Instalando);
    }

    [AvaloniaFact]
    public void La_vista_se_construye_con_su_viewmodel()
    {
        var vm = new ActualizacionesViewModel(
            new ComprobacionFalsa(hayNueva: true), new InstalacionFalsa(ResultadoInstalacionActualizacion.Fallida("x")), 1, _ => { });

        var ventana = AnfitrionDeVista.Anfitrion(new ActualizacionesView { DataContext = vm });

        Assert.NotNull(ventana);
    }
}
