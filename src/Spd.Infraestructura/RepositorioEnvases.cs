using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a Envase vía Dapper/SQLite. Sin DELETE de negocio: solo INSERT y transiciones
/// de estado por UPDATE (Art. III, FR-511).</summary>
public sealed class RepositorioEnvases(SqliteConnection conexion) : IRepositorioEnvases
{
    private const string FormatoFecha = "yyyy-MM-dd";

    private const string Columnas = """
        id, paciente_id, medicamento_id, serie, lote, caducidad, unidades_iniciales,
        unidades_restantes, fecha_entrada, origen, estado, fecha_salida, motivo_salida,
        motivo_salida_detalle, entregado_a
        """;

    public int Crear(Envase e)
        => conexion.ExecuteScalar<int>(
            $"""
            INSERT INTO Envase (
                paciente_id, medicamento_id, serie, lote, caducidad, unidades_iniciales,
                unidades_restantes, fecha_entrada, origen, estado, fecha_salida, motivo_salida,
                motivo_salida_detalle, entregado_a
            ) VALUES (
                @PacienteId, @MedicamentoId, @Serie, @Lote, @Caducidad, @UnidadesIniciales,
                @UnidadesRestantes, @FechaEntrada, @Origen, @Estado, @FechaSalida, @MotivoSalida,
                @MotivoSalidaDetalle, @EntregadoA
            ) RETURNING id
            """,
            AParametros(e));

    public void Actualizar(Envase e)
        => conexion.Execute(
            """
            UPDATE Envase SET
                unidades_restantes = @UnidadesRestantes, estado = @Estado, fecha_salida = @FechaSalida,
                motivo_salida = @MotivoSalida, motivo_salida_detalle = @MotivoSalidaDetalle,
                entregado_a = @EntregadoA, modificado_en = @ModificadoEn
            WHERE id = @Id
            """,
            AParametros(e));

    public Envase? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<EnvaseFila>($"SELECT {Columnas} FROM Envase WHERE id = @id", new { id })
            ?.AEnvase();

    public Envase? ObtenerPorSerie(string serie)
        => conexion.QuerySingleOrDefault<EnvaseFila>($"SELECT {Columnas} FROM Envase WHERE serie = @serie", new { serie })
            ?.AEnvase();

    public IReadOnlyList<Envase> ListarEnCustodiaDePacienteYMedicamento(int pacienteId, int medicamentoId)
        => conexion.Query<EnvaseFila>(
                $"""
                SELECT {Columnas} FROM Envase
                WHERE paciente_id = @pacienteId AND medicamento_id = @medicamentoId AND estado = 'EnCustodia'
                ORDER BY unidades_restantes ASC, caducidad ASC
                """,
                new { pacienteId, medicamentoId })
            .Select(f => f.AEnvase())
            .ToList();

    public IReadOnlyList<Envase> ListarEnCustodiaDePaciente(int pacienteId)
        => conexion.Query<EnvaseFila>(
                $"SELECT {Columnas} FROM Envase WHERE paciente_id = @pacienteId AND estado = 'EnCustodia' ORDER BY caducidad ASC",
                new { pacienteId })
            .Select(f => f.AEnvase())
            .ToList();

    public IReadOnlyList<Envase> ListarHistoricoDePaciente(int pacienteId)
        => conexion.Query<EnvaseFila>(
                $"SELECT {Columnas} FROM Envase WHERE paciente_id = @pacienteId AND estado != 'EnCustodia' ORDER BY fecha_entrada DESC",
                new { pacienteId })
            .Select(f => f.AEnvase())
            .ToList();

    public IReadOnlyList<Envase> ListarEnCustodiaDeTodos()
        => conexion.Query<EnvaseFila>($"SELECT {Columnas} FROM Envase WHERE estado = 'EnCustodia'")
            .Select(f => f.AEnvase())
            .ToList();

    private static object AParametros(Envase e) => new
    {
        e.Id,
        e.PacienteId,
        e.MedicamentoId,
        e.Serie,
        e.Lote,
        Caducidad = e.Caducidad?.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        e.UnidadesIniciales,
        e.UnidadesRestantes,
        FechaEntrada = e.FechaEntrada == default ? DateTime.UtcNow.ToString("o") : e.FechaEntrada.ToString("o"),
        Origen = e.Origen.ToString(),
        Estado = e.Estado.ToString(),
        FechaSalida = e.FechaSalida?.ToString("o"),
        MotivoSalida = e.MotivoSalida?.ToString(),
        e.MotivoSalidaDetalle,
        e.EntregadoA,
        ModificadoEn = DateTime.UtcNow.ToString("o")
    };

    /// <summary>Fila 1:1 con las columnas leídas de Envase. Evita depender de la conversión
    /// automática de Dapper para los enums (mismo motivo que TratamientoFila).</summary>
    private sealed record EnvaseFila(
        long Id, long PacienteId, long MedicamentoId, string? Serie, string? Lote, string? Caducidad,
        long? UnidadesIniciales, long? UnidadesRestantes, string FechaEntrada, string Origen, string Estado,
        string? FechaSalida, string? MotivoSalida, string? MotivoSalidaDetalle, string? EntregadoA)
    {
        public Envase AEnvase() => new()
        {
            Id = (int)Id,
            PacienteId = (int)PacienteId,
            MedicamentoId = (int)MedicamentoId,
            Serie = Serie,
            Lote = Lote,
            Caducidad = Caducidad is null ? null : DateOnly.ParseExact(Caducidad, FormatoFecha, CultureInfo.InvariantCulture),
            UnidadesIniciales = UnidadesIniciales is null ? null : (int)UnidadesIniciales,
            UnidadesRestantes = UnidadesRestantes is null ? null : (int)UnidadesRestantes,
            FechaEntrada = DateTime.Parse(FechaEntrada, CultureInfo.InvariantCulture),
            Origen = Enum.Parse<OrigenEnvase>(Origen),
            Estado = Enum.Parse<EstadoEnvase>(Estado),
            FechaSalida = FechaSalida is null ? null : DateTime.Parse(FechaSalida, CultureInfo.InvariantCulture),
            MotivoSalida = MotivoSalida is null ? null : Enum.Parse<MotivoSalidaEnvase>(MotivoSalida),
            MotivoSalidaDetalle = MotivoSalidaDetalle,
            EntregadoA = EntregadoA
        };
    }
}
