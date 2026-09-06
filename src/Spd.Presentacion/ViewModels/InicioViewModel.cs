using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Presentacion.Navegacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Pantalla de inicio (Spec 006 FR-691, Spec 009 FR-950; Spec 015 FR-1510..1512): lo que
/// hay pendiente hoy, con cuatro indicadores, la lista de avisos —cada uno lleva al sitio donde se
/// resuelve— y las acciones que se usan a diario.</summary>
public sealed partial class InicioViewModel : ViewModelBase
{
    private readonly IServicioAvisosInicio _servicioAvisos;
    private readonly Navegador _navegador;

    public InicioViewModel(IServicioAvisosInicio servicioAvisos, Navegador navegador)
    {
        _servicioAvisos = servicioAvisos;
        _navegador = navegador;
        Cargar();
    }

    [ObservableProperty] private ObservableCollection<FilaAviso> _avisos = [];
    [ObservableProperty] private IndicadoresInicio _indicadores = new(0, 0, 0, null);

    public bool HayAvisos => Avisos.Count > 0;

    /// <summary>FR-1510: el indicador ambiental muestra un plazo, no un recuento, y solo aparece
    /// cuando efectivamente hay retraso.</summary>
    public string TextoAmbiental => Indicadores.DiasSinLecturaAmbiental is { } dias
        ? $"{dias} días"
        : "al día";

    public bool AmbientalAtrasado => Indicadores.DiasSinLecturaAmbiental is not null;

    [RelayCommand]
    private void Recargar() => Cargar();

    private void Cargar()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        Indicadores = _servicioAvisos.ObtenerIndicadores(hoy);
        Avisos = new ObservableCollection<FilaAviso>(
            System.Linq.Enumerable.Select(_servicioAvisos.Obtener(hoy), a => new FilaAviso(a)));
        OnPropertyChanged(nameof(HayAvisos));
        OnPropertyChanged(nameof(TextoAmbiental));
        OnPropertyChanged(nameof(AmbientalAtrasado));
    }

    /// <summary>FR-1511: pulsar un aviso lleva a donde se arregla. La traducción de área de
    /// aplicación a sección de la interfaz vive aquí y solo aquí.</summary>
    [RelayCommand]
    private void IrAlAviso(FilaAviso? fila)
    {
        if (fila is null) return;
        _navegador.Navegar(fila.Destino);
    }

    [RelayCommand]
    private void IrA(string? seccion)
    {
        if (Enum.TryParse<Seccion>(seccion, out var destino))
            _navegador.Navegar(new Destino(destino));
    }

    /// <summary>Un aviso listo para pintar: el texto que ya trae el servicio, la severidad con la
    /// que se marca la franja y el destino ya resuelto.</summary>
    public sealed class FilaAviso(AvisoInicio aviso)
    {
        public string Tipo => aviso.Tipo;
        public string Texto => aviso.Texto;

        /// <summary>Ninguno de estos avisos bloquea nada (son informativos, Spec 006 FR-691), así
        /// que la severidad máxima es «aviso»; el ambiental es un recordatorio de calidad.</summary>
        public Controles.Severidad Severidad =>
            aviso.Tipo == "Ambiental" ? Controles.Severidad.Apto : Controles.Severidad.Aviso;

        public Destino Destino => new(
            aviso.Area switch
            {
                AreaAviso.Retirada => Seccion.Retirada,
                AreaAviso.Preparaciones => Seccion.Preparaciones,
                AreaAviso.Calidad => Seccion.Calidad,
                _ => Seccion.Inicio
            },
            aviso.PacienteId,
            aviso.NombrePaciente);
    }
}
