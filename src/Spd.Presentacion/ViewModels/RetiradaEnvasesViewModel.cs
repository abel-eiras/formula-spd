using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Pantalla "Retirada de envases" (FR-530..537): qué medicamentos y cuántos envases
/// retirar por paciente antes de la próxima preparación.</summary>
public sealed partial class RetiradaEnvasesViewModel : ViewModelBase
{
    private readonly IServicioListadoRetirada _servicioListadoRetirada;
    private readonly IServicioEnvases _servicioEnvases;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<FilaListadoRetirada> _filas = [];
    [ObservableProperty] private bool _soloConFaltantes = true;
    [ObservableProperty] private string? _mensaje;

    /// <summary>Spec 015 FR-1511: al llegar desde un aviso de faltantes, el listado se abre ya
    /// centrado en ese paciente. `NombrePacienteFiltrado` es solo para poder decirlo en pantalla y
    /// ofrecer quitar el filtro.</summary>
    [ObservableProperty] private int? _filtroPacienteId;
    [ObservableProperty] private string? _nombrePacienteFiltrado;

    public bool HayFiltroDePaciente => FiltroPacienteId is not null;

    [ObservableProperty] private FilaListadoRetirada? _filaSeleccionadaParaRegistrar;
    [ObservableProperty] private string _serie = string.Empty;
    [ObservableProperty] private string? _lote;
    [ObservableProperty] private DateTimeOffset? _caducidad;
    [ObservableProperty] private int? _unidadesIniciales;

    public RetiradaEnvasesViewModel(IServicioListadoRetirada servicioListadoRetirada, IServicioEnvases servicioEnvases, int? usuarioActualId)
    {
        _servicioListadoRetirada = servicioListadoRetirada;
        _servicioEnvases = servicioEnvases;
        _usuarioActualId = usuarioActualId;
        Recalcular();
    }

    [RelayCommand]
    private void Recalcular()
        => Filas = new ObservableCollection<FilaListadoRetirada>(
            _servicioListadoRetirada.ObtenerListado(
                DateOnly.FromDateTime(DateTime.Today),
                new FiltrosListadoRetirada(SoloConFaltantes, PacienteId: FiltroPacienteId)));

    [RelayCommand]
    private void QuitarFiltroDePaciente()
    {
        FiltroPacienteId = null;
        NombrePacienteFiltrado = null;
        OnPropertyChanged(nameof(HayFiltroDePaciente));
        Recalcular();
    }

    /// <summary>Lo llama la fábrica de ViewModels al llegar desde un aviso (FR-1511).</summary>
    public void CentrarEnPaciente(int pacienteId, string? nombre)
    {
        FiltroPacienteId = pacienteId;
        NombrePacienteFiltrado = nombre;
        OnPropertyChanged(nameof(HayFiltroDePaciente));
        Recalcular();
    }

    [RelayCommand]
    private void PrepararRegistro(FilaListadoRetirada fila) => FilaSeleccionadaParaRegistrar = fila;

    [RelayCommand]
    private void ConfirmarRegistro()
    {
        if (FilaSeleccionadaParaRegistrar is not { } fila || Caducidad is null || UnidadesIniciales is null)
        {
            Mensaje = "Caducidad y unidades iniciales son obligatorias (FR-512).";
            return;
        }

        try
        {
            _servicioEnvases.RegistrarEnvase(
                new DatosAltaEnvase(fila.PacienteId, fila.MedicamentoId, Serie, Lote ?? string.Empty,
                    DateOnly.FromDateTime(Caducidad.Value.Date), UnidadesIniciales.Value, OrigenEnvase.Manual),
                _usuarioActualId);
            Mensaje = "Envase registrado. El listado se ha recalculado.";
            FilaSeleccionadaParaRegistrar = null;
            Serie = string.Empty; Lote = null; Caducidad = null; UnidadesIniciales = null;
            Recalcular();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void Imprimir()
    {
        _servicioListadoRetirada.RegistrarImpresion(_usuarioActualId);
        Mensaje = "Impresión registrada en auditoría (el documento en sí es de Spec 007).";
    }

    partial void OnSoloConFaltantesChanged(bool value) => Recalcular();
}
