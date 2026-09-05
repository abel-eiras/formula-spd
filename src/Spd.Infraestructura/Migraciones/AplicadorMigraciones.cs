using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Spd.Infraestructura.Migraciones;

/// <summary>Aplica los scripts SQL numerados embebidos, de forma idempotente y nunca destructiva (Art. VIII.3).</summary>
public sealed class AplicadorMigraciones(SqliteConnection conexion)
{
    private static readonly Regex PatronNombre = new(@"(\d{4})_[^.]+\.sql$", RegexOptions.Compiled);

    public void Aplicar()
    {
        AsegurarTablaVersion();
        var versionActual = ObtenerVersionActual();
        foreach (var (version, sql) in ObtenerMigracionesEmbebidas())
        {
            if (version <= versionActual) continue;
            AplicarUna(version, sql);
        }
    }

    private void AplicarUna(int version, string sql)
    {
        using var transaccion = conexion.BeginTransaction();
        conexion.Execute(sql, transaction: transaccion);
        conexion.Execute(
            "INSERT INTO schema_version (version, aplicado_en) VALUES (@version, @ahora)",
            new { version, ahora = DateTime.UtcNow.ToString("o") },
            transaccion);
        transaccion.Commit();
    }

    private void AsegurarTablaVersion()
        => conexion.Execute(
            "CREATE TABLE IF NOT EXISTS schema_version (version INTEGER PRIMARY KEY, aplicado_en TEXT NOT NULL)");

    private int ObtenerVersionActual()
        => conexion.ExecuteScalar<int?>("SELECT MAX(version) FROM schema_version") ?? 0;

    private static IEnumerable<(int Version, string Sql)> ObtenerMigracionesEmbebidas()
    {
        var ensamblado = typeof(AplicadorMigraciones).Assembly;
        var migraciones = new List<(int Version, string Sql)>();
        foreach (var nombreRecurso in ensamblado.GetManifestResourceNames())
        {
            var coincidencia = PatronNombre.Match(nombreRecurso);
            if (!coincidencia.Success) continue;

            var version = int.Parse(coincidencia.Groups[1].Value);
            using var flujo = ensamblado.GetManifestResourceStream(nombreRecurso)!;
            using var lector = new StreamReader(flujo);
            migraciones.Add((version, lector.ReadToEnd()));
        }
        return migraciones.OrderBy(m => m.Version);
    }
}
