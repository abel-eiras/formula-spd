using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Presentacion.Pacientes;
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
    private readonly IServicioComunicacionesMedico _servicioComunicaciones;
    private readonly int _pacienteId;
    private readonly PacienteContexto? _contexto;
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
    // Las tres preguntas del Anexo I.G que faltaban (corrección 2026-09-06, ChecklistVerificacion).
    [ObservableProperty] private bool _checkFabricantePnt;
    [ObservableProperty] private bool _checkEtiquetaFichaPaciente;
    [ObservableProperty] private bool _checkTrazabilidad;
    [ObservableProperty] private int? _verificadorId;
    [ObservableProperty] private System.Collections.ObjectModel.ObservableCollection<Usuario> _verificadoresDisponibles = [];
    [ObservableProperty] private Usuario? _verificadorSeleccionado;
    [ObservableProperty] private string? _excepcionMotivo;

    // Datos de entrega (FR-661/663): comunes a todos los blísteres que se entreguen juntos.
    [ObservableProperty] private string _entregadoA = string.Empty;
    [ObservableProperty] private bool _primeraEntrega;
    [ObservableProperty] private bool _spdAnteriorRecogido;
    [ObservableProperty] private string? _unidadesNoAdministradas;
    [ObservableProperty] private string? _observacionesAdherencia;
    [ObservableProperty] private bool _cambiosMedicacionReferidos;

    [ObservableProperty] private BlisterFila? _blisterEnReelaboracion;
    [ObservableProperty] private OrigenSolicitudReelaboracion _origenReelaboracion = OrigenSolicitudReelaboracion.Paciente;
    [ObservableProperty] private string _motivoReelaboracion = string.Empty;

    [ObservableProperty] private SpdLinea? _lineaParaRegistrarEnvase;
    [ObservableProperty] private string _envaseSerie = string.Empty;
    [ObservableProperty] private string _envaseLote = string.Empty;
    [ObservableProperty] private DateTimeOffset? _envaseCaducidad;
    [ObservableProperty] private int? _envaseUnidadesIniciales;

    public OrigenSolicitudReelaboracion[] OrigenesDisponibles { get; } = Enum.GetValues<OrigenSolicitudReelaboracion>();

    /// <summary>Art. I.3 (enmienda 3.0.0, decisión del propietario del 2026-09-14): el nomenclátor no dice si
    /// un medicamento es apto para SPD, así que al preparar se enseñan los que están sin confirmar y el
    /// farmacéutico los confirma **todos a la vez**. Es un aviso, nunca un bloqueo: asegurar que lo que se
    /// emblista es apto es obligación del farmacéutico.</summary>
    [ObservableProperty] private string? _medicamentosSinConfirmar;
    [ObservableProperty] private string? _medicamentosNoAptos;
    private IReadOnlyList<int> _idsSinConfirmar = [];

    public bool HayAptitudSinConfirmar => _idsSinConfirmar.Count > 0;
    public bool HayNoAptos => MedicamentosNoAptos is not null;

    public PreparacionViewModel(
        IServicioPreparacion servicio, IServicioMedicamentos servicioMedicamentos,
        IServicioGeneracionDocumentos servicioGeneracionDocumentos, IServicioComunicacionesMedico servicioComunicaciones,
        int pacienteId, int? usuarioActualId, PacienteContexto? contexto = null, IServicioUsuarios? servicioUsuarios = null)
    {
        // FR-1542: el verificador se elige de la lista de usuarios activos, no tecleando su id.
        VerificadoresDisponibles = new System.Collections.ObjectModel.ObservableCollection<Usuario>(
            servicioUsuarios?.ListarActivos() ?? []);
        _contexto = contexto;
        _servicio = servicio;
        _servicioMedicamentos = servicioMedicamentos;
        _servicioGeneracionDocumentos = servicioGeneracionDocumentos;
        _servicioComunicaciones = servicioComunicaciones;
        _pacienteId = pacienteId;
        _usuarioActualId = usuarioActualId;
        Cargar();
    }

    private void Cargar()
    {
        // El material se carga primero: el carril de pasos necesita saber si lo hay para poder decir
        // que el llenado está bloqueado y por qué (FR-1540).
        MaterialesDisponibles = new ObservableCollection<MaterialAcondicionamiento>(_servicio.ListarMaterialesActivos());
        var spds = _servicio.ListarPorFiltro(new FiltrosPreparaciones(PacienteId: _pacienteId));
        Blisteres = new ObservableCollection<BlisterFila>(
            spds.OrderByDescending(s => s.Id)
                .Select(s => new BlisterFila(s, _servicio.ListarLineas(s.Id), MaterialesDisponibles.Count > 0)));
        ComprobarAptitud();
    }

    /// <summary>Solo cuenta lo que todavía se va a emblistar: un blíster verificado o entregado ya pasó por
    /// el farmacéutico.</summary>
    private void ComprobarAptitud()
    {
        var aEmblistar = Blisteres
            .Where(b => b.Spd.Estado is EstadoSpd.Borrador or EstadoSpd.Preparado)
            .SelectMany(b => b.Lineas)
            .Select(l => l.MedicamentoId)
            .Distinct()
            .Select(_servicioMedicamentos.ObtenerPorId)
            .OfType<Medicamento>()
            .ToList();

        var sinConfirmar = aEmblistar.Where(m => m.AptoSpd is null).OrderBy(m => m.Nombre).ToList();
        _idsSinConfirmar = sinConfirmar.Select(m => m.Id).ToList();
        MedicamentosSinConfirmar = sinConfirmar.Count == 0 ? null : string.Join(" · ", sinConfirmar.Select(m => m.Nombre));

        var noAptos = aEmblistar.Where(m => m.AptoSpd == false).Select(m => m.Nombre).OrderBy(n => n).ToList();
        MedicamentosNoAptos = noAptos.Count == 0 ? null : string.Join(" · ", noAptos);

        OnPropertyChanged(nameof(HayAptitudSinConfirmar));
        OnPropertyChanged(nameof(HayNoAptos));
    }

    /// <summary>Una sola confirmación para todos, no una por medicamento. Solo marca los que estaban sin
    /// confirmar: un «no apto» explícito no se toca. Cada uno queda en auditoría con quién lo confirmó.</summary>
    [RelayCommand]
    private void ConfirmarAptitudSpd()
    {
        if (_idsSinConfirmar.Count == 0) return;

        var confirmados = _servicioMedicamentos.ConfirmarAptitudSpd(_idsSinConfirmar, _usuarioActualId);
        Mensaje = $"Aptitud para SPD confirmada en {confirmados} medicamento(s). Queda registrado quién lo ha confirmado.";
        ComprobarAptitud();
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
        CheckAspecto = CheckEtiquetaDatos = CheckEtiquetaValidez = CheckContenido =
            CheckFabricantePnt = CheckEtiquetaFichaPaciente = false;
        // Dos preguntas las puede responder la propia aplicación: la trazabilidad envase→DDP
        // está garantizada por construcción (SPD_Linea_Envase) y la hoja de instrucciones
        // consta como generada si tiene impreso_instrucciones_en. El verificador puede desmarcarlas.
        CheckTrazabilidad = true;
        CheckInstrucciones = fila.Spd.ImpresoInstruccionesEn is not null;
        ExcepcionMotivo = null;
    }

    [RelayCommand]
    private void ConfirmarVerificacion()
    {
        if (BlisterEnVerificacion is null || VerificadorId is null) return;
        try
        {
            var checklist = new ChecklistVerificacion(
                CheckContenido, CheckFabricantePnt, CheckEtiquetaDatos, CheckEtiquetaFichaPaciente,
                CheckTrazabilidad, CheckEtiquetaValidez, CheckInstrucciones, CheckAspecto);
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
            var datos = new DatosEntregaSpd(
                DateOnly.FromDateTime(DateTime.Today), EntregadoA, PrimeraEntrega, PrimeraEntrega ? null : SpdAnteriorRecogido,
                string.IsNullOrWhiteSpace(UnidadesNoAdministradas) ? null : UnidadesNoAdministradas,
                string.IsNullOrWhiteSpace(ObservacionesAdherencia) ? null : ObservacionesAdherencia,
                CambiosMedicacionPreguntado: true, null, CambiosMedicacionReferidos);
            var entregados = _servicio.RegistrarEntrega(datos, pendientes, _usuarioActualId);
            Mensaje = $"{pendientes.Count} blíster(es) entregado(s).";
            if (CambiosMedicacionReferidos) AbrirComunicacionPorCambioReferido(entregados);
            UnidadesNoAdministradas = ObservacionesAdherencia = null;
            CambiosMedicacionReferidos = false;
            Cargar();
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    /// <summary>Spec 008 FR-805 y Spec 006 FR-663: si en la entrega el paciente refiere cambios de
    /// medicación, sus tratamientos en SPD quedan pendientes de revisión y se abre la comunicación
    /// al médico ya preparada.
    ///
    /// El destinatario es el **médico de cabecera del paciente**, que es de quien habla el FR-805: el
    /// paciente refiere un cambio suyo, no de un medicamento concreto. Antes se cogía el prescriptor
    /// de la primera línea que apareciera, que con varios tratamientos de médicos distintos podía ser
    /// cualquiera. Si el paciente no tiene médico de cabecera se cae a ese prescriptor, y si tampoco
    /// lo hay se abre la pestaña vacía para elegirlo a mano: nunca se adivina.</summary>
    private void AbrirComunicacionPorCambioReferido(IReadOnlyList<SPD> entregados)
    {
        Mensaje += " Cambios de medicación referidos: el tratamiento queda pendiente de revisión; redacte la comunicación al médico.";

        var medicoDeCabecera = _contexto?.Paciente?.MedicoId;
        if (medicoDeCabecera is { } medicoId)
        {
            var prerrelleno = _servicioComunicaciones.PrepararDesdeAvisoCambioReferido(_pacienteId, medicoId)
                with { IncidenciasDetectadas = $"El paciente refiere cambios en su medicación en la entrega del {DateTime.Today:dd/MM/yyyy}." };
            _contexto?.IrA(PestanaPaciente.Comunicaciones, prerrelleno);
            return;
        }

        var tratamientoId = Blisteres.Where(b => entregados.Any(e => e.Id == b.Spd.Id))
            .SelectMany(b => b.Lineas).Select(l => l.TratamientoId).FirstOrDefault();
        if (tratamientoId != 0)
        {
            _contexto?.IrA(PestanaPaciente.Comunicaciones, _servicioComunicaciones.PrepararDesdeTratamiento(tratamientoId));
            return;
        }

        _contexto?.IrA(PestanaPaciente.Comunicaciones);
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

    /// <summary>FR-682 (PNT I §4.4.1: una hoja "en cada entrega"): una hoja para los blísteres de la
    /// última sesión si tienen el mismo contenido.</summary>
    [RelayCommand]
    private void ImprimirInstruccionesSesion()
    {
        var ultima = Blisteres.FirstOrDefault();
        if (ultima is null) return;
        try
        {
            var resultado = _servicioGeneracionDocumentos.GenerarInstruccionesSesion(ultima.Spd.SesionId, _usuarioActualId);
            foreach (var b in Blisteres.Where(b => b.Spd.SesionId == ultima.Spd.SesionId))
                _servicio.RegistrarImpresion(b.Spd.Id, TipoDocumentoSpd.Instrucciones, _usuarioActualId);
            Mensaje = $"Hoja de instrucciones de la sesión generada: {resultado.RutaCompleta}";
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

    /// <summary>Un blíster de la sesión. Además de sus datos lleva ya calculado lo que la pantalla
    /// necesita mostrar: el carril de pasos (FR-1540) y la rejilla de alvéolos (FR-1541).</summary>
    public sealed record BlisterFila(SPD Spd, IReadOnlyList<SpdLinea> Lineas, bool HayMaterial = true)
    {
        public IReadOnlyList<Preparacion.PasoPreparacion> Pasos { get; } =
            Preparacion.PasoPreparacion.Derivar(Spd, HayMaterial);

        public Preparacion.RejillaAlveolosDatos Rejilla { get; } =
            Preparacion.MapaAlveolos.Rejilla(Lineas, Spd.ValidezDesde);

        /// <summary>FR-1540 (H4.6): cada impresión se ofrece cuando su paso está disponible. Un
        /// botón apagado con su motivo al lado dice más que uno encendido que devuelve un error.</summary>
        public bool PuedeImprimirEtiquetas => !Paso(Preparacion.ClavePaso.Etiquetado).EstaBloqueado;
        public bool PuedeImprimirInstrucciones => !Paso(Preparacion.ClavePaso.Instrucciones).EstaBloqueado;
        public bool PuedeVerificar => Paso(Preparacion.ClavePaso.Verificacion).EsActual;
        public bool PuedeCerrarLlenado => Paso(Preparacion.ClavePaso.Llenado).EsActual;

        private Preparacion.PasoPreparacion Paso(Preparacion.ClavePaso clave)
            => Pasos.First(p => p.Clave == clave);
    }

    /// <summary>FR-1542. Quién firma una verificación es dato legal (Art. II): elegirlo de una lista
    /// de personas, y no tecleando un número, evita atribuir la firma a quien no verificó.</summary>
    partial void OnVerificadorSeleccionadoChanged(Usuario? value) => VerificadorId = value?.Id;

    partial void OnVerificadorIdChanged(int? value)
        => VerificadorSeleccionado = value is null
            ? null
            : System.Linq.Enumerable.FirstOrDefault(VerificadoresDisponibles, u => u.Id == value);
}
