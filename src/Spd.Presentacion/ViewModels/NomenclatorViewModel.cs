using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Configuración / Nomenclátor (FR-051). La descarga nunca es automática (Art. VI.3). Tras descargar,
/// los medicamentos nuevos se dan de alta en el catálogo (Spec 003 FR-320, revisado el 2026-09-14).</summary>
public sealed partial class NomenclatorViewModel : ViewModelBase
{
    private readonly IServicioConfiguracionFarmacia _servicioFarmacia;
    private readonly IServicioNomenclator _servicioNomenclator;
    private readonly int? _administradorActualId;
    private readonly IServicioImportacionNomenclator? _servicioImportacion;

    /// <summary>Dónde se guarda el fichero descargado. Configurable para las pruebas.</summary>
    public string RutaFichero { get; init; } = Path.Combine(AppContext.BaseDirectory, "nomenclator", "nomenclator.csv");

    [ObservableProperty] private string? _urlNomenclator;
    [ObservableProperty] private string? _mensaje;
    [ObservableProperty] private bool _descargando;
    [ObservableProperty] private bool _importando;

    public NomenclatorViewModel(
        IServicioConfiguracionFarmacia servicioFarmacia, IServicioNomenclator servicioNomenclator, int? administradorActualId,
        IServicioImportacionNomenclator? servicioImportacion = null)
    {
        _servicioImportacion = servicioImportacion;
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
            var rutaDestino = RutaFichero;
            Directory.CreateDirectory(Path.GetDirectoryName(rutaDestino)!);
            var resultado = await _servicioNomenclator.DescargarNomenclatorAsync(
                UrlNomenclator, rutaDestino, _administradorActualId);
            // FechaUtc se guarda en UTC (correcto para la auditoría); para mostrarla al usuario se
            // convierte a la hora local del sistema, que ya tiene en cuenta el horario de verano.
            if (!resultado.Exito)
            {
                Mensaje = $"No se pudo descargar: {resultado.Motivo}";
                return;
            }

            Mensaje = $"Descargado correctamente el {resultado.FechaUtc.ToLocalTime():g}. " + Importar();
        }
        finally
        {
            Descargando = false;
        }
    }

    /// <summary>Para volver a cargar el último fichero sin descargarlo otra vez.</summary>
    [RelayCommand]
    private void ImportarDescargado()
    {
        if (!File.Exists(RutaFichero))
        {
            Mensaje = "Todavía no hay ningún nomenclátor descargado.";
            return;
        }

        Mensaje = Importar();
    }

    /// <summary>En el hilo de la interfaz a propósito: la conexión a la base no admite uso concurrente, y el
    /// alta de todo el nomenclátor va en una sola transacción de pocos segundos.</summary>
    private string Importar()
    {
        if (_servicioImportacion is null) return string.Empty;

        Importando = true;
        try
        {
            var r = _servicioImportacion.ImportarCompleto(RutaFichero, _administradorActualId);
            return r.Exito
                ? $"Catálogo actualizado. Medicamentos nuevos: {r.AltasActivas}; de baja (quedan inactivos): {r.AltasDeBaja}; " +
                  $"ya estaban y no se han tocado: {r.YaExistian}. Los nuevos tienen la aptitud para SPD sin confirmar: " +
                  "se confirma al preparar. Los cambios de nombre se revisan en «Revisar nomenclátor»."
                : $"No se pudo importar: {r.Error}";
        }
        finally
        {
            Importando = false;
        }
    }
}
