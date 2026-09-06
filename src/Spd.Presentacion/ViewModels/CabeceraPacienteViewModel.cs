using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Controles;
using Spd.Presentacion.Pacientes;

namespace Spd.Presentacion.ViewModels;

/// <summary>Cabecera fija del espacio del paciente (Spec 015 FR-1531). Lo que hay que tener delante
/// **siempre**, sin cambiar de pestaña ni abrir nada: quién es, en qué estado está, si tiene
/// alergias, si la idoneidad y el consentimiento están en regla, cuándo le toca retirar y cuántos
/// blísteres lleva.
///
/// Es lo que antes obligaba a recordar de memoria o a ir y volver entre ventanas, y es justo el dato
/// cuya ausencia en pantalla puede acabar en un blíster preparado para quien no debía.</summary>
public sealed partial class CabeceraPacienteViewModel : ViewModelBase
{
    private readonly PacienteContexto _contexto;
    private readonly IServicioIdoneidadConsentimiento _servicioIdoneidad;
    private readonly IServicioPreparacion _servicioPreparacion;

    [ObservableProperty] private string _nombreCompleto = string.Empty;
    [ObservableProperty] private string _numFicha = "(sin asignar todavía)";
    [ObservableProperty] private string _estado = EstadoPaciente.Evaluacion.ToString();
    [ObservableProperty] private string? _alergias;
    [ObservableProperty] private string _idoneidad = "—";
    [ObservableProperty] private string _diaRetirada = "—";
    [ObservableProperty] private int _blisteres;

    public bool TieneAlergias => !string.IsNullOrWhiteSpace(Alergias);
    public bool HayPaciente => _contexto.Paciente is not null;

    /// <summary>El color es información: un paciente que no está activo o que no cumple idoneidad y
    /// consentimiento se marca en rojo porque no se le puede preparar (Spec 002 FR-213).</summary>
    public VariantePastilla VarianteEstado => _contexto.Paciente?.Estado switch
    {
        EstadoPaciente.Activo => VariantePastilla.Apto,
        EstadoPaciente.Evaluacion => VariantePastilla.Aviso,
        null => VariantePastilla.Neutra,
        _ => VariantePastilla.Bloqueo
    };

    public VariantePastilla VarianteIdoneidad => Idoneidad switch
    {
        "En regla" => VariantePastilla.Apto,
        "—" => VariantePastilla.Neutra,
        _ => VariantePastilla.Bloqueo
    };

    public CabeceraPacienteViewModel(
        PacienteContexto contexto,
        IServicioIdoneidadConsentimiento servicioIdoneidad,
        IServicioPreparacion servicioPreparacion)
    {
        _contexto = contexto;
        _servicioIdoneidad = servicioIdoneidad;
        _servicioPreparacion = servicioPreparacion;
        _contexto.Cambiado += _ => Refrescar();
        Refrescar();
    }

    public void Refrescar()
    {
        var paciente = _contexto.Paciente;
        if (paciente is null)
        {
            NombreCompleto = "Paciente nuevo";
            NumFicha = "(sin asignar todavía)";
            Estado = EstadoPaciente.Evaluacion.ToString();
            Alergias = null;
            Idoneidad = "—";
            DiaRetirada = "—";
            Blisteres = 0;
        }
        else
        {
            NombreCompleto = $"{paciente.Nombre} {paciente.Apellidos}";
            NumFicha = paciente.NumFicha;
            Estado = paciente.Estado.ToString();
            Alergias = paciente.Alergias;
            DiaRetirada = paciente.DiaRetirada ?? "sin día fijado";
            Blisteres = paciente.NBlisteres;

            var estado = _servicioIdoneidad.Consultar(paciente.Id);
            Idoneidad = estado.CumpleParaActivo
                ? "En regla"
                : estado.EvaluacionVigente is null ? "Sin evaluación"
                : estado.ConsentimientoVigente is null ? "Sin consentimiento"
                : "No apto";
        }

        NotificarDerivadas();
    }

    /// <summary>Cuántos blísteres del paciente están preparados o verificados y todavía no
    /// entregados: el dato que dice si hay trabajo a medias con este paciente.</summary>
    public int PendientesDeEntrega => _contexto.PacienteId is not { } id
        ? 0
        : ContarPendientes(id);

    private int ContarPendientes(int pacienteId)
    {
        var todos = _servicioPreparacion.ListarPorFiltro(new FiltrosPreparaciones(PacienteId: pacienteId));
        var n = 0;
        foreach (var spd in todos)
            if (spd.Estado is EstadoSpd.Preparado or EstadoSpd.Verificado) n++;
        return n;
    }

    private void NotificarDerivadas()
    {
        OnPropertyChanged(nameof(TieneAlergias));
        OnPropertyChanged(nameof(HayPaciente));
        OnPropertyChanged(nameof(VarianteEstado));
        OnPropertyChanged(nameof(VarianteIdoneidad));
        OnPropertyChanged(nameof(PendientesDeEntrega));
    }
}
