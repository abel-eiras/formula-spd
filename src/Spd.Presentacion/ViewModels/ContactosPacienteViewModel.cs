using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Pacientes;

namespace Spd.Presentacion.ViewModels;

/// <summary>Contactos del paciente (Spec 001 US3, FR-020..FR-023), dentro de la pestaña de datos.
///
/// No es un listín de teléfonos: el contacto **principal** es el «Familiar próximo» que se imprime en
/// el Anexo, y del marcado como **retira la medicación** sale el DNI del listado de retirada
/// (Spec 005 FR-531). Sin esta pantalla esa columna salía siempre en blanco.</summary>
public sealed partial class ContactosPacienteViewModel : ViewModelBase
{
    private readonly IServicioContactos _servicio;
    private readonly PacienteContexto _contexto;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<Contacto> _contactos = [];
    [ObservableProperty] private bool _verHistorico;
    [ObservableProperty] private string? _mensaje;

    // Formulario de alta y edición.
    [ObservableProperty] private Contacto? _enEdicion;
    [ObservableProperty] private bool _panelAbierto;
    [ObservableProperty] private TipoContacto _tipo = TipoContacto.Familiar;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _apellidos = string.Empty;
    [ObservableProperty] private string? _dni;
    [ObservableProperty] private string? _telefono;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private bool _esPrincipal;
    [ObservableProperty] private bool _retiraMedicacion;

    public TipoContacto[] TiposDisponibles { get; } = Enum.GetValues<TipoContacto>();

    /// <summary>Sin paciente guardado no hay a quién colgarle un contacto (FR-020 referencia
    /// `paciente_id`).</summary>
    public bool HayPaciente => _contexto.PacienteId is not null;

    public bool SinContactos => Contactos.Count == 0;

    /// <summary>FR-021b: si nadie está marcado, se entiende que el paciente retira su propia
    /// medicación y el listado de retirada usa su DNI. Decirlo evita que parezca un olvido.</summary>
    public bool RetiraElPropioPaciente => Contactos.All(c => !c.RetiraMedicacion);

    public string TituloPanel => EnEdicion is null ? "Nuevo contacto" : "Editar contacto";

    public ContactosPacienteViewModel(IServicioContactos servicio, PacienteContexto contexto, int? usuarioActualId)
    {
        _servicio = servicio;
        _contexto = contexto;
        _usuarioActualId = usuarioActualId;
        _contexto.Cambiado += _ => Cargar();
        Cargar();
    }

    private void Cargar()
    {
        Contactos = _contexto.PacienteId is { } id
            ? new ObservableCollection<Contacto>(_servicio.ListarDePaciente(id, VerHistorico))
            : [];

        OnPropertyChanged(nameof(HayPaciente));
        OnPropertyChanged(nameof(SinContactos));
        OnPropertyChanged(nameof(RetiraElPropioPaciente));
    }

    [RelayCommand]
    private void Nuevo()
    {
        EnEdicion = null;
        Tipo = TipoContacto.Familiar;
        Nombre = Apellidos = string.Empty;
        Dni = Telefono = Email = null;
        EsPrincipal = RetiraMedicacion = false;
        Mensaje = null;
        PanelAbierto = true;
        OnPropertyChanged(nameof(TituloPanel));
    }

    [RelayCommand]
    private void Editar(Contacto? contacto)
    {
        if (contacto is null) return;
        EnEdicion = contacto;
        Tipo = contacto.Tipo;
        Nombre = contacto.Nombre;
        Apellidos = contacto.Apellidos;
        Dni = contacto.Dni;
        Telefono = contacto.Telefono;
        Email = contacto.Email;
        EsPrincipal = contacto.EsPrincipal;
        RetiraMedicacion = contacto.RetiraMedicacion;
        Mensaje = null;
        PanelAbierto = true;
        OnPropertyChanged(nameof(TituloPanel));
    }

    [RelayCommand]
    private void CerrarPanel() => PanelAbierto = false;

    [RelayCommand]
    private void Guardar()
    {
        if (_contexto.PacienteId is not { } pacienteId) return;

        var datos = new DatosContacto(Tipo, Nombre, Apellidos, Dni, Telefono, Email, EsPrincipal, RetiraMedicacion);
        try
        {
            if (EnEdicion is null)
            {
                _servicio.Crear(pacienteId, datos, _usuarioActualId);
                Mensaje = "Contacto añadido.";
            }
            else
            {
                _servicio.Actualizar(EnEdicion.Id, datos, _usuarioActualId);
                Mensaje = "Contacto actualizado.";
            }
            PanelAbierto = false;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    /// <summary>FR-023 y Art. III.1: baja lógica; el contacto sigue estando en «ver histórico».</summary>
    [RelayCommand]
    private void DarDeBaja(Contacto? contacto)
    {
        if (contacto is null) return;
        try
        {
            _servicio.DarDeBaja(contacto.Id, _usuarioActualId);
            Mensaje = "Contacto dado de baja. Sigue en el histórico: nada se elimina.";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    partial void OnVerHistoricoChanged(bool value) => Cargar();
}
