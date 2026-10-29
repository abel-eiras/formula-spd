using CommunityToolkit.Mvvm.ComponentModel;
using Spd.Aplicacion;
using Spd.Dominio;

namespace Spd.Presentacion.ViewModels;

/// <summary>Spec 005 FR-513 / Spec 006 FR-620, compartido por las tres altas de envase (Depósito,
/// Retirada, Preparación): las unidades iniciales se proponen desde el catálogo, y si el catálogo no
/// las sabe se ofrece guardarlas. La casilla empieza desmarcada a propósito: el envase puede darse de
/// alta ya empezado, y guardar esas unidades parciales como tamaño de envase estropearía el cálculo de
/// envases a retirar de todos los pacientes (FR-531).</summary>
public sealed partial class UnidadesDelCatalogo : ObservableObject
{
    private int? _medicamentoId;
    private int? _propuestas;

    [ObservableProperty] private bool _ofrecerGuardar;
    [ObservableProperty] private bool _guardar;

    /// <summary>Devuelve las unidades que debe mostrar el formulario tras elegir otro medicamento. Lo
    /// que el usuario ya haya tecleado se respeta; lo que había propuesto la aplicación se sustituye.</summary>
    public int? AlCambiarMedicamento(Medicamento? medicamento, int? unidadesActuales)
    {
        var tecleadasPorElUsuario = unidadesActuales is not null && unidadesActuales != _propuestas;
        _medicamentoId = medicamento?.Id;
        _propuestas = medicamento?.UnidadesEnvase;
        OfrecerGuardar = medicamento is not null && medicamento.UnidadesEnvase is null;
        Guardar = false;
        return tecleadasPorElUsuario ? unidadesActuales : _propuestas;
    }

    public void GuardarSiProcede(IServicioMedicamentos servicio, int unidades, int? usuarioId)
    {
        if (Guardar && OfrecerGuardar && _medicamentoId is { } id && unidades > 0)
            servicio.ActualizarUnidadesEnvase(id, unidades, usuarioId);
    }

    public void Reiniciar()
    {
        _medicamentoId = null;
        _propuestas = null;
        OfrecerGuardar = false;
        Guardar = false;
    }
}
