using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a EvaluacionIdoneidad vía Dapper/SQLite (Art. VIII.4: SQL explícito). Solo
/// INSERT (FR-202, Art. III). `Fila` explícita para no depender de la conversión automática de
/// enums y booleanos (mismo patrón que el resto de repositorios).</summary>
public sealed class RepositorioEvaluacionesIdoneidad(SqliteConnection conexion) : IRepositorioEvaluacionesIdoneidad
{
    private const string Columnas = """
        id, paciente_id, fecha, farmaceutico_id, criterio_1, criterio_2, criterio_3, criterio_4, criterio_5,
        criterio_6, criterio_7, condicion_motivacion, condicion_destreza, observaciones, resultado
        """;

    public int Crear(EvaluacionIdoneidad e)
        => conexion.ExecuteScalar<int>(
            $"""
            INSERT INTO EvaluacionIdoneidad (
                paciente_id, fecha, farmaceutico_id, criterio_1, criterio_2, criterio_3, criterio_4, criterio_5,
                criterio_6, criterio_7, condicion_motivacion, condicion_destreza, observaciones, resultado
            ) VALUES (
                @PacienteId, @Fecha, @FarmaceuticoId, @C1, @C2, @C3, @C4, @C5, @C6, @C7, @Motivacion, @Destreza,
                @Observaciones, @Resultado
            ) RETURNING id
            """,
            new
            {
                e.PacienteId, Fecha = e.Fecha.ToString("o"), e.FarmaceuticoId,
                C1 = e.Criterio1 ? 1 : 0, C2 = e.Criterio2 ? 1 : 0, C3 = e.Criterio3 ? 1 : 0, C4 = e.Criterio4 ? 1 : 0,
                C5 = e.Criterio5 ? 1 : 0, C6 = e.Criterio6 ? 1 : 0, C7 = e.Criterio7 ? 1 : 0,
                Motivacion = e.CondicionMotivacion ? 1 : 0, Destreza = e.CondicionDestreza ? 1 : 0,
                e.Observaciones, Resultado = ResultadoATexto(e.Resultado)
            });

    public EvaluacionIdoneidad? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<Fila>($"SELECT {Columnas} FROM EvaluacionIdoneidad WHERE id = @id", new { id })?.AEvaluacion();

    public IReadOnlyList<EvaluacionIdoneidad> ListarDePaciente(int pacienteId)
        => conexion.Query<Fila>(
                $"SELECT {Columnas} FROM EvaluacionIdoneidad WHERE paciente_id = @pacienteId ORDER BY fecha DESC, id DESC",
                new { pacienteId })
            .Select(f => f.AEvaluacion())
            .ToList();

    public EvaluacionIdoneidad? ObtenerVigente(int pacienteId) => ListarDePaciente(pacienteId).FirstOrDefault();

    internal static string ResultadoATexto(ResultadoIdoneidad r) => r == ResultadoIdoneidad.Apto ? "APTO" : "NO_APTO";

    private sealed record Fila(
        long Id, long PacienteId, string Fecha, long? FarmaceuticoId, long Criterio1, long Criterio2, long Criterio3,
        long Criterio4, long Criterio5, long Criterio6, long Criterio7, long CondicionMotivacion, long CondicionDestreza,
        string? Observaciones, string Resultado)
    {
        public EvaluacionIdoneidad AEvaluacion() => new()
        {
            Id = (int)Id,
            PacienteId = (int)PacienteId,
            Fecha = DateTime.Parse(Fecha, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            FarmaceuticoId = (int?)FarmaceuticoId,
            Criterio1 = Criterio1 == 1, Criterio2 = Criterio2 == 1, Criterio3 = Criterio3 == 1, Criterio4 = Criterio4 == 1,
            Criterio5 = Criterio5 == 1, Criterio6 = Criterio6 == 1, Criterio7 = Criterio7 == 1,
            CondicionMotivacion = CondicionMotivacion == 1, CondicionDestreza = CondicionDestreza == 1,
            Observaciones = Observaciones,
            Resultado = Resultado == "APTO" ? ResultadoIdoneidad.Apto : ResultadoIdoneidad.NoApto
        };
    }
}
