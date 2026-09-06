using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a MaterialAcondicionamiento vía Dapper/SQLite (FR-632).</summary>
public sealed class RepositorioMaterialAcondicionamiento(SqliteConnection conexion) : IRepositorioMaterialAcondicionamiento
{
    private const string FormatoFecha = "yyyy-MM-dd";
    private const string Columnas = "id, descripcion, lote, fecha_entrada, activo";

    public int Crear(MaterialAcondicionamiento m)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO MaterialAcondicionamiento (descripcion, lote, fecha_entrada, activo)
            VALUES (@Descripcion, @Lote, @FechaEntrada, @Activo)
            RETURNING id
            """,
            new { m.Descripcion, m.Lote, FechaEntrada = m.FechaEntrada.ToString(FormatoFecha, CultureInfo.InvariantCulture), Activo = m.Activo ? 1 : 0 });

    public MaterialAcondicionamiento? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<MaterialFila>($"SELECT {Columnas} FROM MaterialAcondicionamiento WHERE id = @id", new { id })
            ?.AMaterial();

    public IReadOnlyList<MaterialAcondicionamiento> ListarActivos()
        => conexion.Query<MaterialFila>($"SELECT {Columnas} FROM MaterialAcondicionamiento WHERE activo = 1 ORDER BY descripcion")
            .Select(f => f.AMaterial())
            .ToList();

    private sealed record MaterialFila(long Id, string Descripcion, string Lote, string FechaEntrada, long Activo)
    {
        public MaterialAcondicionamiento AMaterial() => new()
        {
            Id = (int)Id,
            Descripcion = Descripcion,
            Lote = Lote,
            FechaEntrada = DateOnly.ParseExact(FechaEntrada, FormatoFecha, CultureInfo.InvariantCulture),
            Activo = Activo == 1
        };
    }
}
