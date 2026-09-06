using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Selector de médico reutilizable (Spec 001 FR-032/FR-033).
///
/// Sustituye al campo donde había que teclear el id del médico. Busca por apellidos, nombre o centro
/// sin tildes, y ofrece siempre «Nuevo médico…», porque el momento en que hace falta dar de alta a un
/// médico es justo mientras se registra el tratamiento que ha prescrito: mandar al usuario a otra
/// pantalla es perder lo que estaba escribiendo.
///
/// El alta abre un **panel lateral**, no un diálogo: la Spec 015 sustituyó los diálogos por paneles y
/// manda por ser posterior (anotado en spec.md de la 001 como corrección del FR-033).</summary>
public sealed partial class SelectorMedicoViewModel : ViewModelBase
{
    private readonly IServicioMedicos _servicio;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private string _texto = string.Empty;
    [ObservableProperty] private ObservableCollection<Medico> _resultados = [];
    [ObservableProperty] private Medico? _seleccionado;

    // Alta en línea (FR-033).
    [ObservableProperty] private bool _panelNuevoAbierto;
    [ObservableProperty] private string _nuevoNombre = string.Empty;
    [ObservableProperty] private string _nuevoApellidos = string.Empty;
    [ObservableProperty] private string? _nuevoColegiado;
    [ObservableProperty] private string? _nuevoEspecialidad;
    [ObservableProperty] private string? _nuevoCentro;
    [ObservableProperty] private string? _nuevoTelefono;
    [ObservableProperty] private string? _nuevoEmail;
    [ObservableProperty] private string? _mensaje;

    public SelectorMedicoViewModel(IServicioMedicos servicio, int? usuarioActualId)
    {
        _servicio = servicio;
        _usuarioActualId = usuarioActualId;
    }

    /// <summary>Etiqueta de un médico tal y como pide el FR-032: «Apellidos, Nombre — Centro».</summary>
    public static string Etiqueta(Medico? medico) => medico is null
        ? string.Empty
        : $"{medico.Apellidos}, {medico.Nombre}" + (medico.Centro is { Length: > 0 } c ? $" — {c}" : string.Empty);

    public string EtiquetaSeleccionado => Etiqueta(Seleccionado);

    public bool HaySeleccion => Seleccionado is not null;

    /// <summary>Coloca el selector sobre un médico ya guardado, al abrir un tratamiento existente.</summary>
    public void Establecer(int? medicoId)
    {
        Seleccionado = medicoId is null ? null : _servicio.ObtenerPorId(medicoId.Value);
        Texto = string.Empty;
    }

    [RelayCommand]
    private void Elegir(Medico? medico)
    {
        if (medico is null) return;
        Seleccionado = medico;
        Texto = string.Empty;
    }

    [RelayCommand]
    private void Limpiar()
    {
        Seleccionado = null;
        Texto = string.Empty;
    }

    /// <summary>FR-033. El panel se abre con lo ya tecleado como apellidos: quien escribe «Vidal» y
    /// no lo encuentra está a punto de dar de alta a Vidal.</summary>
    [RelayCommand]
    private void AbrirNuevo()
    {
        NuevoApellidos = Texto.Trim();
        NuevoNombre = string.Empty;
        NuevoColegiado = NuevoEspecialidad = NuevoCentro = NuevoTelefono = NuevoEmail = null;
        Mensaje = null;
        PanelNuevoAbierto = true;
    }

    [RelayCommand]
    private void CerrarNuevo() => PanelNuevoAbierto = false;

    [RelayCommand]
    private void CrearNuevo()
    {
        try
        {
            var resultado = _servicio.Crear(
                new DatosMedico(NuevoNombre, NuevoApellidos, NuevoColegiado, NuevoEspecialidad,
                    NuevoCentro, NuevoTelefono, NuevoEmail),
                _usuarioActualId);

            // FR-033: queda seleccionado en el campo de origen, sin perder lo que se estaba haciendo.
            Seleccionado = resultado.Medico;
            Texto = string.Empty;
            PanelNuevoAbierto = false;

            // FR-034: el aviso de posible duplicado se enseña, pero el alta ya está hecha.
            Mensaje = resultado.Aviso;
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    partial void OnTextoChanged(string value)
        => Resultados = new ObservableCollection<Medico>(_servicio.Buscar(value));

    partial void OnSeleccionadoChanged(Medico? value)
    {
        OnPropertyChanged(nameof(EtiquetaSeleccionado));
        OnPropertyChanged(nameof(HaySeleccion));
    }
}
