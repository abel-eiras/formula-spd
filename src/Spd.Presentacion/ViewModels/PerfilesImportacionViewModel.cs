using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Perfiles genéricos de importación/exportación (FR-1100). El mapeo se edita como texto
/// "campo=columna" por línea, una fila por par, para no depender de una grilla dedicada.</summary>
public sealed partial class PerfilesImportacionViewModel : ViewModelBase
{
    private readonly IServicioPerfilesImportacion _servicio;
    private readonly int? _usuarioActualId;
    private int? _perfilIdEnEdicion;

    [ObservableProperty] private TipoPerfilImportacion _filtroTipo = TipoPerfilImportacion.Pacientes;
    [ObservableProperty] private ObservableCollection<PerfilImportacion> _perfiles = [];
    [ObservableProperty] private string? _mensaje;

    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private TipoPerfilImportacion _tipo = TipoPerfilImportacion.Pacientes;
    [ObservableProperty] private string _separador = ",";
    [ObservableProperty] private string _codificacion = "UTF-8";
    [ObservableProperty] private bool _tieneCabecera = true;
    [ObservableProperty] private string _mapeoTexto = string.Empty;
    [ObservableProperty] private string? _regexUnidadesEnvase;

    public TipoPerfilImportacion[] TiposDisponibles { get; } = Enum.GetValues<TipoPerfilImportacion>();

    public PerfilesImportacionViewModel(IServicioPerfilesImportacion servicio, int? usuarioActualId)
    {
        _servicio = servicio;
        _usuarioActualId = usuarioActualId;
        Cargar();
    }

    private void Cargar() => Perfiles = new ObservableCollection<PerfilImportacion>(_servicio.ListarPorTipo(FiltroTipo));

    partial void OnFiltroTipoChanged(TipoPerfilImportacion value) => Cargar();

    [RelayCommand]
    private void PrepararEdicion(PerfilImportacion perfil)
    {
        _perfilIdEnEdicion = perfil.Id;
        Nombre = perfil.Nombre;
        Tipo = perfil.Tipo;
        Separador = perfil.Separador;
        Codificacion = perfil.Codificacion;
        TieneCabecera = perfil.TieneCabecera;
        MapeoTexto = string.Join('\n', perfil.Mapeo.Select(p => $"{p.Campo}={p.Columna}"));
        RegexUnidadesEnvase = perfil.RegexUnidadesEnvase;
    }

    [RelayCommand]
    private void NuevoPerfil()
    {
        _perfilIdEnEdicion = null;
        Nombre = string.Empty;
        Separador = ",";
        Codificacion = "UTF-8";
        TieneCabecera = true;
        MapeoTexto = string.Empty;
        RegexUnidadesEnvase = null;
        Mensaje = null;
    }

    [RelayCommand]
    private void Guardar()
    {
        var mapeo = MapeoTexto
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(linea => linea.Split('=', 2))
            .Where(partes => partes.Length == 2)
            .Select(partes => new ParCampoColumna(partes[0].Trim(), partes[1].Trim()))
            .ToList();

        if (mapeo.Count == 0)
        {
            Mensaje = "El mapeo debe tener al menos un par campo=columna.";
            return;
        }

        var datos = new DatosAltaPerfilImportacion(Nombre, Tipo, Separador, Codificacion, TieneCabecera, mapeo, RegexUnidadesEnvase);

        if (_perfilIdEnEdicion is { } id)
        {
            _servicio.Actualizar(id, datos, _usuarioActualId);
            Mensaje = "Perfil actualizado.";
        }
        else
        {
            _servicio.Crear(datos, _usuarioActualId);
            Mensaje = "Perfil creado.";
        }

        NuevoPerfil();
        Cargar();
    }
}
