using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Alertas de calidad (Spec 018): registrar una retirada de lote y ver a quién alcanza.
/// Cualquier usuario puede registrarla: una retirada de lote no espera a que llegue el administrador.</summary>
public sealed partial class AlertasCalidadViewModel : ViewModelBase
{
    private readonly IServicioAlertasCalidad _servicio;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<AlertaCalidadFila> _alertas = [];
    [ObservableProperty] private bool _soloActivas = true;
    [ObservableProperty] private AlertaCalidadFila? _seleccionada;
    [ObservableProperty] private TrazabilidadAlerta? _traza;

    [ObservableProperty] private string _referencia = string.Empty;
    [ObservableProperty] private DateTimeOffset? _fechaAlerta = DateTimeOffset.Now.Date;
    [ObservableProperty] private string _cn = string.Empty;
    [ObservableProperty] private string _lotes = string.Empty;
    [ObservableProperty] private string? _descripcion;
    [ObservableProperty] private string? _motivoCierre;
    [ObservableProperty] private string? _mensaje;

    public AlertasCalidadViewModel(IServicioAlertasCalidad servicio, int? usuarioActualId)
    {
        _servicio = servicio;
        _usuarioActualId = usuarioActualId;
        Cargar();
    }

    public bool HaySeleccion => Traza is not null;
    public string ResumenTraza => Traza is null
        ? string.Empty
        : Traza.RequiereAccion
            ? $"{Traza.EnCustodia.Count} caja(s) en custodia que retirar del depósito, {Traza.BlisteresPendientes.Count} blíster(es) sin entregar que rehacer y {Traza.BlisteresEntregados.Count} blíster(es) ya entregados: avisa a esos pacientes o a sus cuidadores."
            : "Este lote no está en ninguna caja en custodia ni en ningún blíster.";

    partial void OnSoloActivasChanged(bool value) => Cargar();

    partial void OnSeleccionadaChanged(AlertaCalidadFila? value)
    {
        Traza = value is null ? null : _servicio.Trazar(value.Alerta.Id);
        OnPropertyChanged(nameof(HaySeleccion));
        OnPropertyChanged(nameof(ResumenTraza));
    }

    [RelayCommand]
    private void Cargar()
    {
        var id = Seleccionada?.Alerta.Id;
        Alertas = new ObservableCollection<AlertaCalidadFila>(_servicio.Listar(SoloActivas));
        Seleccionada = Alertas.FirstOrDefault(a => a.Alerta.Id == id);
    }

    [RelayCommand]
    private void Registrar()
    {
        try
        {
            var creadas = _servicio.Registrar(
                new DatosAltaAlertaCalidad(Referencia, DateOnly.FromDateTime((FechaAlerta ?? DateTimeOffset.Now).Date), Cn, Lotes, Descripcion),
                _usuarioActualId);
            Mensaje = $"Alerta registrada para {creadas.Count} lote(s). Revisa abajo a quién alcanza.";
            Referencia = string.Empty; Cn = string.Empty; Lotes = string.Empty; Descripcion = null;
            Cargar();
            Seleccionada = Alertas.FirstOrDefault(a => a.Alerta.Id == creadas[0].Id);
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void Cerrar()
    {
        if (Seleccionada is null) return;
        try
        {
            _servicio.Cerrar(Seleccionada.Alerta.Id, MotivoCierre ?? string.Empty, DateOnly.FromDateTime(DateTime.Today), _usuarioActualId);
            Mensaje = "Alerta cerrada. Queda en el histórico con su motivo.";
            MotivoCierre = null;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }
}
