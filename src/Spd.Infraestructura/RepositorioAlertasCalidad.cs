using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Alertas de calidad y su trazabilidad inversa (Spec 018). SQL explícito (Art. VIII.4).
/// La coincidencia de lote se decide en el dominio (<see cref="AlertaCalidad.Afecta"/>), no en SQL:
/// SQLite no sabe quitar guiones y espacios, y la regla debe ser una sola.</summary>
public sealed class RepositorioAlertasCalidad(SqliteConnection conexion) : IRepositorioAlertasCalidad
{
    private const string Columnas =
        "id, referencia, fecha_alerta, cn, lote, descripcion, activa, fecha_cierre, motivo_cierre";

    public int Crear(AlertaCalidad alerta, int? usuarioId)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO AlertaCalidad (referencia, fecha_alerta, cn, lote, lote_normalizado, descripcion, activa, creado_por)
            VALUES (@Referencia, @FechaAlerta, @Cn, @Lote, @LoteNormalizado, @Descripcion, 1, @usuarioId);
            SELECT last_insert_rowid();
            """,
            new
            {
                alerta.Referencia, FechaAlerta = alerta.FechaAlerta.ToString("yyyy-MM-dd"), alerta.Cn, alerta.Lote,
                LoteNormalizado = AlertaCalidad.NormalizarLote(alerta.Lote), alerta.Descripcion, usuarioId
            });

    public void Cerrar(int alertaId, DateOnly fecha, string motivo, int? usuarioId)
        => conexion.Execute(
            """
            UPDATE AlertaCalidad SET activa = 0, fecha_cierre = @fecha, motivo_cierre = @motivo,
                modificado_en = @ahora, modificado_por = @usuarioId
            WHERE id = @alertaId
            """,
            new { alertaId, fecha = fecha.ToString("yyyy-MM-dd"), motivo, usuarioId, ahora = DateTime.UtcNow.ToString("o") });

    public AlertaCalidad? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<FilaAlerta>($"SELECT {Columnas} FROM AlertaCalidad WHERE id = @id", new { id })?.AAlerta();

    public IReadOnlyList<AlertaCalidad> Listar(bool soloActivas)
        => conexion.Query<FilaAlerta>(
                $"SELECT {Columnas} FROM AlertaCalidad {(soloActivas ? "WHERE activa = 1" : string.Empty)} ORDER BY activa DESC, fecha_alerta DESC, id DESC")
            .Select(f => f.AAlerta()).ToList();

    public IReadOnlyList<EnvaseAfectado> EnvasesAfectados(AlertaCalidad alerta)
        => conexion.Query<FilaEnvase>(
                """
                SELECT e.id AS EnvaseId, e.paciente_id AS PacienteId, p.nombre || ' ' || p.apellidos AS Paciente,
                       e.serie AS Serie, e.lote AS Lote, e.estado AS Estado, e.unidades_restantes AS UnidadesRestantes,
                       m.cn AS Cn
                FROM Envase e
                JOIN Medicamento m ON m.id = e.medicamento_id
                JOIN Paciente p ON p.id = e.paciente_id
                WHERE e.lote IS NOT NULL AND substr(m.cn, 1, 6) = @cn
                ORDER BY p.apellidos, p.nombre, e.id
                """,
                new { cn = AlertaCalidad.NormalizarCn(alerta.Cn) })
            .Where(f => alerta.Afecta(f.Cn, f.Lote))
            .Select(f => new EnvaseAfectado((int)f.EnvaseId, (int)f.PacienteId, f.Paciente, f.Serie, f.Lote,
                Enum.Parse<EstadoEnvase>(f.Estado), (int?)f.UnidadesRestantes))
            .ToList();

    public IReadOnlyList<SpdAfectado> BlisteresAfectados(AlertaCalidad alerta)
        => conexion.Query<FilaSpd>(
                """
                SELECT DISTINCT s.id AS SpdId, s.num_registro AS NumRegistro, s.paciente_id AS PacienteId,
                       p.nombre || ' ' || p.apellidos AS Paciente, s.estado AS Estado,
                       s.validez_desde AS ValidezDesde, s.validez_hasta AS ValidezHasta,
                       COALESCE(sle.snap_lote, e.lote) AS Lote, COALESCE(sle.snap_cn, m.cn) AS Cn
                FROM SPD_Linea_Envase sle
                JOIN SPD_Linea l ON l.id = sle.spd_linea_id
                JOIN SPD s ON s.id = l.spd_id
                JOIN Envase e ON e.id = sle.envase_id
                JOIN Medicamento m ON m.id = e.medicamento_id
                JOIN Paciente p ON p.id = s.paciente_id
                WHERE substr(COALESCE(sle.snap_cn, m.cn), 1, 6) = @cn
                ORDER BY s.validez_desde DESC, s.id DESC
                """,
                new { cn = AlertaCalidad.NormalizarCn(alerta.Cn) })
            .Where(f => alerta.Afecta(f.Cn, f.Lote))
            .GroupBy(f => f.SpdId).Select(g => g.First())
            .Select(f => new SpdAfectado((int)f.SpdId, f.NumRegistro, (int)f.PacienteId, f.Paciente, Enum.Parse<EstadoSpd>(f.Estado),
                DateOnly.Parse(f.ValidezDesde, CultureInfo.InvariantCulture), DateOnly.Parse(f.ValidezHasta, CultureInfo.InvariantCulture), f.Lote))
            .ToList();

    private sealed class FilaAlerta
    {
        public long Id { get; set; }
        public string Referencia { get; set; } = "";
        public string FechaAlerta { get; set; } = "";
        public string Cn { get; set; } = "";
        public string Lote { get; set; } = "";
        public string? Descripcion { get; set; }
        public long Activa { get; set; }
        public string? FechaCierre { get; set; }
        public string? MotivoCierre { get; set; }

        public AlertaCalidad AAlerta() => new()
        {
            Id = (int)Id, Referencia = Referencia, FechaAlerta = DateOnly.Parse(FechaAlerta, CultureInfo.InvariantCulture),
            Cn = Cn, Lote = Lote, Descripcion = Descripcion, Activa = Activa != 0,
            FechaCierre = FechaCierre is null ? null : DateOnly.Parse(FechaCierre, CultureInfo.InvariantCulture),
            MotivoCierre = MotivoCierre
        };
    }

    private sealed class FilaEnvase
    {
        public long EnvaseId { get; set; }
        public long PacienteId { get; set; }
        public string Paciente { get; set; } = "";
        public string? Serie { get; set; }
        public string? Lote { get; set; }
        public string Estado { get; set; } = "";
        public long? UnidadesRestantes { get; set; }
        public string Cn { get; set; } = "";
    }

    private sealed class FilaSpd
    {
        public long SpdId { get; set; }
        public string NumRegistro { get; set; } = "";
        public long PacienteId { get; set; }
        public string Paciente { get; set; } = "";
        public string Estado { get; set; } = "";
        public string ValidezDesde { get; set; } = "";
        public string ValidezHasta { get; set; } = "";
        public string? Lote { get; set; }
        public string Cn { get; set; } = "";
    }
}
