using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a RegistroAmbiental vía Dapper/SQLite (FR-630/631). Solo alta (Art. III.1).</summary>
public sealed class RepositorioRegistrosAmbientales(SqliteConnection conexion) : IRepositorioRegistrosAmbientales
{
    private const string Columnas = "id, fecha, temperatura, humedad, fuera_rango, usuario_id";

    public int Crear(RegistroAmbiental r)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO RegistroAmbiental (fecha, temperatura, humedad, fuera_rango, usuario_id)
            VALUES (@Fecha, @Temperatura, @Humedad, @FueraRango, @UsuarioId)
            RETURNING id
            """,
            new { Fecha = r.Fecha.ToString("o"), r.Temperatura, r.Humedad, FueraRango = r.FueraRango ? 1 : 0, r.UsuarioId });

    public RegistroAmbiental? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<RegistroAmbientalFila>($"SELECT {Columnas} FROM RegistroAmbiental WHERE id = @id", new { id })
            ?.ARegistro();

    public RegistroAmbiental? ObtenerUltimo()
        => conexion.QuerySingleOrDefault<RegistroAmbientalFila>($"SELECT {Columnas} FROM RegistroAmbiental ORDER BY id DESC LIMIT 1")
            ?.ARegistro();

    private sealed record RegistroAmbientalFila(long Id, string Fecha, double Temperatura, double Humedad, long FueraRango, long? UsuarioId)
    {
        public RegistroAmbiental ARegistro() => new()
        {
            Id = (int)Id,
            Fecha = DateTime.Parse(Fecha, System.Globalization.CultureInfo.InvariantCulture),
            Temperatura = Temperatura,
            Humedad = Humedad,
            FueraRango = FueraRango == 1,
            UsuarioId = UsuarioId is null ? null : (int)UsuarioId
        };
    }
}
