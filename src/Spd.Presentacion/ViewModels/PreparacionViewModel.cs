using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Sesión de preparación de un paciente: crear, llenar, verificar, entregar,
/// continuidad y reelaboración (FR-600..695, FR-6120..6127). Un blíster por fila, aunque la
/// sesión pueda tener dos (FR-603/604).</summary>
public sealed partial class PreparacionViewModel : ViewModelBase
{
    private readonly IServicioPreparacion _servicio;
    private readonly IServicioMedicamentos _servicioMedicamentos;
    private readonly IServicioGeneracionDocumentos _servicioGeneracionDocumentos;
    private readonly int _pacienteId;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<BlisterFila> _blisteres = [];
    [ObservableProperty] private string? _mensaje;

    [ObservableProperty] private double? _temperatura;
    [ObservableProperty] private double? _humedad;
    [ObservableProperty] private string _nuevoMaterialDescripcion = string.Empty;
    [ObservableProperty] private string _nuevoMaterialLote = string.Empty;
    [ObservableProperty] private ObservableCollection<MaterialAcondicionamiento> _materialesDisponibles = [];
    [ObservableProperty] private MaterialAcondicionamiento? _materialSeleccionado;

    [ObservableProperty] private BlisterFila? _blisterEnVerificacion;
    [ObservableProperty] private bool _checkAspecto;
    [ObservableProperty] private bool _checkEtiquetaDatos;
    [ObservableProperty] private bool _checkEtiquetaValidez;
    [ObservableProperty] private bool _checkInstrucciones;
    [ObservableProperty] private bool _checkContenido;
    [ObservableProperty] private int? _verificadorId;
    [ObservableProperty] private string? _excepcionMotivo;

    [ObservableProperty] private string _entregadoA = string.Empty;

    [ObservableProperty] private BlisterFila? _blisterEnReelaboracion;
    [ObservableProperty] private OrigenSolicitudReelaboracion _origenReelaboracion = OrigenSolicitudReelaboracion.Paciente;
    [ObservableProperty] private string _motivoReelaboracion = string.Empty;

    [ObservableProperty] private SpdLinea? _lineaParaRegistrarEnvase;
    [ObservableProperty] private string _envaseSerie = string.Empty;
    [ObservableProperty] private string _envaseLote = string.Empty;
    [ObservableProperty] private DateTimeOffset? _envaseCaducidad;
    [ObservableProperty] private int? _envaseUnidadesIniciales;

    public OrigenSolicitudReelaboracion[] OrigenesDisponibles { get; } = Enum.GetValues<OrigenSolicitudReelaboracion>();

    public PreparacionViewModel(
        IServicioPreparacion servicio, IServicioMedicamentos servicioMedicamentos,
        IServicioGeneracionDocumentos servicioGeneracionDocumentos, int pacienteId, int? usuarioActualId)
    {
        _servicio = servicio;
        _servicioMedicamentos = servicioMedicamentos;
        _servicioGeneracionDocumentos = servicioGeneracionDocumentos;
        _pacienteId = pacienteId;
        _usuarioActualId = usuarioActualId;
        Cargar();
    }

    private void Cargar()
    {
        var spds = _servicio.ListarPorFiltro(new FiltrosPreparaciones(PacienteId: _pacienteId));
        Blisteres = new ObservableCollection<BlisterFila>(
            spds.OrderByDescending(s => s.Id).Select(s => new BlisterFila(s, _servicio.ListarLineas(s.Id))));
        MaterialesDisponibles = new ObservableCollection<MaterialAcondicionamiento>(_servicio.ListarMaterialesActivos());
    }

