using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Pantalla "Importar tratamiento" (FR-570..577): alta masiva de tratamiento+envase por
/// copiar/pegar. El fichero CSV (research.md Decisión 7) usa el mismo servicio; esta pantalla
/// cubre el origen principal descrito en E7 (pegado desde el programa de gestión).</summary>
public sealed partial class ImportarTratamientoViewModel : ViewModelBase
{
    private readonly IServicioImportacionTratamientoEnvase _servicio;
    private readonly int _pacienteId;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private string _textoTabulado = string.Empty;
    [ObservableProperty] private bool _tieneCabecera;
    [ObservableProperty] private int _indiceCn;
    [ObservableProperty] private int _indiceSerie = 1;
    [ObservableProperty] private int _indiceLote = 2;
    [ObservableProperty] private int _indiceCaducidad = 3;

    [ObservableProperty] private string? _resumen;
    [ObservableProperty] private ObservableCollection<string> _filasConError = [];

    public ImportarTratamientoViewModel(IServicioImportacionTratamientoEnvase servicio, int pacienteId, int? usuarioActualId)
    {
        _servicio = servicio;
        _pacienteId = pacienteId;
        _usuarioActualId = usuarioActualId;
    }

    [RelayCommand]
    private void Importar()
    {
        var mapeo = new MapeoColumnasImportacion(
            IndiceCn.ToString(), IndiceSerie.ToString(), IndiceLote.ToString(), IndiceCaducidad.ToString());
        var perfil = new PerfilImportacionTratamiento
        { Nombre = "(pegado directo)", Origen = OrigenImportacionTratamiento.Portapapeles, TieneCabecera = TieneCabecera, Mapeo = mapeo };

        var resultado = _servicio.ImportarDesdePegado(_pacienteId, perfil, TextoTabulado, _usuarioActualId);

        Resumen = $"Envases dados de alta: {resultado.EnvasesCreados.Count}. " +
                  $"Tratamientos nuevos pendientes de posología: {resultado.TratamientosPendientesCreados.Count}. " +
                  $"Filas con CN no encontrado: {resultado.FilasConCnNoEncontrado.Count}. " +
                  $"Filas con serie duplicada: {resultado.FilasConSerieDuplicada.Count}.";

        FilasConError = new ObservableCollection<string>(
            resultado.FilasConCnNoEncontrado.Select(f => $"CN no encontrado: {f.Cn}")
                .Concat(resultado.FilasConSerieDuplicada.Select(e => $"Serie duplicada {e.Fila.NumSerie} (ya en paciente {e.PacienteIdExistente})")));
    }
}
