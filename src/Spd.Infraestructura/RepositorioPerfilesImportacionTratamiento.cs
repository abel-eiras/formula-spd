using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a PerfilImportacionTratamiento vía Dapper/SQLite (FR-572).</summary>
public sealed class RepositorioPerfilesImportacionTratamiento(SqliteConnection conexion)
    : IRepositorioPerfilesImportacionTratamiento
{
    public int Crear(PerfilImportacionTratamiento perfil)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO PerfilImportacionTratamiento (nombre, origen, separador, tiene_cabecera, mapeo)
            VALUES (@Nombre, @Origen, @Separador, @TieneCabecera, @Mapeo)
            RETURNING id
            """,
            new
            {
                perfil.Nombre,
                Origen = perfil.Origen.ToString(),
                perfil.Separador,
                TieneCabecera = perfil.TieneCabecera.HasValue ? (perfil.TieneCabecera.Value ? 1 : 0) : (int?)null,
                Mapeo = JsonSerializer.Serialize(perfil.Mapeo)
            });

    public IReadOnlyList<PerfilImportacionTratamiento> Listar()
        => conexion.Query<PerfilFila>("SELECT id, nombre, origen, separador, tiene_cabecera, mapeo FROM PerfilImportacionTratamiento ORDER BY nombre")
            .Select(f => f.APerfil())
            .ToList();

    private sealed record PerfilFila(long Id, string Nombre, string Origen, string? Separador, long? TieneCabecera, string Mapeo)
    {
        public PerfilImportacionTratamiento APerfil() => new()
        {
            Id = (int)Id,
            Nombre = Nombre,
            Origen = Enum.Parse<OrigenImportacionTratamiento>(Origen),
            Separador = Separador,
            TieneCabecera = TieneCabecera is null ? null : TieneCabecera == 1,
            Mapeo = JsonSerializer.Deserialize<MapeoColumnasImportacion>(Mapeo)!
        };
    }
}
