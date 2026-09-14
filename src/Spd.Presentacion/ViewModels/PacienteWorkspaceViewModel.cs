using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Pacientes;

namespace Spd.Presentacion.ViewModels;

/// <summary>El espacio del paciente (Spec 015 FR-1530..1532): una cabecera fija y siete pestañas,
/// donde antes había siete ventanas que había que abrir, colocar y cerrar.
///
/// Todas las pestañas comparten un <see cref="PacienteContexto"/>, así que lo que hace una se ve en
/// las demás sin releer nada a mano: registrar el consentimiento activa al paciente y la cabecera lo
/// refleja en el acto (Spec 002 FR-213).</summary>
public sealed partial class PacienteWorkspaceViewModel : ViewModelBase
{
    private readonly ServiciosAplicacion _servicios;
    private readonly PacienteContexto _contexto;
    private readonly int? _usuarioActualId;

    /// <summary>Construir las pestañas construye sus ViewModels, y alguno de ellos puede avisar al
    /// contexto mientras se construye. Sin este cerrojo eso vuelve a entrar aquí y no termina nunca
    /// (lo detectaron los tests de esta misma fase, no la prueba manual).</summary>
    private bool _construyendo;

    [ObservableProperty] private ObservableCollection<Pestana> _pestanas = [];
    [ObservableProperty] private Pestana? _pestanaSeleccionada;

    public CabeceraPacienteViewModel Cabecera { get; }
    public PacienteContexto Contexto => _contexto;

    public PacienteWorkspaceViewModel(ServiciosAplicacion servicios, Paciente? paciente, int? usuarioActualId)
    {
        _servicios = servicios;
        _usuarioActualId = usuarioActualId;
        _contexto = new PacienteContexto(servicios.Pacientes);
        _contexto.Establecer(paciente);

        Cabecera = new CabeceraPacienteViewModel(_contexto, servicios.Idoneidad, servicios.Preparacion);
        _contexto.Cambiado += AlCambiarPaciente;
        _contexto.PestanaSolicitada += AlSolicitarPestana;

        ConstruirPestanas();
    }

    /// <summary>Un alta nueva empieza solo con Datos: sin `paciente_id` no hay tratamientos, depósito,
    /// preparación ni comunicaciones que registrar (FR-400, FR-510, FR-600, FR-801). En cuanto se
    /// guarda por primera vez aparecen las otras seis, sin cerrar ni reabrir nada.</summary>
    private void ConstruirPestanas()
    {
        if (_construyendo) return;
        _construyendo = true;
        try
        {
            ConstruirPestanasInterno();
        }
        finally
        {
            _construyendo = false;
        }
    }

    private void ConstruirPestanasInterno()
    {
        var seleccionada = PestanaSeleccionada?.Clave;
        var pestanas = new ObservableCollection<Pestana>
        {
            new(PestanaPaciente.Datos, "Datos",
                new FichaPacienteViewModel(_servicios.Pacientes, _servicios.Medicos, _servicios.Contactos,
                    _contexto, _usuarioActualId))
        };

        if (_contexto.PacienteId is { } id)
        {
            pestanas.Add(new Pestana(PestanaPaciente.Idoneidad, "Idoneidad y consentimiento",
                new IdoneidadConsentimientoViewModel(_servicios.Idoneidad, _servicios.Documentos, id, _usuarioActualId, _contexto)));
            pestanas.Add(new Pestana(PestanaPaciente.Tratamiento, "Tratamiento",
                new TratamientoViewModel(_servicios.Tratamientos, _servicios.Medicamentos, _servicios.Comunicaciones,
                    _servicios.Documentos, _servicios.Medicos, id, _usuarioActualId, _contexto, _servicios.ConsultaCima)));
            pestanas.Add(new Pestana(PestanaPaciente.Deposito, "Depósito",
                new DepositoViewModel(_servicios.Envases, _servicios.Medicamentos, _servicios.ImportacionTratamiento,
                    id, _usuarioActualId)));
            pestanas.Add(new Pestana(PestanaPaciente.Preparacion, "Preparación",
                new PreparacionViewModel(_servicios.Preparacion, _servicios.Medicamentos, _servicios.Documentos,
                    _servicios.Comunicaciones, id, _usuarioActualId, _contexto, _servicios.Usuarios)));
            pestanas.Add(new Pestana(PestanaPaciente.Comunicaciones, "Comunicaciones",
                new ComunicacionesMedicoViewModel(_servicios.Comunicaciones, _servicios.Documentos, id, _usuarioActualId,
                    _servicios.Medicos)));
            pestanas.Add(new Pestana(PestanaPaciente.Documentos, "Documentos",
                new DocumentosPacienteViewModel(_contexto, _servicios.Documentos, _usuarioActualId)));
        }

        Pestanas = pestanas;
        PestanaSeleccionada = pestanas.FirstOrDefault(p => p.Clave == seleccionada) ?? pestanas[0];
        RecalcularIndicadores();
    }

