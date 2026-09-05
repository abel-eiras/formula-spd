using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Tratamientos de un paciente (FR-400..430): alta, cambio de pauta (cierra/abre fila,
/// Art. IV.3/IV.4) e historial. `Cn` se resuelve contra el catálogo de Spec 003 al guardar.</summary>
public sealed partial class TratamientoViewModel : ViewModelBase
{
    private readonly IServicioTratamientos _servicioTratamientos;
    private readonly IServicioMedicamentos _servicioMedicamentos;
    private readonly int _pacienteId;
    private readonly int? _usuarioActualId;
    private int? _tratamientoIdEnEdicion;

    [ObservableProperty] private ObservableCollection<TratamientoFila> _vigentes = [];
    [ObservableProperty] private string? _mensaje;

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
        int pacienteId, int? usuarioActualId)
    {
        _servicioTratamientos = servicioTratamientos;
        _servicioMedicamentos = servicioMedicamentos;
        _pacienteId = pacienteId;
        _usuarioActualId = usuarioActualId;
        CargarVigentes();
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
            medicamento.Id, EnSpd, ProblemaSalud, MedicoId,
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

    public sealed record TratamientoFila(int Id, string NombreMedicamento, Tratamiento Tratamiento);
}
