using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Pestaña Depósito de la ficha de paciente (FR-510..517, FR-540..544): alta de envase,
/// custodia/histórico, salida a SIGRE y entrega fuera de blíster.</summary>
public sealed partial class DepositoViewModel : ViewModelBase
{
    private readonly IServicioEnvases _servicioEnvases;
    private readonly IServicioMedicamentos _servicioMedicamentos;
    private readonly IServicioTratamientos _servicioTratamientos;
    private readonly IServicioImportacionTratamientoEnvase _servicioImportacion;
    private readonly int _pacienteId;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<EnvaseFila> _enCustodia = [];
    [ObservableProperty] private ObservableCollection<EnvaseFila> _historico = [];
    [ObservableProperty] private bool _mostrarHistorico;
    [ObservableProperty] private string? _mensaje;

    /// <summary>Medicamentos con tratamiento vigente en SPD de este paciente (Spec 004), para
    /// elegirlos aquí en vez de teclear el CN: la mayoría de altas de envase son de un medicamento
    /// que ya tiene tratamiento, así que escanear o seleccionar cubre el caso habitual.</summary>
    [ObservableProperty] private ObservableCollection<MedicamentoParaEnvase> _medicamentosConTratamiento = [];
    [ObservableProperty] private MedicamentoParaEnvase? _medicamentoSeleccionado;

    [ObservableProperty] private string _codigoEscaneado = string.Empty;
    [ObservableProperty] private string _cn = string.Empty;
    [ObservableProperty] private string _serie = string.Empty;
    [ObservableProperty] private string? _lote;
    [ObservableProperty] private DateTimeOffset? _caducidad;
    [ObservableProperty] private int? _unidadesIniciales;
    private OrigenEnvase _origenAlta = OrigenEnvase.Manual;

    public UnidadesDelCatalogo Unidades { get; } = new();

    /// <summary>Tras una lectura correcta solo queda revisar las unidades: la vista lleva ahí el foco
    /// para que Intro guarde el envase sin tocar el ratón.</summary>
    public event Action? LecturaAplicada;

    [ObservableProperty] private string? _entregadoA;

    [ObservableProperty] private EnvaseFila? _envaseSeleccionadoParaSalida;
    [ObservableProperty] private MotivoSalidaEnvase _motivoSalidaSeleccionado = MotivoSalidaEnvase.Otro;

    public MotivoSalidaEnvase[] MotivosSalida { get; } = Enum.GetValues<MotivoSalidaEnvase>();

    public DepositoViewModel(
        IServicioEnvases servicioEnvases, IServicioMedicamentos servicioMedicamentos,
        IServicioTratamientos servicioTratamientos, IServicioImportacionTratamientoEnvase servicioImportacion,
        int pacienteId, int? usuarioActualId)
    {
        _servicioEnvases = servicioEnvases;
        _servicioMedicamentos = servicioMedicamentos;
        _servicioTratamientos = servicioTratamientos;
        _servicioImportacion = servicioImportacion;
        _pacienteId = pacienteId;
        _usuarioActualId = usuarioActualId;
        Cargar();
    }

    /// <summary>Spec 015 FR-1533: la importación por pegado deja de ser una ventana y pasa a un
    /// panel lateral del propio depósito. Es lo que hace falta para poder comparar lo pegado con lo
    /// que ya hay en custodia, que es exactamente lo que la ventana tapaba.</summary>
    [ObservableProperty] private ImportarTratamientoViewModel? _panelImportar;

    public bool PanelImportarAbierto => PanelImportar is not null;

    [RelayCommand]
    private void AbrirImportarTratamiento()
    {
        PanelImportar = new ImportarTratamientoViewModel(_servicioImportacion, _pacienteId, _usuarioActualId);
        OnPropertyChanged(nameof(PanelImportarAbierto));
    }

    [RelayCommand]
    private void CerrarPanelImportar()
    {
        PanelImportar = null;
        OnPropertyChanged(nameof(PanelImportarAbierto));
        // Lo importado son envases nuevos en custodia: la lista de detrás tiene que reflejarlo.
        Cargar();
    }

