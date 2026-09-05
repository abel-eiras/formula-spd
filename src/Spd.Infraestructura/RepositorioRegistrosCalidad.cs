using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a los registros de calidad de uso diario vía Dapper/SQLite. Solo alta,
/// nunca actualización ni eliminación (Art. III.1).</summary>
public sealed class RepositorioRegistrosCalidad(SqliteConnection conexion) : IRepositorioRegistrosCalidad
{
    public int Crear(FormacionPersonal formacion)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO FormacionPersonal (usuario_id, nombre_curso, entidad_organizadora, fecha, acreditado)
            VALUES (@UsuarioId, @NombreCurso, @EntidadOrganizadora, @Fecha, @Acreditado) RETURNING id
            """,
            new
            {
                formacion.UsuarioId, formacion.NombreCurso, formacion.EntidadOrganizadora,
                Fecha = formacion.Fecha.ToString("yyyy-MM-dd"), Acreditado = formacion.Acreditado ? 1 : 0
            });

    public IReadOnlyList<FormacionPersonal> ListarFormacion(int usuarioId)
        => conexion.Query<FormacionPersonalFila>(
                """
                SELECT id, usuario_id, nombre_curso, entidad_organizadora, fecha, acreditado
                FROM FormacionPersonal WHERE usuario_id = @usuarioId ORDER BY fecha DESC
                """,
                new { usuarioId })
            .Select(f => f.AFormacion())
            .ToList();

    public int Crear(RecogidaResiduos recogida)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO RecogidaResiduos (fecha, empresa_gestora, usuario_id, observaciones)
            VALUES (@Fecha, @EmpresaGestora, @UsuarioId, @Observaciones) RETURNING id
            """,
            new
            {
                Fecha = recogida.Fecha.ToString("yyyy-MM-dd"), recogida.EmpresaGestora,
                recogida.UsuarioId, recogida.Observaciones
            });

    public IReadOnlyList<RecogidaResiduos> ListarRecogidaResiduos()
        => conexion.Query<RecogidaResiduosFila>(
                "SELECT id, fecha, empresa_gestora, usuario_id, observaciones FROM RecogidaResiduos ORDER BY fecha DESC")
            .Select(f => f.ARecogida())
            .ToList();

    private sealed record FormacionPersonalFila(
        long Id, long UsuarioId, string NombreCurso, string? EntidadOrganizadora, string Fecha, long Acreditado)
    {
        public FormacionPersonal AFormacion() => new()
        {
            Id = (int)Id, UsuarioId = (int)UsuarioId, NombreCurso = NombreCurso,
            EntidadOrganizadora = EntidadOrganizadora,
            Fecha = DateOnly.ParseExact(Fecha, "yyyy-MM-dd"), Acreditado = Acreditado == 1
        };
    }

    private sealed record RecogidaResiduosFila(long Id, string Fecha, string EmpresaGestora, long UsuarioId, string? Observaciones)
    {
        public RecogidaResiduos ARecogida() => new()
        {
            Id = (int)Id, Fecha = DateOnly.ParseExact(Fecha, "yyyy-MM-dd"), EmpresaGestora = EmpresaGestora,
            UsuarioId = (int)UsuarioId, Observaciones = Observaciones
        };
    }
}
