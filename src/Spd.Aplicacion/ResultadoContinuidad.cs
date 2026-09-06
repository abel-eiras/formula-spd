using Spd.Dominio;

namespace Spd.Aplicacion;

/// <summary>Resultado de comparar la sesión anterior con la nueva al "preparar siguiente"
/// (FR-673, research.md Decisión 5 de Spec 006). No son estados persistidos de `SpdLinea` (salvo
/// `EnvasePendiente`, que sí lo es): es la guía que la pantalla usa para saber qué revisar.</summary>
public sealed record ResultadoContinuidad(
    IReadOnlyList<SpdLinea> LineasModificadas,
    IReadOnlyList<Tratamiento> TratamientosNuevos,
    IReadOnlyList<SpdLinea> LineasEliminadas,
    IReadOnlyList<SpdLinea> LineasEnvasePendiente);
