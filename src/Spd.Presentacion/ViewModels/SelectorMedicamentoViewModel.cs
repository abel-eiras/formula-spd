using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Selector de medicamento del tratamiento (Spec 004 FR-400, revisado el 2026-09-14).
///
/// Sustituye al campo donde había que teclear un CN que ya tenía que estar en el catálogo. Con el
/// nomenclátor entero cargado casi siempre está: se busca por nombre sin tildes o por CN. Y cuando no
/// está —una fórmula magistral, un medicamento recién comercializado— se da de alta aquí mismo con
/// «Nuevo medicamento…», como los médicos (Spec 001 FR-033): mandar al usuario al catálogo es perder
/// lo que estaba escribiendo.
///
/// Un medicamento de baja se puede elegir: al hacerlo se reactiva la misma fila (Spec 003 CA-305),
/// nunca se duplica.</summary>
public sealed partial class SelectorMedicamentoViewModel : ViewModelBase
{
    private const int Limite = 20;

    private readonly IServicioMedicamentos _servicio;
    private readonly IServicioConsultaCima? _servicioCima;
    private readonly int? _usuarioActualId;
    private ResultadoConsultaCima? _datosCima;

    [ObservableProperty] private string _texto = string.Empty;
    [ObservableProperty] private ObservableCollection<Medicamento> _resultados = [];
    [ObservableProperty] private Medicamento? _seleccionado;
    [ObservableProperty] private string? _mensaje;

    // Alta en línea.
    [ObservableProperty] private bool _panelNuevoAbierto;
    [ObservableProperty] private string _nuevoCn = string.Empty;
    [ObservableProperty] private string _nuevoNombre = string.Empty;
    [ObservableProperty] private bool _consultandoCima;

    public SelectorMedicamentoViewModel(
        IServicioMedicamentos servicio, IServicioConsultaCima? servicioCima, int? usuarioActualId)
    {
        _servicio = servicio;
        _servicioCima = servicioCima;
        _usuarioActualId = usuarioActualId;
    }

    public static string Etiqueta(Medicamento? medicamento)
        => medicamento is null ? string.Empty : $"{medicamento.Nombre} — CN {medicamento.Cn}";

    public string EtiquetaSeleccionado => Etiqueta(Seleccionado);
    public bool HaySeleccion => Seleccionado is not null;
    public bool PuedeConsultarCima => _servicioCima is not null;

    /// <summary>Informativo, nunca bloquea: la aptitud la confirma el farmacéutico al preparar.</summary>
    public bool SeleccionadoNoApto => Seleccionado?.AptoSpd == false;

    /// <summary>Coloca el selector sobre un medicamento ya guardado (cambio de pauta), o lo vacía.</summary>
    public void Establecer(int? medicamentoId)
    {
        Seleccionado = medicamentoId is null ? null : _servicio.ObtenerPorId(medicamentoId.Value);
        Texto = string.Empty;
        Mensaje = null;
    }

    [RelayCommand]
    private void Elegir(Medicamento? medicamento)
    {
        if (medicamento is null) return;

        Mensaje = null;
        if (!medicamento.Activo)
        {
            // CA-305: se reactiva la misma fila, con su historia y sus id; nunca un duplicado.
            medicamento = _servicio.Crear(new DatosAltaMedicamento(medicamento.Cn, medicamento.Nombre), _usuarioActualId);
            Mensaje = $"{medicamento.Nombre} estaba de baja en el catálogo: se ha reactivado.";
        }

        Seleccionado = medicamento;
        Texto = string.Empty;
    }

    [RelayCommand]
    private void Limpiar() => Establecer(null);

    /// <summary>El panel se abre con lo ya tecleado: si son cifras es el CN, si no, el nombre.</summary>
    [RelayCommand]
    private void AbrirNuevo()
    {
        var tecleado = Texto.Trim();
        var esCn = tecleado.Length > 0 && tecleado.All(char.IsDigit);
        NuevoCn = esCn ? tecleado : string.Empty;
        NuevoNombre = esCn ? string.Empty : tecleado;
        _datosCima = null;
        Mensaje = null;
        PanelNuevoAbierto = true;
    }

    [RelayCommand]
    private void CerrarNuevo() => PanelNuevoAbierto = false;

    /// <summary>Spec 003 FR-323: acción explícita, nunca automática. Nunca rellena la aptitud.</summary>
    [RelayCommand]
    private async Task ConsultarCimaAsync()
    {
        if (_servicioCima is null) return;
        if (string.IsNullOrWhiteSpace(NuevoCn))
        {
            Mensaje = "Escribe el CN antes de consultar CIMA.";
            return;
        }

        ConsultandoCima = true;
        try
        {
            var resultado = await _servicioCima.ConsultarPorCnAsync(NuevoCn.Trim());
            if (resultado.Error is not null)
            {
                Mensaje = $"No se pudo consultar CIMA: {resultado.Error}. Puedes escribir el nombre a mano.";
                return;
            }
            if (!resultado.Encontrado)
            {
                Mensaje = "Ese CN no está en CIMA (puede ser una fórmula magistral). Escribe el nombre a mano.";
                return;
            }

            _datosCima = resultado;
            NuevoNombre = resultado.Nombre!;
            Mensaje = "Datos traídos de CIMA. Revísalos antes de crear.";
        }
        finally
        {
            ConsultandoCima = false;
        }
    }

    [RelayCommand]
    private void CrearNuevo()
    {
        try
        {
            var medicamento = _servicio.Crear(
                new DatosAltaMedicamento(NuevoCn.Trim(), NuevoNombre.Trim()), _usuarioActualId);

            // Lo que CIMA trae además del nombre se guarda también: la forma farmacéutica no está en el
            // nomenclátor y es la que dice qué se puede acondicionar.
            if (_datosCima is { Encontrado: true } cima)
            {
                medicamento.PrincipioActivo ??= cima.PrincipioActivo;
                medicamento.Laboratorio ??= cima.Laboratorio;
                medicamento.FormaFarmaceutica ??= cima.FormaFarmaceutica;
                _servicio.ActualizarDatos(medicamento, _usuarioActualId);
            }

            Seleccionado = medicamento;
            Texto = string.Empty;
            PanelNuevoAbierto = false;
            Mensaje = null;
        }
        catch (MedicamentoDuplicadoException ex)
        {
            // Ya estaba: se elige ese en lugar de fallar, que es lo que quería quien lo buscaba.
            Seleccionado = ex.Existente;
            Texto = string.Empty;
            PanelNuevoAbierto = false;
            Mensaje = $"Ese CN ya estaba en el catálogo: se ha elegido {ex.Existente.Nombre}.";
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    partial void OnTextoChanged(string value)
        => Resultados = new ObservableCollection<Medicamento>(_servicio.Buscar(value, Limite));

    partial void OnSeleccionadoChanged(Medicamento? value)
    {
        OnPropertyChanged(nameof(EtiquetaSeleccionado));
        OnPropertyChanged(nameof(HaySeleccion));
        OnPropertyChanged(nameof(SeleccionadoNoApto));
    }
}