    private void AlCambiarPaciente(Paciente? paciente)
    {
        // Solo se rehacen las pestañas cuando aparece el paciente por primera vez; a partir de ahí
        // reconstruirlas tiraría el trabajo a medias de cada una.
        if (paciente is not null && Pestanas.Count == 1) ConstruirPestanas();
        else RecalcularIndicadores();
    }

    private void AlSolicitarPestana(PestanaPaciente clave, object? argumento)
    {
        var destino = Pestanas.FirstOrDefault(p => p.Clave == clave);
        if (destino is null) return;

        // Único argumento con sentido hoy: el prerrelleno de una comunicación al médico, que antes
        // abría una ventana desde tratamiento o desde preparación (Spec 008 FR-810).
        if (argumento is DatosAltaComunicacionMedico prerrelleno
            && destino.Contenido is ComunicacionesMedicoViewModel comunicaciones)
        {
            comunicaciones.Prerrellenar(prerrelleno);
        }

        PestanaSeleccionada = destino;
    }

    /// <summary>FR-1532: cada pestaña dice si tiene algo pendiente sin necesidad de entrar a mirar.</summary>
    public void RecalcularIndicadores()
    {
        Cabecera.Refrescar();
        if (_contexto.PacienteId is not { } id)
        {
            foreach (var p in Pestanas) p.Pendiente = null;
            return;
        }

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        foreach (var pestana in Pestanas)
        {
            pestana.Pendiente = pestana.Clave switch
            {
                // Idoneidad: no es un recuento, es un impedimento — o está en regla o no lo está.
                PestanaPaciente.Idoneidad => _servicios.Idoneidad.Consultar(id).CumpleParaActivo ? null : 1,
                PestanaPaciente.Deposito => Contar(_servicios.ListadoRetirada
                    .ObtenerListado(hoy, new FiltrosListadoRetirada(SoloConFaltantes: true, PacienteId: id)).Count),
                PestanaPaciente.Preparacion => Contar(_servicios.Preparacion
                    .ListarPorFiltro(new FiltrosPreparaciones(PacienteId: id))
                    .Count(s => s.Estado is EstadoSpd.Preparado or EstadoSpd.Verificado)),
                PestanaPaciente.Comunicaciones => Contar(_servicios.Comunicaciones
                    .ListarDePaciente(id).Count(c => c.Respuesta is null)),
                _ => null
            };
        }

        static int? Contar(int n) => n > 0 ? n : null;
    }

    [RelayCommand]
    private void Seleccionar(Pestana? pestana)
    {
        if (pestana is not null) PestanaSeleccionada = pestana;
    }

    [RelayCommand]
    private void Refrescar() => RecalcularIndicadores();

    /// <summary>Una pestaña: su ViewModel y, si tiene algo pendiente, cuánto.</summary>
    public sealed partial class Pestana(PestanaPaciente clave, string titulo, object contenido) : ObservableObject
    {
        public PestanaPaciente Clave { get; } = clave;
        public string Titulo { get; } = titulo;
        public object Contenido { get; } = contenido;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TienePendiente))]
        [NotifyPropertyChangedFor(nameof(PendienteTexto))]
        private int? _pendiente;

        public bool TienePendiente => Pendiente is > 0;
        public string PendienteTexto => Pendiente?.ToString() ?? string.Empty;
    }
}
