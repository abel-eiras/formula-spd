using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Views.Pacientes;

namespace Spd.Presentacion.ViewModels;

/// <summary>Búsqueda global de pacientes y acceso a su ficha (FR-010/FR-011). Por defecto solo
/// muestra activos y en evaluación (FR-011).</summary>
public sealed partial class BuscadorPacientesViewModel : ViewModelBase
{
    private readonly IServicioPacientes _servicio;
    private readonly IServicioTratamientos _servicioTratamientos;
    private readonly IServicioMedicamentos _servicioMedicamentos;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private string _fragmento = string.Empty;
    [ObservableProperty] private bool _mostrarSoloActivosYEvaluacion = true;
    [ObservableProperty] private ObservableCollection<Paciente> _resultados = [];

    public BuscadorPacientesViewModel(
        IServicioPacientes servicio, IServicioTratamientos servicioTratamientos, IServicioMedicamentos servicioMedicamentos,
        int? usuarioActualId)
    {
        _servicio = servicio;
        _servicioTratamientos = servicioTratamientos;
        _servicioMedicamentos = servicioMedicamentos;
        _usuarioActualId = usuarioActualId;
        Buscar();
    }

    [RelayCommand]
    private void Buscar()
    {
        var filtroEstados = MostrarSoloActivosYEvaluacion
            ? new[] { EstadoPaciente.Activo, EstadoPaciente.Evaluacion }
            : null;
        Resultados = new ObservableCollection<Paciente>(_servicio.Buscar(Fragmento, filtroEstados, null));
    }

    [RelayCommand]
    private void AbrirPaciente(Paciente paciente)
        => new FichaPacienteWindow(_servicio, _servicioTratamientos, _servicioMedicamentos, paciente, _usuarioActualId).Show();

    [RelayCommand]
    private void NuevoPaciente()
        => new FichaPacienteWindow(_servicio, _servicioTratamientos, _servicioMedicamentos, null, _usuarioActualId).Show();

    partial void OnFragmentoChanged(string value) => Buscar();

    partial void OnMostrarSoloActivosYEvaluacionChanged(bool value) => Buscar();
}
