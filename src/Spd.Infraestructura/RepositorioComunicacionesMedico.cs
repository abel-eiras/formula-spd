using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a ComunicacionMedico vía Dapper/SQLite. Solo INSERT y el UPDATE acotado de
/// respuesta (Art. III/FR-807).</summary>
public sealed class RepositorioComunicacionesMedico(SqliteConnection conexion) : IRepositorioComunicacionesMedico
{
    private const string FormatoFecha = "yyyy-MM-dd";

    private const string Columnas = """
        id, paciente_id, medico_id, tipo, fecha, incidencias_detectadas, propuesta,
        respuesta, fecha_respuesta, farmaceutico_id
        """;

    public int Crear(ComunicacionMedico c)
        => conexion.ExecuteScalar<int>(
            $"""
            INSERT INTO ComunicacionMedico (
                paciente_id, medico_id, tipo, fecha, incidencias_detectadas, propuesta,
                respuesta, fecha_respuesta, farmaceutico_id
            ) VALUES (
                @PacienteId, @MedicoId, @Tipo, @Fecha, @IncidenciasDetectadas, @Propuesta,
                @Respuesta, @FechaRespuesta, @FarmaceuticoId
            ) RETURNING id
            """,
            AParametros(c));

    public void ActualizarRespuesta(ComunicacionMedico c)
        => conexion.Execute(
            """
            UPDATE ComunicacionMedico SET respuesta = @Respuesta, fecha_respuesta = @FechaRespuesta, modificado_en = @ModificadoEn
            WHERE id = @Id
            """,
            new
            {
                c.Id,
                c.Respuesta,
                FechaRespuesta = c.FechaRespuesta?.ToString(FormatoFecha, CultureInfo.InvariantCulture),
                ModificadoEn = DateTime.UtcNow.ToString("o")
            });

    public ComunicacionMedico? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<ComunicacionMedicoFila>($"SELECT {Columnas} FROM ComunicacionMedico WHERE id = @id", new { id })
            ?.AComunicacionMedico();

    public IReadOnlyList<ComunicacionMedico> ListarDePaciente(int pacienteId)
        => conexion.Query<ComunicacionMedicoFila>(
                $"SELECT {Columnas} FROM ComunicacionMedico WHERE paciente_id = @pacienteId ORDER BY fecha DESC, id DESC",
                new { pacienteId })
            .Select(f => f.AComunicacionMedico())
            .ToList();

    private static object AParametros(ComunicacionMedico c) => new
    {
        c.PacienteId,
        c.MedicoId,
        Tipo = c.Tipo.ToString(),
        Fecha = c.Fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        c.IncidenciasDetectadas,
        c.Propuesta,
        c.Respuesta,
        FechaRespuesta = c.FechaRespuesta?.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        c.FarmaceuticoId
    };

    private sealed record ComunicacionMedicoFila(
        long Id, long PacienteId, long MedicoId, string Tipo, string Fecha, string? IncidenciasDetectadas,
        string? Propuesta, string? Respuesta, string? FechaRespuesta, long? FarmaceuticoId)
    {
        public ComunicacionMedico AComunicacionMedico() => new()
        {
            Id = (int)Id,
            PacienteId = (int)PacienteId,
            MedicoId = (int)MedicoId,
            Tipo = Enum.Parse<TipoComunicacionMedico>(Tipo),
            Fecha = DateOnly.ParseExact(Fecha, FormatoFecha, CultureInfo.InvariantCulture),
            IncidenciasDetectadas = IncidenciasDetectadas,
            Propuesta = Propuesta,
            Respuesta = Respuesta,
            FechaRespuesta = FechaRespuesta is null ? null : DateOnly.ParseExact(FechaRespuesta, FormatoFecha, CultureInfo.InvariantCulture),
            FarmaceuticoId = FarmaceuticoId is null ? null : (int)FarmaceuticoId
        };
    }
}
