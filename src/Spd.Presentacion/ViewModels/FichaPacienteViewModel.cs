using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Pacientes;

namespace Spd.Presentacion.ViewModels;

/// <summary>Ficha de paciente, pestaña Datos (FR-002/FR-003/FR-008). `Paciente` es null en alta
/// nueva; al guardar por primera vez pasa a contener el paciente ya creado, con su num_ficha
/// asignado (FR-001, no editable — por eso no hay ninguna propiedad `NumFichaNuevo`).
///
/// Spec 015: era el centro de mando que abría las otras seis ventanas del paciente. Ya no abre
/// nada: son pestañas del mismo espacio, y el paciente compartido vive en el
/// <see cref="PacienteContexto"/>. Con eso desaparecen los seis comandos `Abrir…`, las nueve
/// dependencias de servicio que solo servían para construir aquellas ventanas y el parche de releer
/// la ficha al cerrarlas.</summary>
public sealed partial class FichaPacienteViewModel : ViewModelBase
{
    private readonly IServicioPacientes _servicio;
    private readonly PacienteContexto _contexto;
    private readonly int? _usuarioActualId;
    private bool _pendienteConfirmarDuplicado;

    [ObservableProperty] private Paciente? _paciente;
    [ObservableProperty] private string? _mensaje;

    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _apellidos = string.Empty;
    [ObservableProperty] private string? _sexo;
    [ObservableProperty] private string? _dni;
    [ObservableProperty] private DateTimeOffset? _fechaNacimiento;
    [ObservableProperty] private string? _numSs;
    [ObservableProperty] private string? _cip;
    [ObservableProperty] private string? _direccion;
    [ObservableProperty] private string? _cp;
    [ObservableProperty] private string? _poblacion;
    [ObservableProperty] private string? _telefono1;
    [ObservableProperty] private string? _telefono2;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _enfermedadesCronicas;
    [ObservableProperty] private string? _alergias;
    [ObservableProperty] private string? _observaciones;
    [ObservableProperty] private bool _pictogramaComidas;
    [ObservableProperty] private string? _identificadorVisual;
    [ObservableProperty] private string? _diaRetirada;
    [ObservableProperty] private int _nBlisteres = 1;

    public string[] DiasSemanaDisponibles => DiasSemana.Codigos;
    public string[] SexosDisponibles { get; } = ["M", "H"];

    /// <summary>Cabecera permanente de la ficha (FR-008). Vacía hasta el primer guardado.</summary>
    public string NumFicha => Paciente?.NumFicha ?? "(sin asignar todavía)";

    public string Estado => Paciente?.Estado.ToString() ?? EstadoPaciente.Evaluacion.ToString();

    public int? Edad => FechaNacimiento is null
        ? null
        : CalcularEdad(DateOnly.FromDateTime(FechaNacimiento.Value.Date));

    public bool TieneAlergias => !string.IsNullOrWhiteSpace(Alergias);

    /// <summary>Médico de cabecera (FR-031). Hasta ahora la ficha no tenía dónde ponerlo: el alta
    /// pasaba `MedicoId: null` siempre, así que ningún paciente llegaba a tener médico asignado.</summary>
    public SelectorMedicoViewModel SelectorMedico { get; }

    /// <summary>Contactos del paciente (FR-009/FR-020), dentro de esta misma pestaña.</summary>
    public ContactosPacienteViewModel Contactos { get; }

    public FichaPacienteViewModel(
        IServicioPacientes servicio, IServicioMedicos servicioMedicos, IServicioContactos servicioContactos,
        PacienteContexto contexto, int? usuarioActualId)
    {
        _servicio = servicio;
        _contexto = contexto;
        _usuarioActualId = usuarioActualId;
        SelectorMedico = new SelectorMedicoViewModel(servicioMedicos, usuarioActualId);
        Contactos = new ContactosPacienteViewModel(servicioContactos, contexto, usuarioActualId);
        Paciente = contexto.Paciente;
        if (Paciente is not null) CargarDesdePaciente(Paciente);

        // Si otra pestaña cambia al paciente (la idoneidad lo activa, Spec 002 FR-213), los campos
        // y la cabecera de esta se actualizan solos: es lo que antes exigía cerrar la ventana.
        _contexto.Cambiado += AlCambiarElPaciente;
    }

    private void AlCambiarElPaciente(Paciente? paciente)
    {
        if (paciente is null || ReferenceEquals(paciente, Paciente)) return;
        Paciente = paciente;
        CargarDesdePaciente(paciente);
        OnPropertyChanged(nameof(NumFicha));
        OnPropertyChanged(nameof(Estado));
    }

