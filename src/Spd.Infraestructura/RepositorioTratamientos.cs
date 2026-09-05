using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a Tratamiento vía Dapper/SQLite. Sin DELETE/UPDATE de negocio salvo los campos
/// no clínicos (FR-411): cerrar/abrir fila es un INSERT + un UPDATE del `estado`/`fecha_fin` de la
/// fila anterior, nunca una reescritura de sus datos clínicos (Art. IV.3/IV.4).</summary>
public sealed class RepositorioTratamientos(SqliteConnection conexion) : IRepositorioTratamientos
{
    private const string FormatoFecha = "yyyy-MM-dd";

    private const string Columnas = """
        id, paciente_id, medicamento_id, en_spd, problema_salud, medico_id,
        pauta_d, pauta_a, pauta_c, pauta_n, pauta_texto, dias_semana, via, momento,
        fecha_inicio, fecha_fin, fecha_prescripcion_inicial, tipo, conocimiento_cumplimiento,
        incidencias, intervencion, estado, ajuste_unidades_manual
        """;

    public int Crear(Tratamiento t)
        => conexion.ExecuteScalar<int>(
            $"""
            INSERT INTO Tratamiento (
                paciente_id, medicamento_id, en_spd, problema_salud, medico_id,
                pauta_d, pauta_a, pauta_c, pauta_n, pauta_texto, dias_semana, via, momento,
                fecha_inicio, fecha_fin, fecha_prescripcion_inicial, tipo, conocimiento_cumplimiento,
                incidencias, intervencion, estado, ajuste_unidades_manual
            ) VALUES (
                @PacienteId, @MedicamentoId, @EnSpd, @ProblemaSalud, @MedicoId,
                @PautaD, @PautaA, @PautaC, @PautaN, @PautaTexto, @DiasSemana, @Via, @Momento,
                @FechaInicio, @FechaFin, @FechaPrescripcionInicial, @Tipo, @ConocimientoCumplimiento,
                @Incidencias, @Intervencion, @Estado, @AjusteUnidadesManual
            ) RETURNING id
            """,
            AParametros(t));

    public void Actualizar(Tratamiento t)
        => conexion.Execute(
            """
            UPDATE Tratamiento SET
                problema_salud = @ProblemaSalud, medico_id = @MedicoId,
                pauta_d = @PautaD, pauta_a = @PautaA, pauta_c = @PautaC, pauta_n = @PautaN,
                pauta_texto = @PautaTexto, dias_semana = @DiasSemana, via = @Via, momento = @Momento,
                fecha_fin = @FechaFin, tipo = @Tipo, conocimiento_cumplimiento = @ConocimientoCumplimiento,
                incidencias = @Incidencias, intervencion = @Intervencion, estado = @Estado,
                ajuste_unidades_manual = @AjusteUnidadesManual, modificado_en = @ModificadoEn
            WHERE id = @Id
            """,
            AParametros(t));

    public Tratamiento? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<TratamientoFila>($"SELECT {Columnas} FROM Tratamiento WHERE id = @id", new { id })
            ?.ATratamiento();

    public IReadOnlyList<Tratamiento> ListarVigentesDePaciente(int pacienteId)
        => conexion.Query<TratamientoFila>(
                $"""
                SELECT {Columnas} FROM Tratamiento
                WHERE paciente_id = @pacienteId AND fecha_fin IS NULL AND estado != 'Finalizado'
                ORDER BY fecha_inicio DESC
                """,
                new { pacienteId })
            .Select(f => f.ATratamiento())
            .ToList();

    public IReadOnlyList<Tratamiento> ListarHistorialDeMedicamento(int pacienteId, int medicamentoId)
        => conexion.Query<TratamientoFila>(
                $"""
                SELECT {Columnas} FROM Tratamiento
                WHERE paciente_id = @pacienteId AND medicamento_id = @medicamentoId
                ORDER BY fecha_inicio, id
                """,
                new { pacienteId, medicamentoId })
            .Select(f => f.ATratamiento())
            .ToList();

    private static object AParametros(Tratamiento t) => new
    {
        t.Id,
        t.PacienteId,
        t.MedicamentoId,
        EnSpd = t.EnSpd ? 1 : 0,
        t.ProblemaSalud,
        t.MedicoId,
        PautaD = t.PautaD?.ToString(),
        PautaA = t.PautaA?.ToString(),
        PautaC = t.PautaC?.ToString(),
        PautaN = t.PautaN?.ToString(),
        t.PautaTexto,
        t.DiasSemana,
        t.Via,
        t.Momento,
        FechaInicio = t.FechaInicio.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        FechaFin = t.FechaFin?.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        FechaPrescripcionInicial = t.FechaPrescripcionInicial.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        Tipo = t.Tipo.ToString(),
        t.ConocimientoCumplimiento,
        t.Incidencias,
        t.Intervencion,
        Estado = t.Estado.ToString(),
        t.AjusteUnidadesManual,
        ModificadoEn = DateTime.UtcNow.ToString("o")
    };

    /// <summary>Fila 1:1 con las columnas leídas de Tratamiento. Evita depender de la conversión
    /// automática de Dapper para los enums y los booleanos (mismo motivo que PacienteFila).</summary>
    private sealed record TratamientoFila(
        long Id, long PacienteId, long MedicamentoId, long EnSpd, string? ProblemaSalud, long? MedicoId,
        string? PautaD, string? PautaA, string? PautaC, string? PautaN, string? PautaTexto,
        string DiasSemana, string? Via, string? Momento, string FechaInicio, string? FechaFin,
        string FechaPrescripcionInicial, string Tipo, string? ConocimientoCumplimiento,
        string? Incidencias, string? Intervencion, string Estado, long? AjusteUnidadesManual)
    {
        public Tratamiento ATratamiento() => new()
        {
            Id = (int)Id,
            PacienteId = (int)PacienteId,
            MedicamentoId = (int)MedicamentoId,
            EnSpd = EnSpd == 1,
            ProblemaSalud = ProblemaSalud,
            MedicoId = MedicoId is null ? null : (int)MedicoId,
            PautaD = PautaD is null ? null : Enum.Parse<FraccionDosis>(PautaD),
            PautaA = PautaA is null ? null : Enum.Parse<FraccionDosis>(PautaA),
            PautaC = PautaC is null ? null : Enum.Parse<FraccionDosis>(PautaC),
            PautaN = PautaN is null ? null : Enum.Parse<FraccionDosis>(PautaN),
            PautaTexto = PautaTexto,
            DiasSemana = DiasSemana,
            Via = Via,
            Momento = Momento,
            FechaInicio = DateOnly.ParseExact(FechaInicio, FormatoFecha, CultureInfo.InvariantCulture),
            FechaFin = FechaFin is null ? null : DateOnly.ParseExact(FechaFin, FormatoFecha, CultureInfo.InvariantCulture),
            FechaPrescripcionInicial = DateOnly.ParseExact(FechaPrescripcionInicial, FormatoFecha, CultureInfo.InvariantCulture),
            Tipo = Enum.Parse<TipoTratamiento>(Tipo),
            ConocimientoCumplimiento = ConocimientoCumplimiento,
            Incidencias = Incidencias,
            Intervencion = Intervencion,
            Estado = Enum.Parse<EstadoTratamiento>(Estado),
            AjusteUnidadesManual = AjusteUnidadesManual is null ? null : (int)AjusteUnidadesManual
        };
    }
}
