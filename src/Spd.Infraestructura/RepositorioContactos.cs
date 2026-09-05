using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso a los contactos de un paciente vía Dapper/SQLite. La baja es lógica (Art. III.1).</summary>
public sealed class RepositorioContactos(SqliteConnection conexion) : IRepositorioContactos
{
    private const string Columnas = """
        id, paciente_id, tipo, nombre, apellidos, dni, telefono, email, es_principal,
        retira_medicacion, activo, fecha_baja
        """;

    public Contacto? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<ContactoFila>($"SELECT {Columnas} FROM Contacto WHERE id = @id", new { id })
            ?.AContacto();

    public IReadOnlyList<Contacto> ListarDePaciente(int pacienteId, bool incluirBaja)
    {
        var filtroActivo = incluirBaja ? "" : "AND activo = 1";
        return conexion.Query<ContactoFila>(
                $"SELECT {Columnas} FROM Contacto WHERE paciente_id = @pacienteId {filtroActivo} ORDER BY id",
                new { pacienteId })
            .Select(f => f.AContacto())
            .ToList();
    }

    public int Crear(Contacto contacto)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO Contacto (
                paciente_id, tipo, nombre, apellidos, dni, telefono, email, es_principal,
                retira_medicacion, activo
            ) VALUES (
                @PacienteId, @Tipo, @Nombre, @Apellidos, @Dni, @Telefono, @Email, @EsPrincipal,
                @RetiraMedicacion, @Activo
            ) RETURNING id
            """,
            AParametros(contacto));

    public void Actualizar(Contacto contacto)
        => conexion.Execute(
            """
            UPDATE Contacto SET
                tipo = @Tipo, nombre = @Nombre, apellidos = @Apellidos, dni = @Dni,
                telefono = @Telefono, email = @Email, es_principal = @EsPrincipal,
                retira_medicacion = @RetiraMedicacion, activo = @Activo, fecha_baja = @FechaBaja
            WHERE id = @Id
            """,
            new
            {
                contacto.Id, Tipo = TipoATexto(contacto.Tipo), contacto.Nombre, contacto.Apellidos,
                contacto.Dni, contacto.Telefono, contacto.Email,
                EsPrincipal = contacto.EsPrincipal ? 1 : 0, RetiraMedicacion = contacto.RetiraMedicacion ? 1 : 0,
                Activo = contacto.Activo ? 1 : 0, contacto.FechaBaja
            });

    private static object AParametros(Contacto c) => new
    {
        c.PacienteId, Tipo = TipoATexto(c.Tipo), c.Nombre, c.Apellidos, c.Dni, c.Telefono, c.Email,
        EsPrincipal = c.EsPrincipal ? 1 : 0, RetiraMedicacion = c.RetiraMedicacion ? 1 : 0,
        Activo = c.Activo ? 1 : 0
    };

    private static string TipoATexto(TipoContacto tipo) => tipo switch
    {
        TipoContacto.Familiar => "FAMILIAR",
        TipoContacto.RepresentanteLegal => "REPRESENTANTE_LEGAL",
        TipoContacto.PersonaAutorizada => "PERSONA_AUTORIZADA",
        TipoContacto.Cuidador => "CUIDADOR",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    private static TipoContacto TextoATipo(string texto) => texto switch
    {
        "FAMILIAR" => TipoContacto.Familiar,
        "REPRESENTANTE_LEGAL" => TipoContacto.RepresentanteLegal,
        "PERSONA_AUTORIZADA" => TipoContacto.PersonaAutorizada,
        "CUIDADOR" => TipoContacto.Cuidador,
        _ => throw new ArgumentOutOfRangeException(nameof(texto))
    };

    /// <summary>Fila 1:1 con las columnas leídas de Contacto. Evita depender de la conversión
    /// automática de Dapper para el enum `tipo` y los booleanos (mismo motivo que UsuarioFila en
    /// Spec 000).</summary>
    private sealed record ContactoFila(
        long Id, long PacienteId, string Tipo, string Nombre, string Apellidos, string? Dni,
        string? Telefono, string? Email, long EsPrincipal, long RetiraMedicacion, long Activo,
        string? FechaBaja)
    {
        public Contacto AContacto() => new()
        {
            Id = (int)Id,
            PacienteId = (int)PacienteId,
            Tipo = TextoATipo(Tipo),
            Nombre = Nombre,
            Apellidos = Apellidos,
            Dni = Dni,
            Telefono = Telefono,
            Email = Email,
            EsPrincipal = EsPrincipal == 1,
            RetiraMedicacion = RetiraMedicacion == 1,
            Activo = Activo == 1,
            FechaBaja = FechaBaja is null ? null : DateTime.Parse(FechaBaja)
        };
    }
}
