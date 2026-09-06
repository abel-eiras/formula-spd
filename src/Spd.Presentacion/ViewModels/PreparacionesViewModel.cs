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
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<FilaPreparacion> _filas = [];
    [ObservableProperty] private string? _mensaje;
    [ObservableProperty] private string? _filtroPaciente;
    [ObservableProperty] private EstadoSpd? _filtroEstado;
    [ObservableProperty] private bool _soloPendientes = true;
    [ObservableProperty] private bool _loteFicha = true;
    [ObservableProperty] private bool _loteEtiquetas = true;
    [ObservableProperty] private bool _loteInstrucciones = true;

    public EstadoSpd?[] EstadosDisponibles { get; } = [null, EstadoSpd.Borrador, EstadoSpd.Preparado, EstadoSpd.Verificado, EstadoSpd.Entregado, EstadoSpd.Anulado];

    public PreparacionesViewModel(
        IServicioPreparacion servicioPreparacion, IServicioPacientes servicioPacientes, IServicioUsuarios servicioUsuarios,
        IServicioMedicamentos servicioMedicamentos, IServicioGeneracionDocumentos servicioDocumentos,
        IServicioComunicacionesMedico servicioComunicaciones, IServicioGeneracionLote servicioLote, int? usuarioActualId)
    {
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
    private void AbrirPreparacion(FilaPreparacion fila)
        => new PreparacionWindow(_servicioPreparacion, _servicioMedicamentos, _servicioDocumentos, _servicioComunicaciones, fila.Spd.PacienteId, _usuarioActualId).Show();

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
