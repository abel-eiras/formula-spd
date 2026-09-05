using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Aplicacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Revisión del nomenclátor ya descargado (FR-320..FR-322): compara con el catálogo y
/// aplica altas o cambios de nombre solo con confirmación fila a fila. Nunca toca descripción
/// física ni aptitud SPD (FR-321).</summary>
public sealed partial class RevisionNomenclatorViewModel : ViewModelBase
{
    // Misma ruta fija que usa NomenclatorViewModel (Spec 000) al descargar (FR-051/FR-320).
    private static string RutaFicheroDescargado
        => Path.Combine(AppContext.BaseDirectory, "nomenclator", "nomenclator.csv");

    private readonly IServicioImportacionNomenclator _servicio;
    private readonly int? _usuarioActualId;

    [ObservableProperty] private ObservableCollection<FilaNomenclator> _nuevos = [];
    [ObservableProperty] private ObservableCollection<ComparacionFila> _conNombreDistinto = [];
    [ObservableProperty] private int _sinCambios;
    [ObservableProperty] private string? _mensaje;

    public RevisionNomenclatorViewModel(IServicioImportacionNomenclator servicio, int? usuarioActualId)
    {
        _servicio = servicio;
        _usuarioActualId = usuarioActualId;
        Recargar();
    }

    [RelayCommand]
    private void Recargar()
    {
        if (!File.Exists(RutaFicheroDescargado))
        {
            Mensaje = "No hay nomenclátor descargado. Descárguelo primero desde Configuración / Nomenclátor.";
            Nuevos = [];
            ConNombreDistinto = [];
            SinCambios = 0;
            return;
        }

        var resultado = _servicio.CompararConNomenclator(RutaFicheroDescargado);
        if (!resultado.Exito)
        {
            Mensaje = resultado.Error;
            return;
        }

        Nuevos = new ObservableCollection<FilaNomenclator>(resultado.Nuevos);
        ConNombreDistinto = new ObservableCollection<ComparacionFila>(resultado.ConNombreDistinto);
        SinCambios = resultado.SinCambios;
        Mensaje = null;
    }

    [RelayCommand]
    private void AplicarAlta(FilaNomenclator fila)
    {
        _servicio.AplicarAltaDesdeNomenclator(fila, _usuarioActualId);
        Mensaje = $"Medicamento {fila.Cn} creado desde el nomenclátor.";
        Recargar();
    }

    [RelayCommand]
    private void AplicarNombre(ComparacionFila fila)
    {
        _servicio.AplicarNombreDesdeNomenclator(fila.Existente.Id, fila.NombreNomenclator, _usuarioActualId);
        Mensaje = $"Nombre de {fila.Existente.Cn} actualizado desde el nomenclátor.";
        Recargar();
    }
}
