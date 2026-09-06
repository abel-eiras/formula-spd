using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Comunicaciones al médico de un paciente (FR-800..807): alta, listado y registro de
/// respuesta. `prerrelleno` (FR-804/805) fija médico y deja incidencias en blanco para redactar,
/// sin guardar nada hasta que el usuario pulse "Guardar" (FR-807: no hay borradores).</summary>
public sealed partial class ComunicacionesMedicoViewModel : ViewModelBase
{
    private readonly IServicioComunicacionesMedico _servicio;
    private readonly IServicioGeneracionDocumentos _servicioDocumentos;
    private readonly int _pacienteId;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<ComunicacionMedico> _comunicaciones = [];
    [ObservableProperty] private string? _mensaje;

    [ObservableProperty] private int? _medicoId;
    [ObservableProperty] private TipoComunicacionMedico _tipo = TipoComunicacionMedico.Presentacion;
    [ObservableProperty] private string? _incidenciasDetectadas;
    [ObservableProperty] private string? _propuesta;

    [ObservableProperty] private ComunicacionMedico? _comunicacionSeleccionadaParaRespuesta;
    [ObservableProperty] private string _respuestaTexto = string.Empty;

    [ObservableProperty] private System.Collections.ObjectModel.ObservableCollection<Spd.Dominio.Medico> _medicosDisponibles = [];
    [ObservableProperty] private Spd.Dominio.Medico? _medicoSeleccionado;

    public TipoComunicacionMedico[] TiposDisponibles { get; } = Enum.GetValues<TipoComunicacionMedico>();

    public ComunicacionesMedicoViewModel(
        IServicioComunicacionesMedico servicio, IServicioGeneracionDocumentos servicioDocumentos, int pacienteId, int? usuarioActualId,
        DatosAltaComunicacionMedico? prerrelleno = null, IServicioMedicos? servicioMedicos = null)
    {
        // FR-1542: el médico se elige por nombre, no tecleando su id.
        MedicosDisponibles = new System.Collections.ObjectModel.ObservableCollection<Spd.Dominio.Medico>(
            servicioMedicos?.ListarActivos() ?? []);
        _servicio = servicio;
        _servicioDocumentos = servicioDocumentos;
        _pacienteId = pacienteId;
        _usuarioActualId = usuarioActualId;
        if (prerrelleno is not null) Prerrellenar(prerrelleno);
        Cargar();
    }

    /// <summary>Spec 015 FR-1530: llegando desde la pestaña de tratamiento o de preparación, el
    /// formulario aparece ya relleno con lo que hay que consultarle al médico. Antes esto se pasaba
    /// al constructor de una ventana nueva; ahora la pestaña ya existe y hay que poder rellenarla
    /// en cualquier momento (Spec 008 FR-810).</summary>
    public void Prerrellenar(DatosAltaComunicacionMedico prerrelleno)
    {
        MedicoId = prerrelleno.MedicoId;
        Tipo = prerrelleno.Tipo;
        IncidenciasDetectadas = prerrelleno.IncidenciasDetectadas;
        Propuesta = prerrelleno.Propuesta;
    }

    private void Cargar() => Comunicaciones = new ObservableCollection<ComunicacionMedico>(_servicio.ListarDePaciente(_pacienteId));

    [RelayCommand]
    private void Guardar()
    {
        try
        {
            _servicio.Crear(new DatosAltaComunicacionMedico(_pacienteId, MedicoId, Tipo, IncidenciasDetectadas, Propuesta), _usuarioActualId);
            Mensaje = "Comunicación guardada.";
            MedicoId = null; IncidenciasDetectadas = null; Propuesta = null;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void PrepararRespuesta(ComunicacionMedico comunicacion) => ComunicacionSeleccionadaParaRespuesta = comunicacion;

    /// <summary>FR-806 (Spec 007 `CARTA-PRES`/`CARTA-INC`, Anexo I.C del PNT I).</summary>
    [RelayCommand]
    private void ImprimirCarta(ComunicacionMedico comunicacion)
    {
        try
        {
            var resultado = _servicioDocumentos.GenerarCartaMedico(comunicacion.Id, _usuarioActualId);
            Mensaje = $"Carta generada: {resultado.RutaCompleta}";
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void ConfirmarRespuesta()
    {
        if (ComunicacionSeleccionadaParaRespuesta is null) return;
        _servicio.RegistrarRespuesta(ComunicacionSeleccionadaParaRespuesta.Id, RespuestaTexto, DateOnly.FromDateTime(DateTime.Today), _usuarioActualId);
        Mensaje = "Respuesta registrada.";
        ComunicacionSeleccionadaParaRespuesta = null;
        RespuestaTexto = string.Empty;
        Cargar();
    }

    /// <summary>FR-1542: la lista de médicos y el `MedicoId` que se guarda se mantienen en sintonía
    /// en los dos sentidos; el número deja de teclearse y por tanto deja de poder equivocarse.</summary>
    partial void OnMedicoSeleccionadoChanged(Spd.Dominio.Medico? value) => MedicoId = value?.Id;

    partial void OnMedicoIdChanged(int? value)
        => MedicoSeleccionado = value is null
            ? null
            : System.Linq.Enumerable.FirstOrDefault(MedicosDisponibles, m => m.Id == value);
}
