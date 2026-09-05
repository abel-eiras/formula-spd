using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;
using Spd.Infraestructura;

namespace Spd.Presentacion.ViewModels;

/// <summary>Configuración / Farmacia: datos, logo, prefijos y rutas (FR-010..FR-013, FR-030..FR-032).</summary>
public sealed partial class FarmaciaViewModel : ViewModelBase
{
    private readonly IServicioConfiguracionFarmacia _servicio;
    private readonly GestorLogoFarmacia _gestorLogo;
    private readonly IServicioBackup _servicioBackup;
    private readonly int? _administradorActualId;
    private readonly Farmacia _farmacia;

    [ObservableProperty] private string _codigoSanitario;
    [ObservableProperty] private string _nombre;
    [ObservableProperty] private string _titularOComunidadBienes;
    [ObservableProperty] private string _cif;
    [ObservableProperty] private string _direccion;
    [ObservableProperty] private string _cp;
    [ObservableProperty] private string _poblacion;
    [ObservableProperty] private string _telefono;
    [ObservableProperty] private string? _fax;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _whatsapp;
    [ObservableProperty] private string? _logo;
    [ObservableProperty] private string? _rutaNuevoLogo;

    [ObservableProperty] private string _prefijoNumFicha;
    [ObservableProperty] private string _prefijoNumSpd;

    [ObservableProperty] private string? _rutaBackup;
    [ObservableProperty] private string? _rutaDocumentosGenerados;

    public string[] DiasSemanaDisponibles => DiasSemana.Codigos;
    [ObservableProperty] private string _diaRetiradaDefecto;
    [ObservableProperty] private int _nBlisteresDefecto;
    [ObservableProperty] private int _diasAntelacionListado;
    [ObservableProperty] private double _tempMin;
    [ObservableProperty] private double _tempMax;
    [ObservableProperty] private double _hrMin;
    [ObservableProperty] private double _hrMax;
    [ObservableProperty] private int _umbralReutilizacionLecturaAmbientalHoras;

    [ObservableProperty] private string? _mensaje;

    public FarmaciaViewModel(
        IServicioConfiguracionFarmacia servicio, GestorLogoFarmacia gestorLogo, IServicioBackup servicioBackup,
        int? administradorActualId)
    {
        _servicio = servicio;
        _gestorLogo = gestorLogo;
        _servicioBackup = servicioBackup;
        _administradorActualId = administradorActualId;
        _farmacia = servicio.ObtenerConfiguracion();

        _codigoSanitario = _farmacia.CodigoSanitario;
        _nombre = _farmacia.Nombre;
        _titularOComunidadBienes = _farmacia.TitularOComunidadBienes;
        _cif = _farmacia.Cif;
        _direccion = _farmacia.Direccion;
        _cp = _farmacia.Cp;
        _poblacion = _farmacia.Poblacion;
        _telefono = _farmacia.Telefono;
        _fax = _farmacia.Fax;
        _email = _farmacia.Email;
        _whatsapp = _farmacia.Whatsapp;
        _logo = _farmacia.Logo;
        _prefijoNumFicha = _farmacia.PrefijoNumFicha;
        _prefijoNumSpd = _farmacia.PrefijoNumSpd;
        _rutaBackup = _farmacia.RutaBackup;
        _rutaDocumentosGenerados = _farmacia.RutaDocumentosGenerados;
        _diaRetiradaDefecto = _farmacia.DiaRetiradaDefecto;
        _nBlisteresDefecto = _farmacia.NBlisteresDefecto;
        _diasAntelacionListado = _farmacia.DiasAntelacionListado;
        _tempMin = _farmacia.TempMin;
        _tempMax = _farmacia.TempMax;
        _hrMin = _farmacia.HrMin;
        _hrMax = _farmacia.HrMax;
        _umbralReutilizacionLecturaAmbientalHoras = _farmacia.UmbralReutilizacionLecturaAmbientalHoras;
    }

    [RelayCommand]
    private void Guardar()
    {
        AplicarCamposAlaEntidad();
        _servicio.ActualizarDatosFarmacia(_farmacia, _administradorActualId);
        _servicio.ActualizarPrefijos(PrefijoNumFicha, PrefijoNumSpd, _administradorActualId);
        Mensaje = "Datos guardados.";
    }

    [RelayCommand]
    private void GuardarValoresDefecto()
    {
        var valores = new DatosValoresDefecto(
            DiaRetiradaDefecto, NBlisteresDefecto, DiasAntelacionListado,
            TempMin, TempMax, HrMin, HrMax, UmbralReutilizacionLecturaAmbientalHoras);
        _servicio.ActualizarValoresDefecto(valores, _administradorActualId);
        Mensaje = "Valores por defecto guardados.";
    }

    [RelayCommand]
    private void ValidarRutaBackup() => Mensaje = DescribirValidacion("Copias de seguridad", RutaBackup);

    // FR-1003: backup manual bajo demanda, mismo procedimiento que el automático al cerrar.
    [RelayCommand]
    private void GenerarBackupAhora()
    {
        var resultado = _servicioBackup.GenerarBackup(_administradorActualId, esAutomatico: false);
        Mensaje = resultado.Exito
            ? $"Backup generado: {resultado.RutaZip}."
            : $"No se pudo generar el backup: {resultado.Motivo}";
    }

    [RelayCommand]
    private void ValidarRutaDocumentos() => Mensaje = DescribirValidacion("Documentos generados", RutaDocumentosGenerados);

    [RelayCommand]
    private void AplicarNuevoLogo()
    {
        if (string.IsNullOrWhiteSpace(RutaNuevoLogo))
        {
            return;
        }
        Logo = _gestorLogo.GuardarNuevoLogo(RutaNuevoLogo, _farmacia.Logo);
        _farmacia.Logo = Logo;
        RutaNuevoLogo = null;
    }

    private string DescribirValidacion(string etiqueta, string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
        {
            return $"{etiqueta}: sin ruta configurada.";
        }

        var resultado = _servicio.ValidarRuta(ruta);
        if (!resultado.Existe)
        {
            return $"{etiqueta}: la ruta no existe.";
        }
        if (!resultado.Escribible)
        {
            return $"{etiqueta}: la ruta existe pero no se puede escribir en ella.";
        }
        return resultado.CoincideConCarpetaInstalacion
            ? $"{etiqueta}: válida, pero está dentro de la carpeta de instalación (aviso, FR-032)."
            : $"{etiqueta}: válida.";
    }

    private void AplicarCamposAlaEntidad()
    {
        _farmacia.CodigoSanitario = CodigoSanitario;
        _farmacia.Nombre = Nombre;
        _farmacia.TitularOComunidadBienes = TitularOComunidadBienes;
        _farmacia.Cif = Cif;
        _farmacia.Direccion = Direccion;
        _farmacia.Cp = Cp;
        _farmacia.Poblacion = Poblacion;
        _farmacia.Telefono = Telefono;
        _farmacia.Fax = Fax;
        _farmacia.Email = Email;
        _farmacia.Whatsapp = Whatsapp;
        _farmacia.RutaBackup = RutaBackup;
        _farmacia.RutaDocumentosGenerados = RutaDocumentosGenerados;
    }
}
