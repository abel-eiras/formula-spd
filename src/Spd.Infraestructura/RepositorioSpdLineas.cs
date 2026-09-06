using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a SPD_Linea vía Dapper/SQLite. Instantánea inmutable salvo `estado_linea`,
/// `incidencias` y `motivo_exclusion` (Art. IV.3).</summary>
public sealed class RepositorioSpdLineas(SqliteConnection conexion) : IRepositorioSpdLineas
{
    private const string Columnas = """
        id, spd_id, tratamiento_id, medicamento_id, snap_nombre, snap_cn, snap_pauta_d, snap_pauta_a,
        snap_pauta_c, snap_pauta_n, snap_dias_semana, snap_desc_texto, snap_momento, unidades_dosis,
        unidades_envase, incidencias, estado_linea, motivo_exclusion
        """;

    public int Crear(SpdLinea l)
        => conexion.ExecuteScalar<int>(
            $"""
            INSERT INTO SPD_Linea (
                spd_id, tratamiento_id, medicamento_id, snap_nombre, snap_cn, snap_pauta_d, snap_pauta_a,
                snap_pauta_c, snap_pauta_n, snap_dias_semana, snap_desc_texto, snap_momento, unidades_dosis,
                unidades_envase, incidencias, estado_linea, motivo_exclusion
            ) VALUES (
                @SpdId, @TratamientoId, @MedicamentoId, @SnapNombre, @SnapCn, @SnapPautaD, @SnapPautaA,
                @SnapPautaC, @SnapPautaN, @SnapDiasSemana, @SnapDescTexto, @SnapMomento, @UnidadesDosis,
                @UnidadesEnvase, @Incidencias, @EstadoLinea, @MotivoExclusion
            ) RETURNING id
            """,
            AParametros(l));

    public void Actualizar(SpdLinea l)
        => conexion.Execute(
            """
            UPDATE SPD_Linea SET
                incidencias = @Incidencias, estado_linea = @EstadoLinea, motivo_exclusion = @MotivoExclusion,
                unidades_dosis = @UnidadesDosis, unidades_envase = @UnidadesEnvase
            WHERE id = @Id
            """,
            AParametros(l));

    public SpdLinea? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<SpdLineaFila>($"SELECT {Columnas} FROM SPD_Linea WHERE id = @id", new { id })?.ALinea();

    public IReadOnlyList<SpdLinea> ListarPorSpd(int spdId)
        => conexion.Query<SpdLineaFila>($"SELECT {Columnas} FROM SPD_Linea WHERE spd_id = @spdId ORDER BY id", new { spdId })
            .Select(f => f.ALinea())
            .ToList();

    private static object AParametros(SpdLinea l) => new
    {
        l.Id,
        l.SpdId,
        l.TratamientoId,
        l.MedicamentoId,
        l.SnapNombre,
        l.SnapCn,
        SnapPautaD = l.SnapPautaD?.ToString(),
        SnapPautaA = l.SnapPautaA?.ToString(),
        SnapPautaC = l.SnapPautaC?.ToString(),
        SnapPautaN = l.SnapPautaN?.ToString(),
        l.SnapDiasSemana,
        l.SnapDescTexto,
        l.SnapMomento,
        l.UnidadesDosis,
        l.UnidadesEnvase,
        l.Incidencias,
        EstadoLinea = l.EstadoLinea.ToString(),
        l.MotivoExclusion
    };

    // Dapper 2.1.79: la materialización rápida de un record por constructor exige que el tipo del
    // parámetro coincida con el tipo real que devuelve el lector (REAL -> double, nunca decimal
    // directamente) o falla con "no matching constructor" (mismo hallazgo que Spec 009).
    private sealed record SpdLineaFila(
        long Id, long SpdId, long TratamientoId, long MedicamentoId, string SnapNombre, string SnapCn,
        string? SnapPautaD, string? SnapPautaA, string? SnapPautaC, string? SnapPautaN, string SnapDiasSemana,
        string? SnapDescTexto, string? SnapMomento, double UnidadesDosis, long UnidadesEnvase, string? Incidencias,
        string EstadoLinea, string? MotivoExclusion)
    {
        public SpdLinea ALinea() => new()
        {
            Id = (int)Id,
            SpdId = (int)SpdId,
            TratamientoId = (int)TratamientoId,
            MedicamentoId = (int)MedicamentoId,
            SnapNombre = SnapNombre,
            SnapCn = SnapCn,
            SnapPautaD = SnapPautaD is null ? null : Enum.Parse<FraccionDosis>(SnapPautaD),
            SnapPautaA = SnapPautaA is null ? null : Enum.Parse<FraccionDosis>(SnapPautaA),
            SnapPautaC = SnapPautaC is null ? null : Enum.Parse<FraccionDosis>(SnapPautaC),
            SnapPautaN = SnapPautaN is null ? null : Enum.Parse<FraccionDosis>(SnapPautaN),
            SnapDiasSemana = SnapDiasSemana,
            SnapDescTexto = SnapDescTexto,
            SnapMomento = SnapMomento,
            UnidadesDosis = (decimal)UnidadesDosis,
            UnidadesEnvase = (int)UnidadesEnvase,
            Incidencias = Incidencias,
            EstadoLinea = Enum.Parse<EstadoLinea>(EstadoLinea),
            MotivoExclusion = MotivoExclusion
        };
    }
}
