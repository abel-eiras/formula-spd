using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Alta y listado de recogidas de residuos no SIGRE (FR-930).</summary>
public sealed partial class RecogidaResiduosViewModel : ViewModelBase
{
    private readonly IServicioRegistrosCalidad _servicio;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private string _empresaGestora = string.Empty;
    [ObservableProperty] private string? _observaciones;
    [ObservableProperty] private ObservableCollection<RecogidaResiduos> _resultados = [];
    [ObservableProperty] private string? _mensaje;

    public RecogidaResiduosViewModel(IServicioRegistrosCalidad servicio, int? usuarioActualId)
    {
        _servicio = servicio;
        _usuarioActualId = usuarioActualId;
        Recargar();
    }

    [RelayCommand]
    private void Registrar()
    {
        _servicio.RegistrarRecogidaResiduos(
            new DatosRecogidaResiduos(DateOnly.FromDateTime(DateTime.Today), EmpresaGestora, Observaciones), _usuarioActualId);
        Mensaje = "Recogida registrada.";
        EmpresaGestora = string.Empty;
        Observaciones = null;
        Recargar();
    }

    private void Recargar() => Resultados = new ObservableCollection<RecogidaResiduos>(_servicio.ListarRecogidaResiduos());
}
