using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a PerfilImportacion vía Dapper/SQLite (FR-1100).</summary>
public sealed class RepositorioPerfilesImportacion(SqliteConnection conexion) : IRepositorioPerfilesImportacion
{
    private const string Columnas = "id, nombre, tipo, separador, codificacion, tiene_cabecera, mapeo, regex_unidades_envase";

    public int Crear(PerfilImportacion p)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO PerfilImportacion (nombre, tipo, separador, codificacion, tiene_cabecera, mapeo, regex_unidades_envase)
            VALUES (@Nombre, @Tipo, @Separador, @Codificacion, @TieneCabecera, @Mapeo, @RegexUnidadesEnvase)
            RETURNING id
            """,
            AParametros(p));

    public void Actualizar(PerfilImportacion p)
        => conexion.Execute(
            """
            UPDATE PerfilImportacion SET
                tipo = @Tipo, separador = @Separador, codificacion = @Codificacion, tiene_cabecera = @TieneCabecera,
                mapeo = @Mapeo, regex_unidades_envase = @RegexUnidadesEnvase, modificado_en = @ModificadoEn
            WHERE id = @Id
            """,
            new
            {
                p.Id, Tipo = p.Tipo.ToString(), p.Separador, p.Codificacion, TieneCabecera = p.TieneCabecera ? 1 : 0,
                Mapeo = JsonSerializer.Serialize(p.Mapeo), p.RegexUnidadesEnvase, ModificadoEn = DateTime.UtcNow.ToString("o")
            });

    public PerfilImportacion? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<PerfilFila>($"SELECT {Columnas} FROM PerfilImportacion WHERE id = @id", new { id })
            ?.APerfil();

    public PerfilImportacion? ObtenerPorNombre(string nombre)
        => conexion.QuerySingleOrDefault<PerfilFila>($"SELECT {Columnas} FROM PerfilImportacion WHERE nombre = @nombre", new { nombre })
            ?.APerfil();

    public IReadOnlyList<PerfilImportacion> ListarPorTipo(TipoPerfilImportacion tipo)
        => conexion.Query<PerfilFila>($"SELECT {Columnas} FROM PerfilImportacion WHERE tipo = @tipo ORDER BY nombre", new { tipo = tipo.ToString() })
            .Select(f => f.APerfil())
            .ToList();

    private static object AParametros(PerfilImportacion p) => new
    {
        p.Nombre, Tipo = p.Tipo.ToString(), p.Separador, p.Codificacion, TieneCabecera = p.TieneCabecera ? 1 : 0,
        Mapeo = JsonSerializer.Serialize(p.Mapeo), p.RegexUnidadesEnvase
    };

    private sealed record PerfilFila(
        long Id, string Nombre, string Tipo, string Separador, string Codificacion, long TieneCabecera,
        string Mapeo, string? RegexUnidadesEnvase)
    {
        public PerfilImportacion APerfil() => new()
        {
            Id = (int)Id,
            Nombre = Nombre,
            Tipo = Enum.Parse<TipoPerfilImportacion>(Tipo),
            Separador = Separador,
            Codificacion = Codificacion,
            TieneCabecera = TieneCabecera == 1,
            Mapeo = JsonSerializer.Deserialize<List<ParCampoColumna>>(Mapeo)!,
            RegexUnidadesEnvase = RegexUnidadesEnvase
        };
    }
}
