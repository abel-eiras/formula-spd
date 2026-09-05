using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a los registros de calidad de uso diario vía Dapper/SQLite. Solo alta,
/// nunca actualización ni eliminación (Art. III.1).</summary>
public sealed class RepositorioRegistrosCalidad(SqliteConnection conexion) : IRepositorioRegistrosCalidad
{
    public int Crear(RegistroAmbiental registro)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO RegistroAmbiental (fecha_hora, temperatura, humedad, usuario_id, observaciones, spd_id, fuera_rango)
            VALUES (@FechaHora, @Temperatura, @Humedad, @UsuarioId, @Observaciones, @SpdId, @FueraDeRango)
            RETURNING id
            """,
            new
            {
                FechaHora = registro.FechaHora.ToString("o"), registro.Temperatura, registro.Humedad,
                registro.UsuarioId, registro.Observaciones, registro.SpdId,
                FueraDeRango = registro.FueraDeRango ? 1 : 0
            });

    public IReadOnlyList<RegistroAmbiental> ListarAmbiental()
        // Alias explícitos: con snake_case desnudo, Dapper 2.1.79 no siempre resuelve por nombre
        // el parámetro `long? SpdId` del constructor de un record con 8 columnas (comprobado con
        // un caso mínimo reproducible), aunque MatchNamesWithUnderscores esté activo. Alias en
        // PascalCase evita depender de esa resolución implícita (Art. XI.2).
        => conexion.Query<RegistroAmbientalFila>(
                """
                SELECT id AS Id, fecha_hora AS FechaHora, temperatura AS Temperatura, humedad AS Humedad,
                       usuario_id AS UsuarioId, observaciones AS Observaciones, spd_id AS SpdId,
                       fuera_rango AS FueraDeRango
                FROM RegistroAmbiental ORDER BY fecha_hora DESC
                """)
            .Select(f => f.ARegistro())
            .ToList();

    public int Crear(RegistroLimpieza registro)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO RegistroLimpieza (fecha, usuario_id, tipo, observaciones)
            VALUES (@Fecha, @UsuarioId, @Tipo, @Observaciones) RETURNING id
            """,
            new
            {
                Fecha = registro.Fecha.ToString("o"), registro.UsuarioId,
                Tipo = TextoTipoLimpieza(registro.Tipo), registro.Observaciones
            });

    public IReadOnlyList<RegistroLimpieza> ListarLimpieza()
        => conexion.Query<RegistroLimpiezaFila>(
                "SELECT id, fecha, usuario_id, tipo, observaciones FROM RegistroLimpieza ORDER BY fecha DESC")
            .Select(f => f.ARegistro())
            .ToList();

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

    private static string TextoTipoLimpieza(TipoLimpieza tipo) => tipo switch
    {
        TipoLimpieza.PrePreparacion => "PRE_PREPARACION",
        TipoLimpieza.PostPreparacion => "POST_PREPARACION",
        TipoLimpieza.Rutinaria => "RUTINARIA",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    private static TipoLimpieza TextoATipoLimpieza(string texto) => texto switch
    {
        "PRE_PREPARACION" => TipoLimpieza.PrePreparacion,
        "POST_PREPARACION" => TipoLimpieza.PostPreparacion,
        "RUTINARIA" => TipoLimpieza.Rutinaria,
        _ => throw new ArgumentOutOfRangeException(nameof(texto))
    };

    private sealed record RegistroAmbientalFila(
        long Id, string FechaHora, double Temperatura, double Humedad, long UsuarioId,
        string? Observaciones, long? SpdId, long FueraDeRango)
    {
        public RegistroAmbiental ARegistro() => new()
        {
            Id = (int)Id,
            FechaHora = DateTime.Parse(FechaHora),
            Temperatura = Temperatura,
            Humedad = Humedad,
            UsuarioId = (int)UsuarioId,
            Observaciones = Observaciones,
            SpdId = SpdId is null ? null : (int)SpdId,
            FueraDeRango = FueraDeRango == 1
        };
    }

    private sealed record RegistroLimpiezaFila(long Id, string Fecha, long UsuarioId, string Tipo, string? Observaciones)
    {
        public RegistroLimpieza ARegistro() => new()
        {
            Id = (int)Id, Fecha = DateTime.Parse(Fecha), UsuarioId = (int)UsuarioId,
            Tipo = TextoATipoLimpieza(Tipo), Observaciones = Observaciones
        };
    }

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
