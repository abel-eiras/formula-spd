using Spd.Aplicacion;

namespace Spd.Presentacion.ViewModels;

/// <summary>Registros de calidad: formación del personal y recogida de residuos no SIGRE (Spec 009).
/// Agrupa los dos ViewModels de sus pestañas para que la sección sea, como todas, un único ViewModel
/// que el marco puede pintar (Spec 015 H1.5); antes los asignaba a mano la ventana.</summary>
public sealed class RegistrosCalidadViewModel(IServicioRegistrosCalidad servicio, int usuarioActualId) : ViewModelBase
{
    public FormacionPersonalViewModel Formacion { get; } = new(servicio, usuarioActualId);

    public RecogidaResiduosViewModel Residuos { get; } = new(servicio, usuarioActualId);
}
