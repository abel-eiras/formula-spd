using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a Consentimiento vía Dapper/SQLite (Art. VIII.4). INSERT y UPDATE de
/// firma/revocación/impresión; sin DELETE (Art. III).</summary>
public sealed class RepositorioConsentimientos(SqliteConnection conexion) : IRepositorioConsentimientos
{
    private const string FormatoFecha = "yyyy-MM-dd";
    private const string Columnas = """
        id, paciente_id, tipo, contacto_id, fecha_creacion, fecha_firma, fecha_revocacion, motivo_revocacion, impreso_en
        """;

    public int Crear(Consentimiento c)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO Consentimiento (paciente_id, tipo, contacto_id, fecha_creacion, fecha_firma, fecha_revocacion, motivo_revocacion, impreso_en)
            VALUES (@PacienteId, @Tipo, @ContactoId, @FechaCreacion, @FechaFirma, @FechaRevocacion, @MotivoRevocacion, @ImpresoEn)
            RETURNING id
            """,
            Parametros(c));

    public void Actualizar(Consentimiento c)
        => conexion.Execute(
            """
            UPDATE Consentimiento SET
                fecha_firma = @FechaFirma, fecha_revocacion = @FechaRevocacion,
                motivo_revocacion = @MotivoRevocacion, impreso_en = @ImpresoEn
            WHERE id = @Id
            """,
            Parametros(c));

    public Consentimiento? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<Fila>($"SELECT {Columnas} FROM Consentimiento WHERE id = @id", new { id })?.AConsentimiento();

    public IReadOnlyList<Consentimiento> ListarDePaciente(int pacienteId)
        => conexion.Query<Fila>(
                $"SELECT {Columnas} FROM Consentimiento WHERE paciente_id = @pacienteId ORDER BY fecha_creacion DESC, id DESC",
                new { pacienteId })
            .Select(f => f.AConsentimiento())
            .ToList();

    public Consentimiento? ObtenerVigente(int pacienteId)
        => ListarDePaciente(pacienteId).FirstOrDefault(c => c.Vigente);

    private static object Parametros(Consentimiento c) => new
    {
        c.Id, c.PacienteId, Tipo = c.Tipo == TipoConsentimiento.Paciente ? "PACIENTE" : "REPRESENTANTE", c.ContactoId,
        FechaCreacion = c.FechaCreacion.ToString("o"),
        FechaFirma = c.FechaFirma?.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        FechaRevocacion = c.FechaRevocacion?.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        c.MotivoRevocacion,
        ImpresoEn = c.ImpresoEn?.ToString("o")
    };

    private sealed record Fila(
        long Id, long PacienteId, string Tipo, long? ContactoId, string FechaCreacion, string? FechaFirma,
        string? FechaRevocacion, string? MotivoRevocacion, string? ImpresoEn)
    {
        public Consentimiento AConsentimiento() => new()
        {
            Id = (int)Id,
            PacienteId = (int)PacienteId,
            Tipo = Tipo == "PACIENTE" ? TipoConsentimiento.Paciente : TipoConsentimiento.Representante,
            ContactoId = (int?)ContactoId,
            FechaCreacion = DateTime.Parse(FechaCreacion, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            FechaFirma = FechaFirma is null ? null : DateOnly.ParseExact(FechaFirma, FormatoFecha, CultureInfo.InvariantCulture),
            FechaRevocacion = FechaRevocacion is null ? null : DateOnly.ParseExact(FechaRevocacion, FormatoFecha, CultureInfo.InvariantCulture),
            MotivoRevocacion = MotivoRevocacion,
            ImpresoEn = ImpresoEn is null ? null : DateTime.Parse(ImpresoEn, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
        };
    }
}
