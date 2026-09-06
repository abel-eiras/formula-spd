using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a SPD_Linea_Envase vía Dapper/SQLite. Solo alta (Art. III): la instantánea de
/// qué envase cubrió qué unidades no se corrige, se recalcula en una reelaboración (FR-6124).</summary>
public sealed class RepositorioSpdLineaEnvases(SqliteConnection conexion) : IRepositorioSpdLineaEnvases
{
    private const string FormatoFecha = "yyyy-MM-dd";
    private const string Columnas = "id, spd_linea_id, envase_id, unidades_tomadas, snap_serie, snap_lote, snap_caducidad";

    public int Crear(SpdLineaEnvase f)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO SPD_Linea_Envase (spd_linea_id, envase_id, unidades_tomadas, snap_serie, snap_lote, snap_caducidad)
            VALUES (@SpdLineaId, @EnvaseId, @UnidadesTomadas, @SnapSerie, @SnapLote, @SnapCaducidad)
            RETURNING id
            """,
            new
            {
                f.SpdLineaId, f.EnvaseId, f.UnidadesTomadas, f.SnapSerie, f.SnapLote,
                SnapCaducidad = f.SnapCaducidad?.ToString(FormatoFecha, CultureInfo.InvariantCulture)
            });

    public void Actualizar(SpdLineaEnvase f)
        => conexion.Execute(
            "UPDATE SPD_Linea_Envase SET unidades_tomadas = @UnidadesTomadas WHERE id = @Id",
            new { f.Id, f.UnidadesTomadas });

    public IReadOnlyList<SpdLineaEnvase> ListarPorLinea(int spdLineaId)
        => conexion.Query<Fila>($"SELECT {Columnas} FROM SPD_Linea_Envase WHERE spd_linea_id = @spdLineaId", new { spdLineaId })
            .Select(f => f.AFila())
            .ToList();

    public IReadOnlyList<SpdLineaEnvase> ListarPorSpd(int spdId)
        => conexion.Query<Fila>(
                $"""
                SELECT {string.Join(", ", Columnas.Split(", ").Select(c => "sle." + c))} FROM SPD_Linea_Envase sle
                JOIN SPD_Linea sl ON sl.id = sle.spd_linea_id
                WHERE sl.spd_id = @spdId
                """,
                new { spdId })
            .Select(f => f.AFila())
            .ToList();

    // Dapper 2.1.79: mismo hallazgo que Spec 009 — el constructor debe tipar `double`, no
    // `decimal`, para una columna REAL, o la materialización rápida por constructor falla.
    private sealed record Fila(long Id, long SpdLineaId, long EnvaseId, double UnidadesTomadas, string? SnapSerie, string? SnapLote, string? SnapCaducidad)
    {
        public SpdLineaEnvase AFila() => new()
        {
            Id = (int)Id,
            SpdLineaId = (int)SpdLineaId,
            EnvaseId = (int)EnvaseId,
            UnidadesTomadas = (decimal)UnidadesTomadas,
            SnapSerie = SnapSerie,
            SnapLote = SnapLote,
            SnapCaducidad = SnapCaducidad is null ? null : DateOnly.ParseExact(SnapCaducidad, FormatoFecha, CultureInfo.InvariantCulture)
        };
    }
}
