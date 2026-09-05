using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Alta y listado de formaciones del usuario actual (FR-920/FR-921). Sin distinción de
/// categoría profesional (Art. VII.4).</summary>
public sealed partial class FormacionPersonalViewModel : ViewModelBase
{
    private readonly IServicioRegistrosCalidad _servicio;
    private readonly int _usuarioActualId;

    [ObservableProperty] private string _nombreCurso = string.Empty;
    [ObservableProperty] private string? _entidadOrganizadora;
    [ObservableProperty] private DateTimeOffset _fecha = DateTimeOffset.Now;
    [ObservableProperty] private bool _acreditado;
    [ObservableProperty] private ObservableCollection<FormacionPersonal> _resultados = [];
    [ObservableProperty] private string? _mensaje;

    public FormacionPersonalViewModel(IServicioRegistrosCalidad servicio, int usuarioActualId)
    {
        _servicio = servicio;
        _usuarioActualId = usuarioActualId;
        Recargar();
    }

    [RelayCommand]
    private void Registrar()
    {
        _servicio.RegistrarFormacion(
            new DatosFormacion(_usuarioActualId, NombreCurso, EntidadOrganizadora, DateOnly.FromDateTime(Fecha.Date), Acreditado),
            _usuarioActualId);
        Mensaje = "Formación registrada.";
        NombreCurso = string.Empty;
        EntidadOrganizadora = null;
        Acreditado = false;
        Recargar();
    }

    private void Recargar() => Resultados = new ObservableCollection<FormacionPersonal>(_servicio.ListarFormacion(_usuarioActualId));
}
