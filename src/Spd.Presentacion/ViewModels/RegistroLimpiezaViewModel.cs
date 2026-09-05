using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Registro de limpieza de un clic (FR-910/FR-911, CA-901).</summary>
public sealed partial class RegistroLimpiezaViewModel : ViewModelBase
{
    private readonly IServicioRegistrosCalidad _servicio;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private string? _observacionesNuevas;
    [ObservableProperty] private ObservableCollection<RegistroLimpieza> _resultados = [];
    [ObservableProperty] private string? _mensaje;

    public RegistroLimpiezaViewModel(IServicioRegistrosCalidad servicio, int? usuarioActualId)
    {
        _servicio = servicio;
        _usuarioActualId = usuarioActualId;
        Recargar();
    }

    [RelayCommand]
    private void RegistrarPrePreparacion() => Registrar(TipoLimpieza.PrePreparacion);

    [RelayCommand]
    private void RegistrarPostPreparacion() => Registrar(TipoLimpieza.PostPreparacion);

    [RelayCommand]
    private void RegistrarRutinaria() => Registrar(TipoLimpieza.Rutinaria);

    private void Registrar(TipoLimpieza tipo)
    {
        _servicio.RegistrarLimpieza(tipo, ObservacionesNuevas, _usuarioActualId);
        Mensaje = "Limpieza registrada.";
        ObservacionesNuevas = null;
        Recargar();
    }

    private void Recargar() => Resultados = new ObservableCollection<RegistroLimpieza>(_servicio.ListarLimpieza());
}
