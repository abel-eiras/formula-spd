using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Control de cambios del PNT y control de copias, exclusivo de Administrador
/// (FR-940..FR-942).</summary>
public sealed partial class ControlDocumentalViewModel : ViewModelBase
{
    private readonly IServicioControlDocumental _servicio;
    private readonly int _administradorActualId;

    [ObservableProperty] private string _documentoCambio = string.Empty;
    [ObservableProperty] private string _version = string.Empty;
    [ObservableProperty] private string _descripcionCambio = string.Empty;
    [ObservableProperty] private ObservableCollection<ControlCambiosPNT> _cambios = [];

    [ObservableProperty] private string _documentoCopia = string.Empty;
    [ObservableProperty] private int _numCopia = 1;
    [ObservableProperty] private ObservableCollection<ControlCopias> _copias = [];

    [ObservableProperty] private string? _mensaje;

    public ControlDocumentalViewModel(IServicioControlDocumental servicio, int administradorActualId)
    {
        _servicio = servicio;
        _administradorActualId = administradorActualId;
        Recargar();
    }

    [RelayCommand]
    private void RegistrarCambio()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        _servicio.RegistrarCambioPnt(
            new DatosCambioPnt(DocumentoCambio, Version, DescripcionCambio, hoy, _administradorActualId, _administradorActualId, _administradorActualId),
            _administradorActualId);
        Mensaje = "Cambio registrado.";
        DocumentoCambio = string.Empty;
        Version = string.Empty;
        DescripcionCambio = string.Empty;
        Recargar();
    }

    [RelayCommand]
    private void RegistrarCopia()
    {
        _servicio.RegistrarCopia(
            new DatosCopia(DocumentoCopia, NumCopia, _administradorActualId, DateOnly.FromDateTime(DateTime.Today)),
            _administradorActualId);
        Mensaje = "Copia registrada.";
        DocumentoCopia = string.Empty;
        NumCopia = 1;
        Recargar();
    }

    private void Recargar()
    {
        Cambios = new ObservableCollection<ControlCambiosPNT>(_servicio.ListarCambiosPnt(_administradorActualId));
        Copias = new ObservableCollection<ControlCopias>(_servicio.ListarCopias(_administradorActualId));
    }
}
