using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Navegacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Búsqueda de pacientes y acceso a su espacio (FR-010/FR-011). Por defecto solo muestra
/// activos y en evaluación (FR-011). Spec 015: abrir un paciente ya no abre una ventana, navega a su
/// espacio dentro del marco, y por eso este ViewModel deja de necesitar los ocho servicios que solo
/// servían para construir aquella ventana.</summary>
public sealed partial class BuscadorPacientesViewModel : ViewModelBase
{
    private readonly IServicioPacientes _servicio;
    private readonly Navegador _navegador;

    [ObservableProperty] private string _fragmento = string.Empty;
    [ObservableProperty] private bool _mostrarSoloActivosYEvaluacion = true;
    [ObservableProperty] private ObservableCollection<Paciente> _resultados = [];

    public BuscadorPacientesViewModel(IServicioPacientes servicio, Navegador navegador)
    {
        _servicio = servicio;
        _navegador = navegador;
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
        => _navegador.Navegar(new Destino(Seccion.Paciente, paciente.Id));

    /// <summary>Sin `PacienteId`: el espacio se abre en alta nueva, solo con la pestaña de datos
    /// hasta que se guarde por primera vez (FR-1530).</summary>
    [RelayCommand]
    private void NuevoPaciente() => _navegador.Navegar(new Destino(Seccion.Paciente));

    partial void OnFragmentoChanged(string value) => Buscar();

    partial void OnMostrarSoloActivosYEvaluacionChanged(bool value) => Buscar();
}
