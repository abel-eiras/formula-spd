using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Presentacion.Pacientes;

namespace Spd.Presentacion.ViewModels;

/// <summary>Pestaña Documentos del espacio del paciente (Spec 015 FR-1530). Reúne los documentos que
/// son **del paciente** y no de un acto concreto: su ficha (FR-700 `FICHA-PAC`) y la información de
/// protección de datos (Anexo I.D del PNT).
///
/// Deliberadamente **no** trae aquí el consentimiento ni la ficha de preparación: esos se generan
/// donde se firman y donde se elabora, y separarlos del acto que los justifica sería invitar a
/// imprimir un consentimiento que nadie ha firmado.</summary>
public sealed partial class DocumentosPacienteViewModel : ViewModelBase
{
    private readonly PacienteContexto _contexto;
    private readonly IServicioGeneracionDocumentos _servicioDocumentos;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private string? _mensaje;

    public DocumentosPacienteViewModel(
        PacienteContexto contexto, IServicioGeneracionDocumentos servicioDocumentos, int? usuarioActualId)
    {
        _contexto = contexto;
        _servicioDocumentos = servicioDocumentos;
        _usuarioActualId = usuarioActualId;
        _contexto.Cambiado += _ => OnPropertyChanged(nameof(PuedeGenerar));
    }

    /// <summary>Sin paciente guardado no hay nada que imprimir (FR-700 referencia `paciente_id`).</summary>
    public bool PuedeGenerar => _contexto.PacienteId is not null;

    [RelayCommand]
    private void ImprimirFicha()
    {
        if (_contexto.PacienteId is not { } id) return;
        var resultado = _servicioDocumentos.GenerarFichaPaciente(id, _usuarioActualId);
        Mensaje = $"Ficha del paciente generada: {resultado.RutaCompleta}";
    }

    /// <summary>Anexo I.D del PNT: se entrega al paciente junto con el consentimiento informado.</summary>
    [RelayCommand]
    private void ImprimirProteccionDatos()
    {
        if (_contexto.PacienteId is not { } id) return;
        var resultado = _servicioDocumentos.GenerarInformacionProteccionDatos(id, _usuarioActualId);
        Mensaje = $"Información de protección de datos generada: {resultado.RutaCompleta}";
    }
}
