using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Catálogo de médicos (Spec 001 FR-037): listado con búsqueda, cuántos pacientes tienen a
/// cada uno de cabecera, y su ficha para editarlo o darlo de baja.
///
/// Va en el trabajo diario y no en administración: quien registra un tratamiento necesita poder dar
/// de alta al prescriptor en ese momento, y si eso exigiera un Administrador se quedaría
/// bloqueado esperando.</summary>
public sealed partial class CatalogoMedicosViewModel : ViewModelBase
{
    private readonly IServicioMedicos _servicio;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private string _fragmento = string.Empty;
    [ObservableProperty] private ObservableCollection<FilaMedico> _resultados = [];
    [ObservableProperty] private string? _mensaje;

    // Ficha del médico seleccionado (alta y edición comparten formulario).
    [ObservableProperty] private Medico? _seleccionado;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _apellidos = string.Empty;
    [ObservableProperty] private string? _colegiado;
    [ObservableProperty] private string? _especialidad;
    [ObservableProperty] private string? _centro;
    [ObservableProperty] private string? _telefono;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _direccion;

    public bool EsAltaNueva => Seleccionado is null;

    public CatalogoMedicosViewModel(IServicioMedicos servicio, int? usuarioActualId)
    {
        _servicio = servicio;
        _usuarioActualId = usuarioActualId;
        Cargar();
    }

    private void Cargar()
    {
        var medicos = string.IsNullOrWhiteSpace(Fragmento)
            ? _servicio.ListarActivos()
            : _servicio.Buscar(Fragmento);

        Resultados = new ObservableCollection<FilaMedico>(
            medicos.Select(m => new FilaMedico(m, _servicio.ContarPacientesDeCabecera(m.Id))));
    }

    [RelayCommand]
    private void Recargar() => Cargar();

    [RelayCommand]
    private void Nuevo()
    {
        Seleccionado = null;
        Nombre = Apellidos = string.Empty;
        Colegiado = Especialidad = Centro = Telefono = Email = Direccion = null;
        Mensaje = null;
        OnPropertyChanged(nameof(EsAltaNueva));
    }

    [RelayCommand]
    private void Abrir(FilaMedico? fila)
    {
        if (fila is null) return;
        Seleccionado = fila.Medico;
        Nombre = fila.Medico.Nombre;
        Apellidos = fila.Medico.Apellidos;
        Colegiado = fila.Medico.Colegiado;
        Especialidad = fila.Medico.Especialidad;
        Centro = fila.Medico.Centro;
        Telefono = fila.Medico.Telefono;
        Email = fila.Medico.Email;
        Direccion = fila.Medico.Direccion;
        Mensaje = null;
        OnPropertyChanged(nameof(EsAltaNueva));
    }

    [RelayCommand]
    private void Guardar()
    {
        var datos = new DatosMedico(Nombre, Apellidos, Colegiado, Especialidad, Centro, Telefono, Email, Direccion);
        try
        {
            if (Seleccionado is null)
            {
                var resultado = _servicio.Crear(datos, _usuarioActualId);
                Seleccionado = resultado.Medico;
                // FR-034: el posible duplicado se avisa; el alta ya está hecha.
                Mensaje = resultado.Aviso ?? $"Médico {resultado.Medico.Apellidos}, {resultado.Medico.Nombre} creado.";
            }
            else
            {
                _servicio.Actualizar(Seleccionado.Id, datos, _usuarioActualId);
                Mensaje = "Cambios guardados. Se reflejan en todos los pacientes y tratamientos que lo referencian.";
            }
            OnPropertyChanged(nameof(EsAltaNueva));
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    /// <summary>FR-036: la baja se rechaza con la lista de pacientes a los que hay que reasignar.</summary>
    [RelayCommand]
    private void DarDeBaja(FilaMedico? fila)
    {
        if (fila is null) return;
        try
        {
            _servicio.DarDeBaja(fila.Medico.Id, _usuarioActualId);
            Mensaje = $"{fila.Medico.Apellidos}, {fila.Medico.Nombre} dado de baja. Sigue disponible en los tratamientos que ya lo referencian.";
            if (Seleccionado?.Id == fila.Medico.Id) Nuevo();
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    partial void OnFragmentoChanged(string value) => Cargar();

    /// <summary>Una fila del catálogo: el médico y cuántos pacientes lo tienen de cabecera (FR-037),
    /// que es el dato que dice si se puede dar de baja y a quién habría que reasignar.</summary>
    public sealed record FilaMedico(Medico Medico, int PacientesDeCabecera)
    {
        public string Etiqueta => SelectorMedicoViewModel.Etiqueta(Medico);
        public bool SePuedeDarDeBaja => PacientesDeCabecera == 0;
    }
}
