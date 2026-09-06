using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Pacientes;

namespace Spd.Presentacion.ViewModels;

/// <summary>Tratamientos de un paciente (FR-400..430): alta, cambio de pauta (cierra/abre fila,
/// Art. IV.3/IV.4) e historial. `Cn` se resuelve contra el catálogo de Spec 003 al guardar.</summary>
public sealed partial class TratamientoViewModel : ViewModelBase
{
    private readonly IServicioTratamientos _servicioTratamientos;
    private readonly IServicioMedicamentos _servicioMedicamentos;
    private readonly IServicioComunicacionesMedico _servicioComunicaciones;
    private readonly IServicioGeneracionDocumentos _servicioDocumentos;
    private readonly int _pacienteId;
    /// <summary>FR-032/FR-033: el prescriptor se busca por nombre y se puede dar de alta aquí mismo.</summary>
    public SelectorMedicoViewModel SelectorMedico { get; }

    private readonly PacienteContexto? _contexto;
    private readonly int? _usuarioActualId;
    private int? _tratamientoIdEnEdicion;

    [ObservableProperty] private ObservableCollection<TratamientoFila> _vigentes = [];
    [ObservableProperty] private string? _mensaje;

    /// <summary>Línea temporal del historial de un medicamento en este paciente (Spec 004 FR-410,
    /// CA-402). El servicio existía y estaba probado desde hace semanas; faltaba la pantalla.</summary>
    [ObservableProperty] private ObservableCollection<HitoTratamiento> _historial = [];
    [ObservableProperty] private string? _historialDe;

    public bool HayHistorial => Historial.Count > 0;

    [ObservableProperty] private string _cn = string.Empty;
    [ObservableProperty] private bool _enSpd = true;
    [ObservableProperty] private string? _problemaSalud;
    [ObservableProperty] private int? _medicoId;
    [ObservableProperty] private FraccionDosis? _pautaD;
    [ObservableProperty] private FraccionDosis? _pautaA;
    [ObservableProperty] private FraccionDosis? _pautaC;
    [ObservableProperty] private FraccionDosis? _pautaN;
    [ObservableProperty] private string? _pautaTexto;
    [ObservableProperty] private string _diasSemana = "1111111";
    [ObservableProperty] private string? _via;
    [ObservableProperty] private string? _momento;
    [ObservableProperty] private TipoTratamiento _tipo = TipoTratamiento.Cronico;

    public FraccionDosis?[] FraccionesDisponibles { get; } = [null, .. Enum.GetValues<FraccionDosis>().Cast<FraccionDosis?>()];
    public TipoTratamiento[] TiposDisponibles { get; } = Enum.GetValues<TipoTratamiento>();
    public bool EsCambioDePauta => _tratamientoIdEnEdicion is not null;

    public TratamientoViewModel(
        IServicioTratamientos servicioTratamientos, IServicioMedicamentos servicioMedicamentos,
        IServicioComunicacionesMedico servicioComunicaciones, IServicioGeneracionDocumentos servicioDocumentos,
        IServicioMedicos servicioMedicos, int pacienteId, int? usuarioActualId, PacienteContexto? contexto = null)
    {
        SelectorMedico = new SelectorMedicoViewModel(servicioMedicos, usuarioActualId);
        _contexto = contexto;
        _servicioTratamientos = servicioTratamientos;
        _servicioMedicamentos = servicioMedicamentos;
        _servicioComunicaciones = servicioComunicaciones;
        _servicioDocumentos = servicioDocumentos;
        _pacienteId = pacienteId;
        _usuarioActualId = usuarioActualId;
        CargarVigentes();
    }

    // FR-804: prerrellena paciente y médico prescriptor sin guardar nada (FR-807).
    [RelayCommand]
    private void ComunicarIncidencia(TratamientoFila fila)
    {
        var prerrelleno = _servicioComunicaciones.PrepararDesdeTratamiento(fila.Tratamiento.Id);
        // Spec 015 FR-1530: en vez de abrir una ventana, se cambia a la pestaña de comunicaciones
        // con el formulario ya relleno; el tratamiento que la motiva se sigue viendo a un clic.
        _contexto?.IrA(PestanaPaciente.Comunicaciones, prerrelleno);
    }

    private void CargarVigentes()
        => Vigentes = new ObservableCollection<TratamientoFila>(
            _servicioTratamientos.ListarVigentesDePaciente(_pacienteId)
                .Select(t => new TratamientoFila(t.Id, _servicioMedicamentos.ObtenerPorId(t.MedicamentoId)?.Nombre ?? "(medicamento no encontrado)", t)));

    [RelayCommand]
    private void PrepararCambioDePauta(TratamientoFila fila)
    {
        _tratamientoIdEnEdicion = fila.Tratamiento.Id;
        Cn = _servicioMedicamentos.ObtenerPorId(fila.Tratamiento.MedicamentoId)?.Cn ?? string.Empty;
        EnSpd = fila.Tratamiento.EnSpd;
        ProblemaSalud = fila.Tratamiento.ProblemaSalud;
        MedicoId = fila.Tratamiento.MedicoId;
        PautaD = fila.Tratamiento.PautaD;
        PautaA = fila.Tratamiento.PautaA;
        PautaC = fila.Tratamiento.PautaC;
        PautaN = fila.Tratamiento.PautaN;
        PautaTexto = fila.Tratamiento.PautaTexto;
        DiasSemana = fila.Tratamiento.DiasSemana;
        Via = fila.Tratamiento.Via;
        Momento = fila.Tratamiento.Momento;
        Tipo = fila.Tratamiento.Tipo;
        OnPropertyChanged(nameof(EsCambioDePauta));
    }

