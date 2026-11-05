using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Traza de auditoría de toda acción de escritura (Art. VII.6). Solo INSERT (Art. III.3).
/// Cada entrada se sella con el hash de la anterior (Spec 017) para que una manipulación por fuera de
/// la aplicación se pueda detectar.</summary>
public sealed class RegistradorAuditoria(SqliteConnection conexion) : IRegistradorAuditoria, IRepositorioAuditoria
{
    public void Registrar(int? usuarioId, string accion, string entidad, int? entidadId, string? detalle)
    {
        var fechaHora = DateTime.UtcNow.ToString("o");
        // Una sola conexión de por vida en la aplicación: leer el último sello e insertar no se
        // intercalan con otra escritura de auditoría.
        var hashPrevio = CadenaAuditoria.HashPrevioPara(
            conexion.ExecuteScalar<string?>("SELECT hash FROM Auditoria WHERE hash IS NOT NULL ORDER BY id DESC LIMIT 1"));
        var hash = CadenaAuditoria.CalcularHash(hashPrevio, fechaHora, usuarioId, accion, entidad, entidadId, detalle);
        conexion.Execute(
            """
            INSERT INTO Auditoria (fecha_hora, usuario_id, accion, entidad, entidad_id, detalle, hash_previo, hash)
            VALUES (@fechaHora, @usuarioId, @accion, @entidad, @entidadId, @detalle, @hashPrevio, @hash)
            """,
            new { fechaHora, usuarioId, accion, entidad, entidadId, detalle, hashPrevio, hash });
    }

    public IReadOnlyList<EntradaAuditoria> ListarEnOrden()
        => conexion.Query<FilaAuditoria>(
                "SELECT id, fecha_hora, usuario_id, accion, entidad, entidad_id, detalle, hash_previo, hash FROM Auditoria ORDER BY id")
            .Select(f => new EntradaAuditoria(f.Id, f.FechaHora, (int?)f.UsuarioId, f.Accion, f.Entidad, (int?)f.EntidadId,
                f.Detalle, f.HashPrevio, f.Hash))
            .ToList();

    // SQLite devuelve los enteros como Int64: se leen así y se convierten al tipo del dominio.
    private sealed class FilaAuditoria
    {
        public long Id { get; set; }
        public string FechaHora { get; set; } = "";
        public long? UsuarioId { get; set; }
        public string Accion { get; set; } = "";
        public string Entidad { get; set; } = "";
        public long? EntidadId { get; set; }
        public string? Detalle { get; set; }
        public string? HashPrevio { get; set; }
        public string? Hash { get; set; }
    }
}
