using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Alta rápida y listado de lecturas ambientales (FR-900/FR-901/FR-902).</summary>
public sealed partial class RegistroAmbientalViewModel : ViewModelBase
{
    private readonly IServicioRegistrosCalidad _servicio;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private double _temperatura;
    [ObservableProperty] private double _humedad;
    [ObservableProperty] private string? _observaciones;
    [ObservableProperty] private ObservableCollection<RegistroAmbiental> _resultados = [];
    [ObservableProperty] private string? _mensaje;

    public RegistroAmbientalViewModel(IServicioRegistrosCalidad servicio, int? usuarioActualId)
    {
        _servicio = servicio;
        _usuarioActualId = usuarioActualId;
        Recargar();
    }

    [RelayCommand]
    private void Registrar()
    {
        var registro = _servicio.RegistrarAmbiental(new DatosRegistroAmbiental(Temperatura, Humedad, Observaciones), _usuarioActualId);
        Mensaje = registro.FueraDeRango
            ? "Registrado. Aviso: la lectura está fuera del rango configurado."
            : "Registrado.";
        Observaciones = null;
        Recargar();
    }

    private void Recargar() => Resultados = new ObservableCollection<RegistroAmbiental>(_servicio.ListarAmbiental());
}
