using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Una interpretación posible de un escaneo ambiguo, con el texto que se enseña para elegirla.</summary>
public sealed record CandidataLectura(DatosEnvaseEscaneado Datos)
{
    public string Texto => $"Lote {Datos.Lote} · Serie {Datos.NumeroSerie} · Caduca {Datos.Caducidad.ComoFecha():dd/MM/yyyy}";
}

/// <summary>spec-012 FR-1210: cuando el lector se come el separador GS y la cadena admite varias
/// lecturas, la aplicación no adivina. Enseña las candidatas y la persona elige la que coincide con el
/// lote impreso en la caja. Lo usan todas las pantallas que registran envases.</summary>
public sealed partial class LecturaAmbiguaViewModel : ViewModelBase
{
    private Action<DatosEnvaseEscaneado>? _alElegir;

    [ObservableProperty] private bool _visible;
    public ObservableCollection<CandidataLectura> Candidatas { get; } = [];

    public void Mostrar(LecturaGs1 lectura, Action<DatosEnvaseEscaneado> alElegir)
    {
        Candidatas.Clear();
        foreach (var candidata in lectura.Candidatas) Candidatas.Add(new CandidataLectura(candidata));
        _alElegir = alElegir;
        Visible = Candidatas.Count > 0;
    }

    public void Ocultar()
    {
        Candidatas.Clear();
        _alElegir = null;
        Visible = false;
    }

    [RelayCommand]
    private void Elegir(CandidataLectura? candidata)
    {
        if (candidata is null || _alElegir is not { } accion) return;
        Ocultar();
        accion(candidata.Datos);
    }

    [RelayCommand]
    private void Descartar() => Ocultar();
}