    [RelayCommand]
    private void NuevaSesion()
    {
        try
        {
            _servicio.CrearSesion(_pacienteId, _usuarioActualId ?? 0);
            Mensaje = "Sesión de preparación creada.";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void PrepararSiguiente()
    {
        try
        {
            _servicio.PrepararSiguiente(_pacienteId, _usuarioActualId ?? 0, out var resultado);
            Mensaje = $"Sesión siguiente creada. Modificadas: {resultado.LineasModificadas.Count}, " +
                      $"nuevas: {resultado.TratamientosNuevos.Count}, eliminadas: {resultado.LineasEliminadas.Count}, " +
                      $"envase pendiente: {resultado.LineasEnvasePendiente.Count}.";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void CrearMaterial()
    {
        if (string.IsNullOrWhiteSpace(NuevoMaterialDescripcion) || string.IsNullOrWhiteSpace(NuevoMaterialLote)) return;
        _servicio.CrearMaterial(NuevoMaterialDescripcion, NuevoMaterialLote, DateOnly.FromDateTime(DateTime.Today));
        NuevoMaterialDescripcion = string.Empty;
        NuevoMaterialLote = string.Empty;
        MaterialesDisponibles = new ObservableCollection<MaterialAcondicionamiento>(_servicio.ListarMaterialesActivos());
    }

    [RelayCommand]
    private void PasarAPreparado(BlisterFila fila)
    {
        try
        {
            if (MaterialSeleccionado is not null) _servicio.AsignarMaterial(fila.Spd.Id, MaterialSeleccionado.Id);
            var lectura = _servicio.ObtenerOCrearLecturaAmbiental(Temperatura, Humedad, _usuarioActualId);
            _servicio.PasarAPreparado(fila.Spd.Id, lectura.Id, _usuarioActualId);
            Mensaje = "Blíster pasado a PREPARADO.";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void PrepararVerificacion(BlisterFila fila)
    {
        BlisterEnVerificacion = fila;
        CheckAspecto = CheckEtiquetaDatos = CheckEtiquetaValidez = CheckInstrucciones = CheckContenido = false;
        ExcepcionMotivo = null;
    }

    [RelayCommand]
    private void ConfirmarVerificacion()
    {
        if (BlisterEnVerificacion is null || VerificadorId is null) return;
        try
        {
            var checklist = new ChecklistVerificacion(CheckAspecto, CheckEtiquetaDatos, CheckEtiquetaValidez, CheckInstrucciones, CheckContenido);
            _servicio.Verificar(BlisterEnVerificacion.Spd.Id, VerificadorId.Value, checklist, ExcepcionMotivo, _usuarioActualId);
            Mensaje = "Verificación registrada.";
            BlisterEnVerificacion = null;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void EntregarTodosLosPendientes()
    {
        var pendientes = Blisteres.Where(b => b.Spd.Estado == EstadoSpd.Verificado).Select(b => b.Spd.Id).ToList();
        if (pendientes.Count == 0)
        {
            Mensaje = "No hay blísteres VERIFICADOS pendientes de entregar.";
            return;
        }
        try
        {
            var datos = new DatosEntregaSpd(DateOnly.FromDateTime(DateTime.Today), EntregadoA, true, null, null, null, false, null);
            _servicio.RegistrarEntrega(datos, pendientes, _usuarioActualId);
            Mensaje = $"{pendientes.Count} blíster(es) entregado(s).";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void PrepararReelaboracion(BlisterFila fila)
    {
        BlisterEnReelaboracion = fila;
        MotivoReelaboracion = string.Empty;
    }

    [RelayCommand]
    private void ConfirmarReelaboracion()
    {
        if (BlisterEnReelaboracion is null || _usuarioActualId is null) return;
        try
        {
            _servicio.Reelaborar(BlisterEnReelaboracion.Spd.Id, OrigenReelaboracion, MotivoReelaboracion, _usuarioActualId.Value);
            Mensaje = "Blíster reelaborado: nueva versión en PREPARADO, pendiente de verificar de nuevo.";
            BlisterEnReelaboracion = null;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void PrepararRegistroEnvase(SpdLinea linea)
    {
        LineaParaRegistrarEnvase = linea;
        EnvaseSerie = string.Empty;
        EnvaseLote = string.Empty;
        EnvaseCaducidad = null;
        EnvaseUnidadesIniciales = null;
    }

    [RelayCommand]
    private void ConfirmarRegistroEnvase()
    {
        if (LineaParaRegistrarEnvase is null || EnvaseCaducidad is null || EnvaseUnidadesIniciales is null) return;
        try
        {
            var medicamento = _servicioMedicamentos.ObtenerPorId(LineaParaRegistrarEnvase.MedicamentoId);
            if (medicamento is null) return;

            var pacienteId = Blisteres.First(b => b.Lineas.Contains(LineaParaRegistrarEnvase)).Spd.PacienteId;
            var datos = new DatosAltaEnvase(
                pacienteId, medicamento.Id, EnvaseSerie, EnvaseLote,
                DateOnly.FromDateTime(EnvaseCaducidad.Value.Date), EnvaseUnidadesIniciales.Value, OrigenEnvase.Manual);

            _servicio.RegistrarEnvaseDesdeLinea(LineaParaRegistrarEnvase.Id, datos, _usuarioActualId);
            Mensaje = "Envase registrado. La línea ya puede pasar a PREPARADO.";
            LineaParaRegistrarEnvase = null;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    /// <summary>Genera el PDF (Spec 007) y registra `impreso_ficha_en` (Spec 006 FR-680/681) —
    /// las pantallas llaman primero a la generación real y después al registro de auditoría
    /// (contracts/servicios-aplicacion.md de la Spec 007).</summary>
    [RelayCommand]
    private void ImprimirFicha(BlisterFila fila)
    {
        try
        {
            var resultado = _servicioGeneracionDocumentos.GenerarFichaSpd(fila.Spd.Id, _usuarioActualId);
            _servicio.RegistrarImpresion(fila.Spd.Id, TipoDocumentoSpd.Ficha, _usuarioActualId);
            Mensaje = $"Ficha generada: {resultado.RutaCompleta}";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void ImprimirEtiquetas(BlisterFila fila)
    {
        try
        {
            var anverso = _servicioGeneracionDocumentos.GenerarEtiquetaAnverso(fila.Spd.Id, _usuarioActualId);
            var reverso = _servicioGeneracionDocumentos.GenerarEtiquetaReverso(fila.Spd.Id, _usuarioActualId);
            _servicio.RegistrarImpresion(fila.Spd.Id, TipoDocumentoSpd.Etiquetas, _usuarioActualId);
            Mensaje = $"Etiquetas generadas: {anverso.RutaCompleta}; {reverso.RutaCompleta}";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void ImprimirInstrucciones(BlisterFila fila)
    {
        try
        {
            var resultado = _servicioGeneracionDocumentos.GenerarInstrucciones(fila.Spd.Id, _usuarioActualId);
            _servicio.RegistrarImpresion(fila.Spd.Id, TipoDocumentoSpd.Instrucciones, _usuarioActualId);
            Mensaje = $"Instrucciones generadas: {resultado.RutaCompleta}";
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    public sealed record BlisterFila(SPD Spd, IReadOnlyList<SpdLinea> Lineas);
}
