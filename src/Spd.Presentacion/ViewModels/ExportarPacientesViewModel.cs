using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Exportación de pacientes activos a CSV (FR-1130), usando un perfil de tipo
/// `PACIENTES` ya creado (Spec 011, pantalla de perfiles).</summary>
public sealed partial class ExportarPacientesViewModel : ViewModelBase
{
    private readonly IServicioPerfilesImportacion _servicioPerfiles;
    private readonly IServicioExportacionPacientes _servicioExportacion;
    private readonly IServicioPacientes _servicioPacientes;

    [ObservableProperty] private ObservableCollection<PerfilImportacion> _perfilesDisponibles = [];
    [ObservableProperty] private PerfilImportacion? _perfilSeleccionado;
    [ObservableProperty] private string? _csvGenerado;
    [ObservableProperty] private string? _mensaje;

    public ExportarPacientesViewModel(
        IServicioPerfilesImportacion servicioPerfiles, IServicioExportacionPacientes servicioExportacion, IServicioPacientes servicioPacientes)
    {
        _servicioPerfiles = servicioPerfiles;
        _servicioExportacion = servicioExportacion;
        _servicioPacientes = servicioPacientes;
        PerfilesDisponibles = new ObservableCollection<PerfilImportacion>(_servicioPerfiles.ListarPorTipo(TipoPerfilImportacion.Pacientes));
    }

    [RelayCommand]
    private void Exportar()
    {
        if (PerfilSeleccionado is null)
        {
            Mensaje = "Elija un perfil de exportación.";
            return;
        }

        var activos = _servicioPacientes.Buscar(string.Empty, [EstadoPaciente.Activo], null);
        CsvGenerado = _servicioExportacion.ExportarACsv(PerfilSeleccionado, activos);
        Mensaje = $"{activos.Count} pacientes exportados.";
    }
}
