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

    public TipoComunicacionMedico[] TiposDisponibles { get; } = Enum.GetValues<TipoComunicacionMedico>();

    public ComunicacionesMedicoViewModel(
        IServicioComunicacionesMedico servicio, int pacienteId, int? usuarioActualId, DatosAltaComunicacionMedico? prerrelleno = null)
    {
        _servicio = servicio;
        _pacienteId = pacienteId;
        _usuarioActualId = usuarioActualId;
        if (prerrelleno is not null)
        {
            MedicoId = prerrelleno.MedicoId;
            Tipo = prerrelleno.Tipo;
            IncidenciasDetectadas = prerrelleno.IncidenciasDetectadas;
            Propuesta = prerrelleno.Propuesta;
        }
        Cargar();
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
}
