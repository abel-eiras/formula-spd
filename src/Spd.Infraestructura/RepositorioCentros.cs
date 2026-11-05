using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Catálogo de centros y asignación de pacientes (Spec 019). SQL explícito (Art. VIII.4).</summary>
public sealed class RepositorioCentros(SqliteConnection conexion) : IRepositorioCentros
{
    private const string Columnas = "id, nombre, direccion, telefono, persona_contacto, activo, fecha_baja, motivo_baja";

    public int Crear(Centro centro, int? usuarioId)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO Centro (nombre, direccion, telefono, persona_contacto, activo, creado_por)
            VALUES (@Nombre, @Direccion, @Telefono, @PersonaContacto, 1, @usuarioId);
            SELECT last_insert_rowid();
            """,
            new { centro.Nombre, centro.Direccion, centro.Telefono, centro.PersonaContacto, usuarioId });

    public void Actualizar(Centro centro, int? usuarioId)
        => conexion.Execute(
            """
            UPDATE Centro SET nombre = @Nombre, direccion = @Direccion, telefono = @Telefono,
                persona_contacto = @PersonaContacto, activo = @Activo, fecha_baja = @FechaBaja, motivo_baja = @MotivoBaja,
                modificado_en = @ahora, modificado_por = @usuarioId
            WHERE id = @Id
            """,
            new
            {
                centro.Id, centro.Nombre, centro.Direccion, centro.Telefono, centro.PersonaContacto, centro.Activo,
                FechaBaja = centro.FechaBaja?.ToString("o"), centro.MotivoBaja, usuarioId, ahora = DateTime.UtcNow.ToString("o")
            });

    public Centro? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<FilaCentro>($"SELECT {Columnas} FROM Centro WHERE id = @id", new { id })?.ACentro();

    public IReadOnlyList<Centro> Listar(bool incluirBaja)
        => conexion.Query<FilaCentro>(
                $"SELECT {Columnas} FROM Centro {(incluirBaja ? string.Empty : "WHERE activo = 1")} ORDER BY activo DESC, nombre")
            .Select(f => f.ACentro()).ToList();

    public void AsignarPaciente(int pacienteId, int? centroId)
        => conexion.Execute("UPDATE Paciente SET centro_id = @centroId WHERE id = @pacienteId", new { pacienteId, centroId });

    public int? CentroDePaciente(int pacienteId)
        => (int?)conexion.ExecuteScalar<long?>("SELECT centro_id FROM Paciente WHERE id = @pacienteId", new { pacienteId });

    public IReadOnlyList<int> PacientesDe(int centroId)
        => conexion.Query<long>("SELECT id FROM Paciente WHERE centro_id = @centroId ORDER BY apellidos, nombre", new { centroId })
            .Select(id => (int)id).ToList();

    private sealed class FilaCentro
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? PersonaContacto { get; set; }
        public long Activo { get; set; }
        public string? FechaBaja { get; set; }
        public string? MotivoBaja { get; set; }

        public Centro ACentro() => new()
        {
            Id = (int)Id, Nombre = Nombre, Direccion = Direccion, Telefono = Telefono, PersonaContacto = PersonaContacto,
            Activo = Activo != 0, FechaBaja = FechaBaja is null ? null : DateTime.Parse(FechaBaja, System.Globalization.CultureInfo.InvariantCulture),
            MotivoBaja = MotivoBaja
        };
    }
}
