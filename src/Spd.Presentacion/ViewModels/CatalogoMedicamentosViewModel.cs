using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Presentacion.Navegacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Catálogo de medicamentos: listado con búsqueda a la izquierda, formulario de alta o
/// edición a la derecha (FR-300..FR-311), mismo patrón que `UsuariosViewModel` de Spec 000.</summary>
public sealed partial class CatalogoMedicamentosViewModel : ViewModelBase
{
    private readonly IServicioMedicamentos _servicio;
    private readonly IServicioImportacionNomenclator _servicioImportacion;
    private readonly IServicioConsultaCima _servicioConsultaCima;
    private readonly Navegador _navegador;
    private readonly int? _usuarioActualId;
    private int? _medicamentoIdEnEdicion;

    [ObservableProperty] private string _fragmento = string.Empty;
    [ObservableProperty] private ObservableCollection<Medicamento> _resultados = [];
    [ObservableProperty] private string? _mensaje;

    [ObservableProperty] private string _cn = string.Empty;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string? _principioActivo;
    [ObservableProperty] private string? _laboratorio;
    [ObservableProperty] private string? _formaFarmaceutica;
    [ObservableProperty] private bool _fraccionable;
    [ObservableProperty] private string? _gtin;
    /// <summary>FR-301, revisado el 2026-09-14: la casilla tiene tres estados. En blanco es **sin confirmar**:
    /// nadie ha decidido aún si es apto, y lo confirma el farmacéutico al elaborar (Spec 006).</summary>
    [ObservableProperty] private bool? _aptoSpd;
    [ObservableProperty] private string? _motivoNoApto;
    [ObservableProperty] private int? _unidadesEnvase;
    [ObservableProperty] private string? _descForma;
    [ObservableProperty] private string? _descColor;
    [ObservableProperty] private string? _descRanura;
    [ObservableProperty] private string? _descSerigrafia;
    [ObservableProperty] private string? _descTamano;
    [ObservableProperty] private string? _descTexto;
    [ObservableProperty] private bool _consultandoCima;

    public string[] FormasDisponibles { get; } = Enum.GetNames<FormaFarmaceutica>();
    public bool EsAltaNueva => _medicamentoIdEnEdicion is null;

    /// <summary>Con el nomenclátor entero en el catálogo, sin nada escrito no se lista nada: cargar 15.000
    /// filas no ayuda a encontrar ninguna.</summary>
    public bool FaltaBusqueda => (Fragmento?.Trim().Length ?? 0) < ServicioMedicamentos.MinimoCaracteres;

    public bool HayMasResultados => Resultados.Count >= ServicioMedicamentos.LimitePorDefecto;

    public CatalogoMedicamentosViewModel(
        IServicioMedicamentos servicio, IServicioImportacionNomenclator servicioImportacion,
        IServicioConsultaCima servicioConsultaCima, Navegador navegador, int? usuarioActualId)
    {
        _servicio = servicio;
        _servicioImportacion = servicioImportacion;
        _servicioConsultaCima = servicioConsultaCima;
        _navegador = navegador;
        _usuarioActualId = usuarioActualId;
        Buscar();
    }

    [RelayCommand]
    private void Buscar()
    {
        Resultados = new ObservableCollection<Medicamento>(_servicio.Buscar(Fragmento));
        OnPropertyChanged(nameof(FaltaBusqueda));
        OnPropertyChanged(nameof(HayMasResultados));
    }

    // Spec 015 FR-1502: navegar a la sección, no abrir una ventana encima.
    [RelayCommand]
    private void RevisarNomenclator() => _navegador.Navegar(new Destino(Seccion.RevisionNomenclator));

    // Consulta puntual por CN al CIMA REST API público de la AEMPS, en el momento del alta: sirve para
    // lo que no está en el nomenclátor y para completar la forma farmacéutica, que el nomenclátor no
    // trae. Nunca rellena aptitud SPD ni descripción física (Art. I.2/I.3).
    [RelayCommand]
    private async Task ConsultarCimaAsync()
    {
        if (string.IsNullOrWhiteSpace(Cn))
        {
            Mensaje = "Introduzca el CN antes de consultar CIMA.";
            return;
        }

        ConsultandoCima = true;
        try
        {
            var resultado = await _servicioConsultaCima.ConsultarPorCnAsync(Cn);
            if (resultado.Error is not null)
            {
                Mensaje = $"No se pudo consultar CIMA: {resultado.Error}";
                return;
            }

            if (!resultado.Encontrado)
            {
                Mensaje = "Ese CN no está en CIMA (puede ser una fórmula magistral). Rellene los datos a mano.";
                return;
            }

            Nombre = resultado.Nombre!;
            PrincipioActivo = resultado.PrincipioActivo;
            Laboratorio = resultado.Laboratorio;
            FormaFarmaceutica = resultado.FormaFarmaceutica?.ToString();
            Mensaje = resultado.EnvaseIndicado is not null
                ? $"Datos rellenados desde CIMA. Envase indicado por CIMA: {resultado.EnvaseIndicado}."
                : "Datos rellenados desde CIMA. Revíselos antes de guardar.";
        }
        finally
        {
            ConsultandoCima = false;
        }
    }

