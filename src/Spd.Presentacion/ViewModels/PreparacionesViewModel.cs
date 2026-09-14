using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Views.Preparacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Pantalla "Preparaciones" global (Spec 006 FR-690): todos los blísteres cruzando
/// pacientes, con filtro por estado, paciente y elaborador, la columna "envases al día" (Spec 007
/// FR-720) y la generación en lote (FR-721..724).</summary>
public sealed partial class PreparacionesViewModel : ViewModelBase
{
    private readonly IServicioPreparacion _servicioPreparacion;
    private readonly IServicioPacientes _servicioPacientes;
    private readonly IServicioUsuarios _servicioUsuarios;
    private readonly IServicioMedicamentos _servicioMedicamentos;
    private readonly IServicioGeneracionDocumentos _servicioDocumentos;
    private readonly IServicioComunicacionesMedico _servicioComunicaciones;
    private readonly IServicioGeneracionLote _servicioLote;
    private readonly Navegacion.Navegador _navegador;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<FilaPreparacion> _filas = [];
    [ObservableProperty] private string? _mensaje;
    [ObservableProperty] private string? _filtroPaciente;
    [ObservableProperty] private EstadoSpd? _filtroEstado;
    [ObservableProperty] private bool _soloPendientes = true;
    [ObservableProperty] private bool _loteFicha = true;
    [ObservableProperty] private bool _loteEtiquetas = true;
    [ObservableProperty] private bool _loteInstrucciones = true;

    /// <summary>«Nueva preparación» desde la lista (petición del propietario, 2026-09-14): buscar al paciente
    /// y abrir su sesión sin pasar antes por la ficha.</summary>
    [ObservableProperty] private bool _panelNuevaAbierto;
    [ObservableProperty] private string _busquedaPaciente = string.Empty;
    [ObservableProperty] private ObservableCollection<CandidatoPreparacion> _candidatos = [];
    [ObservableProperty] private string? _mensajeNueva;

    public EstadoSpd?[] EstadosDisponibles { get; } = [null, EstadoSpd.Borrador, EstadoSpd.Preparado, EstadoSpd.Verificado, EstadoSpd.Entregado, EstadoSpd.Anulado];

    public PreparacionesViewModel(
        IServicioPreparacion servicioPreparacion, IServicioPacientes servicioPacientes, IServicioUsuarios servicioUsuarios,
        IServicioMedicamentos servicioMedicamentos, IServicioGeneracionDocumentos servicioDocumentos,
        IServicioComunicacionesMedico servicioComunicaciones, IServicioGeneracionLote servicioLote,
        Navegacion.Navegador navegador, int? usuarioActualId)
    {
        _navegador = navegador;
        _servicioPreparacion = servicioPreparacion;
        _servicioPacientes = servicioPacientes;
        _servicioUsuarios = servicioUsuarios;
        _servicioMedicamentos = servicioMedicamentos;
        _servicioDocumentos = servicioDocumentos;
        _servicioComunicaciones = servicioComunicaciones;
        _servicioLote = servicioLote;
        _usuarioActualId = usuarioActualId;
        Cargar();
    }

    [RelayCommand]
    private void Cargar()
    {
        var usuarios = _servicioUsuarios.ListarActivos().ToDictionary(u => u.Id, u => $"{u.Nombre} {u.Apellidos}");
        var pacientes = new Dictionary<int, Paciente?>();
        var alDia = new Dictionary<int, ResultadoEnvasesAlDia>();
        var filtro = Normalizador.QuitarTildesYMayusculas(FiltroPaciente ?? string.Empty);

        var spds = _servicioPreparacion.ListarPorFiltro(new FiltrosPreparaciones(FiltroEstado))
            .Where(s => !SoloPendientes || s.Estado is EstadoSpd.Borrador or EstadoSpd.Preparado or EstadoSpd.Verificado)
            .OrderByDescending(s => s.Id);

        var filas = new List<FilaPreparacion>();
        foreach (var spd in spds)
        {
            if (!pacientes.TryGetValue(spd.PacienteId, out var paciente))
            {
                paciente = _servicioPacientes.ObtenerPorId(spd.PacienteId);
                pacientes[spd.PacienteId] = paciente;
                alDia[spd.PacienteId] = _servicioPreparacion.ComprobarEnvasesAlDia(spd.PacienteId);
            }
            var nombre = paciente is null ? $"paciente {spd.PacienteId}" : $"{paciente.Apellidos}, {paciente.Nombre}";
            if (filtro.Length > 0 && !Normalizador.QuitarTildesYMayusculas(nombre).Contains(filtro)) continue;
            var comprobacion = alDia[spd.PacienteId];
            filas.Add(new FilaPreparacion(
                spd, nombre, paciente?.NumFicha ?? "", usuarios.GetValueOrDefault(spd.ElaboradorId, ""),
                spd.VerificadorId is int v ? usuarios.GetValueOrDefault(v, "") : "",
                comprobacion.AlDia, comprobacion.AlDia ? "SÍ" : $"NO — {comprobacion.Motivo}"));
        }
        Filas = new ObservableCollection<FilaPreparacion>(filas);
    }

    partial void OnFiltroPacienteChanged(string? value) => Cargar();
    partial void OnFiltroEstadoChanged(EstadoSpd? value) => Cargar();
    partial void OnSoloPendientesChanged(bool value) => Cargar();

    [RelayCommand]
    private void AbrirNuevaPreparacion()
    {
        BusquedaPaciente = string.Empty;
        Candidatos = [];
        MensajeNueva = null;
        PanelNuevaAbierto = true;
    }

    [RelayCommand]
    private void CerrarNuevaPreparacion() => PanelNuevaAbierto = false;

