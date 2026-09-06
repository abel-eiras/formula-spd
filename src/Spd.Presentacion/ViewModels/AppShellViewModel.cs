using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Navegacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>El marco único (Spec 015 FR-1500..1502): navegación lateral con contadores y una región
/// de contenido que muestra la sección activa. Ninguna sección abre ventana.</summary>
public sealed partial class AppShellViewModel : ViewModelBase
{
    private readonly FabricaViewModels _fabrica;
    private readonly Navegador _navegador;
    private readonly IServicioAvisosInicio _servicioAvisos;
    private readonly IServicioBusquedaGlobal _servicioBusqueda;

    [ObservableProperty] private object? _contenido;
    [ObservableProperty] private string _tituloSeccion = string.Empty;
    [ObservableProperty] private bool _puedeVolver;

    /// <summary>Búsqueda global (FR-1520..1522): un campo en la cabecera que encuentra pacientes,
    /// medicamentos y blísteres sin tener que acertar antes la pantalla.</summary>
    [ObservableProperty] private string _textoBusqueda = string.Empty;
    [ObservableProperty] private ObservableCollection<ResultadoBusqueda> _resultadosBusqueda = [];
    [ObservableProperty] private bool _busquedaAbierta;

    public Usuario UsuarioActual { get; }
    public string Saludo => $"{UsuarioActual.Nombre} {UsuarioActual.Apellidos} · {UsuarioActual.Rol}";
    public bool EsAdministrador => _navegador.EsAdministrador;

    public ObservableCollection<ItemNavegacion> ItemsTrabajo { get; }
    public ObservableCollection<ItemNavegacion> ItemsAdministracion { get; }

    /// <summary>"Cerrar sesión" vuelve al login sin apagar la aplicación (Spec 000).</summary>
    public event Action? CerrarSesionSolicitado;

    public AppShellViewModel(
        FabricaViewModels fabrica,
        Navegador navegador,
        IServicioAvisosInicio servicioAvisos,
        IServicioBusquedaGlobal servicioBusqueda,
        Usuario usuario)
    {
        _fabrica = fabrica;
        _navegador = navegador;
        _servicioAvisos = servicioAvisos;
        _servicioBusqueda = servicioBusqueda;
        UsuarioActual = usuario;

        ItemsTrabajo = new ObservableCollection<ItemNavegacion>(Items(EntradaNavegacion.GrupoTrabajo));
        ItemsAdministracion = new ObservableCollection<ItemNavegacion>(
            navegador.EsAdministrador ? Items(EntradaNavegacion.GrupoAdministracion) : []);

        _navegador.Navegado += AlNavegar;
        _navegador.Navegar(new Destino(Seccion.Inicio));
    }

    private static IEnumerable<ItemNavegacion> Items(string grupo)
        => EntradaNavegacion.Todas.Where(e => e.Grupo == grupo).Select(e => new ItemNavegacion(e));

    private void AlNavegar(Destino destino)
    {
        Contenido = _fabrica.Crear(destino);
        TituloSeccion = EntradaNavegacion.Todas.FirstOrDefault(e => e.Seccion == destino.Seccion)?.Titulo
                        ?? TituloDeSeccionSinEntrada(destino.Seccion);
        PuedeVolver = _navegador.PuedeVolver;
        foreach (var item in ItemsTrabajo.Concat(ItemsAdministracion))
            item.EsActiva = item.Entrada.Seccion == destino.Seccion;
        RecargarContadores();
    }

    private static string TituloDeSeccionSinEntrada(Seccion seccion) => seccion switch
    {
        Seccion.Ayuda => "Ayuda",
        Seccion.RevisionNomenclator => "Revisión del nomenclátor",
        _ => seccion.ToString()
    };

    /// <summary>Los contadores del menú son los mismos avisos del inicio, agrupados por la sección
    /// donde se resuelven (Spec 015 FR-1501).</summary>
    private void RecargarContadores()
    {
        var avisos = _servicioAvisos.Obtener(DateOnly.FromDateTime(DateTime.Today));
        var porSeccion = new Dictionary<Seccion, int>
        {
            [Seccion.Retirada] = avisos.Count(a => a.Tipo == "Faltantes"),
            [Seccion.Preparaciones] = avisos.Count(a => a.Tipo is "Sin entregar" or "Sesión a medias"),
        };
        foreach (var item in ItemsTrabajo)
            item.Contador = porSeccion.TryGetValue(item.Entrada.Seccion, out var n) && n > 0 ? n : null;
    }

    [RelayCommand]
    private void Navegar(ItemNavegacion item) => _navegador.Navegar(new Destino(item.Entrada.Seccion));

    [RelayCommand]
    private void Atras() => _navegador.Atras();

    [RelayCommand]
    private void AbrirAyuda() => _navegador.Navegar(new Destino(Seccion.Ayuda));

    [RelayCommand]
    private void CerrarSesion() => CerrarSesionSolicitado?.Invoke();

    /// <summary>FR-1521: la lista se rehace a cada tecla; el servicio ya devuelve vacío por debajo
    /// del mínimo de caracteres, así que no hace falta condicionarlo aquí.</summary>
    partial void OnTextoBusquedaChanged(string value)
    {
        ResultadosBusqueda = new ObservableCollection<ResultadoBusqueda>(_servicioBusqueda.Buscar(value));
        BusquedaAbierta = ResultadosBusqueda.Count > 0;
    }

    /// <summary>FR-1522: elegir un resultado lleva a su pantalla, ya centrada en él.</summary>
    [RelayCommand]
    private void AbrirResultado(ResultadoBusqueda? resultado)
    {
        if (resultado is null) return;

        var destino = resultado.Tipo switch
        {
            TipoResultadoBusqueda.Paciente => new Destino(Seccion.Pacientes, resultado.PacienteId, resultado.NombrePaciente),
            TipoResultadoBusqueda.Medicamento => new Destino(Seccion.Catalogo, null, resultado.Titulo),
            TipoResultadoBusqueda.Blister => new Destino(Seccion.Preparaciones, resultado.PacienteId, resultado.NombrePaciente),
            _ => new Destino(Seccion.Inicio)
        };

        CerrarBusqueda();
        _navegador.Navegar(destino);
    }

    [RelayCommand]
    private void CerrarBusqueda()
    {
        // Vaciar el texto ya deja la lista vacía (el servicio no devuelve nada por debajo del
        // mínimo de caracteres); cerrar después evita depender del orden de las notificaciones.
        TextoBusqueda = string.Empty;
        BusquedaAbierta = false;
    }

    /// <summary>Una entrada del menú, con su estado visual.</summary>
    public sealed partial class ItemNavegacion(EntradaNavegacion entrada) : ObservableObject
    {
        public EntradaNavegacion Entrada { get; } = entrada;
        public string Titulo => Entrada.Titulo;

        [ObservableProperty] private bool _esActiva;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TieneContador))]
        [NotifyPropertyChangedFor(nameof(ContadorTexto))]
        private int? _contador;

        public bool TieneContador => Contador is > 0;
        public string ContadorTexto => Contador?.ToString() ?? string.Empty;
    }
}