    private void Cargar()
    {
        EnCustodia = new ObservableCollection<EnvaseFila>(_servicioEnvases.ListarEnCustodiaDePaciente(_pacienteId).Select(AFila));
        Historico = new ObservableCollection<EnvaseFila>(_servicioEnvases.ListarHistoricoDePaciente(_pacienteId).Select(AFila));
        MedicamentosConTratamiento = new ObservableCollection<MedicamentoParaEnvase>(
            _servicioTratamientos.ListarVigentesDePaciente(_pacienteId)
                .Where(t => t.EnSpd)
                .Select(t => _servicioMedicamentos.ObtenerPorId(t.MedicamentoId))
                .Where(m => m is not null)
                .Select(m => new MedicamentoParaEnvase(m!.Cn, m.Nombre))
                .DistinctBy(m => m.Cn)
                .OrderBy(m => m.NombreMedicamento));
    }

    private EnvaseFila AFila(Envase e) => new(
        e.Id, _servicioMedicamentos.ObtenerPorId(e.MedicamentoId)?.Nombre ?? "(medicamento no encontrado)", e);

    /// <summary>Elegir un medicamento con tratamiento vigente rellena el CN igual que si se
    /// tecleara: solo evita el tecleo, no cambia cómo se registra el envase.</summary>
    partial void OnMedicamentoSeleccionadoChanged(MedicamentoParaEnvase? value)
    {
        if (value is not null) Cn = value.Cn;
    }

    /// <summary>FR-513: el CN llega tecleado, elegido o escaneado; en los tres casos las unidades
    /// iniciales se proponen desde el catálogo.</summary>
    partial void OnCnChanged(string value)
    {
        var medicamento = string.IsNullOrWhiteSpace(value) ? null : _servicioMedicamentos.ObtenerPorCn(value.Trim());
        UnidadesIniciales = Unidades.AlCambiarMedicamento(medicamento, UnidadesIniciales);
    }

    /// <summary>spec-012: metodo principal para rellenar lote, numero de serie y caducidad. El
    /// Codigo Nacional solo viene en el codigo si el fabricante incluye el AI 712 (FR-1201); cuando
    /// no viene, se introduce a mano igual que hasta ahora.</summary>
    [RelayCommand]
    private void LeerCodigoEscaneado()
    {
        var datos = LectorGs1DataMatrix.Leer(CodigoEscaneado);
        CodigoEscaneado = string.Empty;
        if (datos is null)
        {
            Mensaje = "Código no reconocido (FR-1204): introduce lote, número de serie y caducidad a mano.";
            return;
        }

        Serie = datos.NumeroSerie;
        Lote = datos.Lote;
        Caducidad = new DateTimeOffset(datos.Caducidad.ComoFecha().ToDateTime(TimeOnly.MinValue));
        _origenAlta = OrigenEnvase.Escaneado;

        if (datos.CodigoNacional is { } cn)
        {
            Cn = cn;
            Mensaje = "Lectura aplicada, incluido el CN.";
        }
        else
        {
            Mensaje = "Lectura aplicada. El código no incluye el CN: introdúcelo a mano.";
        }
        LecturaAplicada?.Invoke();
    }

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
                    DateOnly.FromDateTime(Caducidad.Value.Date), UnidadesIniciales.Value, _origenAlta),
                _usuarioActualId);
            Unidades.GuardarSiProcede(_servicioMedicamentos, UnidadesIniciales.Value, _usuarioActualId);

            Mensaje = envase.Caducidad < DateOnly.FromDateTime(DateTime.Today)
                ? "Envase registrado. Aviso: la caducidad ya ha pasado (FR-514); nunca se propondrá para una preparación."
                : "Envase registrado.";
            Cn = string.Empty; Serie = string.Empty; Lote = null; Caducidad = null; UnidadesIniciales = null;
            MedicamentoSeleccionado = null;
            Unidades.Reiniciar();
            _origenAlta = OrigenEnvase.Manual;
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
            _origenAlta = OrigenEnvase.Manual;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    public sealed record EnvaseFila(int Id, string NombreMedicamento, Envase Envase);

    public sealed record MedicamentoParaEnvase(string Cn, string NombreMedicamento);
}
