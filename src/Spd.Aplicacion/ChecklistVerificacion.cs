namespace Spd.Aplicacion;

/// <summary>Los cinco ítems obligatorios de FR-651.</summary>
public sealed record ChecklistVerificacion(
    bool Aspecto, bool EtiquetaDatos, bool EtiquetaValidez, bool Instrucciones, bool Contenido)
{
    public bool TodosAptos => Aspecto && EtiquetaDatos && EtiquetaValidez && Instrucciones && Contenido;
}