    [RelayCommand]
    private void Guardar()
    {
        try
        {
            if (Paciente is null)
            {
                var creado = _servicio.Crear(ConstruirDatosAlta(), _usuarioActualId);
                Paciente = creado;
                // Avisar al contexto es lo que hace aparecer las otras seis pestañas (FR-1530).
                _contexto.Establecer(creado);
                Mensaje = $"Paciente creado con ficha {creado.NumFicha}.";
            }
            else
            {
                AplicarCambiosAPaciente(Paciente);
                _servicio.Actualizar(Paciente, _usuarioActualId);
                _contexto.Establecer(Paciente);
                Mensaje = "Cambios guardados.";
            }
            _pendienteConfirmarDuplicado = false;
            OnPropertyChanged(nameof(NumFicha));
            OnPropertyChanged(nameof(Estado));
        }
        catch (PacienteDuplicadoException ex)
        {
            Mensaje = $"{ex.Message} Pulse \"Guardar\" de nuevo para continuar de todas formas.";
            _pendienteConfirmarDuplicado = true;
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    private DatosAltaPaciente ConstruirDatosAlta() => new(
        Nombre, Apellidos, Sexo, Dni,
        FechaNacimiento is null ? null : DateOnly.FromDateTime(FechaNacimiento.Value.Date),
        NumSs, Cip, Direccion, Cp, Poblacion, Telefono1, Telefono2, Email,
        MedicoId: SelectorMedico.Seleccionado?.Id,
        EnfermedadesCronicas, Alergias, Observaciones, PictogramaComidas, IdentificadorVisual,
        DiaRetirada, NBlisteres, ConfirmarDuplicado: _pendienteConfirmarDuplicado);

    private void AplicarCambiosAPaciente(Paciente paciente)
    {
        paciente.Nombre = Nombre;
        paciente.Apellidos = Apellidos;
        paciente.Sexo = Sexo;
        paciente.Dni = Dni;
        paciente.FechaNacimiento = FechaNacimiento is null ? null : DateOnly.FromDateTime(FechaNacimiento.Value.Date);
        paciente.NumSs = NumSs;
        paciente.Cip = Cip;
        paciente.Direccion = Direccion;
        paciente.Cp = Cp;
        paciente.Poblacion = Poblacion;
        paciente.Telefono1 = Telefono1;
        paciente.Telefono2 = Telefono2;
        paciente.Email = Email;
        // FR-035: se guarda la referencia al médico, nunca una copia de sus datos.
        paciente.MedicoId = SelectorMedico.Seleccionado?.Id;
        paciente.EnfermedadesCronicas = EnfermedadesCronicas;
        paciente.Alergias = Alergias;
        paciente.Observaciones = Observaciones;
        paciente.PictogramaComidas = PictogramaComidas;
        paciente.IdentificadorVisual = IdentificadorVisual;
        paciente.DiaRetirada = DiaRetirada ?? paciente.DiaRetirada;
        paciente.NBlisteres = NBlisteres;
    }

    private void CargarDesdePaciente(Paciente paciente)
    {
        Nombre = paciente.Nombre;
        Apellidos = paciente.Apellidos;
        Sexo = paciente.Sexo;
        Dni = paciente.Dni;
        FechaNacimiento = paciente.FechaNacimiento is null ? null : new DateTimeOffset(paciente.FechaNacimiento.Value.ToDateTime(TimeOnly.MinValue));
        NumSs = paciente.NumSs;
        Cip = paciente.Cip;
        Direccion = paciente.Direccion;
        Cp = paciente.Cp;
        Poblacion = paciente.Poblacion;
        Telefono1 = paciente.Telefono1;
        Telefono2 = paciente.Telefono2;
        Email = paciente.Email;
        SelectorMedico.Establecer(paciente.MedicoId);
        EnfermedadesCronicas = paciente.EnfermedadesCronicas;
        Alergias = paciente.Alergias;
        Observaciones = paciente.Observaciones;
        PictogramaComidas = paciente.PictogramaComidas;
        IdentificadorVisual = paciente.IdentificadorVisual;
        DiaRetirada = paciente.DiaRetirada;
        NBlisteres = paciente.NBlisteres;
    }

    private static int CalcularEdad(DateOnly fechaNacimiento)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var edad = hoy.Year - fechaNacimiento.Year;
        if (hoy < fechaNacimiento.AddYears(edad)) edad--;
        return edad;
    }

    partial void OnFechaNacimientoChanged(DateTimeOffset? value) => OnPropertyChanged(nameof(Edad));

    partial void OnAlergiasChanged(string? value) => OnPropertyChanged(nameof(TieneAlergias));
}