    [RelayCommand]
    private void NuevoMedicamento() => LimpiarFormulario();

    [RelayCommand]
    private void SeleccionarMedicamento(Medicamento medicamento) => CargarEnFormulario(medicamento);

    [RelayCommand]
    private void ProponerDescripcion()
        => DescTexto = _servicio.ProponerDescripcionTexto(ConstruirDatosDescripcion());

    [RelayCommand]
    private void Guardar()
    {
        try
        {
            if (_medicamentoIdEnEdicion is null)
            {
                var creado = _servicio.Crear(new DatosAltaMedicamento(Cn, Nombre), _usuarioActualId);
                _medicamentoIdEnEdicion = creado.Id;
            }

            var medicamento = _servicio.ObtenerPorId(_medicamentoIdEnEdicion.Value)!;
            medicamento.Nombre = Nombre;
            medicamento.PrincipioActivo = PrincipioActivo;
            medicamento.Laboratorio = Laboratorio;
            medicamento.FormaFarmaceutica = string.IsNullOrEmpty(FormaFarmaceutica)
                ? null
                : Enum.Parse<FormaFarmaceutica>(FormaFarmaceutica);
            medicamento.Fraccionable = Fraccionable;
            medicamento.Gtin = Gtin;
            medicamento.AptoSpd = AptoSpd;
            medicamento.MotivoNoApto = MotivoNoApto;
            _servicio.ActualizarDatos(medicamento, _usuarioActualId);

            _servicio.ActualizarDescripcionFisica(medicamento.Id, ConstruirDatosDescripcion(), _usuarioActualId);

            if (UnidadesEnvase is not null)
            {
                _servicio.ActualizarUnidadesEnvase(medicamento.Id, UnidadesEnvase.Value, _usuarioActualId);
            }

            Mensaje = $"Medicamento {medicamento.Cn} guardado.";
            Buscar();
        }
        catch (MedicamentoDuplicadoException ex)
        {
            Mensaje = ex.Message;
        }
        catch (ErrorValidacionException ex)
        {
            Mensaje = ex.Message;
        }
    }

    [RelayCommand]
    private void DarDeBaja(Medicamento medicamento)
    {
        _servicio.DarDeBaja(medicamento.Id, _usuarioActualId);
        Mensaje = $"Medicamento {medicamento.Cn} dado de baja.";
        Buscar();
        if (_medicamentoIdEnEdicion == medicamento.Id) LimpiarFormulario();
    }

    private DatosDescripcionFisica ConstruirDatosDescripcion()
        => new(DescForma, DescColor, DescRanura, DescSerigrafia, DescTamano, DescTexto);

    private void CargarEnFormulario(Medicamento medicamento)
    {
        _medicamentoIdEnEdicion = medicamento.Id;
        Cn = medicamento.Cn;
        Nombre = medicamento.Nombre;
        PrincipioActivo = medicamento.PrincipioActivo;
        Laboratorio = medicamento.Laboratorio;
        FormaFarmaceutica = medicamento.FormaFarmaceutica?.ToString();
        Fraccionable = medicamento.Fraccionable;
        Gtin = medicamento.Gtin;
        AptoSpd = medicamento.AptoSpd;
        MotivoNoApto = medicamento.MotivoNoApto;
        UnidadesEnvase = medicamento.UnidadesEnvase;
        DescForma = medicamento.DescForma;
        DescColor = medicamento.DescColor;
        DescRanura = medicamento.DescRanura;
        DescSerigrafia = medicamento.DescSerigrafia;
        DescTamano = medicamento.DescTamano;
        DescTexto = medicamento.DescTexto;
        OnPropertyChanged(nameof(EsAltaNueva));
    }

    private void LimpiarFormulario()
    {
        _medicamentoIdEnEdicion = null;
        Cn = string.Empty;
        Nombre = string.Empty;
        PrincipioActivo = null;
        Laboratorio = null;
        FormaFarmaceutica = null;
        Fraccionable = false;
        Gtin = null;
        AptoSpd = null;
        MotivoNoApto = null;
        UnidadesEnvase = null;
        DescForma = null;
        DescColor = null;
        DescRanura = null;
        DescSerigrafia = null;
        DescTamano = null;
        DescTexto = null;
        Mensaje = null;
        OnPropertyChanged(nameof(EsAltaNueva));
    }

    partial void OnFragmentoChanged(string value) => Buscar();
}
