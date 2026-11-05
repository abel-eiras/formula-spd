using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Una opción del filtro de centro; `Id` nulo es «todos los pacientes».</summary>
public sealed record OpcionCentro(int? Id, string Nombre)
{
    public override string ToString() => Nombre;
}

/// <summary>Lista de reposición (Spec 019 FR-1910..1912): medicamentos que se agotan pronto con lo
/// que hay en custodia, para pedir a tiempo la dispensación o una receta nueva.</summary>
public sealed partial class ReposicionViewModel : ViewModelBase
{
    private readonly IServicioReposicion _servicio;
    private readonly IServicioDocumentosCentros? _documentos;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private int _horizonteDias = 14;
    [ObservableProperty] private OpcionCentro? _centro;
    [ObservableProperty] private ObservableCollection<FilaReposicion> _filas = [];
    [ObservableProperty] private string? _mensaje;

    public IReadOnlyList<OpcionCentro> OpcionesCentro { get; }

    public ReposicionViewModel(IServicioReposicion servicio, IServicioCentros? servicioCentros,
        IServicioDocumentosCentros? documentos, int? usuarioActualId)
    {
        _servicio = servicio;
        _documentos = documentos;
        _usuarioActualId = usuarioActualId;
        OpcionesCentro = new[] { new OpcionCentro(null, "Todos los pacientes") }
            .Concat((servicioCentros?.Listar(incluirBaja: false) ?? []).Select(c => new OpcionCentro(c.Id, c.Nombre)))
            .ToList();
        _centro = OpcionesCentro[0];
        Calcular();
    }

    [RelayCommand]
    private void Calcular()
    {
        try
        {
            Filas = new ObservableCollection<FilaReposicion>(
                _servicio.Obtener(DateOnly.FromDateTime(DateTime.Today), HorizonteDias, Centro?.Id));
            Mensaje = Filas.Count == 0 ? $"Nada se agota en los próximos {HorizonteDias} días con lo que hay en custodia." : null;
        }
        catch (ErrorValidacionException ex) { Mensaje = ex.Message; }
    }

    [RelayCommand]
    private void Imprimir()
    {
        if (_documentos is null) return;
        try
        {
            var r = _documentos.GenerarListaReposicion(Filas.ToList(), HorizonteDias, DateOnly.FromDateTime(DateTime.Today), _usuarioActualId);
            Mensaje = $"Lista generada: {r.RutaCompleta}";
        }
        catch (ErrorValidacionException ex) { Mensaje = ex.Message; }
    }
}