    partial void OnBusquedaPacienteChanged(string value)
    {
        MensajeNueva = null;
        var texto = value?.Trim() ?? string.Empty;
        if (texto.Length < 2)
        {
            Candidatos = [];
            return;
        }

        Candidatos = new ObservableCollection<CandidatoPreparacion>(
            _servicioPacientes.Buscar(texto, null, null)
                .Take(20)
                .Select(p => new CandidatoPreparacion(p,
                    _servicioPreparacion.ListarPorFiltro(new FiltrosPreparaciones(PacienteId: p.Id))
                        .OrderByDescending(s => s.Id)
                        .FirstOrDefault())));
    }

    /// <summary>Continuidad (FR-6120): parte de la última preparación entregada.</summary>
    [RelayCommand]
    private void PrepararSiguientePara(CandidatoPreparacion? candidato) => AbrirSesion(candidato, siguiente: true);

    [RelayCommand]
    private void CrearSesionPara(CandidatoPreparacion? candidato) => AbrirSesion(candidato, siguiente: false);

    /// <summary>Si la sesión no se puede abrir (paciente no activo, sin consentimiento, sin tratamientos…),
    /// el motivo se queda en el panel y no se navega a ningún sitio.</summary>
    private void AbrirSesion(CandidatoPreparacion? candidato, bool siguiente)
    {
        if (candidato is null) return;

        try
        {
            if (siguiente)
                _servicioPreparacion.PrepararSiguiente(candidato.Paciente.Id, _usuarioActualId ?? 0, out _);
            else
                _servicioPreparacion.CrearSesion(candidato.Paciente.Id, _usuarioActualId ?? 0);

            PanelNuevaAbierto = false;
            _navegador.Navegar(new Navegacion.Destino(
                Navegacion.Seccion.Paciente, candidato.Paciente.Id, nameof(Pacientes.PestanaPaciente.Preparacion)));
        }
        catch (ErrorValidacionException ex)
        {
            MensajeNueva = ex.Message;
        }
    }

    [RelayCommand]
    private void AbrirPreparacion(FilaPreparacion fila)
        // Spec 015 FR-1530: la preparación es una pestaña del espacio del paciente, no una ventana.
        => _navegador.Navegar(new Navegacion.Destino(
            Navegacion.Seccion.Paciente, fila.Spd.PacienteId, nameof(Pacientes.PestanaPaciente.Preparacion)));

    [RelayCommand]
    private void SeleccionarAlDia()
    {
        foreach (var f in Filas) f.Seleccionado = f.EnvasesAlDia;
    }

    /// <summary>FR-722..724: un paciente por sesión; los excluidos no detienen al resto.</summary>
    [RelayCommand]
    private void GenerarLote()
    {
        var pacienteIds = Filas.Where(f => f.Seleccionado).Select(f => f.Spd.PacienteId).Distinct().ToList();
        if (pacienteIds.Count == 0) { Mensaje = "Seleccione al menos un paciente."; return; }
        try
        {
            var resultado = _servicioLote.Generar(pacienteIds, new TiposDocumentoLote(LoteFicha, LoteEtiquetas, LoteInstrucciones), _usuarioActualId ?? 0);
            var lineas = new List<string>
            {
                $"Generados: {resultado.Generados.Count} paciente(s), {resultado.Generados.Sum(g => g.Ficheros.Count)} fichero(s)."
            };
            lineas.AddRange(resultado.Generados.SelectMany(g => g.Avisos));
            lineas.AddRange(resultado.Excluidos.Select(e => $"Excluido {e.Paciente}: {e.Motivo}"));
            lineas.AddRange(resultado.Fallidos.Select(e => $"FALLÓ {e.Paciente}: {e.Motivo}"));
            Mensaje = string.Join("\n", lineas);
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    public sealed record CandidatoPreparacion(Paciente Paciente, SPD? Ultima)
    {
        public string Etiqueta => $"{Paciente.Apellidos}, {Paciente.Nombre}";

        public string Detalle => string.Join(" · ", new[]
        {
            string.IsNullOrEmpty(Paciente.NumFicha) ? null : $"Ficha {Paciente.NumFicha}",
            string.IsNullOrEmpty(Paciente.Cip) ? null : $"CIP {Paciente.Cip}",
            Paciente.Estado.ToString()
        }.Where(t => t is not null));

        public string UltimaTexto => Ultima is null
            ? "Sin preparaciones anteriores"
            : $"Última: {Ultima.NumRegistro} v{Ultima.Version} — {Ultima.Estado}, validez {Ultima.ValidezDesde:dd/MM} a {Ultima.ValidezHasta:dd/MM}";

        public bool PuedePrepararSiguiente => Ultima?.Estado == EstadoSpd.Entregado;
    }

    public sealed partial class FilaPreparacion(SPD spd, string paciente, string numFicha, string elaborador, string verificador, bool envasesAlDia, string envasesAlDiaTexto)
        : ObservableObject
    {
        public SPD Spd { get; } = spd;
        public string Paciente { get; } = paciente;
        public string NumFicha { get; } = numFicha;
        public string Elaborador { get; } = elaborador;
        public string Verificador { get; } = verificador;
        public bool EnvasesAlDia { get; } = envasesAlDia;
        public string EnvasesAlDiaTexto { get; } = envasesAlDiaTexto;
        public string Resumen => $"{Spd.NumRegistro} v{Spd.Version} — {Spd.Estado} — validez {Spd.ValidezDesde:dd/MM} a {Spd.ValidezHasta:dd/MM}" +
                                 (string.IsNullOrEmpty(Elaborador) ? "" : $" — elab. {Elaborador}") + (string.IsNullOrEmpty(Verificador) ? "" : $" — verif. {Verificador}");

        [ObservableProperty] private bool _seleccionado;
    }
}
