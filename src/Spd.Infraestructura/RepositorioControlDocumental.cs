using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso al control documental del PNT vía Dapper/SQLite. Solo alta (Art. III.1). La
/// restricción de "solo Administrador" (FR-942) se comprueba en la capa de Aplicación, no aquí.</summary>
public sealed class RepositorioControlDocumental(SqliteConnection conexion) : IRepositorioControlDocumental
{
    public int Crear(ControlCambiosPNT cambio)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO ControlCambiosPNT (documento, version, descripcion_cambio, fecha, redactado_por, revisado_por, aprobado_por)
            VALUES (@Documento, @Version, @DescripcionCambio, @Fecha, @RedactadoPor, @RevisadoPor, @AprobadoPor)
            RETURNING id
            """,
            new
            {
                cambio.Documento, cambio.Version, cambio.DescripcionCambio,
                Fecha = cambio.Fecha.ToString("yyyy-MM-dd"), cambio.RedactadoPor, cambio.RevisadoPor, cambio.AprobadoPor
            });

    public IReadOnlyList<ControlCambiosPNT> ListarCambiosPnt()
        => conexion.Query<ControlCambiosPntFila>(
                """
                SELECT id, documento, version, descripcion_cambio, fecha, redactado_por, revisado_por, aprobado_por
                FROM ControlCambiosPNT ORDER BY fecha DESC
                """)
            .Select(f => f.ACambio())
            .ToList();

    public int Crear(ControlCopias copia)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO ControlCopias (documento, num_copia, usuario_id, fecha)
            VALUES (@Documento, @NumCopia, @UsuarioId, @Fecha) RETURNING id
            """,
            new { copia.Documento, copia.NumCopia, copia.UsuarioId, Fecha = copia.Fecha.ToString("yyyy-MM-dd") });

    public IReadOnlyList<ControlCopias> ListarCopias()
        => conexion.Query<ControlCopiasFila>(
                "SELECT id, documento, num_copia, usuario_id, fecha FROM ControlCopias ORDER BY fecha DESC")
            .Select(f => f.ACopia())
            .ToList();

    private sealed record ControlCambiosPntFila(
        long Id, string Documento, string Version, string DescripcionCambio, string Fecha,
        long RedactadoPor, long RevisadoPor, long AprobadoPor)
    {
        public ControlCambiosPNT ACambio() => new()
        {
            Id = (int)Id, Documento = Documento, Version = Version, DescripcionCambio = DescripcionCambio,
            Fecha = DateOnly.ParseExact(Fecha, "yyyy-MM-dd"), RedactadoPor = (int)RedactadoPor,
            RevisadoPor = (int)RevisadoPor, AprobadoPor = (int)AprobadoPor
        };
    }

    private sealed record ControlCopiasFila(long Id, string Documento, long NumCopia, long UsuarioId, string Fecha)
    {
        public ControlCopias ACopia() => new()
        {
            Id = (int)Id, Documento = Documento, NumCopia = (int)NumCopia, UsuarioId = (int)UsuarioId,
            Fecha = DateOnly.ParseExact(Fecha, "yyyy-MM-dd")
        };
    }
}
