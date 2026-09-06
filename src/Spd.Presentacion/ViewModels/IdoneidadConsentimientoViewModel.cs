using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Idoneidad y consentimiento de un paciente (Spec 002 FR-200..215): evaluación con
/// propuesta de resultado, consentimientos (alta, firma, impresión, revocación) y alta en línea
/// del representante. La app propone; el farmacéutico decide (Art. V.1, II).</summary>
public sealed partial class IdoneidadConsentimientoViewModel : ViewModelBase
{
    private readonly IServicioIdoneidadConsentimiento _servicio;
    private readonly IServicioGeneracionDocumentos _servicioDocumentos;
    private readonly int _pacienteId;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private string _cabecera = string.Empty;
    [ObservableProperty] private string? _mensaje;
    [ObservableProperty] private bool _sugerirSuspension;

    // --- Evaluación (FR-200/201): los nueve ítems recalculan la propuesta.
    [ObservableProperty][NotifyPropertyChangedFor(nameof(ResultadoPropuestoTexto))] private bool _criterio1;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(ResultadoPropuestoTexto))] private bool _criterio2;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(ResultadoPropuestoTexto))] private bool _criterio3;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(ResultadoPropuestoTexto))] private bool _criterio4;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(ResultadoPropuestoTexto))] private bool _criterio5;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(ResultadoPropuestoTexto))] private bool _criterio6;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(ResultadoPropuestoTexto))] private bool _criterio7;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(ResultadoPropuestoTexto))] private bool _condicionMotivacion;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(ResultadoPropuestoTexto))] private bool _condicionDestreza;
    [ObservableProperty] private string? _observacionesEvaluacion;
    [ObservableProperty] private ResultadoIdoneidad _resultadoSeleccionado = ResultadoIdoneidad.Apto;
    [ObservableProperty] private ObservableCollection<EvaluacionFila> _evaluaciones = [];

    public string[] TextosCriterios => EvaluacionIdoneidad.TextosCriterios;
    public string[] TextosCondiciones => EvaluacionIdoneidad.TextosCondiciones;
    public ResultadoIdoneidad[] ResultadosDisponibles { get; } = Enum.GetValues<ResultadoIdoneidad>();

    public string ResultadoPropuestoTexto => $"Propuesta de la aplicación: {(Propuesta() == ResultadoIdoneidad.Apto ? "APTO" : "NO APTO")}";

    // --- Consentimiento (FR-210..215).
    [ObservableProperty][NotifyPropertyChangedFor(nameof(EsRepresentante))] private TipoConsentimiento _tipoSeleccionado = TipoConsentimiento.Paciente;
    [ObservableProperty] private ObservableCollection<Contacto> _representantesElegibles = [];
    [ObservableProperty] private Contacto? _representanteSeleccionado;
    [ObservableProperty] private ObservableCollection<ConsentimientoFila> _consentimientos = [];
    [ObservableProperty] private DateTimeOffset? _fechaFirma = DateTimeOffset.Now;

    [ObservableProperty] private TipoContacto _nuevoTipo = TipoContacto.RepresentanteLegal;
    [ObservableProperty] private string _nuevoNombre = string.Empty;
    [ObservableProperty] private string _nuevoApellidos = string.Empty;
    [ObservableProperty] private string _nuevoDni = string.Empty;
    [ObservableProperty] private string? _nuevoTelefono;
    [ObservableProperty] private string? _nuevoEmail;

    [ObservableProperty] private ConsentimientoFila? _consentimientoEnRevocacion;
    [ObservableProperty] private string _motivoRevocacion = string.Empty;
    [ObservableProperty] private DateTimeOffset? _fechaRevocacion = DateTimeOffset.Now;

    public TipoConsentimiento[] TiposConsentimiento { get; } = Enum.GetValues<TipoConsentimiento>();
    public TipoContacto[] TiposRepresentante { get; } = [TipoContacto.RepresentanteLegal, TipoContacto.PersonaAutorizada];
    public bool EsRepresentante => TipoSeleccionado == TipoConsentimiento.Representante;

    public IdoneidadConsentimientoViewModel(
        IServicioIdoneidadConsentimiento servicio, IServicioGeneracionDocumentos servicioDocumentos, int pacienteId, int? usuarioActualId)
    {
        _servicio = servicio;
        _servicioDocumentos = servicioDocumentos;
        _pacienteId = pacienteId;
        _usuarioActualId = usuarioActualId;
        Cargar();
    }

    private void Cargar()
    {
        var estado = _servicio.Consultar(_pacienteId);
        var p = estado.Paciente;
        Cabecera = $"{p.Nombre} {p.Apellidos} (ficha {p.NumFicha}) — estado {p.Estado}" +
                   (estado.CumpleParaActivo ? " — idoneidad APTO y consentimiento vigente" : " — pendiente de idoneidad y/o consentimiento");
        Evaluaciones = new ObservableCollection<EvaluacionFila>(estado.Evaluaciones.Select(e => new EvaluacionFila(
            e.Fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
            e.Resultado == ResultadoIdoneidad.Apto ? "APTO" : "NO APTO",
            e.Resultado == e.ResultadoPropuesto() ? "" : $"(propuesta: {(e.ResultadoPropuesto() == ResultadoIdoneidad.Apto ? "APTO" : "NO APTO")})",
            $"{e.Criterios.Count(c => c)} criterio(s); motivación {(e.CondicionMotivacion ? "sí" : "no")}, destreza {(e.CondicionDestreza ? "sí" : "no")}",
            e.Observaciones ?? "")));
        Consentimientos = new ObservableCollection<ConsentimientoFila>(estado.Consentimientos.Select(c => new ConsentimientoFila(
            c,
            (c.Tipo == TipoConsentimiento.Paciente ? "Paciente" : "Representante") +
            (c.FechaFirma is null ? " — sin firmar" : $" — firmado {c.FechaFirma:dd/MM/yyyy}") +
            (c.FechaRevocacion is null ? "" : $" — REVOCADO {c.FechaRevocacion:dd/MM/yyyy}: {c.MotivoRevocacion}") +
            (c.Vigente && c.Id == estado.ConsentimientoVigente?.Id ? " — VIGENTE" : "") +
            (c.ImpresoEn is null ? "" : " (impreso)"),
            PuedeFirmar: c.FechaFirma is null,
            PuedeRevocar: c.Vigente)));
        RepresentantesElegibles = new ObservableCollection<Contacto>(estado.RepresentantesElegibles);
        RepresentanteSeleccionado ??= RepresentantesElegibles.FirstOrDefault();
    }

    private ResultadoIdoneidad Propuesta() => new EvaluacionIdoneidad
    {
        PacienteId = _pacienteId, Criterio1 = Criterio1, Criterio2 = Criterio2, Criterio3 = Criterio3, Criterio4 = Criterio4,
        Criterio5 = Criterio5, Criterio6 = Criterio6, Criterio7 = Criterio7,
        CondicionMotivacion = CondicionMotivacion, CondicionDestreza = CondicionDestreza
    }.ResultadoPropuesto();

    [RelayCommand]
    private void AceptarPropuesta() => ResultadoSeleccionado = Propuesta();

    [RelayCommand]
    private void RegistrarEvaluacion()
    {
        try
        {
            var datos = new DatosEvaluacionIdoneidad(
                Criterio1, Criterio2, Criterio3, Criterio4, Criterio5, Criterio6, Criterio7,
                CondicionMotivacion, CondicionDestreza, ObservacionesEvaluacion, ResultadoSeleccionado, _usuarioActualId);
            var resultado = _servicio.RegistrarEvaluacion(_pacienteId, datos, _usuarioActualId);
            Mensaje = "Evaluación registrada." + (resultado.PacienteActivado ? " El paciente pasa a ACTIVO (FR-213)." : "");
            SugerirSuspension = resultado.SugerirSuspension;
            if (SugerirSuspension) Mensaje += " El paciente está ACTIVO con una evaluación NO APTO: puede pasarlo a SUSPENDIDO.";
            ObservacionesEvaluacion = null;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void CrearRepresentante()
    {
        try
        {
            var contacto = _servicio.CrearRepresentante(
                _pacienteId, new DatosContactoRepresentante(NuevoTipo, NuevoNombre, NuevoApellidos, NuevoDni, NuevoTelefono, NuevoEmail), _usuarioActualId);
            NuevoNombre = NuevoApellidos = NuevoDni = string.Empty;
            NuevoTelefono = NuevoEmail = null;
            Cargar();
            RepresentanteSeleccionado = RepresentantesElegibles.FirstOrDefault(c => c.Id == contacto.Id);
            Mensaje = $"Contacto {contacto.Nombre} {contacto.Apellidos} creado y seleccionado como firmante.";
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void CrearConsentimiento()
    {
        try
        {
            _servicio.CrearConsentimiento(_pacienteId, TipoSeleccionado, EsRepresentante ? RepresentanteSeleccionado?.Id : null, _usuarioActualId);
            Mensaje = "Consentimiento creado: imprímalo, recoja la firma y registre la fecha.";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void Imprimir(ConsentimientoFila fila)
    {
        try
        {
            var resultado = _servicioDocumentos.GenerarConsentimiento(fila.Consentimiento.Id, _usuarioActualId);
            Mensaje = $"Consentimiento generado: {resultado.RutaCompleta}";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void RegistrarFirma(ConsentimientoFila fila)
    {
        if (FechaFirma is null) return;
        try
        {
            var resultado = _servicio.RegistrarFirma(fila.Consentimiento.Id, DateOnly.FromDateTime(FechaFirma.Value.Date), _usuarioActualId);
            Mensaje = "Firma registrada." + (resultado.PacienteActivado ? " El paciente pasa a ACTIVO (FR-213)." : "");
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void PrepararRevocacion(ConsentimientoFila fila)
    {
        ConsentimientoEnRevocacion = fila;
        MotivoRevocacion = string.Empty;
        FechaRevocacion = DateTimeOffset.Now;
    }

    [RelayCommand]
    private void ConfirmarRevocacion()
    {
        if (ConsentimientoEnRevocacion is null || FechaRevocacion is null) return;
        try
        {
            var resultado = _servicio.Revocar(
                ConsentimientoEnRevocacion.Consentimiento.Id, DateOnly.FromDateTime(FechaRevocacion.Value.Date), MotivoRevocacion, _usuarioActualId);
            SugerirSuspension = resultado.SugerirSuspension;
            Mensaje = "Consentimiento revocado." + (SugerirSuspension ? " No queda ningún consentimiento vigente: puede pasar al paciente a SUSPENDIDO (FR-214)." : "");
            ConsentimientoEnRevocacion = null;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void Suspender()
    {
        try
        {
            _servicio.Suspender(_pacienteId, _usuarioActualId);
            SugerirSuspension = false;
            Mensaje = "Paciente pasado a SUSPENDIDO.";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    public sealed record EvaluacionFila(string Fecha, string Resultado, string Discrepancia, string Resumen, string Observaciones);

    public sealed record ConsentimientoFila(Consentimiento Consentimiento, string Descripcion, bool PuedeFirmar, bool PuedeRevocar);
}
