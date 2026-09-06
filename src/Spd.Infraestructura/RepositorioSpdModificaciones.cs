using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a SPD_Modificacion vía Dapper/SQLite. Solo alta: historial íntegro de
/// reelaboraciones (Art. III.4).</summary>
public sealed class RepositorioSpdModificaciones(SqliteConnection conexion) : IRepositorioSpdModificaciones
{
    private const string Columnas = """
        id, spd_id, version_anterior, version_nueva, fecha, usuario_id, origen_solicitud, motivo,
        resumen_cambios, lineas_snapshot_anterior
        """;

    public int Crear(SpdModificacion m)
        => conexion.ExecuteScalar<int>(
            $"""
            INSERT INTO SPD_Modificacion (
                spd_id, version_anterior, version_nueva, fecha, usuario_id, origen_solicitud, motivo,
                resumen_cambios, lineas_snapshot_anterior
            ) VALUES (
                @SpdId, @VersionAnterior, @VersionNueva, @Fecha, @UsuarioId, @OrigenSolicitud, @Motivo,
                @ResumenCambios, @LineasSnapshotAnterior
            ) RETURNING id
            """,
            new
            {
                m.SpdId, m.VersionAnterior, m.VersionNueva, Fecha = m.Fecha.ToString("o"), m.UsuarioId,
                OrigenSolicitud = m.OrigenSolicitud.ToString(), m.Motivo, m.ResumenCambios, m.LineasSnapshotAnterior
            });

    public IReadOnlyList<SpdModificacion> ListarPorSpd(int spdId)
        => conexion.Query<Fila>($"SELECT {Columnas} FROM SPD_Modificacion WHERE spd_id = @spdId ORDER BY id", new { spdId })
            .Select(f => f.AModificacion())
            .ToList();

    private sealed record Fila(
        long Id, long SpdId, long VersionAnterior, long VersionNueva, string Fecha, long UsuarioId,
        string OrigenSolicitud, string Motivo, string ResumenCambios, string LineasSnapshotAnterior)
    {
        public SpdModificacion AModificacion() => new()
        {
            Id = (int)Id,
            SpdId = (int)SpdId,
            VersionAnterior = (int)VersionAnterior,
            VersionNueva = (int)VersionNueva,
            Fecha = DateTime.Parse(Fecha, System.Globalization.CultureInfo.InvariantCulture),
            UsuarioId = (int)UsuarioId,
            OrigenSolicitud = Enum.Parse<OrigenSolicitudReelaboracion>(OrigenSolicitud),
            Motivo = Motivo,
            ResumenCambios = ResumenCambios,
            LineasSnapshotAnterior = LineasSnapshotAnterior
        };
    }
}
