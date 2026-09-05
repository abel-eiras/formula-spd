using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Views.Pacientes;

namespace Spd.Presentacion.ViewModels;

/// <summary>Pestaña Depósito de la ficha de paciente (FR-510..517, FR-540..544): alta de envase,
/// custodia/histórico, salida a SIGRE y entrega fuera de blíster.</summary>
public sealed partial class DepositoViewModel : ViewModelBase
{
    private readonly IServicioEnvases _servicioEnvases;
    private readonly IServicioMedicamentos _servicioMedicamentos;
    private readonly IServicioImportacionTratamientoEnvase _servicioImportacion;
    private readonly int _pacienteId;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<EnvaseFila> _enCustodia = [];
    [ObservableProperty] private ObservableCollection<EnvaseFila> _historico = [];
    [ObservableProperty] private bool _mostrarHistorico;
    [ObservableProperty] private string? _mensaje;

    [ObservableProperty] private string _cn = string.Empty;
    [ObservableProperty] private string _serie = string.Empty;
    [ObservableProperty] private string? _lote;
    [ObservableProperty] private DateTimeOffset? _caducidad;
    [ObservableProperty] private int? _unidadesIniciales;

    [ObservableProperty] private string? _entregadoA;

    [ObservableProperty] private EnvaseFila? _envaseSeleccionadoParaSalida;
    [ObservableProperty] private MotivoSalidaEnvase _motivoSalidaSeleccionado = MotivoSalidaEnvase.Otro;

    public MotivoSalidaEnvase[] MotivosSalida { get; } = Enum.GetValues<MotivoSalidaEnvase>();

    public DepositoViewModel(
        IServicioEnvases servicioEnvases, IServicioMedicamentos servicioMedicamentos,
        IServicioImportacionTratamientoEnvase servicioImportacion, int pacienteId, int? usuarioActualId)
    {
        _servicioEnvases = servicioEnvases;
        _servicioMedicamentos = servicioMedicamentos;
        _servicioImportacion = servicioImportacion;
        _pacienteId = pacienteId;
        _usuarioActualId = usuarioActualId;
        Cargar();
    }

    [RelayCommand]
    private void AbrirImportarTratamiento()
        => new ImportarTratamientoWindow(_servicioImportacion, _pacienteId, _usuarioActualId).Show();

    private void Cargar()
    {
        EnCustodia = new ObservableCollection<EnvaseFila>(_servicioEnvases.ListarEnCustodiaDePaciente(_pacienteId).Select(AFila));
        Historico = new ObservableCollection<EnvaseFila>(_servicioEnvases.ListarHistoricoDePaciente(_pacienteId).Select(AFila));
    }

    private EnvaseFila AFila(Envase e) => new(
        e.Id, _servicioMedicamentos.ObtenerPorId(e.MedicamentoId)?.Nombre ?? "(medicamento no encontrado)", e);

    [RelayCommand]
    private void RegistrarEnvase()
    {
        var medicamento = _servicioMedicamentos.ObtenerPorCn(Cn);
        if (medicamento is null)
        {
            Mensaje = $"No se encuentra ningún medicamento activo con CN {Cn}.";
            return;
        }
        if (Caducidad is null || UnidadesIniciales is null)
        {
            Mensaje = "Caducidad y unidades iniciales son obligatorias (FR-512).";
            return;
        }

        try
        {
            var envase = _servicioEnvases.RegistrarEnvase(
                new DatosAltaEnvase(_pacienteId, medicamento.Id, Serie, Lote ?? string.Empty,
                    DateOnly.FromDateTime(Caducidad.Value.Date), UnidadesIniciales.Value, OrigenEnvase.Manual),
                _usuarioActualId);

            Mensaje = envase.Caducidad < DateOnly.FromDateTime(DateTime.Today)
                ? "Envase registrado. Aviso: la caducidad ya ha pasado (FR-514); nunca se propondrá para una preparación."
                : "Envase registrado.";
            Cn = string.Empty; Serie = string.Empty; Lote = null; Caducidad = null; UnidadesIniciales = null;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void PrepararSalidaSigre(EnvaseFila fila) => EnvaseSeleccionadoParaSalida = fila;

    [RelayCommand]
    private void ConfirmarSalidaSigre()
    {
        if (EnvaseSeleccionadoParaSalida is null) return;
        try
        {
            _servicioEnvases.DarSalidaSigre(EnvaseSeleccionadoParaSalida.Id, MotivoSalidaSeleccionado, null, _usuarioActualId);
            Mensaje = "Salida a SIGRE registrada.";
            EnvaseSeleccionadoParaSalida = null;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void RegistrarEntregaFueraBlister()
    {
        var medicamento = _servicioMedicamentos.ObtenerPorCn(Cn);
        if (medicamento is null)
        {
            Mensaje = $"No se encuentra ningún medicamento activo con CN {Cn}.";
            return;
        }

        try
        {
            _servicioEnvases.RegistrarEntregaFueraBlister(
                new DatosEntregaFueraBlister(_pacienteId, medicamento.Id, string.IsNullOrWhiteSpace(Serie) ? null : Serie, Lote, EntregadoA ?? "paciente"),
                _usuarioActualId);
            Mensaje = "Entrega fuera de blíster registrada.";
            Cn = string.Empty; Serie = string.Empty; Lote = null; EntregadoA = null;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    public sealed record EnvaseFila(int Id, string NombreMedicamento, Envase Envase);
}
