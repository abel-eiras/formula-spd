namespace Spd.Dominio;

/// <summary>Traza de auditoría de toda acción de escritura (Art. VII.6). Solo inserción (Art. III.3).</summary>
public interface IRegistradorAuditoria
{
    void Registrar(int? usuarioId, string accion, string entidad, int? entidadId, string? detalle);
}
