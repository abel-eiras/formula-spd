using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Pantalla de inicio (Spec 006 FR-691, Spec 009 FR-950): lo que hay pendiente hoy. En la
/// fase 2 del rediseño gana los indicadores, los avisos accionables y las acciones rápidas.</summary>
public sealed partial class InicioViewModel(IServicioAvisosInicio servicioAvisos) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<AvisoInicio> _avisos =
        new(servicioAvisos.Obtener(DateOnly.FromDateTime(DateTime.Today)));

    public bool HayAvisos => Avisos.Count > 0;

    [RelayCommand]
    private void Recargar()
    {
        Avisos = new ObservableCollection<AvisoInicio>(servicioAvisos.Obtener(DateOnly.FromDateTime(DateTime.Today)));
        OnPropertyChanged(nameof(HayAvisos));
    }
}