    [RelayCommand]
    private void NuevoTratamiento()
    {
        _tratamientoIdEnEdicion = null;
        Cn = string.Empty;
        EnSpd = true;
        ProblemaSalud = null;
        MedicoId = null;
        PautaD = PautaA = PautaC = PautaN = null;
        PautaTexto = null;
        DiasSemana = "1111111";
        Via = null;
        Momento = null;
        Tipo = TipoTratamiento.Cronico;
        Mensaje = null;
        OnPropertyChanged(nameof(EsCambioDePauta));
    }

    [RelayCommand]
    private void Guardar()
    {
        var medicamento = _servicioMedicamentos.ObtenerPorCn(Cn);
        if (medicamento is null)
        {
            Mensaje = $"No se encuentra ningún medicamento activo con CN {Cn}.";
            return;
        }

        var datos = new DatosAltaTratamiento(
            medicamento.Id, EnSpd, ProblemaSalud, SelectorMedico.Seleccionado?.Id,
            PautaD, PautaA, PautaC, PautaN, PautaTexto, DiasSemana, Via, Momento,
            DateOnly.FromDateTime(DateTime.Now), Tipo);

        if (_tratamientoIdEnEdicion is { } idEnEdicion)
        {
            _servicioTratamientos.CambiarPauta(idEnEdicion, datos, _usuarioActualId);
            Mensaje = "Pauta cambiada: el tratamiento anterior queda en el historial.";
        }
        else
        {
            _servicioTratamientos.Crear(_pacienteId, datos, _usuarioActualId);
            Mensaje = "Tratamiento creado.";
        }

        NuevoTratamiento();
        CargarVigentes();
    }

    /// <summary>FR-410/CA-402: el historial de un medicamento en este paciente, del más reciente al
    /// más antiguo. Un cambio de pauta cierra el tratamiento anterior y abre otro (Art. III: nada se
    /// reescribe), así que el historial es la única forma de ver **qué tomaba y desde cuándo**, que
    /// es justo lo que pregunta un médico cuando llama.</summary>
    [RelayCommand]
    private void VerHistorial(TratamientoFila? fila)
    {
        if (fila is null) return;

        var historial = _servicioTratamientos
            .ListarHistorialDeMedicamento(_pacienteId, fila.Tratamiento.MedicamentoId)
            .OrderByDescending(t => t.FechaInicio)
            .ThenByDescending(t => t.Id)
            .ToList();

        HistorialDe = fila.NombreMedicamento;
        Historial = new ObservableCollection<HitoTratamiento>(
            historial.Select((t, i) => new HitoTratamiento(t, EsVigente: i == 0 && t.FechaFin is null)));
        OnPropertyChanged(nameof(HayHistorial));
    }

    [RelayCommand]
    private void CerrarHistorial()
    {
        Historial = [];
        HistorialDe = null;
        OnPropertyChanged(nameof(HayHistorial));
    }

    public sealed record TratamientoFila(int Id, string NombreMedicamento, Tratamiento Tratamiento);

    /// <summary>Un tramo de la línea temporal: qué pauta estuvo vigente, entre qué fechas y por qué
    /// terminó.</summary>
    public sealed record HitoTratamiento(Tratamiento Tratamiento, bool EsVigente)
    {
        public string Periodo => Tratamiento.FechaFin is { } fin
            ? $"{Tratamiento.FechaInicio:dd/MM/yyyy} — {fin:dd/MM/yyyy}"
            : $"desde {Tratamiento.FechaInicio:dd/MM/yyyy}";

        /// <summary>La pauta en el formato que se lee en el blíster: desayuno-almuerzo-cena-noche.</summary>
        public string Pauta => Tratamiento.PautaTexto is { Length: > 0 } libre
            ? libre
            : string.Join("-", new[] { Tratamiento.PautaD, Tratamiento.PautaA, Tratamiento.PautaC, Tratamiento.PautaN }
                .Select(f => f?.Texto() ?? "0"));

        public string Dias => Tratamiento.DiasSemana == "1111111"
            ? "todos los días"
            : string.Join(" ", Spd.Dominio.DiasSemana.Codigos.Where((_, i) => i < Tratamiento.DiasSemana.Length && Tratamiento.DiasSemana[i] == '1'));

        public string Estado => Tratamiento.Estado.ToString();

        public Controles.VariantePastilla Variante => EsVigente
            ? Controles.VariantePastilla.Apto
            : Tratamiento.Estado == EstadoTratamiento.PendienteRevision
                ? Controles.VariantePastilla.Aviso
                : Controles.VariantePastilla.Neutra;

        public string? Motivo => Tratamiento.Incidencias ?? Tratamiento.Intervencion;
        public bool TieneMotivo => !string.IsNullOrWhiteSpace(Motivo);
    }

    /// <summary>FR-032: el `MedicoId` que se guarda sale de lo elegido en el selector. Al abrir un
    /// tratamiento existente se hace el camino inverso, para que el campo muestre a su prescriptor.</summary>
    partial void OnMedicoIdChanged(int? value) => SelectorMedico.Establecer(value);
}
