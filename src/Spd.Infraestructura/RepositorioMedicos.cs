using Dapper;
using Microsoft.Data.Sqlite;
using Spd.Dominio;

namespace Spd.Infraestructura;

/// <summary>Acceso al catálogo de médicos vía Dapper/SQLite (Art. IV.1).</summary>
public sealed class RepositorioMedicos(SqliteConnection conexion) : IRepositorioMedicos
{
    private const string Columnas = """
        id, nombre, apellidos, colegiado, especialidad, centro, telefono, email, direccion,
        activo, busqueda_normalizada
        """;

    public Medico? ObtenerPorId(int id)
        => conexion.QuerySingleOrDefault<MedicoFila>($"SELECT {Columnas} FROM Medico WHERE id = @id", new { id })?.AMedico();

    public IReadOnlyList<Medico> Buscar(string fragmentoNormalizado)
        => conexion.Query<MedicoFila>(
                $"SELECT {Columnas} FROM Medico WHERE activo = 1 AND busqueda_normalizada LIKE @patron ORDER BY apellidos, nombre",
                new { patron = $"%{fragmentoNormalizado}%" })
            .Select(f => f.AMedico())
            .ToList();

    public IReadOnlyList<Medico> ListarActivos()
        => conexion.Query<MedicoFila>($"SELECT {Columnas} FROM Medico WHERE activo = 1 ORDER BY apellidos, nombre")
            .Select(f => f.AMedico())
            .ToList();

    public int Crear(Medico medico)
        => conexion.ExecuteScalar<int>(
            """
            INSERT INTO Medico (
                nombre, apellidos, colegiado, especialidad, centro, telefono, email, direccion,
                activo, busqueda_normalizada
            ) VALUES (
                @Nombre, @Apellidos, @Colegiado, @Especialidad, @Centro, @Telefono, @Email, @Direccion,
                @Activo, @BusquedaNormalizada
            ) RETURNING id
            """,
            AParametros(medico));

    public void Actualizar(Medico medico)
        => conexion.Execute(
            """
            UPDATE Medico SET
                nombre = @Nombre, apellidos = @Apellidos, colegiado = @Colegiado,
                especialidad = @Especialidad, centro = @Centro, telefono = @Telefono, email = @Email,
                direccion = @Direccion, activo = @Activo, busqueda_normalizada = @BusquedaNormalizada,
                modificado_en = @ModificadoEn
            WHERE id = @Id
            """,
            new
            {
                medico.Id, medico.Nombre, medico.Apellidos, medico.Colegiado, medico.Especialidad,
                medico.Centro, medico.Telefono, medico.Email, medico.Direccion,
                Activo = medico.Activo ? 1 : 0, medico.BusquedaNormalizada,
                ModificadoEn = DateTime.UtcNow.ToString("o")
            });

    private static object AParametros(Medico m) => new
    {
        m.Nombre, m.Apellidos, m.Colegiado, m.Especialidad, m.Centro, m.Telefono, m.Email, m.Direccion,
        Activo = m.Activo ? 1 : 0, m.BusquedaNormalizada
    };

    /// <summary>Fila 1:1 con las columnas leídas de Medico. Evita depender de la conversión
    /// automática de Dapper para el booleano `activo` (mismo motivo que UsuarioFila en Spec 000:
    /// explícito antes que una conversión implícita sin garantía documentada).</summary>
    private sealed record MedicoFila(
        long Id, string Nombre, string Apellidos, string? Colegiado, string Especialidad, string? Centro,
        string? Telefono, string? Email, string? Direccion, long Activo, string BusquedaNormalizada)
    {
        public Medico AMedico() => new()
        {
            Id = (int)Id,
            Nombre = Nombre,
            Apellidos = Apellidos,
            Colegiado = Colegiado,
            Especialidad = Especialidad,
            Centro = Centro,
            Telefono = Telefono,
            Email = Email,
            Direccion = Direccion,
            Activo = Activo == 1,
            BusquedaNormalizada = BusquedaNormalizada
        };
    }
}
