using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Configuración / Nomenclátor (FR-051). La descarga nunca es automática (Art. VI.3).</summary>
public sealed partial class NomenclatorViewModel : ViewModelBase
{
    private readonly IServicioConfiguracionFarmacia _servicioFarmacia;
    private readonly IServicioNomenclator _servicioNomenclator;
    private readonly int? _administradorActualId;

    [ObservableProperty] private string? _urlNomenclator;
    [ObservableProperty] private string? _mensaje;
    [ObservableProperty] private bool _descargando;

    public NomenclatorViewModel(
        IServicioConfiguracionFarmacia servicioFarmacia, IServicioNomenclator servicioNomenclator, int? administradorActualId)
    {
        _servicioFarmacia = servicioFarmacia;
        _servicioNomenclator = servicioNomenclator;
        _administradorActualId = administradorActualId;
        _urlNomenclator = servicioFarmacia.ObtenerConfiguracion().UrlNomenclator;
    }

    [RelayCommand]
    private void GuardarUrl()
    {
        var farmacia = _servicioFarmacia.ObtenerConfiguracion();
        farmacia.UrlNomenclator = UrlNomenclator;
        _servicioFarmacia.ActualizarDatosFarmacia(farmacia, _administradorActualId);
        Mensaje = "URL guardada.";
    }

    [RelayCommand]
    private async Task DescargarAsync()
    {
        if (string.IsNullOrWhiteSpace(UrlNomenclator))
        {
            Mensaje = "No hay URL configurada.";
            return;
        }

        Descargando = true;
        try
        {
            var rutaDestino = Path.Combine(AppContext.BaseDirectory, "nomenclator", "nomenclator.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(rutaDestino)!);
            var resultado = await _servicioNomenclator.DescargarNomenclatorAsync(
                UrlNomenclator, rutaDestino, _administradorActualId);
            // FechaUtc se guarda en UTC (correcto para la auditoría); para mostrarla al usuario se
            // convierte a la hora local del sistema, que ya tiene en cuenta el horario de verano.
            Mensaje = resultado.Exito
                ? $"Descargado correctamente el {resultado.FechaUtc.ToLocalTime():g}."
                : $"No se pudo descargar: {resultado.Motivo}";
        }
        finally
        {
            Descargando = false;
        }
    }
}
