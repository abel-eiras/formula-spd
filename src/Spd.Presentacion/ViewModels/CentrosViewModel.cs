using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Un blíster del albarán, marcable para incluirlo o no en el documento.</summary>
public sealed partial class BlisterAlbaranFila(BlisterParaAlbaran blister) : ObservableObject
{
    public BlisterParaAlbaran Blister { get; } = blister;
    [ObservableProperty] private bool _incluido = true;
}

/// <summary>Centros y residencias (Spec 019): catálogo, pacientes de cada centro y albarán de entrega
/// de los blísteres verificados.</summary>
public sealed partial class CentrosViewModel : ViewModelBase
{
    private readonly IServicioCentros _servicio;
    private readonly IServicioPacientes _servicioPacientes;
    private readonly IServicioDocumentosCentros? _documentos;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<Centro> _centros = [];
    [ObservableProperty] private bool _incluirBaja;
    [ObservableProperty] private Centro? _seleccionado;

    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string? _direccion;
    [ObservableProperty] private string? _telefono;
    [ObservableProperty] private string? _personaContacto;
    [ObservableProperty] private string? _motivoBaja;

    [ObservableProperty] private ObservableCollection<Paciente> _pacientesDelCentro = [];
    [ObservableProperty] private string _busquedaPaciente = string.Empty;
    [ObservableProperty] private ObservableCollection<Paciente> _resultadosPaciente = [];
    [ObservableProperty] private ObservableCollection<BlisterAlbaranFila> _blisteres = [];
    [ObservableProperty] private string? _mensaje;

    public CentrosViewModel(IServicioCentros servicio, IServicioPacientes servicioPacientes,
        IServicioDocumentosCentros? documentos, int? usuarioActualId)
    {
        _servicio = servicio;
        _servicioPacientes = servicioPacientes;
        _documentos = documentos;
        _usuarioActualId = usuarioActualId;
        Cargar();
    }

    public bool HaySeleccion => Seleccionado is not null;

    partial void OnIncluirBajaChanged(bool value) => Cargar();

    partial void OnSeleccionadoChanged(Centro? value)
    {
        Nombre = value?.Nombre ?? string.Empty;
        Direccion = value?.Direccion;
        Telefono = value?.Telefono;
        PersonaContacto = value?.PersonaContacto;
        RecargarDetalle();
        OnPropertyChanged(nameof(HaySeleccion));
    }

    partial void OnBusquedaPacienteChanged(string value)
        => ResultadosPaciente = new ObservableCollection<Paciente>(
            value.Trim().Length < 2 ? [] : _servicioPacientes.Buscar(value, null, null).Take(10));

    private void Cargar()
    {
        var id = Seleccionado?.Id;
        Centros = new ObservableCollection<Centro>(_servicio.Listar(IncluirBaja));
        Seleccionado = Centros.FirstOrDefault(c => c.Id == id);
    }

    private void RecargarDetalle()
    {
        if (Seleccionado is not { } centro)
        {
            PacientesDelCentro = [];
            Blisteres = [];
            return;
        }
        PacientesDelCentro = new ObservableCollection<Paciente>(_servicio.PacientesDe(centro.Id));
        Blisteres = new ObservableCollection<BlisterAlbaranFila>(_servicio.BlisteresParaAlbaran(centro.Id).Select(b => new BlisterAlbaranFila(b)));
    }

    [RelayCommand]
    private void Nuevo()
    {
        Seleccionado = null;
        Nombre = string.Empty; Direccion = null; Telefono = null; PersonaContacto = null;
    }

    [RelayCommand]
    private void Guardar()
    {
        try
        {
            var datos = new DatosCentro(Nombre, Direccion, Telefono, PersonaContacto);
            if (Seleccionado is { } centro)
            {
                _servicio.Actualizar(centro.Id, datos, _usuarioActualId);
                Mensaje = "Centro guardado.";
                Cargar();
            }
            else
            {
                var nuevo = _servicio.Crear(datos, _usuarioActualId);
                Mensaje = "Centro dado de alta.";
                Cargar();
                Seleccionado = Centros.FirstOrDefault(c => c.Id == nuevo.Id);
            }
        }
        catch (ErrorValidacionException ex) { Mensaje = ex.Message; }
    }

    [RelayCommand]
    private void DarDeBaja()
    {
        if (Seleccionado is null) return;
        try
        {
            _servicio.DarDeBaja(Seleccionado.Id, MotivoBaja ?? string.Empty, _usuarioActualId);
            Mensaje = "Centro dado de baja.";
            MotivoBaja = null;
            Seleccionado = null;
            Cargar();
        }
        catch (ErrorValidacionException ex) { Mensaje = ex.Message; }
    }

    [RelayCommand]
    private void AnadirPaciente(Paciente? paciente)
    {
        if (paciente is null || Seleccionado is null) return;
        try
        {
            _servicio.AsignarPaciente(paciente.Id, Seleccionado.Id, _usuarioActualId);
            BusquedaPaciente = string.Empty;
            RecargarDetalle();
        }
        catch (ErrorValidacionException ex) { Mensaje = ex.Message; }
    }

    [RelayCommand]
    private void QuitarPaciente(Paciente? paciente)
    {
        if (paciente is null) return;
        _servicio.AsignarPaciente(paciente.Id, null, _usuarioActualId);
        RecargarDetalle();
    }

    [RelayCommand]
    private void ImprimirAlbaran()
    {
        if (Seleccionado is null || _documentos is null) return;
        try
        {
            var ids = Blisteres.Where(b => b.Incluido).Select(b => b.Blister.SpdId).ToList();
            var resultado = _documentos.GenerarAlbaran(Seleccionado.Id, ids, _usuarioActualId);
            Mensaje = $"Albarán generado: {resultado.RutaCompleta}. Recuerda registrar la entrega de cada blíster.";
        }
        catch (ErrorValidacionException ex) { Mensaje = ex.Message; }
    }
}
