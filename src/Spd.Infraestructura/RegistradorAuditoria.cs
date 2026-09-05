using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Traza de auditoría de toda acción de escritura (Art. VII.6). Solo INSERT (Art. III.3).</summary>
public sealed class RegistradorAuditoria(SqliteConnection conexion) : IRegistradorAuditoria
{
    public void Registrar(int? usuarioId, string accion, string entidad, int? entidadId, string? detalle)
        => conexion.Execute(
            """
            INSERT INTO Auditoria (fecha_hora, usuario_id, accion, entidad, entidad_id, detalle)
            VALUES (@FechaHora, @usuarioId, @accion, @entidad, @entidadId, @detalle)
            """,
            new { FechaHora = DateTime.UtcNow.ToString("o"), usuarioId, accion, entidad, entidadId, detalle });
}
